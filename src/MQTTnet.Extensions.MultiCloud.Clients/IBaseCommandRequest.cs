namespace MQTTnet.Extensions.MultiCloud.Clients
{
    public interface IBaseCommandRequest<T>
    {
        T DeserializeBody(string payload);
    }
}
