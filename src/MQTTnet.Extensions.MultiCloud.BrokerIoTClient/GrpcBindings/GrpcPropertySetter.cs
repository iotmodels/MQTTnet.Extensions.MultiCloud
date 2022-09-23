using MQTTnet.Client;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.GrpcBindings
{
    public class GrpcPropertySetter : RpcTopicBinder
    {
        public GrpcPropertySetter(IMqttClient client, string name)
        {
            var subAck = client.SubscribeAsync($"grpc/{client.Options.ClientId}/props/{name}/set");
            var callBackTopic = $"grpc/{client.Options.ClientId}/props/{name}/set";


            client.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;

                if (topic.Equals(callBackTopic))
                {
                    if (OnCallbackDelegate != null)
                    {
                        byte[] response = await OnCallbackDelegate.Invoke(m.ApplicationMessage.Payload);
                        //_ = client.PublishBinaryAsync($"grpc/{client.Options.ClientId}/props/{name}/set", null, Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true);
                        _ = client.PublishBinaryAsync(
                                $"grpc/{client.Options.ClientId}/props/{name}/ack",
                                response, Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                                false);
                    }
                }
                await Task.Yield();
            };
        }
    }
}
