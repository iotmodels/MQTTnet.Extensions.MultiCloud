﻿using MQTTnet.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class GenericDesiredUpdatePropertyBinder
    {
        public Func<JsonNode, GenericPropertyAck> OnProperty_Updated = null;
        public GenericDesiredUpdatePropertyBinder(IMqttClient connection, IReportPropertyBinder updTwinBinder)
        {
            connection.SubscribeWithReply("$iothub/twin/PATCH/properties/desired/#");
            connection.ApplicationMessageReceivedAsync += async m =>
             {
                 var topic = m.ApplicationMessage.Topic;
                 if (topic.StartsWith("$iothub/twin/PATCH/properties/desired"))
                 {
                     string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                     JsonNode desired = JsonNode.Parse(msg);

                     if (desired != null)
                     {
                         if (OnProperty_Updated != null)
                         {
                             var ack = OnProperty_Updated(desired);
                             if (ack != null)
                             {
                                 updTwinBinder.ReportPropertyAsync(ack.BuildAck()).RunSynchronously();
                             }
                         }
                     }
                 }
                 await Task.Yield();
             };
        }
    }
}
