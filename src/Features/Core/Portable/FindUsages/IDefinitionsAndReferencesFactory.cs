﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Features.RQName;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.FindSymbols.Finders;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.Collections;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.FindUsages
{
    using static FindUsagesHelpers;

    internal interface IDefinitionsAndReferencesFactory : IWorkspaceService
    {
        Task<DefinitionItem?> GetThirdPartyDefinitionItemAsync(
            Solution solution, DefinitionItem definitionItem, CancellationToken cancellationToken);
    }

    [ExportWorkspaceService(typeof(IDefinitionsAndReferencesFactory)), Shared]
    internal class DefaultDefinitionsAndReferencesFactory : IDefinitionsAndReferencesFactory
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public DefaultDefinitionsAndReferencesFactory()
        {
        }

        /// <summary>
        /// Provides an extension point that allows for other workspace layers to add additional
        /// results to the results found by the FindReferences engine.
        /// </summary>
        public virtual Task<DefinitionItem?> GetThirdPartyDefinitionItemAsync(
            Solution solution, DefinitionItem definitionItem, CancellationToken cancellationToken)
        {
            return SpecializedTasks.Null<DefinitionItem>();
        }
    }

    internal static class DefinitionItemExtensions
    {
        private static readonly SymbolDisplayFormat s_namePartsFormat = new(
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType);

        public static DefinitionItem ToNonClassifiedDefinitionItem(
            this ISymbol definition,
            Solution solution,
            bool includeHiddenLocations)
        {
            // Because we're passing in 'false' for 'includeClassifiedSpans', this won't ever have
            // to actually do async work.  This is because the only asynchrony is when we are trying
            // to compute the classified spans for the locations of the definition.  So it's totally 
            // fine to pass in CancellationToken.None and block on the result.
            return ToDefinitionItemAsync(
                definition, solution, isPrimary: false, includeHiddenLocations, includeClassifiedSpans: false,
                options: FindReferencesSearchOptions.Default, cancellationToken: CancellationToken.None).WaitAndGetResult_CanCallOnBackground(CancellationToken.None);
        }

        public static Task<DefinitionItem> ToNonClassifiedDefinitionItemAsync(
            this ISymbol definition,
            Solution solution,
            bool includeHiddenLocations,
            CancellationToken cancellationToken)
        {
            return ToDefinitionItemAsync(
                definition, solution, isPrimary: false, includeHiddenLocations, includeClassifiedSpans: false,
                options: FindReferencesSearchOptions.Default.With(unidirectionalHierarchyCascade: true), cancellationToken: cancellationToken);
        }

        public static Task<DefinitionItem> ToClassifiedDefinitionItemAsync(
            this ISymbol definition,
            Solution solution,
            bool isPrimary,
            bool includeHiddenLocations,
            FindReferencesSearchOptions options,
            CancellationToken cancellationToken)
        {
            return ToDefinitionItemAsync(
                definition, solution, isPrimary,
                includeHiddenLocations, includeClassifiedSpans: true,
                options, cancellationToken);
        }

        public static Task<DefinitionItem> ToClassifiedDefinitionItemAsync(
            this SymbolGroup group, Solution solution, bool isPrimary, bool includeHiddenLocations, FindReferencesSearchOptions options, CancellationToken cancellationToken)
        {
            // Make a single definition item that knows about all the locations of all the symbols in the group.
            var allLocations = group.Symbols.SelectMany(s => s.Locations).ToImmutableArray();
            return ToDefinitionItemAsync(group.Symbols.First(), allLocations, solution, isPrimary, includeHiddenLocations, includeClassifiedSpans: true, options, cancellationToken);
        }

        private static Task<DefinitionItem> ToDefinitionItemAsync(
            ISymbol definition, Solution solution, bool isPrimary, bool includeHiddenLocations, bool includeClassifiedSpans, FindReferencesSearchOptions options, CancellationToken cancellationToken)
        {
            return ToDefinitionItemAsync(definition, definition.Locations, solution, isPrimary, includeHiddenLocations, includeClassifiedSpans, options, cancellationToken);
        }

        private static async Task<DefinitionItem> ToDefinitionItemAsync(
            ISymbol definition,
            ImmutableArray<Location> locations,
            Solution solution,
            bool isPrimary,
            bool includeHiddenLocations,
            bool includeClassifiedSpans,
            FindReferencesSearchOptions options,
            CancellationToken cancellationToken)
        {
            // Ensure we're working with the original definition for the symbol. I.e. When we're 
            // creating definition items, we want to create them for types like Dictionary<TKey,TValue>
            // not some random instantiation of that type.  
            //
            // This ensures that the type will both display properly to the user, as well as ensuring
            // that we can accurately resolve the type later on when we try to navigate to it.
            if (!definition.IsTupleField())
            {
                // In an earlier implementation of the compiler APIs, tuples and tuple fields symbols were definitions
                // We pretend this is still the case
                definition = definition.OriginalDefinition;
            }

            var displayParts = GetDisplayParts(definition);
            var nameDisplayParts = definition.ToDisplayParts(s_namePartsFormat).ToTaggedText();

            var tags = GlyphTags.GetTags(definition.GetGlyph());
            var displayIfNoReferences = definition.ShouldShowWithNoReferenceLocations(
                options, showMetadataSymbolsWithoutReferences: false);

            var properties = GetProperties(definition, isPrimary);

            // If it's a namespace, don't create any normal location.  Namespaces
            // come from many different sources, but we'll only show a single 
            // root definition node for it.  That node won't be navigable.
            using var sourceLocations = TemporaryArray<DocumentSpan>.Empty;
            if (definition.Kind != SymbolKind.Namespace)
            {
                foreach (var location in locations)
                {
                    if (location.IsInMetadata)
                    {
                        return DefinitionItem.CreateMetadataDefinition(
                            tags, displayParts, nameDisplayParts, solution,
                            definition, properties, displayIfNoReferences);
                    }
                    else if (location.IsInSource)
                    {
                        if (!location.IsVisibleSourceLocation() &&
                            !includeHiddenLocations)
                        {
                            continue;
                        }

                        var document = solution.GetDocument(location.SourceTree);
                        if (document != null)
                        {
                            var classificationOptions = ClassificationOptions.From(document.Project);

                            var documentLocation = !includeClassifiedSpans
                                ? new DocumentSpan(document, location.SourceSpan)
                                : await ClassifiedSpansAndHighlightSpanFactory.GetClassifiedDocumentSpanAsync(
                                    document, location.SourceSpan, classificationOptions, cancellationToken).ConfigureAwait(false);

                            sourceLocations.Add(documentLocation);
                        }
                    }
                }
            }

            if (sourceLocations.Count == 0)
            {
                // If we got no definition locations, then create a sentinel one
                // that we can display but which will not allow navigation.
                return DefinitionItem.CreateNonNavigableItem(
                    tags, displayParts,
                    DefinitionItem.GetOriginationParts(definition),
                    properties, displayIfNoReferences);
            }

            var displayableProperties = AbstractReferenceFinder.GetAdditionalFindUsagesProperties(definition);

            return DefinitionItem.Create(
                tags, displayParts, sourceLocations.ToImmutableAndClear(),
                nameDisplayParts, properties, displayableProperties, displayIfNoReferences);
        }

        private static ImmutableDictionary<string, string> GetProperties(ISymbol definition, bool isPrimary)
        {
            var properties = ImmutableDictionary<string, string>.Empty;

            if (isPrimary)
            {
                properties = properties.Add(DefinitionItem.Primary, "");
            }

            var rqName = RQNameInternal.From(definition);
            if (rqName != null)
            {
                properties = properties.Add(DefinitionItem.RQNameKey1, rqName);
            }

            if (definition?.IsConstructor() == true)
            {
                // If the symbol being considered is a constructor include the containing type in case
                // a third party wants to navigate to that.
                rqName = RQNameInternal.From(definition.ContainingType);
                if (rqName != null)
                {
                    properties = properties.Add(DefinitionItem.RQNameKey2, rqName);
                }
            }

            return properties;
        }

        public static async Task<SourceReferenceItem?> TryCreateSourceReferenceItemAsync(
            this ReferenceLocation referenceLocation,
            DefinitionItem definitionItem,
            bool includeHiddenLocations,
            CancellationToken cancellationToken)
        {
            var location = referenceLocation.Location;

            Debug.Assert(location.IsInSource);
            if (!location.IsVisibleSourceLocation() &&
                !includeHiddenLocations)
            {
                return null;
            }

            var document = referenceLocation.Document;
            var sourceSpan = location.SourceSpan;
            var options = ClassificationOptions.From(document.Project);

            var documentSpan = await ClassifiedSpansAndHighlightSpanFactory.GetClassifiedDocumentSpanAsync(
                document, sourceSpan, options, cancellationToken).ConfigureAwait(false);

            return new SourceReferenceItem(definitionItem, documentSpan, referenceLocation.SymbolUsageInfo, referenceLocation.AdditionalProperties);
        }
    }
}
