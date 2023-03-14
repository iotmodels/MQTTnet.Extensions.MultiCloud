using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.AzureIoTClient.Connections;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace MQTTnet.Extensions.MultiCloud.Connections;

public static partial class MqttNetExtensions
{
    public static MqttClientOptionsBuilder WithAzureDpsCredentials(this MqttClientOptionsBuilder builder, ConnectionSettings? cs)
    {
        if (cs?.Auth == AuthType.Sas)
        {
            var resource = $"{cs.IdScope}/registrations/{cs.DeviceId}";
            var username = $"{resource}/api-version=2019-03-31";
            var password = SasAuth.CreateSasToken(resource, cs.SharedAccessKey!, 5);
            builder
                .WithClientId(cs.DeviceId)
                .WithTcpServer("global.azure-devices-provisioning.net", cs.TcpPort)
                .WithCredentials(username, password)
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = true,
                    SslProtocol = SslProtocols.Tls12
                });
        }
        else if (cs?.Auth == AuthType.X509)
        {
            var cert = X509ClientCertificateLocator.Load(cs.X509Key!);
            string registrationId = X509CommonNameParser.GetCNFromCertSubject(cert);
            var resource = $"{cs.IdScope}/registrations/{registrationId}";
            var username = $"{resource}/api-version=2019-03-31";

            builder
                .WithClientId(registrationId)
                .WithTcpServer("global.azure-devices-provisioning.net", cs.TcpPort)
                .WithCredentials(username)
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = true,
                    SslProtocol = SslProtocols.Tls12,
                    Certificates = new List<X509Certificate> { cert }
                });
        }
        else
        {
            throw new NotSupportedException("DPS Does not support auth: " + cs?.Auth);
        }

        return builder;
    }
}
