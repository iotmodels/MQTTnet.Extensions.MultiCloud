﻿using System.Text;

namespace MQTTnet.Extensions.MultiCloud.Connections;

internal class SasAuth
{
    private const string apiversion_2020_09_30 = "2020-09-30";
    internal static string GetUserName(string hostName, string deviceId, string modelId = "") =>
        $"{hostName}/{deviceId}/?api-version={apiversion_2020_09_30}&model-id={modelId}";

    internal static string Sign(string requestString, string key)
    {
        using (var algorithm = new System.Security.Cryptography.HMACSHA256(Convert.FromBase64String(key)))
        {
            return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(requestString)));
        }
    }

    internal static string CreateSasToken(string resource, string sasKey, int minutes)
    {

        var expiry = DateTimeOffset.UtcNow.AddMinutes(minutes).ToUnixTimeSeconds().ToString();
        var sig = System.Net.WebUtility.UrlEncode(Sign($"{resource}\n{expiry}", sasKey));
        return $"SharedAccessSignature sr={resource}&sig={sig}&se={expiry}";
    }

    internal static (string username, string password) GenerateHubSasCredentials(string hostName, string deviceId, string sasKey, string modelId, int minutes = 60) =>
        (GetUserName(hostName, deviceId, modelId), CreateSasToken($"{hostName}/devices/{deviceId}", sasKey, minutes));
}
