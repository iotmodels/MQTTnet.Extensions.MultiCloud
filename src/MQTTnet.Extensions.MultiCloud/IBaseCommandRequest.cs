namespace MQTTnet.Extensions.MultiCloud
{
    public interface IBaseCommandRequest<T>
    {
        T DeserializeBody(string payload);
    }
}
