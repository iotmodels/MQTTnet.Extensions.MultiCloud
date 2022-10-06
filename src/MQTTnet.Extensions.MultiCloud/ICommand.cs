namespace MQTTnet.Extensions.MultiCloud;

public interface ICommand : ICloudToDevice<string, string> { }

public interface ICommand<T> : ICloudToDevice<T, string> { }

public interface ICommand<T, TResp> : ICloudToDevice<T, TResp> { }
