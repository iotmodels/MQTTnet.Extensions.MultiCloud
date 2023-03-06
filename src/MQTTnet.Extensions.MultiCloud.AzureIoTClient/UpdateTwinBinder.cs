using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using MQTTnet.Extensions.MultiCloud.Serializers;
using System.Text;
using System.Text.Json;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class UpdateTwinBinder<T> : RequestResponseBinder<T, int>
{
    public UpdateTwinBinder(IMqttClient c) : base(c, string.Empty)
    {
        var rid = RidCounter.NextValue();
        requestTopicPattern = $"$iothub/twin/PATCH/properties/reported/?$rid={rid}";
        responseTopicSub = "$iothub/twin/res/#";
        responseTopicSuccess = $"$iothub/twin/res/204/?$rid={rid}";
        requireNotEmptyPayload = false;
    }
}
