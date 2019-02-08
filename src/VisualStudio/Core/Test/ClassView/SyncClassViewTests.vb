' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.Editor.Shared.Utilities
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Utilities
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.VisualStudio.LanguageServices.UnitTests.Utilities.VsNavInfo
Imports Microsoft.VisualStudio.Text.Editor.Commanding.Commands
Imports Roslyn.Test.Utilities

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.ClassView
    <[UseExportProvider]>
    Public Class SyncClassViewTests

#Region "C# Tests"

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestClass1()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    $$
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestClass2()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C$$
                {
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestClass3()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class $$C
                {
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestClass4()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                $$class C
                {
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestClass5()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {  $$
                    void M()
                    {
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestClassInNestedNamespaces1()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace X.Y
            {
                class C
                {  $$
                    void M()
                    {
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("X.Y"),
                [Class]("C"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestClassInNestedNamespaces2()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace X
            {
                namespace Y
                {
                    class C
                    {  $$
                        void M()
                        {
                        }
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("X.Y"),
                [Class]("C"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestMethod1()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    void M()
                    {$$
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("M()"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestMethod2()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    void M()
                    $${
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("M()"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestMethod3()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    void $$M()
                    {
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("M()"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestMethod4()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    $$void M()
                    {
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("M()"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestMethod5()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
            $$        void M()
                    {
                    }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("M()"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestField1()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
            $$        int i;
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("i"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestField2()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    int $$i;
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("i"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestProperty1()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
            $$        int P { get; }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("P"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestProperty2()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    int $$P { get; }
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("P"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestEvent1()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
            $$        event System.EventHandler E;
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("E"))
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.ClassView)>
        Public Sub CSharp_TestEvent2()
            Dim workspace =
<Workspace>
    <Project Language="C#" CommonReferences="true" AssemblyName="CSharpTestAssembly">
        <Document>
            namespace N
            {
                class C
                {
                    event System.EventHandler $$E;
                }
            }
        </Document>
    </Project>
</Workspace>

            Test(workspace,
                Package("CSharpTestAssembly"),
                [Namespace]("N"),
                [Class]("C"),
                Member("E"))
        End Sub

#End Region

        Private Sub Test(
            workspaceDefinition As XElement,
            ParamArray presentationNodes As NodeVerifier()
        )

            Using workspace = TestWorkspace.Create(workspaceDefinition, exportProvider:=VisualStudioTestExportProvider.Factory.CreateExportProvider())
                Dim hostDocument = workspace.DocumentWithCursor
                Assert.True(hostDocument IsNot Nothing, "Test defined without cursor position")

                Dim textView = hostDocument.GetTextView()
                Dim subjectBuffer = hostDocument.GetTextBuffer()

                Dim navigationTool = New MockNavigationTool(canonicalNodes:=Nothing, presentationNodes:=presentationNodes)
                Dim serviceProvider = New MockServiceProvider(navigationTool)
                Dim commandHandler = New MockSyncClassViewCommandHandler(workspace.ExportProvider.GetExportedValue(Of IThreadingContext), serviceProvider)

                commandHandler.ExecuteCommand(
                    args:=New SyncClassViewCommandArgs(textView, subjectBuffer), TestCommandExecutionContext.Create())

                navigationTool.VerifyNavInfo()
            End Using

        End Sub

    End Class
End Namespace
