using MQTTnet.Extensions.IoT.Binders.WritableProperty;

namespace MQTTnet.Extensions.IoT
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
