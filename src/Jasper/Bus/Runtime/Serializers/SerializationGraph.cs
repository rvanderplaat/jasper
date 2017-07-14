﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Conneg;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Serializers
{
    public class SerializationGraph
    {
        private readonly HandlerGraph _handlers;
        private readonly Dictionary<string, ISerializer> _serializers = new Dictionary<string, ISerializer>();

        private readonly IList<IMediaReader> _readers = new List<IMediaReader>();
        private readonly IList<IMediaWriter> _writers = new List<IMediaWriter>();

        private readonly ConcurrentDictionary<string, ModelReader> _modelReaders = new ConcurrentDictionary<string, ModelReader>();
        private readonly ConcurrentDictionary<Type, ModelWriter> _modelWriters = new ConcurrentDictionary<Type, ModelWriter>();

        public SerializationGraph(HandlerGraph handlers, IEnumerable<ISerializer> serializers, IEnumerable<IMediaReader> readers, IEnumerable<IMediaWriter> writers)
        {
            _handlers = handlers;
            foreach (var serializer in serializers)
            {
                _serializers.SmartAdd(serializer.ContentType, serializer);
            }

            _readers.AddRange(readers);
            _writers.AddRange(writers);
        }

        public object Deserialize(Envelope envelope, ChannelNode node)
        {
            var contentType = envelope.ContentType ?? node.AcceptedContentTypes.FirstOrDefault() ?? "application/json";

            if (contentType.IsEmpty())
            {
                throw new EnvelopeDeserializationException($"No content type can be determined for {envelope}");
            }

            if (envelope.Data == null || envelope.Data.Length == 0)
            {
                throw new EnvelopeDeserializationException($"No data on the Envelope");
            }

            if (envelope.MessageType.IsNotEmpty())
            {
                var reader = ReaderFor(envelope.MessageType);
                if (reader.HasAnyReaders)
                {
                    try
                    {
                        if (reader.TryRead(envelope.ContentType, envelope.Data, out object model))
                        {
                            return model;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw EnvelopeDeserializationException.ForReadFailure(envelope, ex);
                    }
                }
            }


            throw new EnvelopeDeserializationException($"Unknown content-type '{contentType}' and message-type '{envelope.MessageType}'");
        }

        public ModelWriter WriterFor(Type messageType)
        {
            return _modelWriters.GetOrAdd(messageType, compileWriter);
        }

        private ModelWriter compileWriter(Type messageType)
        {
            var fromSerializers = _serializers.Values.SelectMany(x => x.WritersFor(messageType));
            var writers = _writers.Where(x => x.DotNetType == messageType);

            return new ModelWriter(fromSerializers.Concat(writers).ToArray());
        }

        public ModelReader ReaderFor(string messageType)
        {
            return _modelReaders.GetOrAdd(messageType, compileReader);
        }

        private ModelReader compileReader(string messageType)
        {
            var readers = _readers.Where(x => x.MessageType == messageType).ToArray();
            var chainCandidates = _handlers.Chains.Where(x => x.MessageType.ToTypeAlias() == messageType)
                .Select(x => x.MessageType);

            var candidateTypes = _readers.Select(x => x.DotNetType).Concat(chainCandidates).Distinct();

            var fromSerializers =
                _serializers.Values.SelectMany(x => candidateTypes.SelectMany(x.ReadersFor));

            return new ModelReader(fromSerializers.Concat(readers).ToArray());
        }
    }
}