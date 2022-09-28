using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Untyped
{
    public class GenericDesiredUpdatePropertyBinder
    {
        IMqttClient connection;
        public Func<JsonNode, GenericPropertyAck>? OnProperty_Updated = null;
        public GenericDesiredUpdatePropertyBinder(IMqttClient c, RequestResponseBinder updTwinBinder)
        {
            connection = c;
            _ = connection.SubscribeWithReplyAsync("$iothub/twin/PATCH/properties/desired/#");
            connection.ApplicationMessageReceivedAsync += async m =>
             {
                 var topic = m.ApplicationMessage.Topic;
                 if (topic.StartsWith("$iothub/twin/PATCH/properties/desired"))
                 {
                     string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                     JsonNode? desired = JsonNode.Parse(msg);

                     if (desired != null)
                     {
                         if (OnProperty_Updated != null)
                         {
                             var ack = OnProperty_Updated(desired);
                             if (ack != null)
                             {
                                 //_ = updTwinBinder.ReportPropertyAsync(ack.BuildAck());
                             }
                         }
                     }
                 }
                 await Task.Yield();
             };
        }
        public async Task InitiSubscriptionsAsync() => await connection.SubscribeWithReplyAsync("$iothub/twin/PATCH/properties/desired/#");
    }
}
