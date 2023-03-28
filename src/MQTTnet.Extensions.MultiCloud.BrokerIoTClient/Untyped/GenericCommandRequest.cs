namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.Untyped
{
    public class GenericCommandRequest
    {
        public string? CommandName { get; set; }
        public object? CommandPayload { get; set; }
    }
}
