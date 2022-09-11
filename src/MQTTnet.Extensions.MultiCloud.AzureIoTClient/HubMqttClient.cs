using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud.Clients;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public class HubMqttClient : IHubMqttClient
    {
        public IMqttClient Connection { get; set; }

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
            genericDesiredUpdateProperty = new GenericDesiredUpdatePropertyBinder(c);
        }

        public Func<GenericCommandRequest, Task<CommandResponse>> OnCommandReceived
        {
            get => command.OnCmdDelegate;
            set => command.OnCmdDelegate = value;
        }

        public Func<JsonNode, Task<GenericPropertyAck>> OnPropertyUpdateReceived
        {
            get => genericDesiredUpdateProperty.OnProperty_Updated;
            set => genericDesiredUpdateProperty.OnProperty_Updated = value;
        }

        public Task<string> GetTwinAsync(CancellationToken cancellationToken = default) => getTwinBinder.ReadPropertiesDocAsync(cancellationToken);
        public Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default) => updateTwinBinder.ReportPropertyAsync(payload, cancellationToken);
        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default) => Connection.PublishStringAsync($"devices/{Connection.Options.ClientId}/messages/events/", Json.Stringify(payload), Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, t);
        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, string componentName, CancellationToken t = default) => Connection.PublishStringAsync($"devices/{Connection.Options.ClientId}/messages/events/?$.sub={componentName}", Json.Stringify(payload), Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, t);

    }
}
