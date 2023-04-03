namespace MQTTnet.Extensions.MultiCloud
{
    public interface IGenericCommandResponse
    {
        string? ReponsePayload { get; set; }
        int Status { get; set; }
    }
}