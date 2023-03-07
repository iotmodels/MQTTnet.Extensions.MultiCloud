namespace MQTTnet.Extensions.MultiCloud;

public interface IMessageSerializer<T>
{
    byte[] ToBytes(T payload, string name = "");
    bool TryReadFromBytes(byte[] payload, string name, out T result);
}
