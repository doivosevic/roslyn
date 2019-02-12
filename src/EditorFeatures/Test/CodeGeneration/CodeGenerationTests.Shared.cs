// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeGeneration;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.CodeGeneration
{
    public partial class CodeGenerationTests
    {
        [UseExportProvider]
        public class Shared
        {
            [Fact, Trait(Traits.Feature, Traits.Features.CodeGenerationSortDeclarations)]
            public async Task TestSortingDefaultTypeMemberAccessibility1()
            {
                var generationSource = "public class [|C|] { private string B; public string C; }";
                var initial = "public class [|C|] { string A; }";
                var expected = @"public class C {
    public string C;
    string A;
    private string B;
}";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true);

                initial = "public struct [|S|] { string A; }";
                expected = @"public struct S {
    public string C;
    string A;
    private string B;
}";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true);

                initial = "Public Class [|C|] \n Dim A As String \n End Class";
                expected = @"Public Class C
    Public C As String
    Dim A As String
    Private B As String
End Class";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true);

                initial = "Public Module [|M|] \n Dim A As String \n End Module";
                expected = @"Public Module M
    Public C As String
    Dim A As String
    Private B As String
End Module";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true);

                initial = "Public Structure [|S|] \n Dim A As String \n End Structure";
                expected = @"Public Structure S 
 Dim A As String
    Public C As String
    Private B As String
End Structure";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true);
            }

            [Fact, Trait(Traits.Feature, Traits.Features.CodeGenerationSortDeclarations)]
            public async Task TestDefaultTypeMemberAccessibility2()
            {
                var codeGenOptionNoBody = new CodeGenerationOptions(generateMethodBodies: false);

                var generationSource = "public class [|C|] { private void B(){} public void C(){}  }";
                var initial = "public interface [|I|] { void A(); }";
                var expected = @"public interface I { void A();
    void B();
    void C();
}";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true, codeGenerationOptions: codeGenOptionNoBody);

                initial = "Public Interface [|I|] \n Sub A() \n End Interface";
                expected = @"Public Interface I 
 Sub A()
    Sub B()
    Sub C()
End Interface";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true, codeGenerationOptions: codeGenOptionNoBody);

                initial = "Public Class [|C|] \n Sub A() \n End Sub \n End Class";
                expected = @"Public Class C 
 Sub A() 
 End Sub

    Public Sub C()
    End Sub

    Private Sub B()
    End Sub
End Class";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true);

                initial = "Public Module [|M|] \n Sub A() \n End Sub \n End Module";
                expected = @"Public Module M 
 Sub A() 
 End Sub

    Public Sub C()
    End Sub

    Private Sub B()
    End Sub
End Module";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected, onlyGenerateMembers: true);
            }

            [Fact, Trait(Traits.Feature, Traits.Features.CodeGenerationSortDeclarations)]
            public async Task TestDefaultNamespaceMemberAccessibility1()
            {
                var generationSource = "internal class [|B|]{}";
                var initial = "namespace [|N|] { class A{} }";
                var expected = @"namespace N { class A{}

    internal class B
    {
    }
}";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected);

                initial = "Namespace [|N|] \n Class A \n End Class \n End Namespace";
                expected = @"Namespace N 
 Class A 
 End Class

    Friend Class B
    End Class
End Namespace";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected);
            }

            [Fact, Trait(Traits.Feature, Traits.Features.CodeGenerationSortDeclarations)]
            public async Task TestDefaultNamespaceMemberAccessibility2()
            {
                var generationSource = "public class [|C|]{}";
                var initial = "namespace [|N|] { class A{} }";
                var expected = "namespace N { public class C { } class A{} }";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected);

                initial = "Namespace [|N|] \n Class A \n End Class \n End Namespace";
                expected = @"Namespace N
    Public Class C
    End Class

    Class A 
 End Class 
 End Namespace";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected);
            }

            [Fact, Trait(Traits.Feature, Traits.Features.MetadataAsSource)]
            public async Task TestDocumentationComment()
            {
                var generationSource = @"
public class [|C|]
{
    /// <summary>When in need, a documented method is a friend, indeed.</summary>
    public C() { }
}";
                var initial = "public class [|C|] { }";
                var expected = @"public class C
{
    /// 
    /// <member name=""M:C.#ctor"">
    ///     <summary>When in need, a documented method is a friend, indeed.</summary>
    /// </member>
    /// 
    public C();
}";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected,
                    codeGenerationOptions: new CodeGenerationOptions(generateMethodBodies: false, generateDocumentationComments: true),
                    onlyGenerateMembers: true);
            }

            [Fact, Trait(Traits.Feature, Traits.Features.CodeGeneration)]
            public async Task TestModifiers()
            {
                var generationSource = @"
namespace [|N|]
{
    public class A 
    {
        public virtual string Property { get { return null; } }
        public static abstract string Property1 { get; }

        public virtual void Method1() {}
        public static abstract void Method2() {}
    }

    public class C
    {
        public sealed override string Property { get { return null; } }
        public sealed override void Method1() {} 
    }
}";

                var initial = "namespace [|N|] { }";
                var expected = @"namespace N {
    namespace N
    {
        public class A
        {
            public static abstract string Property1 { get; }
            public virtual string Property { get; }

            public abstract static void Method2();
            public virtual void Method1();
        }

        public class C
        {
            public sealed override string Property { get; }

            public sealed override void Method1();
        }
    }
}";
                await TestGenerateFromSourceSymbolAsync(generationSource, initial, expected,
                    codeGenerationOptions: new CodeGenerationOptions(generateMethodBodies: false));

                var initialVB = "Namespace [|N|] End Namespace";
                var expectedVB = @"Namespace N End NamespaceNamespace N
        Public Class A
            Public Shared MustOverride ReadOnly Property Property1 As String
            Public Overridable ReadOnly Property [Property] As String
            Public MustOverride Shared Sub Method2()
            Public Overridable Sub Method1()
        End Class

        Public Class C
            Public Overrides NotOverridable ReadOnly Property [Property] As String
            Public NotOverridable Overrides Sub Method1()
        End Class
    End Namespace";
                await TestGenerateFromSourceSymbolAsync(generationSource, initialVB, expectedVB,
                    codeGenerationOptions: new CodeGenerationOptions(generateMethodBodies: false));
            }
        }
    }
}
