using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using System.Text.Json;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

internal class GetTwinRequestResponseBinder : RequestResponseBinder<string, JsonDocument>
{
    internal int lastRid = 0;
    public GetTwinRequestResponseBinder(IMqttClient client) : base(client, string.Empty)
    {
        var rid = RidCounter.NextValue();
        lastRid = rid;
        requestTopicPattern = $"$iothub/twin/GET/?$rid={rid}";
        responseTopicSub = "$iothub/twin/res/#";
        responseTopicSuccess = $"$iothub/twin/res/200/?$rid={rid}";
    }
}
