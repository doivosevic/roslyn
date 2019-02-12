// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.Symbols
{
    public class IndexedPropertyTests : CSharpTestBase
    {
        [ClrOnlyFact]
        public void RefParameters()
        {
            var source1 =
@".class interface public abstract import IA
{
  .custom instance void [mscorlib]System.Runtime.InteropServices.CoClassAttribute::.ctor(class [mscorlib]System.Type) = ( 01 00 01 41 00 00 )
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 31 36 35 46 37 35 32 44 2D 45 39 43 34 2D 34 46 37 45 2D 42 30 44 30 2D 43 44 46 44 37 41 33 36 45 32 31 31 00 00 )
  .method public abstract virtual instance int32 get_P(int32& i) { }
  .method public abstract virtual instance void set_P(int32& i, int32 v) { }
  .property instance int32 P(int32&)
  {
    .get instance int32 IA::get_P(int32&)
    .set instance void IA::set_P(int32&, int32)
  }
}
.class public A implements IA
{
  .method public hidebysig specialname rtspecialname instance void .ctor()
  {
    ret
  }
  // i += 1; return 0;
  .method public virtual instance int32 get_P(int32& i)
  {
    ldarg.1
    ldarg.1
    ldind.i4
    ldc.i4.1
    add.ovf
    stind.i4
    ldc.i4.0
    ret
  }
  // i += 2; return;
  .method public virtual instance void set_P(int32& i, int32 v)
  {
    ldarg.1
    ldarg.1
    ldind.i4
    ldc.i4.2
    add.ovf
    stind.i4
    ret
  }
  .property instance int32 P(int32&)
  {
    .get instance int32 A::get_P(int32&)
    .set instance void A::set_P(int32&, int32)
  }
}";
            var reference1 = CompileIL(source1);
            var source2 =
@"using System;
class B
{
    static void GetAndSet(IA a)
    {
        var value = a.P[F()[0]];
        a.P[F()[0]] = value;
    }
    static void GetAndSetByRef(IA a)
    {
        var value = a.P[ref F()[0]];
        a.P[ref F()[0]] = value;
    }
    static void CompoundAssignment(IA a)
    {
        a.P[F()[0]] += 1;
    }
    static void CompoundAssignmentByRef(IA a)
    {
        a.P[ref F()[0]] += 1;
    }
    static void Increment(IA a)
    {
        a.P[F()[0]]++;
    }
    static void IncrementByRef(IA a)
    {
        a.P[ref F()[0]]++;
    }
    static void Main()
    {
        var a = new IA();
        GetAndSet(a);
        ReportAndReset();
        GetAndSetByRef(a);
        ReportAndReset();
        CompoundAssignment(a);
        ReportAndReset();
        CompoundAssignmentByRef(a);
        ReportAndReset();
        Increment(a);
        ReportAndReset();
        IncrementByRef(a);
        ReportAndReset();
    }
    static int[] i = { 0 };
    static int[] F()
    {
        Console.WriteLine(""F()"");
        return i;
    }
    static void ReportAndReset()
    {
        Console.WriteLine(""{0}"", i[0]);
        i = new[] { 0 };
    }
}";
            // Note that Dev11 (incorrectly) calls F() twice in a.P[ref F()[0]]
            // for compound assignment and increment.
            var compilation2 = CompileAndVerify(source2, references: new[] { reference1 }, expectedOutput:
@"F()
F()
0
F()
F()
3
F()
0
F()
3
F()
0
F()
3
");
            compilation2.VerifyIL("B.GetAndSet(IA)",
@"{
  // Code size       35 (0x23)
  .maxstack  3
  .locals init (int V_0, //value
  int V_1)
  IL_0000:  ldarg.0
  IL_0001:  call       ""int[] B.F()""
  IL_0006:  ldc.i4.0
  IL_0007:  ldelem.i4
  IL_0008:  stloc.1
  IL_0009:  ldloca.s   V_1
  IL_000b:  callvirt   ""int IA.P[ref int].get""
  IL_0010:  stloc.0
  IL_0011:  ldarg.0
  IL_0012:  call       ""int[] B.F()""
  IL_0017:  ldc.i4.0
  IL_0018:  ldelem.i4
  IL_0019:  stloc.1
  IL_001a:  ldloca.s   V_1
  IL_001c:  ldloc.0
  IL_001d:  callvirt   ""void IA.P[ref int].set""
  IL_0022:  ret
}");
            compilation2.VerifyIL("B.GetAndSetByRef(IA)",
@"{
  // Code size       37 (0x25)
  .maxstack  3
  .locals init (int V_0) //value
  IL_0000:  ldarg.0
  IL_0001:  call       ""int[] B.F()""
  IL_0006:  ldc.i4.0
  IL_0007:  ldelema    ""int""
  IL_000c:  callvirt   ""int IA.P[ref int].get""
  IL_0011:  stloc.0
  IL_0012:  ldarg.0
  IL_0013:  call       ""int[] B.F()""
  IL_0018:  ldc.i4.0
  IL_0019:  ldelema    ""int""
  IL_001e:  ldloc.0
  IL_001f:  callvirt   ""void IA.P[ref int].set""
  IL_0024:  ret
}");
            compilation2.VerifyIL("B.CompoundAssignment(IA)",
@"{
  // Code size       33 (0x21)
  .maxstack  4
  .locals init (IA V_0,
  int V_1,
  int V_2)
  IL_0000:  ldarg.0
  IL_0001:  stloc.0
  IL_0002:  call       ""int[] B.F()""
  IL_0007:  ldc.i4.0
  IL_0008:  ldelem.i4
  IL_0009:  stloc.2
  IL_000a:  ldloc.0
  IL_000b:  ldloc.2
  IL_000c:  stloc.1
  IL_000d:  ldloca.s   V_1
  IL_000f:  ldloc.0
  IL_0010:  ldloc.2
  IL_0011:  stloc.1
  IL_0012:  ldloca.s   V_1
  IL_0014:  callvirt   ""int IA.P[ref int].get""
  IL_0019:  ldc.i4.1
  IL_001a:  add
  IL_001b:  callvirt   ""void IA.P[ref int].set""
  IL_0020:  ret
}");
            compilation2.VerifyIL("B.CompoundAssignmentByRef(IA)",
@"{
  // Code size       31 (0x1f)
  .maxstack  4
  .locals init (IA V_0,
  int& V_1)
  IL_0000:  ldarg.0
  IL_0001:  stloc.0
  IL_0002:  call       ""int[] B.F()""
  IL_0007:  ldc.i4.0
  IL_0008:  ldelema    ""int""
  IL_000d:  stloc.1
  IL_000e:  ldloc.0
  IL_000f:  ldloc.1
  IL_0010:  ldloc.0
  IL_0011:  ldloc.1
  IL_0012:  callvirt   ""int IA.P[ref int].get""
  IL_0017:  ldc.i4.1
  IL_0018:  add
  IL_0019:  callvirt   ""void IA.P[ref int].set""
  IL_001e:  ret
}");
            compilation2.VerifyIL("B.Increment(IA)",
@"{
  // Code size       33 (0x21)
  .maxstack  4
  .locals init (int V_0,
  int V_1,
  int V_2)
  IL_0000:  ldarg.0
  IL_0001:  call       ""int[] B.F()""
  IL_0006:  ldc.i4.0
  IL_0007:  ldelem.i4
  IL_0008:  stloc.1
  IL_0009:  dup
  IL_000a:  ldloc.1
  IL_000b:  stloc.0
  IL_000c:  ldloca.s   V_0
  IL_000e:  callvirt   ""int IA.P[ref int].get""
  IL_0013:  stloc.2
  IL_0014:  ldloc.1
  IL_0015:  stloc.0
  IL_0016:  ldloca.s   V_0
  IL_0018:  ldloc.2
  IL_0019:  ldc.i4.1
  IL_001a:  add
  IL_001b:  callvirt   ""void IA.P[ref int].set""
  IL_0020:  ret
}");
            compilation2.VerifyIL("B.IncrementByRef(IA)",
@"{
  // Code size       31 (0x1f)
  .maxstack  4
  .locals init (int& V_0,
  int V_1)
  IL_0000:  ldarg.0
  IL_0001:  call       ""int[] B.F()""
  IL_0006:  ldc.i4.0
  IL_0007:  ldelema    ""int""
  IL_000c:  stloc.0
  IL_000d:  dup
  IL_000e:  ldloc.0
  IL_000f:  callvirt   ""int IA.P[ref int].get""
  IL_0014:  stloc.1
  IL_0015:  ldloc.0
  IL_0016:  ldloc.1
  IL_0017:  ldc.i4.1
  IL_0018:  add
  IL_0019:  callvirt   ""void IA.P[ref int].set""
  IL_001e:  ret
}");
        }

        [ClrOnlyFact(ClrOnlyReason.Ilasm)]
        public void RefParametersIndexers()
        {
            var source1 =
@"// ComImport
.class interface public abstract import IA
{
  .custom instance void [mscorlib]System.Runtime.InteropServices.CoClassAttribute::.ctor(class [mscorlib]System.Type) = ( 01 00 01 41 00 00 )
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 31 36 35 46 37 35 32 44 2D 45 39 43 34 2D 34 46 37 45 2D 42 30 44 30 2D 43 44 46 44 37 41 33 36 45 32 31 31 00 00 )
  .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string) = {string('P')}
  .method public abstract virtual instance int32 get_P(int32& i) { }
  .method public abstract virtual instance void set_P(int32& i, int32 v) { }
  .property instance int32 P(int32&)
  {
    .get instance int32 IA::get_P(int32&)
    .set instance void IA::set_P(int32&, int32)
  }
}
// Not ComImport
.class interface public abstract IB
{
  .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string) = {string('P')}
  .method public abstract virtual instance int32 get_P(int32& i) { }
  .method public abstract virtual instance void set_P(int32& i, int32 v) { }
  .property instance int32 P(int32&)
  {
    .get instance int32 IB::get_P(int32&)
    .set instance void IB::set_P(int32&, int32)
  }
}
.class public A implements IA
{
  .method public hidebysig specialname rtspecialname instance void .ctor()
  {
    ret
  }
  // i += 1; return 0;
  .method public virtual instance int32 get_P(int32& i)
  {
    ldarg.1
    ldarg.1
    ldind.i4
    ldc.i4.1
    add.ovf
    stind.i4
    ldc.i4.0
    ret
  }
  // i += 2; return;
  .method public virtual instance void set_P(int32& i, int32 v)
  {
    ldarg.1
    ldarg.1
    ldind.i4
    ldc.i4.2
    add.ovf
    stind.i4
    ret
  }
  .property instance int32 P(int32&)
  {
    .get instance int32 A::get_P(int32&)
    .set instance void A::set_P(int32&, int32)
  }
}";
            var reference1 = CompileIL(source1);
            var source2 =
