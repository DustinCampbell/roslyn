// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.TestHooks;
using Microsoft.CodeAnalysis.SignatureHelp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;

namespace Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.SignatureHelp
{
    internal partial class Controller :
        AbstractController<Controller.Session, Model, ISignatureHelpPresenterSession, ISignatureHelpSession>,
        IChainedCommandHandler<TypeCharCommandArgs>,
        IChainedCommandHandler<InvokeSignatureHelpCommandArgs>
    {
        private static readonly object s_controllerPropertyKey = new object();

        private readonly ImmutableHashSet<string> _roles;

        public string DisplayName => EditorFeaturesResources.Signature_Help;

        public Controller(
            IThreadingContext threadingContext,
            ITextView textView,
            ITextBuffer subjectBuffer,
            IIntelliSensePresenter<ISignatureHelpPresenterSession, ISignatureHelpSession> presenter,
            IAsynchronousOperationListener asyncListener,
            IDocumentProvider documentProvider)
            : base(threadingContext, textView, subjectBuffer, presenter, asyncListener, documentProvider, "SignatureHelp")
        {
            _roles = textView.Roles.ToImmutableHashSet();
        }

        internal static Controller GetInstance(
            IThreadingContext threadingContext,
            EditorCommandArgs args,
            IIntelliSensePresenter<ISignatureHelpPresenterSession, ISignatureHelpSession> presenter,
            IAsynchronousOperationListener asyncListener)
        {
            var textView = args.TextView;
            var subjectBuffer = args.SubjectBuffer;
            return textView.GetOrCreatePerSubjectBufferProperty(subjectBuffer, s_controllerPropertyKey,
                (v, b) => new Controller(threadingContext, v, b,
                    presenter,
                    asyncListener,
                    new DocumentProvider(threadingContext)));
        }

        private SnapshotPoint GetCaretPointInViewBuffer()
        {
            AssertIsForeground();
            return this.TextView.Caret.Position.BufferPosition;
        }

        internal override void OnModelUpdated(Model modelOpt)
        {
            AssertIsForeground();
            if (modelOpt == null)
            {
                this.StopModelComputation();
            }
            else
            {
                var selectedItem = modelOpt.SelectedItem;
                var triggerSpan = modelOpt.GetCurrentSpanInView(this.TextView.TextSnapshot);

                // We want the span to actually only go up to the caret.  So get the expected span
                // and then update its end point accordingly.
                var updatedSpan = new SnapshotSpan(triggerSpan.Snapshot, Span.FromBounds(
                    triggerSpan.Start,
                    Math.Max(Math.Min(triggerSpan.End, GetCaretPointInViewBuffer().Position), triggerSpan.Start)));

                var trackingSpan = updatedSpan.CreateTrackingSpan(SpanTrackingMode.EdgeInclusive);

                this.sessionOpt.PresenterSession.PresentItems(
                     trackingSpan, modelOpt.Items, modelOpt.SelectedItem, modelOpt.SelectedParameter);
            }
        }

        private void StartSession(SignatureHelpTrigger triggerInfo)
        {
            AssertIsForeground();
            VerifySessionIsInactive();

            this.sessionOpt = new Session(this, Presenter.CreateSession(TextView, SubjectBuffer, null));
            this.sessionOpt.ComputeModel(triggerInfo);
        }

        private SignatureHelpService GetSignatureHelpService()
        {
            if (!Workspace.TryGetWorkspace(this.SubjectBuffer.AsTextContainer(), out var workspace))
            {
                return null;
            }

            return workspace.Services.GetLanguageServices(this.SubjectBuffer).GetService<SignatureHelpService>();
        }

        private OptionSet GetOptions()
        {
            if (!Workspace.TryGetWorkspace(this.SubjectBuffer.AsTextContainer(), out var workspace))
            {
                return null;
            }

            return workspace.Options;
        }

        private void Retrigger()
        {
            AssertIsForeground();
            if (!IsSessionActive)
            {
                return;
            }

            if (!this.TextView.GetCaretPoint(this.SubjectBuffer).HasValue)
            {
                StopModelComputation();
                return;
            }

            sessionOpt.ComputeModel(SignatureHelpTrigger.CreateUpdateTrigger());
        }
    }
}
