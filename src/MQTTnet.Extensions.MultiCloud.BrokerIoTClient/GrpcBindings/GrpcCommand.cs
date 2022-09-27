using MQTTnet.Client;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.BrokerIoTClient.GrpcBindings
{
    public class GrpcCommand : RpcTopicBinder
    {
        public GrpcCommand(IMqttClient client, string name)
        {
            var subAck = client.SubscribeAsync($"grpc/{client.Options.ClientId}/cmd/{name}");
            var callBackTopic = $"grpc/{client.Options.ClientId}/cmd/{name}";

            client.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;

                if (topic.Equals(callBackTopic))
                {
                    if (OnCallbackDelegate != null)
                    {
                        byte[] response = await OnCallbackDelegate.Invoke(m.ApplicationMessage.Payload);
                        _ = client.PublishBinaryAsync(
                                $"grpc/{client.Options.ClientId}/cmd/{name}/resp",
                                response, Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                                false);
                    }
                }
                await Task.Yield();
            };
        }
    }
}