@"class C
{
    static void M(IB b)
    {
        int x = 0;
        int y = 0;
        b[y] = b[x];
        b[ref y] = b[ref x];
        b.set_P(ref y, b.get_P(ref x));
    }
}";
            var compilation2 = CreateCompilation(source2, new[] { reference1 });
            compilation2.VerifyDiagnostics(
                // (7,9): error CS1545: Property, indexer, or event 'IB.this[ref int]' is not supported by the language; try directly calling accessor methods 'IB.get_P(ref int)' or 'IB.set_P(ref int, int)'
                Diagnostic(ErrorCode.ERR_BindToBogusProp2, "b[y]").WithArguments("IB.this[ref int]", "IB.get_P(ref int)", "IB.set_P(ref int, int)").WithLocation(7, 9),
                // (7,16): error CS1545: Property, indexer, or event 'IB.this[ref int]' is not supported by the language; try directly calling accessor methods 'IB.get_P(ref int)' or 'IB.set_P(ref int, int)'
                Diagnostic(ErrorCode.ERR_BindToBogusProp2, "b[x]").WithArguments("IB.this[ref int]", "IB.get_P(ref int)", "IB.set_P(ref int, int)").WithLocation(7, 16),
                // (8,9): error CS1545: Property, indexer, or event 'IB.this[ref int]' is not supported by the language; try directly calling accessor methods 'IB.get_P(ref int)' or 'IB.set_P(ref int, int)'
                Diagnostic(ErrorCode.ERR_BindToBogusProp2, "b[ref y]").WithArguments("IB.this[ref int]", "IB.get_P(ref int)", "IB.set_P(ref int, int)").WithLocation(8, 9),
                // (8,20): error CS1545: Property, indexer, or event 'IB.this[ref int]' is not supported by the language; try directly calling accessor methods 'IB.get_P(ref int)' or 'IB.set_P(ref int, int)'
                Diagnostic(ErrorCode.ERR_BindToBogusProp2, "b[ref x]").WithArguments("IB.this[ref int]", "IB.get_P(ref int)", "IB.set_P(ref int, int)").WithLocation(8, 20));
            var source3 =
