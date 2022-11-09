using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Text.Json.Nodes;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public class HubMqttClient : IHubMqttClient
    {
        public IMqttClient Connection { get; set; }
        public string InitialState { get; set; } = String.Empty;

        private readonly TwinRequestResponseBinder twinOperationsBinder;
        private readonly GenericDesiredUpdatePropertyBinder genericDesiredUpdateProperty;
        private readonly GenericCommand command;

        public HubMqttClient(IMqttClient c)
        {
            Connection = c;
            twinOperationsBinder = new TwinRequestResponseBinder(c);
            //updateTwinBinder = new UpdateTwinBinder(c);
            command = new GenericCommand(c);
            genericDesiredUpdateProperty = new GenericDesiredUpdatePropertyBinder(c, twinOperationsBinder!);
        }

        public Func<GenericCommandRequest, GenericCommandResponse> OnCommandReceived
        {
            get => command.OnCmdDelegate!;
            set => command.OnCmdDelegate = value;
        }

        public Func<JsonNode, GenericPropertyAck> OnPropertyUpdateReceived
        {
            get => genericDesiredUpdateProperty.OnProperty_Updated!;
            set => genericDesiredUpdateProperty.OnProperty_Updated = value;
        }

        public Task<string> GetTwinAsync(CancellationToken cancellationToken = default) => twinOperationsBinder.GetTwinAsync(cancellationToken);
        public Task<int> UpdateTwinAsync(object payload, CancellationToken cancellationToken = default) => twinOperationsBinder.UpdateTwinAsync(payload, cancellationToken);
        public async Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default)
        {
            string clientSegment = Connection.Options.ClientId;
            if (clientSegment.Contains('/')) //should be a module
            {
                clientSegment = clientSegment.Replace("/", "/modules/");
            }
            return await Connection.PublishBinaryAsync($"devices/{clientSegment}/messages/events/",
                new UTF8JsonSerializer().ToBytes(payload),
                Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                false, t);
        }
    }
}
