using MessagePack;

namespace MQTTnet.Extensions.MultiCloud.Serializers;

public class MsgPackSerializer : IMessageSerializer
{
    public byte[] ToBytes<T>(T payload, string name = "") => MessagePackSerializer.Serialize(payload);

    public bool TryReadFromBytes<T>(byte[] payload, string name, out T result)
    {
        result = MessagePackSerializer.Deserialize<T>(payload);
        return true;
    }
}
