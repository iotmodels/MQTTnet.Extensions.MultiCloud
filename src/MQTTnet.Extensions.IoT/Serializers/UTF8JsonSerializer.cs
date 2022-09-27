using System.Text;
using System.Text.Json;

namespace MQTTnet.Extensions.IoT.Serializers;

public class UTF8JsonSerializer : IMessageSerializer
{
    public T FromBytes<T>(byte[] payload) => JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload))!;
    public byte[] ToBytes<T>(T payload) => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));
}
