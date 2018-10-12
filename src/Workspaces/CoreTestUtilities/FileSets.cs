// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.UnitTests.TestFiles;

namespace Microsoft.CodeAnalysis.UnitTests
{
    public static class FileSets
    {
        public static FileSet BaseFiles { get; } = new FileSet(
            (@"NuGet.Config", Resources.NuGet_Config),
            (@"Directory.Build.props", Resources.Directory_Build_props),
            (@"Directory.Build.targets", Resources.Directory_Build_targets));

        public static FileSet SimpleCSharpSolution { get; } = BaseFiles.With(
            (@"TestSolution.sln", Resources.SolutionFiles.CSharp),
            (@"CSharpProject\CSharpProject.csproj", Resources.ProjectFiles.CSharp.CSharpProject),
            (@"CSharpProject\CSharpClass.cs", Resources.SourceFiles.CSharp.CSharpClass),
            (@"CSharpProject\Properties\AssemblyInfo.cs", Resources.SourceFiles.CSharp.AssemblyInfo));

        public static FileSet CSharpProjectReferenceSolution { get; } = BaseFiles.With(
            (@"CSharpProjectReference.sln", Resources.SolutionFiles.CSharp_ProjectReference),
            (@"CSharpProject\CSharpProject.csproj", Resources.ProjectFiles.CSharp.CSharpProject),
            (@"CSharpProject\CSharpClass.cs", Resources.SourceFiles.CSharp.CSharpClass),
            (@"CSharpProject\Properties\AssemblyInfo.cs", Resources.SourceFiles.CSharp.AssemblyInfo),
            (@"CSharpProject\CSharpProject_ProjectReference.csproj", Resources.ProjectFiles.CSharp.ProjectReference),
            (@"CSharpProject\CSharpConsole.cs", Resources.SourceFiles.CSharp.CSharpConsole));

        public static FileSet MixedLanguageSolution { get; } = BaseFiles.With(
            (@"TestSolution.sln", Resources.SolutionFiles.VB_and_CSharp),
            (@"CSharpProject\CSharpProject.csproj", Resources.ProjectFiles.CSharp.CSharpProject),
            (@"CSharpProject\CSharpClass.cs", Resources.SourceFiles.CSharp.CSharpClass),
            (@"CSharpProject\Properties\AssemblyInfo.cs", Resources.SourceFiles.CSharp.AssemblyInfo),
            (@"VisualBasicProject\VisualBasicProject.vbproj", Resources.ProjectFiles.VisualBasic.VisualBasicProject),
            (@"VisualBasicProject\VisualBasicClass.vb", Resources.SourceFiles.VisualBasic.VisualBasicClass),
            (@"VisualBasicProject\My Project\Application.Designer.vb", Resources.SourceFiles.VisualBasic.Application_Designer),
            (@"VisualBasicProject\My Project\Application.myapp", Resources.SourceFiles.VisualBasic.Application),
            (@"VisualBasicProject\My Project\AssemblyInfo.vb", Resources.SourceFiles.VisualBasic.AssemblyInfo),
            (@"VisualBasicProject\My Project\Resources.Designer.vb", Resources.SourceFiles.VisualBasic.Resources_Designer),
            (@"VisualBasicProject\My Project\Resources.resx", Resources.SourceFiles.VisualBasic.Resources),
            (@"VisualBasicProject\My Project\Settings.Designer.vb", Resources.SourceFiles.VisualBasic.Settings_Designer),
            (@"VisualBasicProject\My Project\Settings.settings", Resources.SourceFiles.VisualBasic.Settings));

        public static FileSet AnalyzerReferenceSolution { get; } = BaseFiles.With(
            (@"AnalyzerReference.sln", Resources.SolutionFiles.AnalyzerReference),
            (@"AnalyzerSolution\CSharpProject.dll", Resources.Dlls.CSharpProject),
            (@"AnalyzerSolution\CSharpProject_AnalyzerReference.csproj", Resources.ProjectFiles.CSharp.AnalyzerReference),
            (@"AnalyzerSolution\CSharpClass.cs", Resources.SourceFiles.CSharp.CSharpClass),
            (@"AnalyzerSolution\XamlFile.xaml", Resources.SourceFiles.Xaml.MainWindow),
            (@"AnalyzerSolution\VisualBasicProject_AnalyzerReference.vbproj", Resources.ProjectFiles.VisualBasic.AnalyzerReference),
            (@"AnalyzerSolution\VisualBasicClass.vb", Resources.SourceFiles.VisualBasic.VisualBasicClass),
            (@"AnalyzerSolution\My Project\Application.Designer.vb", Resources.SourceFiles.VisualBasic.Application_Designer),
            (@"AnalyzerSolution\My Project\Application.myapp", Resources.SourceFiles.VisualBasic.Application),
            (@"AnalyzerSolution\My Project\AssemblyInfo.vb", Resources.SourceFiles.VisualBasic.AssemblyInfo),
            (@"AnalyzerSolution\My Project\Resources.Designer.vb", Resources.SourceFiles.VisualBasic.Resources_Designer),
            (@"AnalyzerSolution\My Project\Resources.resx", Resources.SourceFiles.VisualBasic.Resources),
            (@"AnalyzerSolution\My Project\Settings.Designer.vb", Resources.SourceFiles.VisualBasic.Settings_Designer),
            (@"AnalyzerSolution\My Project\Settings.settings", Resources.SourceFiles.VisualBasic.Settings));

