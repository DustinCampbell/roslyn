// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.SignatureHelp;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using VSCommanding = Microsoft.VisualStudio.Commanding;

namespace Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.SignatureHelp
{
    internal partial class Controller
    {
        VSCommanding.CommandState IChainedCommandHandler<TypeCharCommandArgs>.GetCommandState(TypeCharCommandArgs args, Func<VSCommanding.CommandState> nextHandler)
        {
            AssertIsForeground();

            // We just defer to the editor here.  We do not interfere with typing normal characters.
            return nextHandler();
        }

        void IChainedCommandHandler<TypeCharCommandArgs>.ExecuteCommand(TypeCharCommandArgs args, Action nextHandler, CommandExecutionContext context)
        {
            AssertIsForeground();

            // Note: while we're doing this, we don't want to hear about buffer changes (since we
            // know they're going to happen).  So we disconnect and reconnect to the event
            // afterwards.  That way we can hear about changes to the buffer that don't happen
            // through us.
            this.TextView.TextBuffer.PostChanged -= OnTextViewBufferPostChanged;
            try
            {
                nextHandler();
            }
            finally
            {
                this.TextView.TextBuffer.PostChanged += OnTextViewBufferPostChanged;
            }

            var typedChar = args.TypedChar;

            // We only want to process typechar if it is a normal typechar and no one else is
            // involved.  i.e. if there was a typechar, but someone processed it and moved the caret
            // somewhere else then we don't want signature help.  Also, if a character was typed but
            // something intercepted and placed different text into the editor, then we don't want
            // to proceed. 
            //
            // Note: we do not want to pass along a text version here.  It is expected that multiple
            // version changes may happen when we call 'nextHandler' and we will still want to
            // proceed.  For example, if the user types "WriteL(", then that will involve two text
            // changes as completion commits that out to "WriteLine(".  But we still want to provide
            // sig help in this case.
            if (this.TextView.TypeCharWasHandledStrangely(this.SubjectBuffer, typedChar))
            {
                // If we were computing anything, we stop.  We only want to process a typechar
                // if it was a normal character.
                DismissSessionIfActive();
                return;
            }

            var signatureHelpService = GetSignatureHelpService();
            if (signatureHelpService == null)
            {
                return;
            }

            var options = GetOptions();

            if (!IsSessionActive)
            {
                // No computation at all.  If this is not a trigger character, we just ignore it and
                // stay in this state.  Otherwise, if it's a trigger character, start up a new
                // computation and start computing the model in the background.
                if (IsTriggerCharacter(signatureHelpService, typedChar, options))
                {
                    // First create the session that represents that we now have a potential 
                    // signature help list. Then tell it to start computing.
                    StartSession(SignatureHelpTrigger.CreateInsertionTrigger(typedChar));
                    return;
                }
                else
                {
                    // No need to do anything.  Just stay in the state where we have no session.
                    return;
                }
            }
            else
            {
                var computed = false;
                if (IsRetriggerCharacter(signatureHelpService, typedChar, options))
                {
                    // The user typed a character that might close the scope of the current model.
                    // In this case, we should requery all providers.
                    //
                    // e.g.     Math.Max(Math.Min(1,2)$$
                    sessionOpt.ComputeModel(SignatureHelpTrigger.CreateUpdateTrigger(typedChar));
                    computed = true;
                }

                if (IsTriggerCharacter(signatureHelpService, typedChar, options))
                {
                    // The character typed was something like "(".  It can both filter a list if
                    // it was in a string like: Goo(bar, "(
                    //
                    // Or it can trigger a new list. Ask the computation to compute again.
                    sessionOpt.ComputeModel(SignatureHelpTrigger.CreateInsertionTrigger(typedChar));
                    computed = true;
                }

                if (!computed)
                {
                    // A character was typed and we haven't updated our model; do so now.
                    sessionOpt.ComputeModel(SignatureHelpTrigger.CreateUpdateTrigger());
                }
            }
        }

        private bool IsTriggerCharacter(SignatureHelpService signatureHelpService, char typedChar, OptionSet options)
            => signatureHelpService.IsTriggerCharacter(typedChar, _roles, options);

        private bool IsRetriggerCharacter(SignatureHelpService signatureHelpService, char typedChar, OptionSet options)
            => signatureHelpService.IsRetriggerCharacter(typedChar, _roles, options);
    }
}
