using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Binders;
using System.Web;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

public class UpdateTwinBinder<T> : RequestResponseBinder<T, int>
{
    public UpdateTwinBinder(IMqttClient c) : base(c, string.Empty, true)
    {
        var rid = RidCounter.NextValue();
        requestTopicPattern = $"$iothub/twin/PATCH/properties/reported/?$rid={rid}";
        responseTopicSub = "$iothub/twin/res/#";
        responseTopicSuccess = $"$iothub/twin/res/204/?$rid={rid}";
        requireNotEmptyPayload = false;
        VersionExtractor = topic =>
        {
            var segments = topic.Split('/');
            int twinVersion = -1;
            string rid = string.Empty;
            if (topic.Contains('?'))
            {
                var qs = HttpUtility.ParseQueryString(segments[^1]);
                if (int.TryParse(qs["$version"], out int v))
                {
                    twinVersion = v;
                }
            }
            return twinVersion;
        };
    }
}
