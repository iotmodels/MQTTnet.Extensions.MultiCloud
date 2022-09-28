namespace MQTTnet.Extensions.MultiCloud;

public interface IDeviceToCloud<T>
{
    Task SendMessageAsync(T payload, CancellationToken cancellationToken = default);
}
