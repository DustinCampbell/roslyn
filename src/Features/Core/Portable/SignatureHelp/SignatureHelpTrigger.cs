// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.SignatureHelp
{
    /// <summary>
    /// The action that triggered Signature Help to start.
    /// </summary>
    internal struct SignatureHelpTrigger
    {
        /// <summary>
        /// The reason that Signature Help was started.
        /// </summary>
        public SignatureHelpTriggerKind Kind { get; }

        /// <summary>
        /// The character associated with the triggering action.
        /// </summary>
        public char Character { get; }

        internal SignatureHelpTrigger(SignatureHelpTriggerKind kind, char character = '\0')
            : this()
        {
            Contract.ThrowIfTrue(kind == SignatureHelpTriggerKind.Insertion && character == '\0');
            this.Kind = kind;
            this.Character = character;
        }

        /// <summary>
        /// The default <see cref="SignatureHelpTrigger"/> when none is specified.
        /// </summary>
        public static readonly SignatureHelpTrigger Invoke =
            new SignatureHelpTrigger(SignatureHelpTriggerKind.Invoke);

        /// <summary>
        /// Creates a new instance of a <see cref="SignatureHelpTrigger"/> association with the insertion of a typed character into the document.
        /// </summary>
        public static SignatureHelpTrigger CreateInsertionTrigger(char insertedCharacter)
            => new SignatureHelpTrigger(SignatureHelpTriggerKind.Insertion, insertedCharacter);

        /// <summary>
        /// Creates a new instance of a <see cref="SignatureHelpTrigger"/> to update Signature Help, with an optional typed character.
        /// </summary>
        public static SignatureHelpTrigger CreateUpdateTrigger(char insertedCharacter = '\0')
            => new SignatureHelpTrigger(SignatureHelpTriggerKind.Update, insertedCharacter);
    }
}
