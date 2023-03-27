using MQTTnet.Adapter;
using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud.Connections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MQTTnet.Extensions.MultiCloud.AwsIoTClient;

public static class AwsClientFactory
{
    public static string NuGetPackageVersion => $"{ThisAssembly.AssemblyName} {ThisAssembly.NuGetPackageVersion}";
    public static ConnectionSettings? ComputedSettings { get; private set; }

    public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(string connectinString, bool withBirth = false, CancellationToken cancellationToken = default) =>
        await CreateFromConnectionSettingsAsync(new ConnectionSettings(connectinString), withBirth, cancellationToken);

    public static async Task<IMqttClient> CreateFromConnectionSettingsAsync(ConnectionSettings cs, bool withBirth = false, CancellationToken cancellationToken = default)
    {
        MqttClient? mqtt = new MqttFactory().CreateMqttClient(MqttNetTraceLogger.CreateTraceLogger()) as MqttClient;
        try
        {
            var connAck = await mqtt!.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build(), cancellationToken);
            if (connAck.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new ApplicationException($"Cannot connect to {cs}");
            }
            ComputedSettings = cs;
        }
        catch (MqttConnectingFailedException ex)
        {
            if (ex.ResultCode == MqttClientConnectResultCode.UnspecifiedError
                && ex.InnerException!.Message == "The operation has timed out.")
            {
                var connAck = await mqtt!.ConnectAsync(new MqttClientOptionsBuilder().WithConnectionSettings(cs).Build(), cancellationToken);
                if (connAck.ResultCode != MqttClientConnectResultCode.Success)
                {
                    throw new ApplicationException($"Cannot connect to {cs}");
                }
                ComputedSettings = cs;
            }
        }

        if (withBirth)
        {
            var birthPayload = new ShadowSerializer().ToBytes(
                   new BirthConvention.BirthMessage(BirthConvention.ConnectionStatus.online)
                   {
                       ModelId = cs.ModelId
                   });

            var pubAck = await mqtt.PublishBinaryAsync(
               BirthConvention.BirthTopic(mqtt!.Options.ClientId),
               birthPayload,
               Protocol.MqttQualityOfServiceLevel.AtLeastOnce, true, cancellationToken);
            if (pubAck.ReasonCode != MqttClientPublishReasonCode.Success)
            {
                throw new ApplicationException($"Error publishing Birth {cs}");
            }
        }

        return mqtt!;
    }
}
