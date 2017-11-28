// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Serialization;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Execution
{
    internal partial class CustomAssetFactory
    {
        /// <summary>
        /// workspace analyzer specific asset.
        /// 
        /// we need this to prevent dlls from other languages such as typescript, f#, xaml and etc
        /// from loading at OOP start up.
        /// 
        /// unlike project analyzer, analyzer that got installed from vsix doesn't do shadow copying
        /// so we don't need to load assembly to find out actual filepath.
        /// </summary>
        private sealed class WorkspaceAnalyzerReferenceAsset : CustomAsset
        {
            private readonly AnalyzerReference _reference;
            private readonly Serializer _serializer;

            public WorkspaceAnalyzerReferenceAsset(AnalyzerReference reference, Serializer serializer)
                : base(serializer.CreateChecksum(reference, CancellationToken.None), SerializationKind.AnalyzerReference)
            {
                _reference = reference;
                _serializer = serializer;
            }

            public override Task WriteObjectToAsync(ObjectWriter writer, CancellationToken cancellationToken)
            {
                // host analyzer is not shadow copied, no need to load assembly to get real path
                // this also prevent us from loading assemblies for all vsix analyzers preemptively
                const bool usePathFromAssembly = false;

                _serializer.SerializeAnalyzerReference(_reference, writer, usePathFromAssembly, cancellationToken);
                return SpecializedTasks.EmptyTask;
            }
        }
    }
}
