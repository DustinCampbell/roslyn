// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.UnitTests.TestFiles;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.UnitTests
{
    [UseExportProvider]
    public class WorkspaceTestBase : TestBase
    {
        protected readonly TempDirectory SolutionDir;

        protected static readonly TimeSpan AsyncEventTimeout = TimeSpan.FromMinutes(5);

        public WorkspaceTestBase()
        {
            SolutionDir = Temp.CreateDirectory();
        }

        /// <summary>
        /// Gets an absolute file name for a file relative to the tests solution directory.
        /// </summary>
        public string GetSolutionFileName(string relativeFileName)
            => Path.Combine(SolutionDir.Path, relativeFileName);

        protected void CreateCSharpFilesWith(string propertyName, string value)
        {
            FileSets.SimpleCSharpSolution
                .With(@"CSharpProject\CSharpProject.csproj", Resources.ProjectFiles.CSharp.AllOptions)
                .ReplaceFileElement(@"CSharpProject\CSharpProject.csproj", propertyName, value)
                .CreateIn(SolutionDir);
        }

        protected void CreateVBFilesWith(string propertyName, string value)
        {
            FileSets.MixedLanguageSolution
                .ReplaceFileElement(@"VisualBasicProject\VisualBasicProject.vbproj", propertyName, value)
                .CreateIn(SolutionDir);
        }

        protected static string GetParentDirOfParentDirOfContainingDir(string fileName)
        {
            var containingDir = Directory.GetParent(fileName).FullName;
            var parentOfContainingDir = Directory.GetParent(containingDir).FullName;

            return Directory.GetParent(parentOfContainingDir).FullName;
        }

        protected Document AssertSemanticVersionChanged(Document document, SourceText newText)
        {
            var docVersion = document.GetTopLevelChangeTextVersionAsync().Result;
            var projVersion = document.Project.GetSemanticVersionAsync().Result;

            var text = document.GetTextAsync().Result;
            var newDoc = document.WithText(newText);

            var newDocVersion = newDoc.GetTopLevelChangeTextVersionAsync().Result;
            var newProjVersion = newDoc.Project.GetSemanticVersionAsync().Result;

            Assert.NotEqual(docVersion, newDocVersion);
            Assert.NotEqual(projVersion, newProjVersion);

            return newDoc;
        }

        protected Document AssertSemanticVersionUnchanged(Document document, SourceText newText)
        {
            var docVersion = document.GetTopLevelChangeTextVersionAsync().Result;
            var projVersion = document.Project.GetSemanticVersionAsync().Result;

            var text = document.GetTextAsync().Result;
            var newDoc = document.WithText(newText);

            var newDocVersion = newDoc.GetTopLevelChangeTextVersionAsync().Result;
            var newProjVersion = newDoc.Project.GetSemanticVersionAsync().Result;

            Assert.Equal(docVersion, newDocVersion);
            Assert.Equal(projVersion, newProjVersion);

            return newDoc;
        }
    }
}
