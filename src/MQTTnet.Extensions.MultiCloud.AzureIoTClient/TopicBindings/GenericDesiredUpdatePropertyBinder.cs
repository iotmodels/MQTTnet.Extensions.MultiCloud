using MQTTnet.Client;
using System;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.TopicBindings
{
    public class GenericDesiredUpdatePropertyBinder
    {
        public Func<JsonNode, Task<GenericPropertyAck>> OnProperty_Updated = null;
        public GenericDesiredUpdatePropertyBinder(IMqttClient connection)
        {
            var subAck = connection.SubscribeAsync("$iothub/twin/PATCH/properties/desired/#").Result;
            ReplySubscriptions.Add("$iothub/twin/PATCH/properties/desired/#");
            subAck.TraceErrors();
            IPropertyStoreWriter updateTwin = new UpdateTwinBinder(connection);
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
                             var ack = await OnProperty_Updated(desired);
                             if (ack != null)
                             {
                                 _ = updateTwin.ReportPropertyAsync(ack.BuildAck());
                             }
                         }
                     }
                 }
             };
        }


    }
}
