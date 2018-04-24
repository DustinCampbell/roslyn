// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Shared.Utilities;

namespace Microsoft.CodeAnalysis.SignatureHelp
{
    /// <summary>
    /// A subtype of <see cref="SignatureHelpService"/> that aggregates completions from one or more <see cref="SignatureHelpProvider"/>s.
    /// </summary>
    internal abstract class SignatureHelpServiceWithProviders : SignatureHelpService
    {
        private readonly Workspace _workspace;
        private IEnumerable<Lazy<SignatureHelpProvider, SignatureHelpProviderMetadata>> _importedProviders;

        public SignatureHelpServiceWithProviders(Workspace workspace)
        {
            _workspace = workspace;
        }

        protected virtual ImmutableArray<SignatureHelpProvider> GetBuiltInProviders()
            => ImmutableArray<SignatureHelpProvider>.Empty;

        private IEnumerable<Lazy<SignatureHelpProvider, SignatureHelpProviderMetadata>> GetImportedProviders()
        {
            if (_importedProviders == null)
            {
                var language = this.Language;
                var mefExporter = (IMefHostExportProvider)_workspace.Services.HostServices;

                var providers = ExtensionOrderer.Order(
                        mefExporter.GetExports<SignatureHelpProvider, SignatureHelpProviderMetadata>()
                        .Where(lz => lz.Metadata.Language == language)
                        ).ToList();

                Interlocked.CompareExchange(ref _importedProviders, providers, null);
            }

            return _importedProviders;
        }

    }
}
