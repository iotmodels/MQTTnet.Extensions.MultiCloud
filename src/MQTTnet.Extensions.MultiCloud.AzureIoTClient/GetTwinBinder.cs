using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

internal class GetTwinBinder : RequestResponseBinder<string, string>
{
    internal int lastRid = 0;
    public GetTwinBinder(IMqttClient client) : base(client, string.Empty, true)
    {
        var rid = RidCounter.NextValue();
        lastRid = rid;
        requestTopicPattern = $"$iothub/twin/GET/?$rid={rid}";
        responseTopicSub = "$iothub/twin/res/#";
        responseTopicSuccess = $"$iothub/twin/res/200/?$rid={rid}";
    }
}
