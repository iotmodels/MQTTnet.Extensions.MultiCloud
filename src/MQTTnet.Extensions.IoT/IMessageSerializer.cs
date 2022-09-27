namespace MQTTnet.Extensions.IoT;

public interface IMessageSerializer
{
    byte[] ToBytes<T>(T payload);
    T FromBytes<T>(byte[] payload);
}
