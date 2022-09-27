namespace MQTTnet.Extensions.IoT;

public interface ICommand<T, TResp> : ICloudToDevice<T, TResp> { }
