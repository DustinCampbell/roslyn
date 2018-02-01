// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.MSBuild.Logging;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.MSBuild
{
    /// <summary>
    /// Represents a project file loaded from disk.
    /// </summary>
    internal sealed class ProjectFileInfo
    {
        /// <summary>
        /// The path to this project file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// The directory of this project file.
        /// </summary>
        public string Directory { get; }

        /// <summary>
        /// The language of this project.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// The path to the output file this project generates.
        /// </summary>
        public string OutputFilePath { get; }

        /// <summary>
        /// The command line args used to compile the project.
        /// </summary>
        public IReadOnlyList<string> CommandLineArgs { get; }

        /// <summary>
        /// The source documents.
        /// </summary>
        public IReadOnlyList<DocumentFileInfo> Documents { get; }

        /// <summary>
        /// The additional documents.
        /// </summary>
        public IReadOnlyList<DocumentFileInfo> AdditionalDocuments { get; }

        /// <summary>
        /// References to other projects.
        /// </summary>
        public IReadOnlyList<ProjectFileReference> ProjectReferences { get; }

        /// <summary>
        /// The error message produced when a failure occurred attempting to get the info. 
        /// If a failure occurred some or all of the information may be inaccurate or incomplete.
        /// </summary>
        public DiagnosticLog Log { get; }

        public ProjectFileInfo(
            string filePath,
            string language,
            string outputFilePath,
            IEnumerable<string> commandLineArgs,
            IEnumerable<DocumentFileInfo> documents,
            IEnumerable<DocumentFileInfo> additionalDocuments,
            IEnumerable<ProjectFileReference> projectReferences,
            DiagnosticLog log)
        {
            this.FilePath = filePath;
            this.Directory = Path.GetDirectoryName(filePath);
            this.Language = language;
            this.OutputFilePath = outputFilePath;
            this.CommandLineArgs = commandLineArgs.ToImmutableArrayOrEmpty();
            this.Documents = documents.ToImmutableReadOnlyListOrEmpty();
            this.AdditionalDocuments = additionalDocuments.ToImmutableArrayOrEmpty();
            this.ProjectReferences = projectReferences.ToImmutableReadOnlyListOrEmpty();
            this.Log = log;
        }

        public static ProjectFileInfo CreateEmpty(string filePath, string language, DiagnosticLog log)
            => new ProjectFileInfo(
                filePath, language,
                outputFilePath: null,
                commandLineArgs: SpecializedCollections.EmptyEnumerable<string>(),
                documents: SpecializedCollections.EmptyEnumerable<DocumentFileInfo>(),
                additionalDocuments: SpecializedCollections.EmptyEnumerable<DocumentFileInfo>(),
                projectReferences: SpecializedCollections.EmptyEnumerable<ProjectFileReference>(),
                log);
    }
}
