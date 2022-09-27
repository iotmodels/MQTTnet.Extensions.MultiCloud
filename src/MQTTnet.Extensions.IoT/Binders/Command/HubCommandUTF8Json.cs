using MQTTnet.Client;
using MQTTnet.Extensions.IoT.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.IoT.Binders.Command
{
    public class HubCommandUTF8Json<T, TResp> : CloudToDeviceBinder<T, TResp>, ICommand<T, TResp>
    {
        int reqRid = -1;

        public HubCommandUTF8Json(IMqttClient client, string name)
        : base(client, name, new UTF8JsonSerializer())
        {
            TopicTemplate = "$iothub/methods/POST/{name}/#";
            ResponseTopic = "$iothub/methods/res/200/?$rid={rid}";

            PreProcessMessage = topic =>
            {
                (reqRid, _) = TopicParser.ParseTopic(topic);
            };

            PostProcessMessage = resp => reqRid.ToString();
        }
    }
}
