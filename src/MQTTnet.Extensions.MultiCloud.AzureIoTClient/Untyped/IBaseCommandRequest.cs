namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{
    public interface IBaseCommandRequest<T>
    {
        T DeserializeBody(string payload);
    }
}
