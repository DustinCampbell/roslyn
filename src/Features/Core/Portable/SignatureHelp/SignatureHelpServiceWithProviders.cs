// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.ErrorReporting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.SignatureHelp
{
    /// <summary>
    /// A subtype of <see cref="SignatureHelpService"/> that aggregates completions from one or more <see cref="SignatureHelpProvider"/>s.
    /// </summary>
    internal abstract class SignatureHelpServiceWithProviders : SignatureHelpService
    {
        private readonly object _gate = new object();

        private readonly Dictionary<string, SignatureHelpProvider> _nameToProvider = new Dictionary<string, SignatureHelpProvider>();
        private readonly Dictionary<ImmutableHashSet<string>, ImmutableArray<SignatureHelpProvider>> _rolesToProviders;
        private readonly Func<ImmutableHashSet<string>, ImmutableArray<SignatureHelpProvider>> _createRoleProviders;
        private readonly Func<string, SignatureHelpProvider> _getProviderByName;

        protected readonly Workspace Workspace;

        private IEnumerable<Lazy<SignatureHelpProvider, SignatureHelpProviderMetadata>> _importedProviders;

        protected SignatureHelpServiceWithProviders(Workspace workspace)
        {
            Workspace = workspace;

            _rolesToProviders = new Dictionary<ImmutableHashSet<string>, ImmutableArray<SignatureHelpProvider>>(HashSetEqualityComparer<string>.Instance);
            _createRoleProviders = CreateRoleProviders;
            _getProviderByName = GetProviderByName;
        }

        protected virtual ImmutableArray<SignatureHelpProvider> GetBuiltInProviders()
            => ImmutableArray<SignatureHelpProvider>.Empty;

        private IEnumerable<Lazy<SignatureHelpProvider, SignatureHelpProviderMetadata>> GetImportedProviders()
        {
            if (_importedProviders == null)
            {
                var language = this.Language;
                var mefExporter = (IMefHostExportProvider)Workspace.Services.HostServices;

                var providers = ExtensionOrderer.Order(
                    mefExporter.GetExports<SignatureHelpProvider, SignatureHelpProviderMetadata>(language));

                Interlocked.CompareExchange(ref _importedProviders, providers, null);
            }

            return _importedProviders;
        }

        private ImmutableArray<SignatureHelpProvider> _testProviders = ImmutableArray<SignatureHelpProvider>.Empty;

        internal void SetTestProviders(IEnumerable<SignatureHelpProvider> testProviders)
        {
            lock (_gate)
            {
                _testProviders = testProviders != null
                    ? testProviders.ToImmutableArray()
                    : ImmutableArray<SignatureHelpProvider>.Empty;

                _rolesToProviders.Clear();
                _nameToProvider.Clear();
            }
        }

        private ImmutableArray<SignatureHelpProvider> CreateRoleProviders(ImmutableHashSet<string> roles)
        {
            var providers = GetAllProviders(roles);

            foreach (var provider in providers)
            {
                _nameToProvider[provider.Name] = provider;
            }

            return providers;
        }

        private ImmutableArray<SignatureHelpProvider> GetAllProviders(ImmutableHashSet<string> roles)
        {
            // If test providers have been set, use those instead of the built-in or imported providers.
            if (!_testProviders.IsDefaultOrEmpty)
            {
                return _testProviders;
            }

            var builtin = GetBuiltInProviders();
            var imported = GetImportedProviders()
                .Where(lz => lz.Metadata.Roles == null || lz.Metadata.Roles.Length == 0 || roles.Overlaps(lz.Metadata.Roles))
                .Select(lz => lz.Value);

            var providers = builtin.Concat(imported);
            return providers.ToImmutableArray();
        }

        internal protected SignatureHelpProvider GetProvider(SignatureHelpItem item)
        {
            SignatureHelpProvider provider = null;

            if (item.Properties.TryGetValue("Provider", out var name))
            {
                lock (_gate)
                {
                    provider = _nameToProvider.GetOrAdd(name, _getProviderByName);
                }
            }

            return provider;
        }

        private SignatureHelpProvider GetProviderByName(string providerName)
        {
            var providers = GetAllProviders(roles: ImmutableHashSet<string>.Empty);
            return providers.FirstOrDefault(p => p.Name == providerName);
        }

        public override async Task<SignatureList> GetSignaturesAsync(
            Document document,
            int caretPosition,
            SignatureHelpTrigger trigger,
            ImmutableHashSet<string> roles,
            OptionSet options,
            CancellationToken cancellationToken)
        {
            options = options ?? await document.GetOptionsAsync(cancellationToken).ConfigureAwait(false);
            var providers = GetAllProviders(roles);

            try
            {
                SignatureList bestList = null;

                // TODO(cyrusn): We're calling into extensions, we need to make ourselves resilient
                // to the extension crashing.
                foreach (var provider in providers)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var currentList = await provider.GetItemsAsync(document, caretPosition, trigger, cancellationToken).ConfigureAwait(false);
                    if (currentList != null && currentList.ApplicableSpan.IntersectsWith(caretPosition))
                    {
                        // If another provider provides sig help items, then only take them if they
                        // start after the last batch of items.  i.e. we want the set of items that
                        // conceptually are closer to where the caret position is.  This way if you have:
                        //
                        //  Goo(new Bar($$
                        //
                        // Then invoking sig help will only show the items for "new Bar(" and not also
                        // the items for "Goo(..."
                        if (IsBetter(bestList, currentList.ApplicableSpan))
                        {
                            bestList = currentList;
                        }
                    }
                }

                return bestList;
            }
            catch (Exception e) when (FatalError.ReportUnlessCanceled(e))
            {
                throw ExceptionUtilities.Unreachable;
            }
        }

        private static bool IsBetter(SignatureList bestItems, TextSpan currentTextSpan)
        {
            // If we have no best text span, then this span is definitely better.
            if (bestItems == null)
            {
                return true;
            }

            // Otherwise we want the one that is conceptually the innermost signature.  So it's
            // only better if the distance from it to the caret position is less than the best
            // one so far.
            return currentTextSpan.Start > bestItems.ApplicableSpan.Start;
        }

        public override bool IsTriggerCharacter(char ch, ImmutableHashSet<string> roles = null, OptionSet options = null)
        {
            options = options ?? Workspace.Options;

            // TODO(DustinCa): options are not currently used. Should they be passed to providers for future use?

            var providers = GetAllProviders(roles);
            return providers.Any(p => p.IsTriggerCharacter(ch));
        }

        public override bool IsRetriggerCharacter(char ch, ImmutableHashSet<string> roles = null, OptionSet options = null)
        {
            options = options ?? Workspace.Options;

            // TODO(DustinCa): options are not currently used. Should they be passed to providers for future use?

            var providers = GetAllProviders(roles);
            return providers.Any(p => p.IsRetriggerCharacter(ch));
        }
    }
}
