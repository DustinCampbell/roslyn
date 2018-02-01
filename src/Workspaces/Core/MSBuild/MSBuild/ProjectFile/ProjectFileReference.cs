// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.MSBuild
{
    /// <summary>
    /// Represents a reference to another project file.
    /// </summary>
    internal sealed class ProjectFileReference
    {
        /// <summary>
        /// The absolute path to the referenced project file.
        /// </summary>
        /// <remarks>
        /// In some cases (such as when the project's property value is malformed), the value may
        /// not represent a legal path. If these cases, <see cref="HasBadPath"/> will return true.
        /// </remarks>
        public string Path { get; }

        /// <summary>
        /// If true, the value of <see cref="Path"/> is not a legal path.
        /// </summary>
        public bool HasBadPath { get; }

        /// <summary>
        /// The aliases assigned to this reference, if any.
        /// </summary>
        public ImmutableArray<string> Aliases { get; }

        public ProjectFileReference(string path, ImmutableArray<string> aliases = default, bool hasBadPath = false)
        {
            this.Path = path;
            this.Aliases = aliases.IsDefault ? ImmutableArray<string>.Empty : aliases;
            this.HasBadPath = hasBadPath;
        }
    }
}
