// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities;
using Microsoft.VisualStudio.IntegrationTest.Utilities.OutOfProcess;
using Roslyn.Test.Utilities;
using Xunit;
using ProjectUtils = Microsoft.VisualStudio.IntegrationTest.Utilities.Common.ProjectUtils;

namespace Roslyn.VisualStudio.IntegrationTests.CSharp
{
    [Collection(nameof(SharedIntegrationHostFixture))]
    public class CSharpGenerateTypeDialog : AbstractEditorTest
    {
        protected override string LanguageName => LanguageNames.CSharp;

        private GenerateTypeDialog_OutOfProc GenerateTypeDialog => VisualStudio.GenerateTypeDialog;

        public CSharpGenerateTypeDialog(VisualStudioInstanceFactory instanceFactory)
                    : base(instanceFactory, nameof(CSharpGenerateTypeDialog))
        {
        }

        [WpfFact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateType)]
        public void OpenAndCloseDialog()
        {
            SetUpEditor(@"class C
{
    void Method() 
    { 
        $$A a;    
    }
}
");

            VisualStudio.Editor.Verify.CodeAction("Generate new type...",
                applyFix: true,
                blockUntilComplete: false);

            GenerateTypeDialog.VerifyOpen();
            GenerateTypeDialog.ClickCancel();
            GenerateTypeDialog.VerifyClosed();
        }
    }
}
