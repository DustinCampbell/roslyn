// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Remote
{
    internal abstract partial class RemoteHostClient
    {
        /// <summary>
        /// NoOpClient is used if a user killed our remote host process. Basically this client never create a session.
        /// </summary>
        internal class NoOpClient : RemoteHostClient
        {
            public NoOpClient(Workspace workspace) :
                base(workspace)
            {
            }

            public override Task<Connection> TryCreateConnectionAsync(string serviceName, object callbackTarget, CancellationToken cancellationToken)
            {
                return SpecializedTasks.Default<Connection>();
            }

            protected override void OnStarted()
            {
                // do nothing
            }

            protected override void OnStopped()
            {
                // do nothing
            }
        }
    }
}
