// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeCleanup;
using Microsoft.CodeAnalysis.CodeCleanup.Providers;
using Microsoft.CodeAnalysis.SemanticModelWorkspaceService;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.UnitTests.CodeCleanup
{
    using CSharp = Microsoft.CodeAnalysis.CSharp;

    [UseExportProvider]
    public class CodeCleanupTests
    {
#if false
        [Fact]
        public void DefaultCSharpCodeCleanups()
        {
            var codeCleanups = CodeCleaner.GetDefaultProviders(LanguageNames.CSharp);
            Assert.NotNull(codeCleanups);
            Assert.NotEmpty(codeCleanups);
        }

        [Fact]
        public void DefaultVisualBasicCodeCleanups()
        {
            var codeCleanups = CodeCleaner.GetDefaultProviders(LanguageNames.VisualBasic);
            Assert.NotNull(codeCleanups);
            Assert.NotEmpty(codeCleanups);
        }
#endif

        [Fact]
        public async Task CodeCleaners_NoSpans()
        {
            var document = CreateDocument("class C { }", LanguageNames.CSharp);
            var cleanDocument = await CodeCleaner.CleanupAsync(document, ImmutableArray<TextSpan>.Empty);

            Assert.Equal(document, cleanDocument);
        }

        [Fact]
        public async Task CodeCleaners_Document()
        {
            var document = CreateDocument("class C { }", LanguageNames.CSharp);
            var cleanDocument = await CodeCleaner.CleanupAsync(document);

            Assert.Equal(document, cleanDocument);
        }

        [Fact]
        public async Task CodeCleaners_Span()
        {
            var document = CreateDocument("class C { }", LanguageNames.CSharp);
            var cleanDocument = await CodeCleaner.CleanupAsync(document, (await document.GetSyntaxRootAsync()).FullSpan);

            Assert.Equal(document, cleanDocument);
        }

        [Fact]
        public async Task CodeCleaners_Spans()
        {
            var document = CreateDocument("class C { }", LanguageNames.CSharp);
            var cleanDocument = await CodeCleaner.CleanupAsync(document, ImmutableArray.Create(
                (await document.GetSyntaxRootAsync()).FullSpan));

            Assert.Equal(document, cleanDocument);
        }

        [Fact]
        public async Task CodeCleaners_Annotation()
        {
            var document = CreateDocument("class C { }", LanguageNames.CSharp);
            var annotation = new SyntaxAnnotation();
            document = document.WithSyntaxRoot((await document.GetSyntaxRootAsync()).WithAdditionalAnnotations(annotation));

            var cleanDocument = await CodeCleaner.CleanupAsync(document, annotation);

            Assert.Equal(document, cleanDocument);
        }

        [Fact]
        public void EntireRange()
        {
            VerifyRange("{|b:{|r:class C {}|}|}");
        }

        [Fact]
        public void EntireRange_Merge()
        {
            VerifyRange("{|r:class {|b:C { }|} class {|b: B { } |}|}");
        }

        [Fact]
        public void EntireRange_EndOfFile()
        {
            VerifyRange("{|r:class {|b:C { }|} class {|b: B { } |} |}");
        }

        [Fact]
        public void EntireRangeWithTransformation_RemoveClass()
        {
            var expectedResult = default(IEnumerable<TextSpan>);
            var transformer = new SimpleCodeCleanupProvider("TransformerCleanup", async (doc, spans, cancellationToken) =>
            {
                var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
                root = root.RemoveCSharpMember(0);

                expectedResult = SpecializedCollections.SingletonEnumerable(root.FullSpan);

                return doc.WithSyntaxRoot(root);
            });

            VerifyRange("{|b:class C {}|}", transformer, ref expectedResult);
        }

        [Fact]
        public void EntireRangeWithTransformation_AddMember()
        {
            var expectedResult = default(IEnumerable<TextSpan>);
            var transformer = new SimpleCodeCleanupProvider("TransformerCleanup", async (doc, spans, cancellationToken) =>
            {
                var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
                var @class = root.GetMember(0);
                var classWithMember = @class.AddCSharpMember(CreateCSharpMethod(), 0);
                root = root.ReplaceNode(@class, classWithMember);

                expectedResult = SpecializedCollections.SingletonEnumerable(root.FullSpan);

                return doc.WithSyntaxRoot(root);
            });

            VerifyRange("{|b:class C {}|}", transformer, ref expectedResult);
        }

        [Fact]
        public void RangeWithTransformation_AddMember()
        {
            var expectedResult = default(IEnumerable<TextSpan>);
            var transformer = new SimpleCodeCleanupProvider("TransformerCleanup", async (doc, spans, cancellationToken) =>
            {
                var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
                var @class = root.GetMember(0).GetMember(0);
                var classWithMember = @class.AddCSharpMember(CreateCSharpMethod(), 0);
                root = root.ReplaceNode(@class, classWithMember);

                expectedResult = SpecializedCollections.SingletonEnumerable(root.GetMember(0).GetMember(0).GetCodeCleanupSpan());

                return doc.WithSyntaxRoot(root);
            });

            VerifyRange("namespace N { {|b:class C {}|} }", transformer, ref expectedResult);
        }

        [Fact]
        public void RangeWithTransformation_RemoveMember()
        {
            var expectedResult = default(IEnumerable<TextSpan>);
            var transformer = new SimpleCodeCleanupProvider("TransformerCleanup", async (doc, spans, cancellationToken) =>
            {
                var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
                var @class = root.GetMember(0).GetMember(0);
                var classWithMember = @class.RemoveCSharpMember(0);
                root = root.ReplaceNode(@class, classWithMember);

                expectedResult = SpecializedCollections.SingletonEnumerable(root.GetMember(0).GetMember(0).GetCodeCleanupSpan());

                return doc.WithSyntaxRoot(root);
            });

            VerifyRange("namespace N { {|b:class C { void Method() { } }|} }", transformer, ref expectedResult);
        }

        [Fact]
        public void MultipleRange_Overlapped()
        {
            VerifyRange("namespace N {|r:{ {|b:class C { {|b:void Method() { }|} }|} }|}");
        }

        [Fact]
        public void MultipleRange_Adjacent()
        {
            VerifyRange("namespace N {|r:{ {|b:class C { |}{|b:void Method() { } }|} }|}");
        }

        [Fact]
        public void MultipleRanges()
        {
            VerifyRange("namespace N { class C {|r:{ {|b:void Method() { }|} }|} class C2 {|r:{ {|b:void Method() { }|} }|} }");
        }

        [Fact]
        public void RangeWithTransformation_OutsideOfRange()
        {
            var expectedResult = default(IEnumerable<TextSpan>);
            var transformer = new SimpleCodeCleanupProvider("TransformerCleanup", async (doc, spans, cancellationToken) =>
            {
                var root = await doc.GetSyntaxRootAsync().ConfigureAwait(false);
                var member = root.GetMember(0).GetMember(0).GetMember(0);
                var previousToken = member.GetFirstToken().GetPreviousToken().GetPreviousToken();
                var nextToken = member.GetLastToken().GetNextToken().GetNextToken();

                root = root.ReplaceToken(previousToken, CSharp.SyntaxFactory.Identifier(previousToken.LeadingTrivia, previousToken.ValueText, previousToken.TrailingTrivia));
                root = root.ReplaceToken(nextToken, CSharp.SyntaxFactory.Token(nextToken.LeadingTrivia, CSharp.CSharpExtensions.Kind(nextToken), nextToken.TrailingTrivia));

                expectedResult = SpecializedCollections.EmptyEnumerable<TextSpan>();

                return doc.WithSyntaxRoot(root);
            });

            VerifyRange("namespace N { class C { {|b:void Method() { }|} } }", transformer, ref expectedResult);
        }

        public static CSharp.Syntax.MethodDeclarationSyntax CreateCSharpMethod(string returnType = "void", string methodName = "Method")
        {
            return CSharp.SyntaxFactory.MethodDeclaration(CSharp.SyntaxFactory.ParseTypeName(returnType), CSharp.SyntaxFactory.Identifier(methodName));
        }

        private void VerifyRange(string codeWithMarker, string language = LanguageNames.CSharp)
        {
            MarkupTestFile.GetSpans(codeWithMarker,
                out var codeWithoutMarker, out IDictionary<string, ImmutableArray<TextSpan>> namedSpans);

            var expectedResult = namedSpans.ContainsKey("r") ? namedSpans["r"] as IEnumerable<TextSpan> : SpecializedCollections.EmptyEnumerable<TextSpan>();

            VerifyRange(codeWithoutMarker, ImmutableArray<ICodeCleanupProvider>.Empty, namedSpans["b"], ref expectedResult, language);
        }

        private void VerifyRange(string codeWithMarker, ICodeCleanupProvider transformer, ref IEnumerable<TextSpan> expectedResult, string language = LanguageNames.CSharp)
        {
            MarkupTestFile.GetSpans(codeWithMarker,
                out var codeWithoutMarker, out IDictionary<string, ImmutableArray<TextSpan>> namedSpans);

            VerifyRange(codeWithoutMarker, ImmutableArray.Create(transformer), namedSpans["b"], ref expectedResult, language);
        }

        private void VerifyRange(string code, ImmutableArray<ICodeCleanupProvider> codeCleanups, ImmutableArray<TextSpan> spans, ref IEnumerable<TextSpan> expectedResult, string language)
        {
            var result = default(IEnumerable<TextSpan>);
            var spanCodeCleanup = new SimpleCodeCleanupProvider("TestCodeCleanup", (d, s, c) =>
            {
                result = s;
                return Task.FromResult(d);
            });

            var document = CreateDocument(code, language);

            CodeCleaner.CleanupAsync(document, spans, codeCleanups.Concat(spanCodeCleanup)).Wait();

            var sortedSpans = result.ToList();
            var expectedSpans = expectedResult.ToList();

            sortedSpans.Sort();
            expectedSpans.Sort();

            AssertEx.Equal(expectedSpans, sortedSpans);
        }

        private static Document CreateDocument(string code, string language)
        {
            var solution = new AdhocWorkspace().CurrentSolution;
            var projectId = ProjectId.CreateNewId();
            var project = solution.AddProject(projectId, "Project", "Project.dll", language).GetProject(projectId);

            return project.AddDocument("Document", SourceText.From(code));
        }
    }
}
