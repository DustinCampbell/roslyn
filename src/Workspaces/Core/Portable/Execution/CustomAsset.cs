// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Serialization;

namespace Microsoft.CodeAnalysis.Execution
{
    /// <summary>
    /// Asset that is not part of solution, but want to participate in <see cref="IRemotableDataService"/>
    /// </summary>
    internal abstract class CustomAsset : RemotableData
    {
        protected CustomAsset(Checksum checksum, SerializationKind kind)
            : base(checksum, kind)
        {
        }
    }
}
