using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Text;
using System.Text.Json;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient.Dps
{
    public class MqttDpsClient
    {
        private readonly IMqttClient mqttClient;
        private int rid = 0;
        private readonly TaskCompletionSource<DpsStatus> tcs = new();
        private readonly string modelId;
        public MqttDpsClient(IMqttClient c, string mid)
        {
            mqttClient = c;
            modelId = mid;
            var subAck = mqttClient.SubscribeAsync("$dps/registrations/res/#").ConfigureAwait(false);
            mqttClient.ApplicationMessageReceivedAsync += async m =>
            {
                var topic = m.ApplicationMessage.Topic;
                if (topic.StartsWith($"$dps/registrations/res/"))
                {
                    var topicSegments = topic.Split('/');
                    int reqStatus = Convert.ToInt32(topicSegments[3]);
                    var payload = Encoding.UTF8.GetString(m.ApplicationMessage.Payload);
                    if (reqStatus >= 400)
                    {
                        tcs.SetException(new ApplicationException(payload));
                    }
                    var dpsRes = JsonSerializer.Deserialize<DpsStatus>(payload);
                    if (dpsRes != null && dpsRes.Status == "assigning")
                    {
                        // TODO: ready retry-after
                        await Task.Delay(2500); //avoid throtling
                        var pollTopic = $"$dps/registrations/GET/iotdps-get-operationstatus/?$rid={rid++}&operationId={dpsRes.OperationId}";
                        var puback = await mqttClient.PublishBinaryAsync(pollTopic, Array.Empty<byte>());
                    }
                    else
                    {
                        if (dpsRes != null && dpsRes.Status == "assigned")
                        {
                            tcs.SetResult(dpsRes);
                        }

                        if (dpsRes != null && dpsRes.Status == "disabled")
                        {
                            tcs.SetException(new ApplicationException("Device ID disabled in DPS"));
                        }
                    }
                }
            };
        }

        public async Task<DpsStatus> ProvisionDeviceIdentity()
        {
            var putTopic = $"$dps/registrations/PUT/iotdps-register/?$rid={rid++}";
            var registrationId = mqttClient.Options.ClientId;
            var bytes = new UTF8JsonSerializer<object>().ToBytes(new { registrationId, payload = new { modelId } });
            var puback = await mqttClient.PublishBinaryAsync(putTopic, bytes);
            if (puback.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                throw new ApplicationException("PubAck > 0 publishing DPS PUT");
            }
            return await tcs.Task.TimeoutAfter(TimeSpan.FromSeconds(30));
        }

    }
}
