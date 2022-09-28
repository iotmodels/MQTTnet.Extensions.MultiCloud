namespace MQTTnet.Extensions.MultiCloud
{
    public interface IWritableProperty<T, TResp> : ICloudToDevice<T, TResp>
    {
        T? Value { get; set; }
    }

    public interface IWritableProperty<T> : ICloudToDevice<T, Ack<T>>
    {
        T? Value { get; set; }
        int? Version { get; set; }
    }
}
