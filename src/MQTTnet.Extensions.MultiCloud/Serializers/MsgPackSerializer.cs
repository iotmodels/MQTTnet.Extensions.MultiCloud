using MessagePack;
using System.Xml.Linq;

namespace MQTTnet.Extensions.MultiCloud.Serializers;

public class MsgPackSerializer : IMessageSerializer
{
    public T FromBytes<T>(byte[] payload, string name = "") => MessagePackSerializer.Deserialize<T>(payload);
    public byte[] ToBytes<T>(T payload, string name = "") => MessagePackSerializer.Serialize(payload);
}
