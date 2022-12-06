namespace MQTTnet.Extensions.MultiCloud;

public interface IReadOnlyProperty<T> : IDeviceToCloud<T>
{
    T? Value { get; set; }
    int Version { get; set; }
    void InitProperty(string initialState);
    Task SendMessageAsync(CancellationToken cancellationToken = default);
}
