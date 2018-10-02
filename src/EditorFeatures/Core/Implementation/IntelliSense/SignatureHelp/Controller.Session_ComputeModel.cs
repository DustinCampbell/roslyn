// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.ErrorReporting;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.SignatureHelp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.SignatureHelp
{
    internal partial class Controller
    {
        internal partial class Session
        {
            public void ComputeModel(
                ImmutableArray<SignatureHelpProvider> providers,
                SignatureHelpTrigger triggerInfo)
            {
                AssertIsForeground();

                var caretPosition = Controller.TextView.GetCaretPoint(Controller.SubjectBuffer).Value;
                var disconnectedBufferGraph = new DisconnectedBufferGraph(Controller.SubjectBuffer, Controller.TextView.TextBuffer);

                // If we've already computed a model, then just use that.  Otherwise, actually
                // compute a new model and send that along.
                Computation.ChainTaskAndNotifyControllerWhenFinished(
                    (model, cancellationToken) => ComputeModelInBackgroundAsync(
                        model, caretPosition, disconnectedBufferGraph,
                        triggerInfo, cancellationToken));
            }

            private async Task<Model> ComputeModelInBackgroundAsync(
                Model currentModel,
                SnapshotPoint caretPosition,
                DisconnectedBufferGraph disconnectedBufferGraph,
                SignatureHelpTrigger triggerInfo,
                CancellationToken cancellationToken)
            {
                try
                {
                    using (Logger.LogBlock(FunctionId.SignatureHelp_ModelComputation_ComputeModelInBackground, cancellationToken))
                    {
                        AssertIsBackground();
                        cancellationToken.ThrowIfCancellationRequested();

                        var document = await Controller.DocumentProvider.GetDocumentAsync(caretPosition.Snapshot, cancellationToken).ConfigureAwait(false);
                        if (document == null)
                        {
                            return currentModel;
                        }

                        if (triggerInfo.Kind == SignatureHelpTriggerKind.Update)
                        {
                            if (currentModel == null)
                            {
                                return null;
                            }

                            if (triggerInfo.Character != '\0' &&
                                !currentModel.Provider.IsRetriggerCharacter(triggerInfo.Character))
                            {
                                return currentModel;
                            }
                        }

                        var service = SignatureHelpService.GetService(document);
                        if (service == null)
                        {
                            // couldn't get a service, so we can't produce a model.
                            return null;
                        }

                        // first try to query the providers that can trigger on the specified character
                        var list = await service.GetSignaturesAsync(
                            document, caretPosition, triggerInfo,
                            cancellationToken: cancellationToken).ConfigureAwait(false);

                        if (list == null || list.Provider == null)
                        {
                            // No provider produced items. So we can't produce a model
                            return null;
                        }

                        if (currentModel != null &&
                            currentModel.Provider == list.Provider &&
                            currentModel.GetCurrentSpanInSubjectBuffer(disconnectedBufferGraph.SubjectBufferSnapshot).Span.Start == list.ApplicableSpan.Start &&
                            currentModel.Items.IndexOf(currentModel.SelectedItem) == list.SelectedItemIndex &&
                            currentModel.ArgumentIndex == list.ArgumentIndex &&
                            currentModel.ArgumentCount == list.ArgumentCount &&
                            currentModel.ArgumentName == list.ArgumentName)
                        {
                            // The new model is the same as the current model.  Return the currentModel
                            // so we keep the active selection.
                            return currentModel;
                        }

                        var selectedItem = GetSelectedItem(currentModel, list, list.Provider, out var userSelected);

                        var model = new Model(disconnectedBufferGraph, list.ApplicableSpan, list.Provider,
                            list.Items, selectedItem, list.ArgumentIndex, list.ArgumentCount, list.ArgumentName,
                            selectedParameter: 0, userSelected);

                        var syntaxFactsService = document.GetLanguageService<ISyntaxFactsService>();
                        var isCaseSensitive = syntaxFactsService == null || syntaxFactsService.IsCaseSensitive;
                        var selection = DefaultSignatureHelpSelector.GetSelection(model.Items,
                            model.SelectedItem, model.UserSelected, model.ArgumentIndex, model.ArgumentCount, model.ArgumentName, isCaseSensitive);

                        return model.WithSelectedItem(selection.SelectedItem, selection.UserSelected)
                                    .WithSelectedParameter(selection.SelectedParameter);
                    }
                }
                catch (Exception e) when (FatalError.ReportUnlessCanceled(e))
                {
                    throw ExceptionUtilities.Unreachable;
                }
            }

            private static bool SequenceEquals(IEnumerable<string> s1, IEnumerable<string> s2)
            {
                if (s1 == s2)
                {
                    return true;
                }

                return s1 != null && s2 != null && s1.SequenceEqual(s2);
            }

            private static SignatureHelpItem GetSelectedItem(Model currentModel, SignatureList items, SignatureHelpProvider provider, out bool userSelected)
            {
                // Try to find the most appropriate item in the list to select by default.

                // If it's the same provider as the previous model we have, and we had a user-selection,
                // then try to return the user-selection.
                if (currentModel != null && currentModel.Provider == provider && currentModel.UserSelected)
                {
                    var userSelectedItem = items.Items.FirstOrDefault(i => DisplayPartsMatch(i, currentModel.SelectedItem));
                    if (userSelectedItem != null)
                    {
                        userSelected = true;
                        return userSelectedItem;
                    }
                }
                userSelected = false;

                // If the provider specified a selected item, then pick that one.
                if (items.SelectedItemIndex.HasValue)
                {
                    return items.Items[items.SelectedItemIndex.Value];
                }

                SignatureHelpItem lastSelectionOrDefault = null;
                if (currentModel != null && currentModel.Provider == provider)
                {
                    // If the provider did not pick a default, and it's the same provider as the previous
                    // model we have, then try to return the same item that we had before.
                    lastSelectionOrDefault = items.Items.FirstOrDefault(i => DisplayPartsMatch(i, currentModel.SelectedItem));
                }

                if (lastSelectionOrDefault == null)
                {
                    // Otherwise, just pick the first item we have.
                    lastSelectionOrDefault = items.Items.First();
                }

                return lastSelectionOrDefault;
            }

            private static bool DisplayPartsMatch(SignatureHelpItem i1, SignatureHelpItem i2)
                => i1.GetAllParts().SequenceEqual(i2.GetAllParts(), CompareParts);

            private static bool CompareParts(TaggedText p1, TaggedText p2)
                => p1.ToString() == p2.ToString();
        }
    }
}
