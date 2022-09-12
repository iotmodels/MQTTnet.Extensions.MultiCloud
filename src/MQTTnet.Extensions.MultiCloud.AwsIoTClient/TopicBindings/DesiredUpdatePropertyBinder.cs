using MQTTnet.Client;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient.TopicBindings
{
    public class DesiredUpdatePropertyBinder<T>
    {
        public Func<PropertyAck<T>, Task<PropertyAck<T>>>? OnProperty_Updated = null;
        public DesiredUpdatePropertyBinder(IMqttClient connection, string propertyName, string componentName = "")
        {
            var subAck = connection.SubscribeAsync($"$aws/things/{connection.Options.ClientId}/shadow/update/accepted").Result;
            subAck.TraceErrors();
            IPropertyStoreWriter updateShadow = new UpdateShadowBinder(connection);
            connection.ApplicationMessageReceivedAsync += async m =>
             {
                 var topic = m.ApplicationMessage.Topic;
                 if (topic.StartsWith($"$aws/things/{connection.Options.ClientId}/shadow/update/accepted"))
                 {
                     string msg = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                     JsonNode? root = JsonNode.Parse(msg);
                     JsonNode desired = root!["state"]!["desired"]!;
                     JsonNode? desiredProperty = null;
                     if (string.IsNullOrEmpty(componentName))
                     {
                         desiredProperty = desired![propertyName]!;
                     }
                     else
                     {
                         if (desired != null &&
                            desired[componentName] != null &&
                             desired![componentName]![propertyName] != null &&
                             desired![componentName]!["__t"] != null &&
                             desired![componentName]!["__t"]!.GetValue<string>() == "c")
                         {
                             desiredProperty = desired![componentName]![propertyName]!;
                         }
                     }

                     if (desiredProperty != null)
                     {
                         if (OnProperty_Updated != null)
                         {
                             var property = new PropertyAck<T>(propertyName, componentName)
                             {
                                 Value = desiredProperty.Deserialize<T>()!,
                                 Version = root["version"]?.GetValue<int>() ?? 0
                             };
                             var ack = await OnProperty_Updated(property);
                             if (ack != null)
                             {
                                 _ = updateShadow.ReportPropertyAsync(ack.ToAckDict());
                             }
                         }
                     }
                 }
             };
        }
    }
}
