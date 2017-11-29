// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Execution;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Serialization
{
    /// <summary>
    /// serialize and deserialize objects to stream.
    /// some of these could be moved into actual object, but putting everything here is a bit easier to find I believe.
    /// </summary>
    internal partial class Serializer
    {
        public void SerializeSourceText(ITemporaryStorageWithName storage, SourceText text, ObjectWriter writer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            writer.WriteInt32((int)text.ChecksumAlgorithm);
            _referenceSerialization.WriteTo(text.Encoding, writer, cancellationToken);

            if (storage != null && storage.Name != null)
            {
                writer.WriteDataLocation(DataLocation.MemoryMapFile);
                writer.WriteMemoryMapFileProperties(storage.Name, storage.Offset, storage.Size);
            }
            else
            {
                writer.WriteDataLocation(DataLocation.Stream);
                text.WriteTo(writer, cancellationToken);
            }
        }

        private SourceText DeserializeSourceText(ObjectReader reader, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // REVIEW: why IDE services doesnt care about checksumAlgorithm?
            var checksumAlgorithm = (SourceHashAlgorithm)reader.ReadInt32();
            var encoding = _referenceSerialization.ReadEncodingFrom(reader, cancellationToken);

            var location = reader.ReadDataLocation();
            if (location == DataLocation.MemoryMapFile)
            {
                var (name, offset, size) = reader.ReadMemoryMapFileLocation();

                var storage = _temporaryStorage.AttachTemporaryTextStorage(name, offset, size, encoding, cancellationToken);

                return storage.ReadText(cancellationToken);
            }

            Contract.ThrowIfFalse(location == DataLocation.Stream);
            return _textFactory.ReadFrom(reader, encoding, cancellationToken);
        }

        public void SerializeCompilationOptions(CompilationOptions options, ObjectWriter writer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var language = options.Language;

            // TODO: once compiler team adds ability to serialize compilation options to ObjectWriter directly, we won't need this.
            writer.WriteString(language);

            var service = GetOptionsSerializationService(language);
            service.WriteTo(options, writer, cancellationToken);
        }

        private CompilationOptions DeserializeCompilationOptions(ObjectReader reader, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var language = reader.ReadString();

            var service = GetOptionsSerializationService(language);
            return service.ReadCompilationOptionsFrom(reader, cancellationToken);
        }

        public void SerializeParseOptions(ParseOptions options, ObjectWriter writer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var language = options.Language;

            // TODO: once compiler team adds ability to serialize parse options to ObjectWriter directly, we won't need this.
            writer.WriteString(language);

            var service = GetOptionsSerializationService(language);
            service.WriteTo(options, writer, cancellationToken);
        }

        private ParseOptions DeserializeParseOptions(ObjectReader reader, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var language = reader.ReadString();

            var service = GetOptionsSerializationService(language);
            return service.ReadParseOptionsFrom(reader, cancellationToken);
        }

        public void SerializeProjectReference(ProjectReference reference, ObjectWriter writer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            reference.ProjectId.WriteTo(writer);
            writer.WriteValue(reference.Aliases.ToArray());
            writer.WriteBoolean(reference.EmbedInteropTypes);
        }

        private ProjectReference DeserializeProjectReference(ObjectReader reader, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var projectId = ProjectId.ReadFrom(reader);
            var aliases = reader.ReadArray<string>();
            var embedInteropTypes = reader.ReadBoolean();

            return new ProjectReference(projectId, aliases.ToImmutableArrayOrEmpty(), embedInteropTypes);
        }

        public void SerializeMetadataReference(MetadataReference reference, ObjectWriter writer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _referenceSerialization.WriteTo(reference, writer, cancellationToken);
        }

        private MetadataReference DeserializeMetadataReference(ObjectReader reader, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _referenceSerialization.ReadMetadataReferenceFrom(reader, cancellationToken);
        }

        public void SerializeAnalyzerReference(AnalyzerReference reference, ObjectWriter writer, bool usePathFromAssembly, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _referenceSerialization.WriteTo(reference, writer, usePathFromAssembly, cancellationToken);
        }

        private AnalyzerReference DeserializeAnalyzerReference(ObjectReader reader, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _referenceSerialization.ReadAnalyzerReferenceFrom(reader, cancellationToken);
        }
    }
}
