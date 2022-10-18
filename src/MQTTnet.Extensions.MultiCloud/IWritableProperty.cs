namespace MQTTnet.Extensions.MultiCloud;

public interface IWritableProperty<T, TResp> : ICloudToDevice<T, TResp>
{
    T? Value { get; set; }
    int? Version { get; set; }
    Task InitPropertyAsync(string intialState, TResp defaultValue, CancellationToken cancellationToken = default);
}

public interface IWritableProperty<T> : ICloudToDevice<T, Ack<T>>, IDeviceToCloud<Ack<T>>
{
    T? Value { get; set; }
    int? Version { get; set; }
    Task InitPropertyAsync(string initialState, T defaultValue, CancellationToken cancellationToken = default);
}
