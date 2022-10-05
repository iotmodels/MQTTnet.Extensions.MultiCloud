using System.Web;

namespace MQTTnet.Extensions.MultiCloud.Binders
{
    internal class TopicParser
    {
        internal static TopicParameters ParseTopic(string topic)
        {
            var segments = topic.Split('/');
            int twinVersion = -1;
            int rid = -1;
            if (topic.Contains('?'))
            {
                var qs = HttpUtility.ParseQueryString(segments[^1]);
                if (int.TryParse(qs["$version"], out int v))
                {
                    twinVersion = v;
                }

                if (int.TryParse(qs["$rid"], out int r))
                {
                    rid = r;
                }
            }
            return new TopicParameters() { Rid = rid, Version = twinVersion };
        }
    }
}
