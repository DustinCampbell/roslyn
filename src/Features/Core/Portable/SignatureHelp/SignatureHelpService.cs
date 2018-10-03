// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.SignatureHelp
{
    /// <summary>
    /// A per language service for constructing context dependent list of signatures that 
    /// can be presented to a user during typing in an editor.
    /// </summary>
    internal abstract class SignatureHelpService : ILanguageService
    {
        /// <summary>
        /// Gets the service corresponding to the specified document.
        /// </summary>
        public static SignatureHelpService GetService(Document document)
            => document?.GetLanguageService<SignatureHelpService>();

        /// <summary>
        /// The language from <see cref="LanguageNames"/> this service corresponds to.
        /// </summary>
        public abstract string Language { get; }

        /// <summary>
        /// Returns true if the character recently inserted in the text should trigger Signature Help.
        /// </summary>
        /// <param name="ch">The character that was typed.</param>
        /// <param name="roles">Optional set of roles associated with the editor state.</param>
        /// <param name="options">Optional options that override the default options.</param>
        /// <returns>
        /// This API uses <see cref="SourceText"/> rather than <see cref="Document"/> so implementations can be based on text,
        /// not syntax or semantic information.
        /// </returns>
        public virtual bool IsTriggerCharacter(
            char ch,
            ImmutableHashSet<string> roles = null,
            OptionSet options = null)
        {
            return false;
        }

        /// <summary>
        /// Returns true if the character recently inserted in the text should trigger Signature Help.
        /// </summary>
        /// <param name="ch">The character that was typed.</param>
        /// <param name="roles">Optional set of roles associated with the editor state.</param>
        /// <param name="options">Optional options that override the default options.</param>
        /// <returns>
        /// This API uses <see cref="SourceText"/> rather than <see cref="Document"/> so implementations can be based on text,
        /// not syntax or semantic information.
        /// </returns>
        public virtual bool IsRetriggerCharacter(
            char ch,
            ImmutableHashSet<string> roles = null,
            OptionSet options = null)
        {
            return false;
        }

        /// <summary>
        /// Gets the signatures available at the caret position.
        /// </summary>
        /// <param name="document">The document that signature help is requested within.</param>
        /// <param name="caretPosition">The position of the caret after the triggering action.</param>
        /// <param name="trigger">The triggering action.</param>
        /// <param name="roles">Optional set of roles associated with the editor state.</param>
        /// <param name="options">Optional options that override the default options.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task<SignatureList> GetSignaturesAsync(
            Document document,
            int caretPosition,
            SignatureHelpTrigger trigger = default,
            ImmutableHashSet<string> roles = null,
            OptionSet options = null,
            CancellationToken cancellationToken = default);
    }
}
