// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Serialization;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Execution
{
    internal partial class CustomAssetFactory
    {
        private sealed class SimpleCustomAsset : CustomAsset
        {
            private readonly Action<ObjectWriter, CancellationToken> _writer;

            public SimpleCustomAsset(SerializationKind kind, Action<ObjectWriter, CancellationToken> writer)
                : base(CreateChecksumFromStreamWriter(kind, writer), kind)
            {
                // unlike SolutionAsset which gets checksum from solution states, this one build one by itself.
                _writer = writer;
            }

            public override Task WriteObjectToAsync(ObjectWriter writer, CancellationToken cancellationToken)
            {
                _writer(writer, cancellationToken);
                return SpecializedTasks.EmptyTask;
            }

            private static Checksum CreateChecksumFromStreamWriter(SerializationKind kind, Action<ObjectWriter, CancellationToken> writer)
            {
                using (var stream = SerializableBytes.CreateWritableStream())
                using (var objectWriter = new ObjectWriter(stream))
                {
                    objectWriter.WriteInt32((int)kind);
                    writer(objectWriter, CancellationToken.None);
                    return Checksum.Create(stream);
                }
            }
        }
    }
}
