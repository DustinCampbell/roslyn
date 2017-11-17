// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Remote
{
    /// <summary>
    /// This represents client in client/server model.
    /// 
    /// The user can create a connection to communicate with the server (remote host) through this client.
    /// </summary>
    internal abstract partial class RemoteHostClient
    {
        public Workspace Workspace { get; }

        protected RemoteHostClient(Workspace workspace)
        {
            Workspace = workspace;
        }

        public event EventHandler<bool> StatusChanged;

        /// <summary>
        /// Create <see cref="Connection"/> for the <paramref name="serviceName"/> if possible; otherwise, return null.
        /// 
        /// Creating session could fail if remote host is not available. one of example will be user killing
        /// remote host.
        /// </summary>
        public abstract Task<Connection> TryCreateConnectionAsync(string serviceName, object callbackTarget, CancellationToken cancellationToken);

        protected abstract void OnStarted();

        protected abstract void OnStopped();

        internal void Shutdown()
        {
            // this should be only used by RemoteHostService to shutdown this remote host
            Stop();
        }

        protected void Start()
        {
            OnStarted();
            OnStatusChanged(true);
        }

        protected void Stop()
        {
            OnStopped();
            OnStatusChanged(false);
        }

        private void OnStatusChanged(bool started)
        {
            StatusChanged?.Invoke(this, started);
        }
    }
}
