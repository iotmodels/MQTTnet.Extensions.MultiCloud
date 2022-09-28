﻿using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
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

        private readonly RequestResponseBinder twinOperationsBinder;
        private readonly GenericDesiredUpdatePropertyBinder genericDesiredUpdateProperty;
        private readonly GenericCommand command;

        public HubMqttClient(IMqttClient c)
        {
            Connection = c;
            twinOperationsBinder = new RequestResponseBinder(c);
            //updateTwinBinder = new UpdateTwinBinder(c);
            command = new GenericCommand(c);
            genericDesiredUpdateProperty = new GenericDesiredUpdatePropertyBinder(c, twinOperationsBinder!);
        }

        

        //public async Task InitState()
        //{
        //    //await command.InitSubscriptionsAsync();
        //    //await genericDesiredUpdateProperty.InitiSubscriptionsAsync();
        //    InitialState = await GetTwinAsync();
        //}

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

        public Task<string> GetTwinAsync(CancellationToken cancellationToken = default) => 
            twinOperationsBinder.GetTwinAsync(cancellationToken);
        public Task<string> ReportPropertyAsync(object payload, CancellationToken cancellationToken = default) =>
            twinOperationsBinder.UpdateTwinAsync(payload, cancellationToken);
        public Task<MqttClientPublishResult> SendTelemetryAsync(object payload, CancellationToken t = default) => 
            Connection.PublishBinaryAsync($"devices/{Connection.Options.ClientId}/messages/events/", 
                new UTF8JsonSerializer().ToBytes(payload), 
                Protocol.MqttQualityOfServiceLevel.AtLeastOnce, 
                false, t);
        

    }
}
