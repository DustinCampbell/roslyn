// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Execution
{
    /// <summary>
    /// Information related to pinned solution
    /// </summary>
    internal class PinnedSolutionInfo
    {
        /// <summary>
        /// Unique ID for this pinned solution
        /// 
        /// This later used to find matching solution between VS and remote host
        /// </summary>
        public readonly int ScopeId;

        /// <summary>
        /// This indicates whether this scope is for primary branch or not (not forked solution)
        /// 
        /// Features like OOP will use this flag to see whether caching information related to this solution
        /// can benefit other requests or not
        /// </summary>
        public readonly bool FromPrimaryBranch;
        public readonly Checksum SolutionChecksum;

        public PinnedSolutionInfo(int scopeId, bool fromPrimaryBranch, Checksum solutionChecksum)
        {
            ScopeId = scopeId;
            FromPrimaryBranch = fromPrimaryBranch;
            SolutionChecksum = solutionChecksum;
        }
    }
}
