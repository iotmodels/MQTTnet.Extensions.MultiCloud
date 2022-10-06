namespace MQTTnet.Extensions.MultiCloud;

public interface IMessageSerializer
{
    byte[] ToBytes<T>(T payload, string name = "");
    T? FromBytes<T>(byte[] payload, string name = "");
}
