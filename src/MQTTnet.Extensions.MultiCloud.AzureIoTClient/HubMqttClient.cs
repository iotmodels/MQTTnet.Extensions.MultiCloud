using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
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
        public Func<GenericCommandRequest, GenericCommandResponse> OnCommandReceived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Func<JsonNode, GenericPropertyAck> OnPropertyUpdateReceived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private readonly RequestResponseBinder getTwinBinder;
        private readonly IWritableProperty<GenericPropertyAck> updateTwinBinder;
        private readonly IWritableProperty<GenericPropertyAck> genericDesiredUpdateProperty;
        //private readonly GenericCommand command;

        public HubMqttClient(IMqttClient c)
        {
            Connection = c;
            //getTwinBinder = new GetTwinBinder(c);
            //updateTwinBinder = new UpdateTwinBinder(c);
            //command = new GenericCommand(c);
            //genericDesiredUpdateProperty = new GenericDesiredUpdatePropertyBinder(c, updateTwinBinder);
        }

        public Task<string> GetTwinAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default)
        {
            throw new NotImplementedException();
        }

        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, string componentName, CancellationToken t = default)
        {
            throw new NotImplementedException();
        }

        //public async Task InitState()
        //{
        //    //await command.InitSubscriptionsAsync();
        //    //await genericDesiredUpdateProperty.InitiSubscriptionsAsync();
        //    InitialState = await GetTwinAsync();
        //}

        //public Func<GenericCommandRequest, GenericCommandResponse> OnCommandReceived
        //{
        //    get => command.OnCmdDelegate;
        //    set => command.OnCmdDelegate = value;
        //}

        //public Func<JsonNode, GenericPropertyAck> OnPropertyUpdateReceived
        //{
        //    get => genericDesiredUpdateProperty.OnProperty_Updated;
        //    set => genericDesiredUpdateProperty.OnProperty_Updated = value;
        //}

        //public Task<string> GetTwinAsync(CancellationToken cancellationToken = default) => getTwinBinder.ReadPropertiesDocAsync(cancellationToken);
        //public Task<int> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default) => updateTwinBinder.ReportPropertyAsync(payload, cancellationToken);
        //public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default) => Connection.PublishStringAsync($"devices/{Connection.Options.ClientId}/messages/events/", Json.Stringify(payload), Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, t);
        //public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, string componentName, CancellationToken t = default) => Connection.PublishStringAsync($"devices/{Connection.Options.ClientId}/messages/events/?$.sub={componentName}", Json.Stringify(payload), Protocol.MqttQualityOfServiceLevel.AtLeastOnce, false, t);

    }
}
