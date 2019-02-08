' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Threading.Tasks
Imports Microsoft.CodeAnalysis.Editor.UnitTests
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
Imports Microsoft.CodeAnalysis.FindSymbols
Imports Microsoft.CodeAnalysis.Shared.Extensions
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.VisualStudio.LanguageServices.Implementation.Library
Imports Microsoft.VisualStudio.LanguageServices.UnitTests.Utilities.VsNavInfo
Imports Microsoft.VisualStudio.Shell.Interop

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.VsNavInfo
    <[UseExportProvider]>
    Public Class VsNavInfoTests

#Region "C# Tests"

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestNamespace() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace $$N { }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("CSharpTestAssembly"),
                    [Namespace]("N")
                 },
                 presentationNodes:={
                    Package("CSharpTestAssembly"),
                    [Namespace]("N")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestClass() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class $$C { }
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("CSharpTestAssembly"),
                    [Namespace]("N"),
                    [Class]("C")
                 },
                 presentationNodes:={
                    Package("CSharpTestAssembly"),
                    [Namespace]("N"),
                    [Class]("C")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMethod() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    void $$M() { }
                }
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("CSharpTestAssembly"),
                    [Namespace]("N"),
                    [Class]("C"),
                    Member("M()")
                 },
                 presentationNodes:={
                    Package("CSharpTestAssembly"),
                    [Namespace]("N"),
                    [Class]("C"),
                    Member("M()")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMethod_Parameters() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    int $$M(int x, int y)
                    {
                        return x + y;
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("CSharpTestAssembly"),
                    [Namespace]("N"),
                    [Class]("C"),
                    Member("M(int, int)")
                 },
                 presentationNodes:={
                    Package("CSharpTestAssembly"),
                    [Namespace]("N"),
                    [Class]("C"),
                    Member("M(int, int)")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMetadata_Class1() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            using System;
            class C
            {
                String$$ s;
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Class]("String")
                 },
                 presentationNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Class]("String")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMetadata_Class2() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            using System.Text;
            class C
            {
                StringBuilder$$ sb;
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Namespace]("Text"),
                    [Class]("StringBuilder")
                 },
                 presentationNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System.Text"),
                    [Class]("StringBuilder")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMetadata_Ctor1() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            using System.Text;
            class C
            {
                StringBuilder sb = new StringBuilder$$();
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Namespace]("Text"),
                    [Class]("StringBuilder"),
                    Member("StringBuilder()")
                 },
                 presentationNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System.Text"),
                    [Class]("StringBuilder"),
                    Member("StringBuilder()")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMetadata_Ctor2() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            using System;
            class C
            {
                String s = new String$$(' ', 42);
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Class]("String"),
                    Member("String(char, int)")
                 },
                 presentationNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Class]("String"),
                    Member("String(char, int)")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMetadata_Method() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            using System;
            class C
            {
                String s = new String(' ', 42).Replace$$(' ', '\r');
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Class]("String"),
                    Member("Replace(char, char)")
                 },
                 presentationNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Class]("String"),
                    Member("Replace(char, char)")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMetadata_GenericType() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            using System.Collections.Generic;
            class C
            {
                $$List&lt;int&gt; s;
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Namespace]("Collections"),
                    [Namespace]("Generic"),
                    [Class]("List<T>")
                 },
                 presentationNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System.Collections.Generic"),
                    [Class]("List<T>")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestMetadata_GenericMethod() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            using System;
            class C
            {
                void M()
                {
                    var a = new int[] { 1, 2, 3, 4, 5 };
                    var r = Array.AsReadOnly$$(a);
                }
            }
        </Document>
    </Project>
</Workspace>

            Await TestAsync(workspace,
                 canonicalNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Class]("Array"),
                    Member("AsReadOnly<T>(T[])")
                 },
                 presentationNodes:={
                    Package("Z:\FxReferenceAssembliesUri"),
                    [Namespace]("System"),
                    [Class]("Array"),
                    Member("AsReadOnly<T>(T[])")
                 })
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestNull_Parameter() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            class C
            {
                void M(int i$$) { }
            }
        </Document>
    </Project>
