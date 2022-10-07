using Google.Protobuf;

namespace MQTTnet.Extensions.MultiCloud.Serializers;

public class ProtobufSerializer : IMessageSerializer
{
    private readonly MessageParser? _parser;
    public ProtobufSerializer() { }
    public ProtobufSerializer(MessageParser parser) => _parser = parser;
    public T FromBytes<T>(byte[] payload, string name = "") => (T)_parser!.ParseFrom(payload);
    public byte[] ToBytes<T>(T payload, string name = "") => (payload as IMessage).ToByteArray();

    public bool TryReadFromBytes<T>(byte[] payload, string name, out T result)
    {
        bool found = false;
        if (string.IsNullOrEmpty(name))
        {
            found = true;
            result = FromBytes<T>(payload);
        }
        else
        {
            IMessage msg = _parser!.ParseFrom(payload);
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
