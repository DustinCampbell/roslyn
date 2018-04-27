using System;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Workspaces.Desktop.Analyzers;

namespace Microsoft.CodeAnalysis.Workspaces.Desktop.CSharp.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CSharpMSBuildWorkspaceUpgradeAnalyzer : MSBuildWorkspaceUpgradeAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            throw new NotImplementedException();
        }
    }
}
