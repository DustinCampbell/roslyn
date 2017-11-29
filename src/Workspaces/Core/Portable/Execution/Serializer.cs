// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Execution;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Internal.Log;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Serialization
{
    /// <summary>
    /// serialize and deserialize objects to stream.
    /// some of these could be moved into actual object, but putting everything here is a bit easier to find I believe.
    /// 
    /// also, consider moving this serializer to use C# BOND serializer 
    /// https://github.com/Microsoft/bond
    /// </summary>
    internal partial class Serializer
    {
        private readonly HostWorkspaceServices _hostServices;

        private readonly IReferenceSerializationService _referenceSerialization;
        private readonly ITemporaryStorageService2 _temporaryStorage;
        private readonly ITextFactoryService _textFactory;

        private readonly ConcurrentDictionary<string, IOptionsSerializationService> _lazyOptionsSerializationByLanguage;

        public Serializer(Solution solution) : this(solution.Workspace)
        {
        }

        public Serializer(Workspace workspace) : this(workspace.Services)
        {
        }

        public Serializer(HostWorkspaceServices hostServices)
        {
            _hostServices = hostServices;

            _referenceSerialization = _hostServices.GetService<IReferenceSerializationService>();
            _temporaryStorage = _hostServices.TemporaryStorage as ITemporaryStorageService2;
            _textFactory = _hostServices.TextFactory;

            _lazyOptionsSerializationByLanguage = new ConcurrentDictionary<string, IOptionsSerializationService>(
                concurrencyLevel: 2,
                capacity: _hostServices.SupportedLanguages.Count());
        }

        public Checksum CreateChecksum(object value, CancellationToken cancellationToken)
        {
            var kind = value.GetSerializationKind();

            using (Logger.LogBlock(FunctionId.Serializer_CreateChecksum, kind.ToStringFast(), cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (value is IChecksummedObject)
                {
                    return ((IChecksummedObject)value).Checksum;
                }

                switch (kind)
                {
                    case SerializationKind.Null:
                        return Checksum.Null;

                    case SerializationKind.CompilationOptions:
                    case SerializationKind.ParseOptions:
                    case SerializationKind.ProjectReference:
                        return Checksum.Create(kind, value, this);

                    case SerializationKind.MetadataReference:
                        return Checksum.Create(kind, _referenceSerialization.CreateChecksum((MetadataReference)value, cancellationToken));

                    case SerializationKind.AnalyzerReference:
                        return Checksum.Create(kind, _referenceSerialization.CreateChecksum((AnalyzerReference)value, cancellationToken));

                    case SerializationKind.SourceText:
                        return Checksum.Create(kind, ((SourceText)value).GetChecksum());

                    default:
                        // object that is not part of solution is not supported since we don't know what inputs are required to serialize it
                        throw ExceptionUtilities.UnexpectedValue(kind);
                }
            }
        }

        public void Serialize(object value, ObjectWriter writer, CancellationToken cancellationToken)
        {
            var kind = value.GetSerializationKind();

            using (Logger.LogBlock(FunctionId.Serializer_Serialize, kind.ToString(), cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (value is ChecksumWithChildren)
                {
                    SerializeChecksumWithChildren((ChecksumWithChildren)value, writer, cancellationToken);
                    return;
                }

                switch (kind)
                {
                    case SerializationKind.Null:
                        // do nothing
                        return;

                    case SerializationKind.SolutionAttributes:
                    case SerializationKind.ProjectAttributes:
                    case SerializationKind.DocumentAttributes:
                        ((IObjectWritable)value).WriteTo(writer);
                        return;

                    case SerializationKind.CompilationOptions:
                        SerializeCompilationOptions((CompilationOptions)value, writer, cancellationToken);
                        return;

                    case SerializationKind.ParseOptions:
                        SerializeParseOptions((ParseOptions)value, writer, cancellationToken);
                        return;

                    case SerializationKind.ProjectReference:
                        SerializeProjectReference((ProjectReference)value, writer, cancellationToken);
                        return;

                    case SerializationKind.MetadataReference:
                        SerializeMetadataReference((MetadataReference)value, writer, cancellationToken);
                        return;

                    case SerializationKind.AnalyzerReference:
                        SerializeAnalyzerReference((AnalyzerReference)value, writer, usePathFromAssembly: true, cancellationToken: cancellationToken);
                        return;

                    case SerializationKind.SourceText:
                        SerializeSourceText(storage: null, text: (SourceText)value, writer: writer, cancellationToken: cancellationToken);
                        return;

                    default:
                        // object that is not part of solution is not supported since we don't know what inputs are required to serialize it
                        throw ExceptionUtilities.UnexpectedValue(kind);
                }
            }
        }

        public T Deserialize<T>(SerializationKind kind, ObjectReader reader, CancellationToken cancellationToken)
        {
            using (Logger.LogBlock(FunctionId.Serializer_Deserialize, kind.ToString(), cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch (kind)
                {
                    case SerializationKind.Null:
                        return default;

                    case SerializationKind.SolutionState:
                    case SerializationKind.ProjectState:
                    case SerializationKind.DocumentState:
                    case SerializationKind.Projects:
                    case SerializationKind.Documents:
                    case SerializationKind.TextDocuments:
                    case SerializationKind.ProjectReferences:
                    case SerializationKind.MetadataReferences:
                    case SerializationKind.AnalyzerReferences:
                        return (T)(object)DeserializeChecksumWithChildren(reader, cancellationToken);

                    case SerializationKind.SolutionAttributes:
                        return (T)(object)SolutionInfo.SolutionAttributes.ReadFrom(reader);
                    case SerializationKind.ProjectAttributes:
                        return (T)(object)ProjectInfo.ProjectAttributes.ReadFrom(reader);
                    case SerializationKind.DocumentAttributes:
                        return (T)(object)DocumentInfo.DocumentAttributes.ReadFrom(reader);
                    case SerializationKind.CompilationOptions:
                        return (T)(object)DeserializeCompilationOptions(reader, cancellationToken);
                    case SerializationKind.ParseOptions:
                        return (T)(object)DeserializeParseOptions(reader, cancellationToken);
                    case SerializationKind.ProjectReference:
                        return (T)(object)DeserializeProjectReference(reader, cancellationToken);
                    case SerializationKind.MetadataReference:
                        return (T)(object)DeserializeMetadataReference(reader, cancellationToken);
                    case SerializationKind.AnalyzerReference:
                        return (T)(object)DeserializeAnalyzerReference(reader, cancellationToken);
                    case SerializationKind.SourceText:
                        return (T)(object)DeserializeSourceText(reader, cancellationToken);
                    case SerializationKind.OptionSet:
                        return (T)(object)DeserializeOptionSet(reader, cancellationToken);

                    default:
                        throw ExceptionUtilities.UnexpectedValue(kind);
                }
            }
        }

        private IOptionsSerializationService GetOptionsSerializationService(string languageName)
        {
            return _lazyOptionsSerializationByLanguage.GetOrAdd(languageName, n => _hostServices.GetLanguageServices(n).GetService<IOptionsSerializationService>());
        }
    }
}
