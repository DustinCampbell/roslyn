// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Serialization;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Remote.Serialization
{
    internal static class Extensions
    {
        public static T[] ReadArray<T>(this ObjectReader reader)
        {
            return (T[])reader.ReadValue();
        }

        public static SerializationKind GetSerializationKind(this object value)
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

        public static DataLocation ReadDataLocation(this ObjectReader reader)
        {
            return (DataLocation)reader.ReadInt32();
        }

        public static (string name, long offset, long size) ReadMemoryMapFileLocation(this ObjectReader reader)
        {
            var name = reader.ReadString();
            var offset = reader.ReadInt64();
            var size = reader.ReadInt64();

            return (name, offset, size);
        }

        public static void WriteDataLocation(this ObjectWriter writer, DataLocation location)
        {
            writer.WriteInt32((int)location);
        }

        public static void WriteMemoryMapFileProperties(this ObjectWriter writer, string name, long offset, long size)
        {
            writer.WriteString(name);
            writer.WriteInt64(offset);
            writer.WriteInt64(size);
        }
    }
}