@"class C
{
    static void Main()
    {
        var a = new IA();
        int x = 0;
        int y = 0;
        a[y] = a[x];
        Report(x, y);
        a[ref y] = a[ref x];
        Report(x, y);
        a.set_P(ref y, a.get_P(ref x));
        Report(x, y);
    }
    static void Report(int x, int y)
    {
        System.Console.WriteLine(""{0}, {1}"", x, y);
    }
}";
            var compilation3 = CompileAndVerify(source3, references: new[] { reference1 }, expectedOutput:
@"0, 0
1, 2
2, 4
");
        }

        [ClrOnlyFact]
        public void OptionalRefParameters()
        {
            var source1 =
@".class interface public abstract import IA
{
  .custom instance void [mscorlib]System.Runtime.InteropServices.CoClassAttribute::.ctor(class [mscorlib]System.Type) = ( 01 00 01 41 00 00 )
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 31 36 35 46 37 35 32 44 2D 45 39 43 34 2D 34 46 37 45 2D 42 30 44 30 2D 43 44 46 44 37 41 33 36 45 32 31 31 00 00 )
  .method public abstract virtual instance int32 get_P(int32& x, [opt] int32& y)
  {
    .param[2] = int32(0)
  }
  .method public abstract virtual instance void set_P(int32& x, [opt] int32& y, int32 v)
  {
    .param[2] = int32(0)
  }
  .property instance int32 P(int32&, int32&)
  {
    .get instance int32 IA::get_P(int32&, int32&)
    .set instance void IA::set_P(int32&, int32&, int32)
  }
}
.class public A implements IA
{
  .method public hidebysig specialname rtspecialname instance void .ctor()
  {
    ret
  }
  // y += 1; return 0
  .method public virtual instance int32 get_P(int32& x, int32& y)
  {
    ldarg.2
    ldc.i4.0
    ceq
    brtrue L1
    ldarg.2
    ldarg.2
    ldind.i4
    ldc.i4.1
    add.ovf
    stind.i4
  L1:
    ldc.i4.0
    ret
  }
  // y += 2
  .method public virtual instance void set_P(int32& x, int32& y, int32 v)
  {
    ldarg.2
    ldc.i4.0
    ceq
    brtrue L1
    ldarg.2
    ldarg.2
    ldind.i4
    ldc.i4.2
    add.ovf
    stind.i4
  L1:
    ret
  }
  .property instance int32 P(int32&, int32&)
  {
    .get instance int32 A::get_P(int32&, int32&)
    .set instance void A::set_P(int32&, int32&, int32)
  }
}";
            var reference1 = CompileIL(source1);
            var source2 =
