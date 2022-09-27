﻿using Google.Protobuf;
using System.Xml.Linq;

namespace MQTTnet.Extensions.IoT.Serializers;

public class ProtoBuffSerializer : IMessageSerializer
{
    private readonly MessageParser? _parser;
    public ProtoBuffSerializer() { }
    public ProtoBuffSerializer(MessageParser parser) => _parser = parser;
    public T FromBytes<T>(byte[] payload, string name = "") => (T)_parser!.ParseFrom(payload);
    public byte[] ToBytes<T>(T payload, string name = "") => (payload as IMessage).ToByteArray();
}