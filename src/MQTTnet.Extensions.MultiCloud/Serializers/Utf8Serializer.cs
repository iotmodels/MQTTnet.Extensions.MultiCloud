using System.Text;

namespace MQTTnet.Extensions.MultiCloud.Serializers
{
    internal class Utf8Serializer : IMessageSerializer<string>
    {
        public byte[] ToBytes(string payload, string name = "")
        {
            return Encoding.UTF8.GetBytes(payload);
        }

        public bool TryReadFromBytes(byte[] payload, string name, out string result)
        {
            result = Encoding.UTF8.GetString(payload);
            return true;
        }
    }
}
