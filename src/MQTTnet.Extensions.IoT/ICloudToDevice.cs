namespace MQTTnet.Extensions.IoT;

public interface ICloudToDevice<T, TResp>
{
    Func<T, Task<TResp>>? OnMessage { get; set; }
}
