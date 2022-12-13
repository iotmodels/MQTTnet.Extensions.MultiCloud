using System.Diagnostics;

namespace mqtt_connection;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        if (_configuration.GetSection("ConsoleTracing").Get<bool>()==true)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttClient = await ClientFactory.CreateFromConnectionStringAsync(_configuration.GetConnectionString("cs")!);
        _logger.LogWarning("Connected: {s} with {c}", ClientFactory.ConnectionSettings!, ClientFactory.SdkInfo);

        mqttClient.DisconnectedAsync += async r => await Task.Run(() => _logger.LogError("Device Disconnected. {r}", r.Reason));

        while (!stoppingToken.IsCancellationRequested)
        {
            await mqttClient.PublishAsync(new MQTTnet.MqttApplicationMessageBuilder()
                .WithTopic($"devices/{mqttClient.Options.ClientId}/messages/events/")
                .WithPayload(System.Text.Json.JsonSerializer.Serialize(new { Environment.WorkingSet }))
                .Build());

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(5000, stoppingToken);
        }
    }
}