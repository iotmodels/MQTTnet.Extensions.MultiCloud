﻿//  <auto-generated/> 


using MQTTnet.Client;
using MQTTnet.Extensions.MultiCloud;

namespace dtmi_rido_pnp_sensehat;

public interface Isensehat 
{
    public const string ModelId = "dtmi:rido:pnp:sensehat;1";
    public IMqttClient Connection { get; }
    public string InitialState { get; set; }
    public IReadOnlyProperty<string> Property_piri { get; set; }
    public IReadOnlyProperty<string> Property_ipaddr { get; set; }
    public IReadOnlyProperty<string> Property_sdkInfo { get; set; }
    public IWritableProperty<int> Property_interval { get; set; }
    public IWritableProperty<bool> Property_combineTelemetry { get; set; }
    public ITelemetry<double> Telemetry_t1 { get; set; }
    public ITelemetry<double> Telemetry_t2 { get; set; }
    public ITelemetry<double> Telemetry_h { get; set; }
    public ITelemetry<double> Telemetry_p { get; set; }
    public ITelemetry<double> Telemetry_m { get; set; }

    public ICommand<Cmd_ChangeLCDColor_Request, Cmd_ChangeLCDColor_Response> Command_ChangeLCDColor { get; set; }
    public Task<MqttClientPublishResult> SendTelemetryAsync(AllTelemetries payload, CancellationToken t);
}

public class AllTelemetries
{
    public double t1 { get; set; }
    public double t2 { get; set; }
    public double h { get; set; }
    public double p { get; set; }
    public double m { get; set; }
}

public class Cmd_ChangeLCDColor_Request : IBaseCommandRequest<Cmd_ChangeLCDColor_Request>
{
    //public DiagnosticsMode DiagnosticsMode { get; set; }
    public string request;

    public Cmd_ChangeLCDColor_Request DeserializeBody(string payload)
    {
        return new Cmd_ChangeLCDColor_Request()
        {
            //DiagnosticsMode = System.Text.Json.JsonSerializer.Deserialize<DiagnosticsMode>(payload)
            request = System.Text.Json.JsonSerializer.Deserialize<string>(payload)
        };
    }

    public Cmd_ChangeLCDColor_Request DeserializeBody(byte[] payload)
    {
        throw new NotImplementedException();
    }
}

public class Cmd_ChangeLCDColor_Response : IBaseCommandResponse
{
    //public Dictionary<string, string> diagnosticResults { get; set; } = new Dictionary<string, string>();
    public int Status { get; set; }
    public object ReponsePayload { get; set; }
    public byte[] ResponseBytes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
