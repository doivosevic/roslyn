// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis.CodeGeneration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.LanguageServices;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.CodeGeneration
{
    [UseExportProvider]
    public abstract class AbstractCodeGenerationTests
    {
        private static SyntaxNode Simplify(
            AdhocWorkspace workspace,
            SyntaxNode syntaxNode,
            string languageName)
        {
            var projectId = ProjectId.CreateNewId();

            var project = workspace.CurrentSolution
                .AddProject(projectId, languageName, $"{languageName}.dll", languageName).GetProject(projectId);

            var normalizedSyntax = syntaxNode.NormalizeWhitespace().ToFullString();
            var document = project.AddMetadataReference(TestReferences.NetFx.v4_0_30319.mscorlib)
                .AddDocument("Fake Document", SourceText.From(normalizedSyntax));

            var annotatedDocument = document.WithSyntaxRoot(
                    document.GetSyntaxRootAsync().Result.WithAdditionalAnnotations(Simplification.Simplifier.Annotation));

            var annotatedRootNode = annotatedDocument.GetSyntaxRootAsync().Result;

            var simplifiedDocument = Simplification.Simplifier.ReduceAsync(annotatedDocument).Result;

            var rootNode = simplifiedDocument.GetSyntaxRootAsync().Result;

            return rootNode;
        }

        private static SyntaxNode WrapExpressionInBoilerplate(SyntaxNode expression, SyntaxGenerator codeDefFactory)
        {
            return codeDefFactory.CompilationUnit(
                codeDefFactory.NamespaceImportDeclaration(codeDefFactory.IdentifierName("System")),
                codeDefFactory.ClassDeclaration(
                    "C",
                    members: new[]
                    {
                        codeDefFactory.MethodDeclaration(
                            "Dummy",
                            returnType: null,
                            statements: new[]
                            {
                                codeDefFactory.LocalDeclarationStatement("test", expression)
                            })
                    })
                );
        }
        
        protected static ITypeSymbol CreateClass(string name)
        {
            return CodeGenerationSymbolFactory.CreateNamedTypeSymbol(
                attributes: default, accessibility: default, modifiers: default, TypeKind.Class, name);
        }
    }
}
