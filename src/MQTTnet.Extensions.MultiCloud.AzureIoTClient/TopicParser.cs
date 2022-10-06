using System.Web;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient;

internal class TopicParser
{
    internal static (int rid, int twinVersion) ParseTopic(string topic)
    {
        var segments = topic.Split('/');
        int twinVersion = -1;
        int rid = -1;
        if (topic.Contains('?'))
        {
            var qs = HttpUtility.ParseQueryString(segments[^1]);
            if (int.TryParse(qs["$rid"], out rid))
            {
                twinVersion = Convert.ToInt32(qs["$version"]);
            }
        }
        return (rid, twinVersion);
    }
}
