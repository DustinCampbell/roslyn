using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis.Workspaces.Desktop.Analyzers
{
    public abstract class MSBuildWorkspaceUpgradeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MSBuildWorkspace0001";
        private const string Category = "API";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(DesktopAnalyzersResources.AnalyzerTitle), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(DesktopAnalyzersResources.AnalyzerMessageFormat), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(DesktopAnalyzersResources.AnalyzerDescription), DesktopAnalyzersResources.ResourceManager, typeof(DesktopAnalyzersResources));

        private static readonly DiagnosticDescriptor rule = new DiagnosticDescriptor(
            DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
    }
}
