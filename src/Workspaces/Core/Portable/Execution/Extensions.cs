// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Serialization;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Execution
{
    internal static class Extensions
    {
        public static T[] ReadArray<T>(this ObjectReader reader)
        {
            return (T[])reader.ReadValue();
        }

        public static SerializationKind GetWellKnownSynchronizationKind(this object value)
        {
            switch (value)
            {
                case SolutionStateChecksums _: return SerializationKind.SolutionState;
                case ProjectStateChecksums _: return SerializationKind.ProjectState;
                case DocumentStateChecksums _: return SerializationKind.DocumentState;
                case ProjectChecksumCollection _: return SerializationKind.Projects;
                case DocumentChecksumCollection _: return SerializationKind.Documents;
                case TextDocumentChecksumCollection _: return SerializationKind.TextDocuments;
                case ProjectReferenceChecksumCollection _: return SerializationKind.ProjectReferences;
                case MetadataReferenceChecksumCollection _: return SerializationKind.MetadataReferences;
                case AnalyzerReferenceChecksumCollection _: return SerializationKind.AnalyzerReferences;
                case SolutionInfo.SolutionAttributes _: return SerializationKind.SolutionAttributes;
                case ProjectInfo.ProjectAttributes _: return SerializationKind.ProjectAttributes;
                case DocumentInfo.DocumentAttributes _: return SerializationKind.DocumentAttributes;
                case CompilationOptions _: return SerializationKind.CompilationOptions;
                case ParseOptions _: return SerializationKind.ParseOptions;
                case ProjectReference _: return SerializationKind.ProjectReference;
                case MetadataReference _: return SerializationKind.MetadataReference;
                case AnalyzerReference _: return SerializationKind.AnalyzerReference;
                case TextDocumentState _: return SerializationKind.RecoverableSourceText;
                case SourceText _: return SerializationKind.SourceText;
                case OptionSet _: return SerializationKind.OptionSet;
            }

            throw ExceptionUtilities.UnexpectedValue(value);
        }
    }
}
