using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public class HubMqttClient : IHubMqttClient
    {
        public IMqttClient Connection { get; set; }
        public string InitialState { get; set; } = String.Empty;

        //private readonly TwinRequestResponseBinder twinOperationsBinder;

        private readonly GetTwinBinder getTwinBinder;
        private readonly UpdateTwinBinder<object> updateTwinBinder;

        private readonly GenericDesiredUpdatePropertyBinder genericDesiredUpdateProperty;
        private readonly GenericCommand command;

        public HubMqttClient(IMqttClient c)
        {
            Connection = c;
            //twinOperationsBinder = new TwinRequestResponseBinder(c);

            getTwinBinder = new GetTwinBinder(c);
            updateTwinBinder = new UpdateTwinBinder<object>(c);
            command = new GenericCommand(c);
            genericDesiredUpdateProperty = new GenericDesiredUpdatePropertyBinder(c, updateTwinBinder!);
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

        public async Task<string> GetTwinAsync(CancellationToken cancellationToken = default)
        {
            var twin = await getTwinBinder.InvokeAsync(Connection.Options.ClientId, string.Empty, cancellationToken);
            return twin!.ToString()!;
        }

        public async Task<int> UpdateTwinAsync(object payload, CancellationToken cancellationToken = default)
        {
            var twin = await updateTwinBinder.InvokeAsync(Connection.Options.ClientId, JsonSerializer.Serialize(payload), cancellationToken);
            return twin;
        }

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