@"class C
{
    static void MissingArg(IA a)
    {
        a.P[x]++;
    }
    static void ValueArgs(IA a)
    {
        a.P[x, y]++;
    }
    static void RefArgs(IA a)
    {
        a.P[ref x, ref y]++;
    }
    static void Main()
    {
        var a = new IA();
        MissingArg(a);
        ReportAndReset();
        ValueArgs(a);
        ReportAndReset();
        RefArgs(a);
        ReportAndReset();
    }
    static int x;
    static int y;
    static void ReportAndReset()
    {
        System.Console.WriteLine(""{0}, {1}"", x, y);
        x = 0;
        y = 0;
    }
}";
            var compilation2 = CompileAndVerify(source2, references: new[] { reference1 }, expectedOutput:
@"0, 0
0, 0
0, 3");
            compilation2.VerifyIL("C.MissingArg(IA)",
@"{
  // Code size       39 (0x27)
  .maxstack  5
  .locals init (int V_0,
  int V_1,
  int V_2,
  int V_3)
  IL_0000:  ldarg.0
  IL_0001:  ldsfld     ""int C.x""
  IL_0006:  stloc.2
  IL_0007:  dup
  IL_0008:  ldloc.2
  IL_0009:  stloc.0
  IL_000a:  ldloca.s   V_0
  IL_000c:  ldc.i4.0
  IL_000d:  stloc.1
  IL_000e:  ldloca.s   V_1
  IL_0010:  callvirt   ""int IA.P[ref int, ref int].get""
  IL_0015:  stloc.3
  IL_0016:  ldloc.2
  IL_0017:  stloc.0
  IL_0018:  ldloca.s   V_0
  IL_001a:  ldc.i4.0
  IL_001b:  stloc.1
  IL_001c:  ldloca.s   V_1
  IL_001e:  ldloc.3
  IL_001f:  ldc.i4.1
  IL_0020:  add
  IL_0021:  callvirt   ""void IA.P[ref int, ref int].set""
  IL_0026:  ret
}");
            compilation2.VerifyIL("C.ValueArgs(IA)",
@"{
  // Code size       47 (0x2f)
  .maxstack  5
  .locals init (int V_0,
  int V_1,
  int V_2,
  int V_3,
  int V_4)
  IL_0000:  ldarg.0
  IL_0001:  ldsfld     ""int C.x""
  IL_0006:  stloc.2
  IL_0007:  ldsfld     ""int C.y""
  IL_000c:  stloc.3
  IL_000d:  dup
  IL_000e:  ldloc.2
  IL_000f:  stloc.0
  IL_0010:  ldloca.s   V_0
  IL_0012:  ldloc.3
  IL_0013:  stloc.1
  IL_0014:  ldloca.s   V_1
  IL_0016:  callvirt   ""int IA.P[ref int, ref int].get""
  IL_001b:  stloc.s    V_4
  IL_001d:  ldloc.2
  IL_001e:  stloc.0
  IL_001f:  ldloca.s   V_0
  IL_0021:  ldloc.3
  IL_0022:  stloc.1
  IL_0023:  ldloca.s   V_1
  IL_0025:  ldloc.s    V_4
  IL_0027:  ldc.i4.1
  IL_0028:  add
  IL_0029:  callvirt   ""void IA.P[ref int, ref int].set""
  IL_002e:  ret
}");
            compilation2.VerifyIL("C.RefArgs(IA)",
@"{
  // Code size       33 (0x21)
  .maxstack  5
  .locals init (int& V_0,
  int& V_1,
  int V_2)
  IL_0000:  ldarg.0
  IL_0001:  ldsflda    ""int C.x""
  IL_0006:  stloc.0
  IL_0007:  ldsflda    ""int C.y""
  IL_000c:  stloc.1
  IL_000d:  dup
  IL_000e:  ldloc.0
  IL_000f:  ldloc.1
  IL_0010:  callvirt   ""int IA.P[ref int, ref int].get""
  IL_0015:  stloc.2
  IL_0016:  ldloc.0
  IL_0017:  ldloc.1
  IL_0018:  ldloc.2
  IL_0019:  ldc.i4.1
  IL_001a:  add
  IL_001b:  callvirt   ""void IA.P[ref int, ref int].set""
  IL_0020:  ret
}");
        }
        
        /// <summary>
        /// CanBeReferencedByName should return false if
        /// the accessor name is not a valid identifier.
        /// </summary>
        [ClrOnlyFact]
        public void CanBeReferencedByName_InvalidName()
        {
            // Note: Dev11 treats I.Q as invalid so Q is not recognized from source.
            var source1 =
@".class interface public abstract import I
{
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 31 36 35 46 37 35 32 44 2D 45 39 43 34 2D 34 46 37 45 2D 42 30 44 30 2D 43 44 46 44 37 41 33 36 45 32 31 31 00 00 )
  .method public abstract virtual instance object valid_name(object) { }
  .method public abstract virtual instance object invalid.name(object) { }
  .property instance object P(object)
  {
    .get instance object I::valid_name(object)
  }
  .property instance object Q(object)
  {
    .get instance object I::invalid.name(object)
  }
}";
            var reference1 = CompileIL(source1);
            var source2 =
