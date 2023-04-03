namespace MQTTnet.Extensions.MultiCloud
{
    public interface IGenericCommand
    {
        Func<IGenericCommandRequest, Task<IGenericCommandResponse>>? OnCmdDelegate { get; set; }
    }
}