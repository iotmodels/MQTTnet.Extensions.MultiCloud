using System;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud
{
    public interface ICommand<T, TResponse>
        where T : IBaseCommandRequest<T>, new()
        where TResponse : BaseCommandResponse
    {
        Func<T, Task<TResponse>>? OnCmdDelegate { get; set; }
    }
}