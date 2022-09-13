using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public class HubMqttClient : IHubMqttClient
    {
        public IMqttClient Connection { get; set; }

        public string InitialState { get; set; } = String.Empty;

        private readonly IPropertyStoreReader getTwinBinder;
        private readonly IReportPropertyBinder updateTwinBinder;
        private readonly GenericDesiredUpdatePropertyBinder genericDesiredUpdateProperty;
        private readonly GenericCommand command;

        public HubMqttClient(IMqttClient c)
        {
            Connection = c;
            getTwinBinder = new GetTwinBinder(c);
            updateTwinBinder = new UpdateTwinBinder(c);
            command = new GenericCommand(c);
            genericDesiredUpdateProperty = new GenericDesiredUpdatePropertyBinder(c, updateTwinBinder);
        }

        public async Task InitState()
        {
            InitialState = await GetTwinAsync();
        }

        public Func<GenericCommandRequest, Task<CommandResponse>> OnCommandReceived
        {
            get => command.OnCmdDelegate;
            set => command.OnCmdDelegate = value;
        }

        public Func<JsonNode, GenericPropertyAck> OnPropertyUpdateReceived
        {
            get => genericDesiredUpdateProperty.OnProperty_Updated;
            set => genericDesiredUpdateProperty.OnProperty_Updated = value;
        }

        public Task<string> GetTwinAsync(CancellationToken cancellationToken = default) => getTwinBinder.ReadPropertiesDocAsync(cancellationToken);
        public Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default) => updateTwinBinder.ReportPropertyAsync(payload, cancellationToken);
        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default) 
            => Connection.PublishJsonAsync($"devices/{Connection.Options.ClientId}/messages/events/", payload, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, t);
        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, string componentName, CancellationToken t = default) 
            => Connection.PublishJsonAsync($"devices/{Connection.Options.ClientId}/messages/events/?$.sub={componentName}", payload, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, t);

    }
}