@"class C
{
    static void M(I i)
    {
        var o = i.P[1];
        o = i.Q[2];
        o = i.valid_name(1);
    }
}";
            var compilation2 = CompileAndVerify(source2, references: new[] { reference1 }, verify: Verification.Passes);

            var @namespace = (NamespaceSymbol)compilation2.Compilation.GlobalNamespace;
            // Indexed property with valid name.
            var type = @namespace.GetMember<NamedTypeSymbol>("I");
            var property = type.GetMember<PropertySymbol>("P");
            Assert.False(property.MustCallMethodsDirectly);
            Assert.True(property.CanCallMethodsDirectly());
            Assert.True(property.GetMethod.CanBeReferencedByName);
            Assert.True(property.GetMethod.CanBeReferencedByNameIgnoringIllegalCharacters);
            // Indexed property with invalid name.
            property = type.GetMember<PropertySymbol>("Q");
            Assert.False(property.MustCallMethodsDirectly);
            Assert.True(property.CanCallMethodsDirectly());
            Assert.False(property.GetMethod.CanBeReferencedByName);
            Assert.True(property.GetMethod.CanBeReferencedByNameIgnoringIllegalCharacters);

            compilation2.VerifyIL("C.M(I)",
@"{
  // Code size       40 (0x28)
  .maxstack  2
  IL_0000:  ldarg.0
  IL_0001:  ldc.i4.1
  IL_0002:  box        ""int""
  IL_0007:  callvirt   ""object I.P[object].get""
  IL_000c:  pop
  IL_000d:  ldarg.0
  IL_000e:  ldc.i4.2
  IL_000f:  box        ""int""
  IL_0014:  callvirt   ""object I.Q[object].get""
  IL_0019:  pop
  IL_001a:  ldarg.0
  IL_001b:  ldc.i4.1
  IL_001c:  box        ""int""
  IL_0021:  callvirt   ""object I.P[object].get""
  IL_0026:  pop
  IL_0027:  ret
}");
        }

        [Fact]
        public void NotIndexedProperties()
        {
            var source =
@"using System.ComponentModel;
[DefaultProperty(""R"")]
class A
{
    internal object P { get { return null; } }
    internal object[] Q { get { return null; } }
    internal object this[int index] { get { return null; } }
}
class B
{
    static void M(A a)
    {
        object o;
        o = a.P[1];
        o = a.Q[2];
        o = a.R[3];
    }
}";
            CreateCompilation(source).VerifyDiagnostics(
                // (14,13): error CS0021: Cannot apply indexing with [] to an expression of type 'object'
                Diagnostic(ErrorCode.ERR_BadIndexLHS, "a.P[1]").WithArguments("object").WithLocation(14, 13),
                // (16,15): error CS1061: 'A' does not contain a definition for 'R' and no extension method 'R' accepting a first argument of type 'A' could be found (are you missing a using directive or an assembly reference?)
                Diagnostic(ErrorCode.ERR_NoSuchMemberOrExtension, "R").WithArguments("A", "R").WithLocation(16, 15));
        }
        
        [WorkItem(546441, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546441")]
        [Fact]
        public void UnimplementedIndexedProperty()
        {
            // From Microsoft.Vbe.Interop, Version=14.0.0.0.
            var il = @"
.class public auto ansi sealed Microsoft.Vbe.Interop.vbext_ProcKind
       extends [mscorlib]System.Enum
{
	.field public specialname rtspecialname int32 value__
	.field public static literal valuetype Microsoft.Vbe.Interop.vbext_ProcKind vbext_pk_Get = int32(0x00000003)
} // end of class Microsoft.Vbe.Interop.vbext_ProcKind


.class interface public abstract auto ansi import Microsoft.Vbe.Interop._CodeModule
{
	.method public hidebysig newslot specialname abstract virtual 
			instance string  marshal( bstr)  get_ProcOfLine([in] int32 Line,
															[out] valuetype Microsoft.Vbe.Interop.vbext_ProcKind& ProcKind) runtime managed internalcall
	{
	  .custom instance void [mscorlib]System.Runtime.InteropServices.DispIdAttribute::.ctor(int32) = ( 01 00 0E 00 02 60 00 00 )                         // .....`..
	}

	.property string ProcOfLine(int32,
								valuetype Microsoft.Vbe.Interop.vbext_ProcKind&)
	{
	  .custom instance void [mscorlib]System.Runtime.InteropServices.DispIdAttribute::.ctor(int32) = ( 01 00 0E 00 02 60 00 00 )                         // .....`..
	  .get instance string Microsoft.Vbe.Interop._CodeModule::get_ProcOfLine(int32,
																			 valuetype Microsoft.Vbe.Interop.vbext_ProcKind&)
	}
} // end of class Microsoft.Vbe.Interop._CodeModule

.class interface public abstract auto ansi import Microsoft.Vbe.Interop.CodeModule
       implements Microsoft.Vbe.Interop._CodeModule
{
  .custom instance void [mscorlib]System.Runtime.InteropServices.CoClassAttribute::.ctor(class [mscorlib]System.Type) = ( 01 00 25 4D 69 63 72 6F 73 6F 66 74 2E 56 62 65   // ..%Microsoft.Vbe
                                                                                                                          2E 49 6E 74 65 72 6F 70 2E 43 6F 64 65 4D 6F 64   // .Interop.CodeMod
                                                                                                                          75 6C 65 43 6C 61 73 73 00 00 )                   // uleClass..
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 30 30 30 32 45 31 36 45 2D 30 30 30 30   // ..$0002E16E-0000
                                                                                                  2D 30 30 30 30 2D 43 30 30 30 2D 30 30 30 30 30   // -0000-C000-00000
                                                                                                  30 30 30 30 30 34 36 00 00 )                      // 0000046..
} // end of class Microsoft.Vbe.Interop.CodeModule
";

            var source = @"
using Microsoft.Vbe.Interop;

class C : CodeModule
{
}

class D : CodeModule
{
    public string get_ProcOfLine(int line, out Microsoft.Vbe.Interop.vbext_ProcKind procKind) { throw null; }
}
";
            var comp = CreateCompilationWithILAndMscorlib40(source, il);
            comp.VerifyDiagnostics(
                // (4,7): error CS0535: 'C' does not implement interface member 'Microsoft.Vbe.Interop._CodeModule.ProcOfLine[int, out Microsoft.Vbe.Interop.vbext_ProcKind].get'
                // class C : CodeModule
                Diagnostic(ErrorCode.ERR_UnimplementedInterfaceMember, "CodeModule").WithArguments("C", "Microsoft.Vbe.Interop._CodeModule.ProcOfLine[int, out Microsoft.Vbe.Interop.vbext_ProcKind].get"));

            var interfaceProperty = comp.GlobalNamespace
                .GetMember<NamespaceSymbol>("Microsoft")
                .GetMember<NamespaceSymbol>("Vbe")
                .GetMember<NamespaceSymbol>("Interop")
                .GetMember<NamedTypeSymbol>("_CodeModule")
                .GetMember<PropertySymbol>("ProcOfLine");
            Assert.True(interfaceProperty.IsIndexedProperty);

            var sourceType1 = comp.GlobalNamespace.GetMember<NamedTypeSymbol>("C");
            Assert.Null(sourceType1.FindImplementationForInterfaceMember(interfaceProperty));
            Assert.Null(sourceType1.FindImplementationForInterfaceMember(interfaceProperty.GetMethod));

            var sourceType2 = comp.GlobalNamespace.GetMember<NamedTypeSymbol>("D");
            Assert.Null(sourceType2.FindImplementationForInterfaceMember(interfaceProperty));
            Assert.NotNull(sourceType2.FindImplementationForInterfaceMember(interfaceProperty.GetMethod));
        }

        [WorkItem(530571, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530571")]
        [Fact(Skip = "530571")]
        public void GetAccessorMethodBug16439()
        {
            var il = @"
.class interface public abstract import InterfaceA
{
  .custom instance void [mscorlib]System.Runtime.InteropServices.CoClassAttribute::.ctor(class [mscorlib]System.Type) = ( 01 00 01 41 00 00 )
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 31 36 35 46 37 35 32 44 2D 45 39 43 34 2D 34 46 37 45 2D 42 30 44 30 2D 43 44 46 44 37 41 33 36 45 32 31 31 00 00 )
  .property instance int32 P1(int32)
  {
    .get instance int32 InterfaceA::get_P1(int32)
    .set instance void InterfaceA::set_P1(int32, int32)
  }
  .method public abstract virtual instance int32 get_P1(int32 i) { }
  .method public abstract virtual instance void set_P1(int32 i, int32 v) { }
}

.class public A implements InterfaceA
{
  .method public hidebysig specialname rtspecialname instance void .ctor()
  {
    ret
  }
  .property instance int32 P1(int32)
  {
    .get instance int32 InterfaceA::get_P1(int32)
    .set instance void InterfaceA::set_P1(int32, int32)
  }
  .method public virtual instance int32 get_P1(int32 i)
  {
    ldc.i4.1
    call       void [mscorlib]System.Console::WriteLine(int32)
    ldc.i4.0
    ret
  }
  .method public virtual instance void set_P1(int32 i, int32 v)
  {
    ldc.i4.2
    call       void [mscorlib]System.Console::WriteLine(int32)
    ret
  }
}
";

            var source = @"
class Test
{
   public static void Main()
   {
     InterfaceA ia = new A();
     System.Console.WriteLine(ia.P1[10]);
   }
}
";
            string expectedOutput = @"1
0";
            var compilation = CreateCompilationWithILAndMscorlib40(source, il, options: TestOptions.ReleaseExe);
            CompileAndVerify(compilation, expectedOutput: expectedOutput);
        }
    }
}
