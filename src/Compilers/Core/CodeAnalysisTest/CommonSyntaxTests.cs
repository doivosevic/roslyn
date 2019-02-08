// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;
using CS = Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis.UnitTests
{
    public class CommonSyntaxTests
    {
        [Fact]
        public void SyntaxNodeOrToken()
        {
            var d = default(SyntaxNodeOrToken);

            Assert.True(d.IsToken);
            Assert.False(d.IsNode);

            Assert.Equal(0, d.RawKind);
            Assert.Equal("", d.Language);
            Assert.Equal(default(TextSpan), d.FullSpan);
            Assert.Equal(default(TextSpan), d.Span);
            Assert.Equal("", d.ToString());
            Assert.Equal("", d.ToFullString());
            Assert.Equal("SyntaxNodeOrToken None ", d.GetDebuggerDisplay());
        }

        [Fact]
        public void SyntaxNodeOrToken1()
        {
            var d = (SyntaxNodeOrToken)((SyntaxNode)null);

            Assert.False(d.IsToken);
            Assert.True(d.IsNode);

            Assert.False(d.IsEquivalentTo(default(SyntaxNodeOrToken)));

            Assert.Equal(0, d.RawKind);
            Assert.Equal("", d.Language);
            Assert.Equal(default(TextSpan), d.FullSpan);
            Assert.Equal(default(TextSpan), d.Span);
            Assert.Equal("", d.ToString());
            Assert.Equal("", d.ToFullString());
            Assert.Equal("SyntaxNodeOrToken None ", d.GetDebuggerDisplay());
        }

        [Fact]
        public void CommonSyntaxToString_CSharp()
        {
            SyntaxNode node = CSharp.SyntaxFactory.IdentifierName("test");
            Assert.Equal("test", node.ToString());

            SyntaxNodeOrToken nodeOrToken = node;
            Assert.Equal("test", nodeOrToken.ToString());

            SyntaxToken token = node.DescendantTokens().Single();
            Assert.Equal("test", token.ToString());

            SyntaxTrivia trivia = CSharp.SyntaxFactory.Whitespace("test");
            Assert.Equal("test", trivia.ToString());
        }

        [Fact]
        public void CommonSyntaxTriviaSpan_CSharp()
        {
            var csharpToken = CSharp.SyntaxFactory.ParseExpression("1 + 123 /*hello*/").GetLastToken();
            var csharpTriviaList = csharpToken.TrailingTrivia;
            Assert.Equal(2, csharpTriviaList.Count);

            var csharpTrivia = csharpTriviaList.ElementAt(1);
            Assert.Equal(CSharp.SyntaxKind.MultiLineCommentTrivia, CSharp.CSharpExtensions.Kind(csharpTrivia));

            var correctSpan = csharpTrivia.Span;
            Assert.Equal(8, correctSpan.Start);
            Assert.Equal(17, correctSpan.End);

            var commonTrivia = (SyntaxTrivia)csharpTrivia; //direct conversion
            Assert.Equal(correctSpan, commonTrivia.Span);

            var commonTriviaList = (SyntaxTriviaList)csharpTriviaList;

            var commonTrivia2 = commonTriviaList[1]; //from converted list
            Assert.Equal(correctSpan, commonTrivia2.Span);

            var commonToken = (SyntaxToken)csharpToken;
            var commonTriviaList2 = commonToken.TrailingTrivia;

            var commonTrivia3 = commonTriviaList2[1]; //from converted token
            Assert.Equal(correctSpan, commonTrivia3.Span);

            var csharpTrivia2 = (SyntaxTrivia)commonTrivia; //direct conversion
            Assert.Equal(correctSpan, csharpTrivia2.Span);

            var csharpTriviaList2 = (SyntaxTriviaList)commonTriviaList;

            var csharpTrivia3 = csharpTriviaList2.ElementAt(1); //from converted list
            Assert.Equal(correctSpan, csharpTrivia3.Span);
        }

        [Fact]
        public void TestTrackNodes()
        {
            var expr = CSharp.SyntaxFactory.ParseExpression("a + b + c + d");

            var exprB = expr.DescendantNodes().OfType<CSharp.Syntax.IdentifierNameSyntax>().First(n => n.Identifier.ToString() == "b");

            var trackedExpr = expr.TrackNodes(exprB);

            // replace each expression with a parenthesized expression
            trackedExpr = trackedExpr.ReplaceNodes(
                        nodes: trackedExpr.DescendantNodes().OfType<CSharp.Syntax.ExpressionSyntax>(),
                        computeReplacementNode: (node, rewritten) => CSharp.SyntaxFactory.ParenthesizedExpression(rewritten));

            trackedExpr = trackedExpr.NormalizeWhitespace();
            Assert.Equal("(((a) + (b)) + (c)) + (d)", trackedExpr.ToString());

            var trackedB = trackedExpr.GetCurrentNodes(exprB).First();
            Assert.Equal(CSharp.SyntaxKind.ParenthesizedExpression, CSharp.CSharpExtensions.Kind(trackedB.Parent));
        }

        [Fact]
        public void TestTrackNodesWithDuplicateIdAnnotations()
        {
            var expr = CSharp.SyntaxFactory.ParseExpression("a + b + c + d");

            var exprB = expr.DescendantNodes().OfType<CSharp.Syntax.IdentifierNameSyntax>().First(n => n.Identifier.ToString() == "b");

            var trackedExpr = expr.TrackNodes(exprB);
            var annotation = trackedExpr.GetAnnotatedNodes(SyntaxNodeExtensions.IdAnnotationKind).First()
                                        .GetAnnotations(SyntaxNodeExtensions.IdAnnotationKind).First();

            // replace each expression with a parenthesized expression
            trackedExpr = trackedExpr.ReplaceNodes(
                        nodes: trackedExpr.DescendantNodes().OfType<CSharp.Syntax.ExpressionSyntax>(),
                        computeReplacementNode: (node, rewritten) => CSharp.SyntaxFactory.ParenthesizedExpression(rewritten).WithAdditionalAnnotations(annotation));

            trackedExpr = trackedExpr.NormalizeWhitespace();
            Assert.Equal("(((a) + (b)) + (c)) + (d)", trackedExpr.ToString());

            var trackedB = trackedExpr.GetCurrentNodes(exprB).First();
            Assert.Equal(CSharp.SyntaxKind.ParenthesizedExpression, CSharp.CSharpExtensions.Kind(trackedB.Parent));
        }
    }
}
