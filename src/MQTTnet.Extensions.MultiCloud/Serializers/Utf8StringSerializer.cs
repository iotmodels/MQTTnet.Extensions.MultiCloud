using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.Serializers
{
    public class Utf8StringSerializer : IMessageSerializer
    {
        public byte[] ToBytes<T>(T payload, string name = "") => Encoding.UTF8.GetBytes((payload as string)!);
        

        public bool TryReadFromBytes<T>(byte[] payload, string name, out T result) 
        {
            result = (T)Convert.ChangeType(Encoding.UTF8.GetString(payload), typeof(T));
            return true;
        }
    }
}
