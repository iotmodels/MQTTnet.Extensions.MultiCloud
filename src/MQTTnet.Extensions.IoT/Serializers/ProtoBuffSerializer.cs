using Google.Protobuf;

namespace MQTTnet.Extensions.IoT.Serializers;

public class ProtoBuffSerializer : IMessageSerializer
{
    private readonly MessageParser? _parser;
    public ProtoBuffSerializer() { }
    public ProtoBuffSerializer(MessageParser parser) => _parser = parser;
    public T FromBytes<T>(byte[] payload) => (T)_parser!.ParseFrom(payload);
    public byte[] ToBytes<T>(T payload) => (payload as IMessage).ToByteArray();
}
