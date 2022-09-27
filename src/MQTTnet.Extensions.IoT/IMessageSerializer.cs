namespace MQTTnet.Extensions.IoT;

public interface IMessageSerializer
{
    byte[] ToBytes<T>(T payload, string name = "");
    T FromBytes<T>(byte[] payload, string name = "");

}
