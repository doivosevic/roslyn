// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CodeGeneration;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Formatting.Rules;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.Composition;
using Roslyn.Test.Utilities;

namespace Microsoft.CodeAnalysis.Editor.UnitTests
{
    /// <summary>
    /// This type provides cached <see cref="IExportProviderFactory"/> instances for use in tests. These factories allow
    /// for efficient creation of <see cref="ExportProvider"/> instances without sharing mutable state.
    /// </summary>
    public static class TestExportProvider
    {
        private static Lazy<ComposableCatalog> s_lazyEntireAssemblyCatalogWithCSharpAndVisualBasic =
            new Lazy<ComposableCatalog>(() => CreateAssemblyCatalogWithCSharpAndVisualBasic());

        private static Lazy<IExportProviderFactory> s_lazyExportProviderFactoryWithCSharpAndVisualBasic =
            new Lazy<IExportProviderFactory>(() => ExportProviderCache.GetOrCreateExportProviderFactory(EntireAssemblyCatalogWithCSharpAndVisualBasic));

        public static ComposableCatalog EntireAssemblyCatalogWithCSharpAndVisualBasic
            => s_lazyEntireAssemblyCatalogWithCSharpAndVisualBasic.Value;

        public static IExportProviderFactory ExportProviderFactoryWithCSharpAndVisualBasic
            => s_lazyExportProviderFactoryWithCSharpAndVisualBasic.Value;

        public static ExportProvider ExportProviderWithCSharpAndVisualBasic
            => ExportProviderFactoryWithCSharpAndVisualBasic.CreateExportProvider();

        private static Lazy<ComposableCatalog> s_lazyMinimumCatalogWithCSharpAndVisualBasic =
            new Lazy<ComposableCatalog>(() => ExportProviderCache.CreateTypeCatalog(GetNeutralAndCSharpAndVisualBasicTypes())
                        .WithParts(ExportProviderCache.GetOrCreateAssemblyCatalog(MinimalTestExportProvider.GetEditorAssemblies())));

        private static Lazy<IExportProviderFactory> s_lazyMinimumExportProviderFactoryWithCSharpAndVisualBasic =
            new Lazy<IExportProviderFactory>(() => ExportProviderCache.GetOrCreateExportProviderFactory(MinimumCatalogWithCSharpAndVisualBasic));

        public static ComposableCatalog MinimumCatalogWithCSharpAndVisualBasic
            => s_lazyMinimumCatalogWithCSharpAndVisualBasic.Value;

        public static IExportProviderFactory MinimumExportProviderFactoryWithCSharpAndVisualBasic
            => s_lazyMinimumExportProviderFactoryWithCSharpAndVisualBasic.Value;

        private static Type[] GetNeutralAndCSharpAndVisualBasicTypes()
        {
            var types = new[]
            {
                // ROSLYN
                typeof(CodeAnalysis.CSharp.IntroduceVariable.CSharpIntroduceVariableService), // Ensures that CSharpFeatures is included in the composition
                typeof(CSharp.ContentType.ContentTypeDefinitions), // CSharp Content Type
                typeof(CSharp.Formatting.Indentation.CSharpIndentationService),
                typeof(CodeAnalysis.CSharp.CSharpCompilationFactoryService),
                typeof(CodeAnalysis.CSharp.CSharpSyntaxTreeFactoryServiceFactory), // CSharpServicesCore
                typeof(CodeAnalysis.CSharp.CodeGeneration.CSharpCodeGenerationServiceFactory),
                typeof(CodeAnalysis.CSharp.CSharpSyntaxFactsServiceFactory),
                typeof(CodeAnalysis.CSharp.FindSymbols.CSharpDeclaredSymbolInfoFactoryService),
                typeof(CodeAnalysis.CSharp.CSharpSymbolDeclarationService),
                typeof(CodeAnalysis.CSharp.Formatting.CSharpFormattingService),
                typeof(CSharp.LanguageServices.CSharpSymbolDisplayServiceFactory),
                typeof(CSharp.Interactive.CSharpInteractiveEvaluator),
                typeof(CodeAnalysis.CSharp.Simplification.CSharpSimplificationService),
                typeof(CodeAnalysis.CSharp.Rename.CSharpRenameConflictLanguageService),
                typeof(CodeAnalysis.CSharp.CSharpSemanticFactsServiceFactory),
                typeof(CodeAnalysis.CSharp.CodeGeneration.CSharpSyntaxGenerator),
                typeof(CSharp.LanguageServices.CSharpContentTypeLanguageService),
                typeof(CodeAnalysis.CSharp.Execution.CSharpOptionsSerializationService),
                typeof(CodeAnalysis.Execution.DesktopReferenceSerializationServiceFactory),
                typeof(CodeAnalysis.Execution.SerializerServiceFactory),
                typeof(CodeAnalysis.Shared.TestHooks.AsynchronousOperationListenerProvider),
                typeof(PrimaryWorkspace),
                typeof(TestExportProvider),
                typeof(ThreadingContext),
            };

            return ServiceTestExportProvider.GetLanguageNeutralTypes()
                .Concat(types)
                .Concat(DesktopTestHelpers.GetAllTypesImplementingGivenInterface(
                    typeof(CodeAnalysis.CSharp.Formatting.DefaultOperationProvider).Assembly, typeof(ISyntaxFormattingService)))
                .Concat(DesktopTestHelpers.GetAllTypesImplementingGivenInterface(
                    typeof(CodeAnalysis.CSharp.Formatting.DefaultOperationProvider).Assembly, typeof(IFormattingRule)))
                .Concat(DesktopTestHelpers.GetAllTypesImplementingGivenInterface(
                    typeof(CodeAnalysis.CSharp.Formatting.DefaultOperationProvider).Assembly, typeof(ICodeGenerationService)))
                .Concat(TestHelpers.GetAllTypesWithStaticFieldsImplementingType(typeof(CodeAnalysis.CSharp.Formatting.CSharpFormattingOptions).Assembly, typeof(CodeAnalysis.Options.IOption)))
                .Distinct()
                .ToArray();
        }

        private static IExportProviderFactory CreateExportProviderFactoryWithCSharpAndVisualBasic()
        {
            return ExportProviderCache.GetOrCreateExportProviderFactory(EntireAssemblyCatalogWithCSharpAndVisualBasic);
        }

        private static ComposableCatalog CreateAssemblyCatalogWithCSharpAndVisualBasic()
        {
            return ExportProviderCache
                .GetOrCreateAssemblyCatalog(GetCSharpAndVisualBasicAssemblies(), ExportProviderCache.CreateResolver())
                .WithCompositionService();
        }

        public static IEnumerable<Assembly> GetCSharpAndVisualBasicAssemblies()
        {
            return GetNeutralAndCSharpAndVisualBasicTypes().Select(t => t.Assembly).Distinct().Concat(MinimalTestExportProvider.GetEditorAssemblies());
        }
    }
}
