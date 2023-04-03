namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{
    public class GenericCommandRequest : IGenericCommandRequest
    {
        public string? CommandName { get; set; }
        public string? CommandPayload { get; set; }
    }
}
