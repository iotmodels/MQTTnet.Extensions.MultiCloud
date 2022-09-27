namespace MQTTnet.Extensions.IoT;

public interface ICloudToDevice<T, TResp>
{
    public Func<T, Task<TResp>>? OnMessage { get; set; }
}
