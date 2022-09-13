using System;

namespace MQTTnet.Extensions.MultiCloud
{
    public interface ICommand<T, TResponse>
        where T : IBaseCommandRequest<T>, new()
        where TResponse : BaseCommandResponse
    {
        Func<T, TResponse>? OnCmdDelegate { get; set; }
    }
}