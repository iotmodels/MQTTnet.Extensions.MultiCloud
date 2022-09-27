using System;
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
            if (topic.Contains("?"))
            {
                var qs = HttpUtility.ParseQueryString(segments[segments.Length - 1]);
                int.TryParse(qs["$rid"], out rid);
                twinVersion = Convert.ToInt32(qs["$version"]);
            }
            return new TopicParameters() { Rid = rid, Version = twinVersion };
        }
    }
}
