﻿using MQTTnet.Client;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;

namespace MQTTnet.Extensions.MultiCloud.Connections;

public static partial class MqttNetExtensions
{
    internal static MqttClientOptionsBuilder WithAzureIoTHubCredentials(this MqttClientOptionsBuilder builder, ConnectionSettings? cs)
    {
        string? hostName = cs!.HostName!;
        if (!string.IsNullOrEmpty(cs.GatewayHostName))
        {
            //hostName = cs.GatewayHostName;
            builder.WithTcpServer(cs.GatewayHostName, cs.TcpPort);
        }
        else
        {
            builder.WithTcpServer(hostName, cs.TcpPort);
        }    


        if (cs?.Auth == AuthType.Sas)
        {
            if (string.IsNullOrEmpty(cs.ModuleId))
            {
                cs.ClientId = cs.DeviceId;
            }
            else
            {
                cs.ClientId = $"{cs.DeviceId}/{cs.ModuleId}";
            }

            builder.WithTlsSettings(cs);
            return builder.WithAzureIoTHubCredentialsSas(hostName, cs.DeviceId!, cs.ModuleId!, cs.HostName!, cs.SharedAccessKey!, cs.ModelId!, cs.SasMinutes, cs.GatewayHostName!);
        }
        else if (cs?.Auth == AuthType.X509)
        {
            var cert = X509ClientCertificateLocator.Load(cs.X509Key!);
            string clientId = X509CommonNameParser.GetCNFromCertSubject(cert);
            cs.ClientId = clientId;
            if (clientId.Contains('/')) //is a module
            {
                var segmentsId = clientId.Split('/');
                cs.DeviceId = segmentsId[0];
                cs.ModuleId = segmentsId[1];
            }
            else
            {
                cs.DeviceId = clientId;
            }

            builder.WithTlsSettings(cs);
            return builder.WithAzureIoTHubCredentialsX509(hostName, cs.ClientId!, cs.ModelId!);
        }
        else
        {
            throw new ApplicationException("Auth not supported: " + cs?.Auth);
        }
    }

    public static MqttClientOptionsBuilder WithAzureIoTHubCredentialsSas(this MqttClientOptionsBuilder builder, string hostName, string deviceId, string moduleId, string audience, string sasKey, string modelId, int sasMinutes, string gatewayHostName)
    {
        string target = deviceId;
        if (!string.IsNullOrEmpty(moduleId))
        {
            target = $"{deviceId}/{moduleId}";
            audience = $"{hostName}/devices/{deviceId}/modules/{moduleId}";
        }
        

        (string username, string password) = SasAuth.GenerateHubSasCredentials(hostName, target, sasKey, audience, modelId, sasMinutes, gatewayHostName);
        builder.WithCredentials(username, password);
        return builder;
    }

    public static MqttClientOptionsBuilder WithAzureIoTHubCredentialsX509(this MqttClientOptionsBuilder builder, string hostName, string deviceId, string modelId)
    {
        string username = SasAuth.GetUserName(hostName, deviceId, modelId);
        builder.WithCredentials(username);
        return builder;
    }
}
