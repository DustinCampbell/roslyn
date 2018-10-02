// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.SignatureHelp
{
    internal abstract class SignatureHelpProvider
    {
        internal string Name { get; }

        protected SignatureHelpProvider()
        {
            this.Name = this.GetType().FullName;
        }

        /// <summary>
        /// Returns true if the character might trigger completion, 
        /// e.g. '(' and ',' for method invocations 
        /// </summary>
        public abstract bool IsTriggerCharacter(char ch);

        /// <summary>
        /// Returns true if the character might end a Signature Help session, 
        /// e.g. ')' for method invocations.  
        /// </summary>
        public abstract bool IsRetriggerCharacter(char ch);

        /// <summary>
        /// Returns valid signature help items at the specified position in the document.
        /// </summary>
        public abstract Task<SignatureList> GetItemsAsync(Document document, int position, SignatureHelpTrigger triggerInfo, CancellationToken cancellationToken);
    }
}
