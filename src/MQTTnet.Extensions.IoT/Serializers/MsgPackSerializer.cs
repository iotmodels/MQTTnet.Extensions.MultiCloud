using MessagePack;

namespace MQTTnet.Extensions.IoT.Serializers;

public class MsgPackSerializer : IMessageSerializer
{
    public T FromBytes<T>(byte[] payload) => MessagePackSerializer.Deserialize<T>(payload);

    public byte[] ToBytes<T>(T payload) => MessagePackSerializer.Serialize(payload);
}
