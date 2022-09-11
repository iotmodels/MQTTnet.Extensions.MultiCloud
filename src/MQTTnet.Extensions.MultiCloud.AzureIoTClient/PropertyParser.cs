using System.Text.Json.Nodes;

namespace MQTTnet.Extensions.MultiCloud.AzureIoTClient
{
    public class PropertyParser
    {
        public static JsonNode ReadPropertyFromDesired(JsonNode desired, string propertyName, string componentName)
        {
            JsonNode result = null;
            if (string.IsNullOrEmpty(componentName))
            {
                result = desired?[propertyName];
            }
            else
            {
                if (desired[componentName] != null &&
                    desired[componentName][propertyName] != null &&
                    desired[componentName]["__t"] != null &&
                    desired[componentName]["__t"].GetValue<string>() == "c")
                {
                    result = desired?[componentName][propertyName];
                }
            }

            return result;
        }
    }
}
