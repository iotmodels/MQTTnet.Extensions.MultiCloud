namespace MQTTnet.Extensions.IoT;

public interface IDeviceToCloud<T>
{
    Task SendMessageAsync(T payload, CancellationToken cancellationToken = default);
}