        public static FileSet DuplicatedGuids { get; } = BaseFiles.With(
            (@"DuplicatedGuids.sln", Resources.SolutionFiles.DuplicatedGuids),
            (@"ReferenceTest\ReferenceTest.csproj", Resources.ProjectFiles.CSharp.DuplicatedGuidReferenceTest),
            (@"Library1\Library1.csproj", Resources.ProjectFiles.CSharp.DuplicatedGuidLibrary1),
            (@"Library2\Library2.csproj", Resources.ProjectFiles.CSharp.DuplicatedGuidLibrary2));

        public static FileSet CircularProjectReferences { get; } = BaseFiles.With(
            (@"CircularSolution.sln", Resources.SolutionFiles.CircularSolution),
            (@"CircularCSharpProject1.csproj", Resources.ProjectFiles.CSharp.CircularProjectReferences_CircularCSharpProject1),
            (@"CircularCSharpProject2.csproj", Resources.ProjectFiles.CSharp.CircularProjectReferences_CircularCSharpProject2));

        public static FileSet NetCoreApp { get; } = BaseFiles.With(
            (@"Project.csproj", Resources.ProjectFiles.CSharp.NetCoreApp2_Project),
            (@"Program.cs", Resources.SourceFiles.CSharp.NetCoreApp2_Program));

        public static FileSet NetCoreAppAndLibrary { get; } = BaseFiles.With(
            (@"Project\Project.csproj", Resources.ProjectFiles.CSharp.NetCoreApp2AndLibrary_Project),
            (@"Project\Program.cs", Resources.SourceFiles.CSharp.NetCoreApp2AndLibrary_Program),
            (@"Library\Library.csproj", Resources.ProjectFiles.CSharp.NetCoreApp2AndLibrary_Library),
            (@"Library\Class1.cs", Resources.SourceFiles.CSharp.NetCoreApp2AndLibrary_Class1));

        public static FileSet NetCoreAppAndTwoLibraries { get; } = BaseFiles.With(
            (@"Project\Project.csproj", Resources.ProjectFiles.CSharp.NetCoreApp2AndTwoLibraries_Project),
            (@"Project\Program.cs", Resources.SourceFiles.CSharp.NetCoreApp2AndTwoLibraries_Program),
            (@"Library1\Library1.csproj", Resources.ProjectFiles.CSharp.NetCoreApp2AndTwoLibraries_Library1),
            (@"Library1\Class1.cs", Resources.SourceFiles.CSharp.NetCoreApp2AndTwoLibraries_Class1),
            (@"Library2\Library2.csproj", Resources.ProjectFiles.CSharp.NetCoreApp2AndTwoLibraries_Library2),
            (@"Library2\Class2.cs", Resources.SourceFiles.CSharp.NetCoreApp2AndTwoLibraries_Class2));

        public static FileSet NetCoreMultiTFM { get; } = BaseFiles.With(
            (@"Project.csproj", Resources.ProjectFiles.CSharp.NetCoreMultiTFM_Project),
            (@"Program.cs", Resources.SourceFiles.CSharp.NetCoreApp2_Program));

        public static FileSet NetCoreMultiTFM_ProjectReference { get; } = BaseFiles.With(
            (@"Project\Project.csproj", Resources.ProjectFiles.CSharp.NetCoreMultiTFM_ProjectReference_Project),
            (@"Project\Program.cs", Resources.SourceFiles.CSharp.NetCoreMultiTFM_ProjectReference_Program),
            (@"Library\Library.csproj", Resources.ProjectFiles.CSharp.NetCoreMultiTFM_ProjectReference_Library),
            (@"Library\Class1.cs", Resources.SourceFiles.CSharp.NetCoreMultiTFM_ProjectReference_Class1));

        public static FileSet NetCoreMultiTFM_ProjectReferenceWithReversedTFMs { get; } = BaseFiles.With(
            (@"Project\Project.csproj", Resources.ProjectFiles.CSharp.NetCoreMultiTFM_ProjectReferenceWithReversedTFMs_Project),
            (@"Project\Program.cs", Resources.SourceFiles.CSharp.NetCoreMultiTFM_ProjectReferenceWithReversedTFMs_Program),
            (@"Library\Library.csproj", Resources.ProjectFiles.CSharp.NetCoreMultiTFM_ProjectReferenceWithReversedTFMs_Library),
            (@"Library\Class1.cs", Resources.SourceFiles.CSharp.NetCoreMultiTFM_ProjectReferenceWithReversedTFMs_Class1));

        public static FileSet NetCoreMultiTFM_ProjectReferenceToFSharp { get; } = BaseFiles.With(
            (@"Solution.sln", Resources.SolutionFiles.NetCoreMultiTFM_ProjectReferenceToFSharp),
            (@"csharplib\csharplib.csproj", Resources.ProjectFiles.CSharp.NetCoreMultiTFM_ProjectReferenceToFSharp_CSharpLib),
            (@"csharplib\Class1.cs", Resources.SourceFiles.CSharp.NetCoreMultiTFM_ProjectReferenceToFSharp_CSharpLib_Class1),
            (@"fsharplib\fsharplib.fsproj", Resources.ProjectFiles.FSharp.NetCoreMultiTFM_ProjectReferenceToFSharp_FSharpLib),
            (@"fsharplib\Library.fs", Resources.SourceFiles.FSharp.NetCoreMultiTFM_ProjectReferenceToFSharp_FSharpLib_Library));
    }
}
