// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Serialization;
using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis.Execution
{
    /// <summary>
    /// Factory to create custom assets that are not part of a solution but want to participate in <see cref="IRemotableDataService"/>
    /// </summary>
    internal class CustomAssetFactory
    {
        private readonly Serializer _serializer;

        private CustomAssetFactory(HostWorkspaceServices services)
        {
            _serializer = new Serializer(services);
        }

        public CustomAssetFactory(Solution solution)
            : this(solution.Workspace)
        {
        }

        public CustomAssetFactory(Workspace workspace)
            : this(workspace.Services)
        {
        }

        public CustomAsset Create(OptionSet options, string language, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new SimpleCustomAsset(WellKnownSynchronizationKind.OptionSet,
                (writer, cancellationTokenOnStreamWriting) => _serializer.SerializeOptionSet(options, language, writer, cancellationTokenOnStreamWriting));
        }

        public CustomAsset Create(AnalyzerReference reference, CancellationToken cancellationToken)
        {
            return new WorkspaceAnalyzerReferenceAsset(reference, _serializer);
        }
    }
}
