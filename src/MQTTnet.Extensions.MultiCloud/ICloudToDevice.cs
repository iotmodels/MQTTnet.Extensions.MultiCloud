namespace MQTTnet.Extensions.MultiCloud;

public interface ICloudToDevice<T, TResp>
{
    Func<T, Task<TResp>>? OnMessage { get; set; }
}
