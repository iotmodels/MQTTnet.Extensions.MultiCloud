using MessagePack;

namespace MQTTnet.Extensions.MultiCloud.Serializers;

public class MsgPackSerializer : IMessageSerializer
{
    public T FromBytes<T>(byte[] payload, string name = "") => MessagePackSerializer.Deserialize<T>(payload);
    public byte[] ToBytes<T>(T payload, string name = "") => MessagePackSerializer.Serialize(payload);

    public bool TryReadFromBytes<T>(byte[] payload, string name, out T result)
    {
        throw new NotImplementedException();
    }
}
