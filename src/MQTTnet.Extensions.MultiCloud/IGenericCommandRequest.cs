namespace MQTTnet.Extensions.MultiCloud
{
    public interface IGenericCommandRequest
    {
        string? CommandName { get; set; }
        string? CommandPayload { get; set; }
    }
}