</Workspace>

            Await TestIsNullAsync(workspace)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestNull_Local() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            class C
            {
                void M()
                {
                    int i$$;
                }
            }
        </Document>
    </Project>
</Workspace>

            Await TestIsNullAsync(workspace)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.VsNavInfo)>
        Public Async Function TestCSharp_TestNull_Label() As Task
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            class C
            {
                void M()
                {
                    label$$:
                        int i;
                }
            }
        </Document>
    </Project>
</Workspace>

            Await TestIsNullAsync(workspace)
        End Function

#End Region

        Private Shared Async Function TestAsync(
            workspaceDefinition As XElement,
            Optional useExpandedHierarchy As Boolean = False,
            Optional canonicalNodes As NodeVerifier() = Nothing,
            Optional presentationNodes As NodeVerifier() = Nothing
        ) As Task

            Using workspace = TestWorkspace.Create(workspaceDefinition, exportProvider:=VisualStudioTestExportProvider.Factory.CreateExportProvider())
                Dim hostDocument = workspace.DocumentWithCursor
                Assert.True(hostDocument IsNot Nothing, "Test defined without cursor position")

                Dim document = workspace.CurrentSolution.GetDocument(hostDocument.Id)
                Dim semanticModel = Await document.GetSemanticModelAsync()
                Dim position As Integer = hostDocument.CursorPosition.Value
                Dim symbol = Await SymbolFinder.FindSymbolAtPositionAsync(semanticModel, position, workspace).ConfigureAwait(False)
                Assert.True(symbol IsNot Nothing, $"Could not find symbol as position, {position}")

                Dim libraryService = document.GetLanguageService(Of ILibraryService)

                Dim project = document.Project
                Dim compilation = Await project.GetCompilationAsync()
                Dim navInfo = libraryService.NavInfoFactory.CreateForSymbol(symbol, document.Project, compilation, useExpandedHierarchy)
                Assert.True(navInfo IsNot Nothing, $"Could not retrieve nav info for {symbol.ToDisplayString()}")

                If canonicalNodes IsNot Nothing Then
                    Dim enumerator As IVsEnumNavInfoNodes = Nothing
                    IsOK(navInfo.EnumCanonicalNodes(enumerator))

                    VerifyNodes(enumerator, canonicalNodes)
                End If

                If presentationNodes IsNot Nothing Then
                    Dim enumerator As IVsEnumNavInfoNodes = Nothing
                    IsOK(navInfo.EnumPresentationNodes(CUInt(_LIB_LISTFLAGS.LLF_NONE), enumerator))

                    VerifyNodes(enumerator, presentationNodes)
                End If
            End Using
        End Function

        Private Shared Async Function TestIsNullAsync(
            workspaceDefinition As XElement,
            Optional useExpandedHierarchy As Boolean = False
        ) As Task

            Using workspace = TestWorkspace.Create(workspaceDefinition, exportProvider:=VisualStudioTestExportProvider.Factory.CreateExportProvider())
                Dim hostDocument = workspace.DocumentWithCursor
                Assert.True(hostDocument IsNot Nothing, "Test defined without cursor position")

                Dim document = workspace.CurrentSolution.GetDocument(hostDocument.Id)
                Dim semanticModel = Await document.GetSemanticModelAsync()
                Dim position As Integer = hostDocument.CursorPosition.Value
                Dim symbol = Await SymbolFinder.FindSymbolAtPositionAsync(semanticModel, position, workspace).ConfigureAwait(False)
                Assert.True(symbol IsNot Nothing, $"Could not find symbol as position, {position}")

                Dim libraryService = document.GetLanguageService(Of ILibraryService)

                Dim project = document.Project
                Dim compilation = Await project.GetCompilationAsync()
                Dim navInfo = libraryService.NavInfoFactory.CreateForSymbol(symbol, document.Project, compilation, useExpandedHierarchy)
                Assert.Null(navInfo)
            End Using
        End Function

    End Class
End Namespace
