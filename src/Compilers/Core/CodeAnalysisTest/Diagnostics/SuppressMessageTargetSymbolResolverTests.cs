// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.UnitTests.Diagnostics
{
    public class SuppressMessageTargetSymbolResolverTests
    {
        [Fact]
        public void TestResolveGlobalNamespace1()
        {
            VerifyNamespaceResolution("namespace $$N {}", LanguageNames.CSharp, false, "N");
        }
        
        [Fact]
        public void TestResolveNestedNamespace1()
        {
            VerifyNamespaceResolution(@"
namespace A
{
    namespace B.$$C
    {
    }
}
",
                LanguageNames.CSharp, false, "A.B.C");
        }
        
        [Fact]
        public void TestResolveNamespaceWithSameNameAsGenericInterface1()
        {
            VerifyNamespaceResolution(@"
namespace $$IGoo
{
}
interface IGoo<T>
{
}
",
                LanguageNames.CSharp, false, "IGoo");
        }

        [Fact]
        public void TestDontPartiallyResolveNamespace1()
        {
            VerifyNoNamespaceResolution(@"
namespace A
{
    namespace B
    {
    }
}
",
                LanguageNames.CSharp, false, "A+B", "A#B");
        }

        [Fact]
        public void TestResolveGlobalType1()
        {
            VerifyTypeResolution("class $$C {}", LanguageNames.CSharp, false, "C");
        }

        [Fact]
        public void TestResolveTypeInNamespace1()
        {
            VerifyTypeResolution(@"
namespace N1.N2
{
    class $$C
    {
    }
}
",
                LanguageNames.CSharp, false, "N1.N2.C");
        }

        [Fact]
        public void TestResolveTypeNestedInGlobalType1()
        {
            VerifyTypeResolution(@"
class C
{
    interface $$D
    {
    }
}
",
                LanguageNames.CSharp, false, "C+D");
        }
        
        [Fact]
        public void TestResolveNestedType1()
        {
            VerifyTypeResolution(@"
namespace N
{
    class C
    {
        class D
        {
            class $$E
            { }
        }
    }
}
",
                LanguageNames.CSharp, false, "N.C+D+E");
        }
        
        [Fact]
        public void TestResolveGenericType1()
        {
            VerifyTypeResolution(@"
class D<T>
{
}
class $$D<T1, T2, T3>
{
}
",
                LanguageNames.CSharp, false, "D`3");
        }
        
        [Fact]
        public void TestDontResolveGenericType1()
        {
            VerifyNoTypeResolution(@"
class D<T1, T2, T3>
{
}
",
                LanguageNames.CSharp, false, "D");
        }
        
        [Fact]
        public void TestDontPartiallyResolveType1()
        {
            VerifyNoTypeResolution(@"
class A
{
    class B
    {
    }
}
",
                LanguageNames.CSharp, false, "A.B");
        }
        
        [Fact]
        public void TestResolveField1()
        {
            VerifyMemberResolution(@"
class C
{
    string $$s;
}
",
                LanguageNames.CSharp, false,
                "C.#s",
                "C.s");
        }
        
        [Fact]
        public void TestResolveProperty1()
        {
            VerifyMemberResolution(@"
class C
{
    public string $$StringProperty { get; set; }
}
",
                LanguageNames.CSharp, false,
                "C.#StringProperty",
                "C.StringProperty");
        }
        
        [Fact]
        public void TestResolveEvent1()
        {
            VerifyMemberResolution(@"
class C
{
    public event System.EventHandler<int> $$E;
}
",
                LanguageNames.CSharp, false,
                "e:C.#E",
                "C.E");
        }
        
        [Fact]
        public void TestDontResolveNonEvent1()
        {
            VerifyNoMemberResolution(@"
public class C
{
    public int E;
}
",
               LanguageNames.CSharp, false, "e:C.E");
        }
        
        [Fact]
        public void TestResolvePropertySetMethod1()
        {
            VerifyMemberResolution(@"
class C
{
    public string StringProperty { get; $$set; }
}
",
                LanguageNames.CSharp, false,
                "C.#set_StringProperty(System.String)",
                "C.set_StringProperty(System.String):System.Void");
        }
        
        [Fact]
        public void TestResolveEventAddMethod()
        {
            VerifyMemberResolution(@"
class C
{
    public delegate void Del(int x);
    public event Del E
    {
        $$add { }
        remove { }
    }
}
",
                LanguageNames.CSharp, false,
                "C.#add_E(C.Del)",
                "C.add_E(C.Del):System.Void");
        }

        [Fact]
        public void TestResolveEventRemoveMethod()
        {
            VerifyMemberResolution(@"
class C
{
    public delegate void Del(int x);
    public event Del E
    {
        add { }
        $$remove { }
    }
}
",
                LanguageNames.CSharp, false,
                "C.#remove_E(C.Del)",
                "C.remove_E(C.Del):System.Void");
        }

        [Fact]
        public void TestResolveVoidMethod1()
        {
            VerifyMemberResolution(@"
class C
{
    void Goo() {}
    void $$Goo(int x) {}
    void Goo(string x) {}
}
",
            LanguageNames.CSharp, false,
            "C.#Goo(System.Int32)",
            "C.Goo(System.Int32):System.Void");
        }
        
        [Fact]
        public void TestResolveMethod1()
        {
            VerifyMemberResolution(@"
class C
{
    void Goo() {}
    string Goo(int x) {}
    string $$Goo(string x) {}
}
",
            LanguageNames.CSharp, false,
            "C.#Goo(System.String)",
            "C.Goo(System.String):System.String");
        }
        
        [Fact]
        public void TestResolveOverloadedGenericMethod1()
        {
            VerifyMemberResolution(@"
class C
{
    int Goo<T>(T x) {}
    int $$Goo<T>(T x, params T[] y) {}
}
",
                LanguageNames.CSharp, false,
                "C.#Goo`1(!!0,!!0[])",
                "C.Goo(T,T[]):System.Int32");

            VerifyMemberResolution(@"
class C
{
    int [|Goo|]<T>(T x) {}
    int [|Goo|]<T>(T x, T y) {}
}
",
                LanguageNames.CSharp, false, "C.Goo():System.Int32");
        }
        
        [Fact]
        public void TestResolveMethodOverloadedOnArity1()
        {
            VerifyMemberResolution(@"
interface I
{
    void M<T>();
    void $$M<T1, T2>();
}
",
                LanguageNames.CSharp, false, "I.#M`2()");

            VerifyMemberResolution(@"
interface I
{
    void [|M|]<T>();
    void [|M|]<T1, T2>();
}
",
                LanguageNames.CSharp, false, "I.M():System.Void");
        }
        
        [Fact]
        public void TestResolveConstructor1()
        {
            VerifyMemberResolution(@"
class C
{
    $$C() {}
}
",
                LanguageNames.CSharp, false,
                "C.#.ctor()",
                "C..ctor()");
        }
        
        [Fact]
        public void TestResolveStaticConstructor1()
        {
            VerifyMemberResolution(@"
class C
{
    static $$C() {}
}
",
                LanguageNames.CSharp, false,
                "C.#.cctor()",
                "C..cctor()");
        }
        
        [Fact]
        public void TestResolveSimpleOperator1()
        {
            VerifyMemberResolution(@"
class C
{
    public static bool operator $$==(C a, C b)
    {
        return true;
    }
}
",
                LanguageNames.CSharp, false,
                "C.#op_Equality(C,C)",
                "C.op_Equality(C,C):System.Boolean");
        }
        
        [Fact]
        public void TestResolveIndexer1()
        {
            VerifyMemberResolution(@"
class C
{
    public C $$this[int i, string j]
    {
        get { return this; }
    }

    public C this[string i]
    {
        get { return this; }
    }
}
",
                LanguageNames.CSharp, false,
                "C.#Item[System.Int32,System.String]",
                "C.Item[System.Int32,System.String]");
        }
        
        [Fact]
        public void TestResolveIndexerAccessorMethod()
        {
            VerifyMemberResolution(@"
class C
{
    public C this[int i]
    {
        get { return this; }
    }

    public C this[string i]
    {
        $$get { return this; }
    }
}
",
                LanguageNames.CSharp, false,
                "C.#get_Item(System.String)",
                "C.get_Item(System.String):C");
        }

        [Fact]
        public void TestResolveExplicitOperator()
        {
            VerifyMemberResolution(@"
class C
{
    public static explicit operator $$bool(C c)
    {
        return C == null;
    }

    public static explicit operator string(C c)
    {
        return string.Empty;
    }
}
",
                LanguageNames.CSharp, false,
                "C.#op_Explicit(C):System.Boolean",
                "C.op_Explicit(C):System.Boolean");
        }

        [Fact]
        public void TestResolveMethodWithComplexParameterTypes1()
        {
            VerifyMemberResolution(@"
class C
{
    public unsafe static bool $$IsComplex<T0, T1>(int* a, ref int b, ref T0 c, T1[] d)
    {
        return true;
    }
}
",
                LanguageNames.CSharp, false,
                "C.#IsComplex`2(System.Int32*,System.Int32&,!!0&,!!1[])",
                "C.IsComplex(System.Int32*,System.Int32&,T0&,T1[]):System.Boolean");
        }
        
        [Fact]
        public void TestFinalize1()
        {
            VerifyMemberResolution(@"
class A
{
    ~$$A()
    {
    }
}
",
                LanguageNames.CSharp, false,
                "A.#Finalize()",
                "A.Finalize():System.Void");
        }
        
        [Fact]
        public void TestResolveMethodWithComplexReturnType1()
        {
            VerifyMemberResolution(@"
class C
{
    public static T[][][,,][,] $$GetComplex<T>()
    {
        return null;
    }
}
",
                LanguageNames.CSharp, false,
                "C.#GetComplex`1()",
                "C.GetComplex():T[,][,,][][]");
        }
        
        [Fact]
        public void TestResolveMethodWithGenericParametersAndReturnTypeFromContainingClass1()
        {
            VerifyMemberResolution(@"
public class C<T0>
{
    public class D<T1>
    {
        public T3 $$M<T2, T3>(T0 a, T1 b, T2 c)
        {
            return default(T3);
        }
    }
}
",
                LanguageNames.CSharp, false,
                "C`1+D`1.#M`2(!0,!1,!!0)",
                "C`1+D`1.M(T0,T1,T2):T3");
        }
        
        [Fact]
        public void TestResolveIndexerWithGenericParametersTypeFromContainingClass1()
        {
            VerifyMemberResolution(@"
public class C<T0>
{
    public class D<T1>
    {
        public T0 $$this[T1 a]
        {
            get { return default(T0); }
        }
    }
}
",
                LanguageNames.CSharp, false,
                "C`1+D`1.#Item[!1]",
                "C`1+D`1.Item[!1]:!0");
        }
        
        [Fact]
        public void TestResolveMethodOnOutParameter1()
        {
            VerifyMemberResolution(@"
class C
{
    void M0(int x)
    {
    }

    void $$M1(out int x)
    {
        x = 1;
    }
}
",
                LanguageNames.CSharp, false,
                "C.#M1(System.Int32&)",
                "C.M1(System.Int32&):System.Void");
        }
        
        [Fact]
        public void TestResolveMethodWithInstantiatedGenericParameterAndReturnType1()
        {
            VerifyMemberResolution(@"
class G<T0,T1>
{
}

class C<T3>
{
    G<int,int> $$M<T4>(G<double, double> g, G<T3, T4[]> h)
    {
    }
}
",
                LanguageNames.CSharp, false,
                "C.#M`1(G`2<System.Double,System.Double>,G`2<!0,!!0[]>)",
                "C.M(G`2<System.Double,System.Double>,G`2<T3,T4[]>):G`2<System.Int32,System.Int32>");
        }
        
        [Fact]
        public void TestResolveEscapedName1()
        {
            VerifyMemberResolution(@"
namespace @namespace
{
    class @class
    {
        int $$@if;
    }
}
",
                LanguageNames.CSharp, false,
                "namespace.class.if");
        }
        
        [Fact]
        public void TestResolveMethodIgnoresConvention1()
        {
            VerifyMemberResolution(@"
class C
{
    string $$Goo(string x) {}
}
",
            LanguageNames.CSharp, false,
            "C.#[vararg]Goo(System.String)",
            "C.#[cdecl]Goo(System.String)",
            "C.#[fastcall]Goo(System.String)",
            "C.#[stdcall]Goo(System.String)",
            "C.#[thiscall]Goo(System.String)");
        }
        
        [Fact]
        public void TestNoResolutionForMalformedNames1()
        {
            VerifyNoMemberResolution(@"
public class C<T0>
{
    public class D<T4>
    {
        int @namespace;

        public T3 M<T2, T3>(T0 a, T4 b, T2 c)
        {
            return null;
        }
    }
}
",
                LanguageNames.CSharp, false,
                "C`1+D`1.#M`2(!0,!1,!!0", // Missing close paren
                "C`1+D`1.M`2(T0,T4,T2):", // Missing return type
                "C`1+D`1.M`2(T0,T4,T2", // Missing close paren
                "C`1+D`1+M`2(T0,T4,T2)", // '+' instead of '.' delimiter
                "C`1.D`1.M`2(T0,T4,T2)", // '.' instead of '+' delimiter
                "C`1+D`1.@namespace", // Escaped name
                "C`1+D`1.#[blah]M`2(!0,!1,!!0)"); // Invalid calling convention
        }
        
        private static void VerifyNamespaceResolution(string markup, string language, bool rootNamespace, params string[] fxCopFullyQualifiedNames)
        {
            string rootNamespaceName = "";
            if (rootNamespace)
            {
                rootNamespaceName = "RootNamespace";
            }

            VerifyResolution(markup, fxCopFullyQualifiedNames, SuppressMessageAttributeState.TargetScope.Namespace, language, rootNamespaceName);
        }

        private static void VerifyNoNamespaceResolution(string markup, string language, bool rootNamespace, params string[] fxCopFullyQualifiedNames)
        {
            string rootNamespaceName = "";
            if (rootNamespace)
            {
                rootNamespaceName = "RootNamespace";
            }

            VerifyNoResolution(markup, fxCopFullyQualifiedNames, SuppressMessageAttributeState.TargetScope.Namespace, language, rootNamespaceName);
        }

        private static void VerifyTypeResolution(string markup, string language, bool rootNamespace, params string[] fxCopFullyQualifiedNames)
        {
            string rootNamespaceName = "";
            if (rootNamespace)
            {
                rootNamespaceName = "RootNamespace";
            }

            VerifyResolution(markup, fxCopFullyQualifiedNames, SuppressMessageAttributeState.TargetScope.Type, language, rootNamespaceName);
        }

        private static void VerifyNoTypeResolution(string markup, string language, bool rootNamespace, params string[] fxCopFullyQualifiedNames)
        {
            string rootNamespaceName = "";
            if (rootNamespace)
            {
                rootNamespaceName = "RootNamespace";
            }

            VerifyNoResolution(markup, fxCopFullyQualifiedNames, SuppressMessageAttributeState.TargetScope.Type, language, rootNamespaceName);
        }

        private static void VerifyMemberResolution(string markup, string language, bool rootNamespace, params string[] fxCopFullyQualifiedNames)
        {
            string rootNamespaceName = "";
            if (rootNamespace)
            {
                rootNamespaceName = "RootNamespace";
            }

            VerifyResolution(markup, fxCopFullyQualifiedNames, SuppressMessageAttributeState.TargetScope.Member, language, rootNamespaceName);
        }

        private static void VerifyNoMemberResolution(string markup, string language, bool rootNamespace, params string[] fxCopFullyQualifiedNames)
        {
            string rootNamespaceName = "";
            if (rootNamespace)
            {
                rootNamespaceName = "RootNamespace";
            }

            VerifyNoResolution(markup, fxCopFullyQualifiedNames, SuppressMessageAttributeState.TargetScope.Member, language, rootNamespaceName);
        }

        private static void VerifyResolution(string markup, string[] fxCopFullyQualifiedNames, SuppressMessageAttributeState.TargetScope scope, string language, string rootNamespace)
        {
            // Parse out the span containing the declaration of the expected symbol
            MarkupTestFile.GetPositionAndSpans(markup,
                out var source, out var pos, out IDictionary<string, ImmutableArray<TextSpan>> spans);

            Assert.True(pos != null || spans.Count > 0, "Must specify a position or spans marking expected symbols for resolution");

            // Get the expected symbol from the given position
            var syntaxTree = CreateSyntaxTree(source, language);
            var compilation = CreateCompilation(syntaxTree, language, rootNamespace);
            var model = compilation.GetSemanticModel(syntaxTree);
            var expectedSymbols = new List<ISymbol>();

            bool shouldResolveSingleSymbol = pos != null;
            if (shouldResolveSingleSymbol)
            {
                expectedSymbols.Add(GetSymbolAtPosition(model, pos.Value));
            }
            else
            {
                foreach (var span in spans.Values.First())
                {
                    expectedSymbols.Add(GetSymbolAtPosition(model, span.Start));
                }
            }

            // Resolve the symbol based on each given FxCop fully-qualified name
            foreach (var fxCopName in fxCopFullyQualifiedNames)
            {
                var symbols = SuppressMessageAttributeState.ResolveTargetSymbols(compilation, fxCopName, scope);

                if (shouldResolveSingleSymbol)
                {
                    var expectedSymbol = expectedSymbols.Single();

                    if (symbols.Count() > 1)
                    {
                        Assert.True(false,
                            string.Format("Expected to resolve FxCop fully-qualified name '{0}' to '{1}': got multiple symbols:\r\n{2}",
                                fxCopName, expectedSymbol, string.Join("\r\n", symbols)));
                    }

                    var symbol = symbols.SingleOrDefault();
                    Assert.True(expectedSymbol == symbol,
                        string.Format("Failed to resolve FxCop fully-qualified name '{0}' to symbol '{1}': got '{2}'",
                            fxCopName, expectedSymbol, symbol));
                }
                else
                {
                    foreach (var symbol in symbols)
                    {
                        Assert.True(expectedSymbols.Contains(symbol),
                            string.Format("Failed to resolve FxCop fully-qualified name '{0}' to symbols:\r\n{1}\r\nResolved to unexpected symbol '{2}'",
                                fxCopName, string.Join("\r\n", expectedSymbols), symbol));
                    }
                }
            }
        }

        private static ISymbol GetSymbolAtPosition(SemanticModel model, int pos)
        {
            var token = model.SyntaxTree.GetRoot().FindToken(pos);
            Assert.NotNull(token.Parent);

            var location = token.GetLocation();
            var q = from node in token.Parent.AncestorsAndSelf()
                    let candidate = model.GetDeclaredSymbol(node)
                    where candidate != null && candidate.Locations.Contains(location)
                    select candidate;

            var symbol = q.FirstOrDefault();
            Assert.NotNull(symbol);
            return symbol;
        }

        private static void VerifyNoResolution(string source, string[] fxCopFullyQualifiedNames, SuppressMessageAttributeState.TargetScope scope, string language, string rootNamespace)
        {
            var compilation = CreateCompilation(source, language, rootNamespace);

            foreach (var fxCopName in fxCopFullyQualifiedNames)
            {
                var symbols = SuppressMessageAttributeState.ResolveTargetSymbols(compilation, fxCopName, scope);

                Assert.True(symbols.FirstOrDefault() == null,
                    string.Format("Did not expect FxCop fully-qualified name '{0}' to resolve to any symbol: resolved to:\r\n{1}",
                        fxCopName, string.Join("\r\n", symbols)));
            }
        }

        private static Compilation CreateCompilation(SyntaxTree syntaxTree, string language, string rootNamespace)
        {
            string projectName = "TestProject";

            return CSharpCompilation.Create(
                projectName,
                syntaxTrees: new[] { syntaxTree },
                references: new[] { TestBase.MscorlibRef });
        }

        private static Compilation CreateCompilation(string source, string language, string rootNamespace)
        {
            return CreateCompilation(CreateSyntaxTree(source, language), language, rootNamespace);
        }

        private static SyntaxTree CreateSyntaxTree(string source, string language)
        {
            string fileName = language == LanguageNames.CSharp ? "Test.cs" : "Test.vb";

            return CSharpSyntaxTree.ParseText(source, path: fileName);
        }
    }
}
