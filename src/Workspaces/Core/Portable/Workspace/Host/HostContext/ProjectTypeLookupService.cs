// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Host.HostContext
{
    [ExportWorkspaceService(typeof(IProjectTypeLookupService), ServiceLayer.Default), Shared]
    internal class ProjectTypeLookupService : IProjectTypeLookupService
    {
        private const string CSharpProjectType = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

        public string GetProjectType(Workspace workspace, ProjectId projectId)
        {
            if (workspace == null || projectId == null)
            {
                return string.Empty;
            }

            var project = workspace.CurrentSolution.GetProject(projectId);
            var language = project?.Language;

            switch (language)
            {
                case LanguageNames.CSharp:
                    return CSharpProjectType;
                default:
                    return string.Empty;
            }
        }
    }
}
