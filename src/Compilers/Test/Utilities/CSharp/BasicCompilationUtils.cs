// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public static class BasicCompilationUtils
    {
        private static BasicTestBase s_instance;

        private static BasicTestBase Instance => s_instance ?? (s_instance = new BasicTestBase());

        private sealed class BasicTestBase : CommonTestBase
        {
            internal override string VisualizeRealIL(IModuleSymbol peModule, CodeAnalysis.CodeGen.CompilationTestData.MethodData methodData, IReadOnlyDictionary<int, string> markers)
            {
                throw new NotImplementedException();
            }
        }
    }
}
