﻿using Google.Protobuf;

namespace MQTTnet.Extensions.MultiCloud.Serializers;

public class ProtobufSerializer : IMessageSerializer
{
    private readonly MessageParser? _parser;
    public ProtobufSerializer() { }
    public ProtobufSerializer(MessageParser parser) => _parser = parser;
    public byte[] ToBytes<T>(T payload, string name = "") => (payload as IMessage).ToByteArray();

    public bool TryReadFromBytes<T>(byte[] payload, string name, out T result)
    {
        if (payload == null || payload.Length == 0)
        {
            result = default!;
            return false;
        }
        bool found = false;
        IMessage msg = _parser!.ParseFrom(payload);
        if (string.IsNullOrEmpty(name))
        {
            found = true;
            result = (T)msg;
        }
        else
        {
            if (msg.ToString()!.Contains(name)) // find better way
            {
                result = (T)msg;
                found = true;
            }
            else
            {
                result = default!;
            }
        }
        return found;
    }
}
