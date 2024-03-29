using MQTTnet.Extensions.MultiCloud.BrokerIoTClient;

namespace ControlApp
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var mqttClient = await BrokerClientFactory.CreateFromConnectionSettingsAsync(_configuration.GetConnectionString("cs")!, false, stoppingToken);
            TelemetryClient<double> telClient = new(mqttClient, "workingSet");
            CommandClient<string, string> cmdClient = new(mqttClient, "echo");

            var res = await cmdClient.InvokeAsync("mqtt-command-device", "hello3", stoppingToken);

            _logger.LogInformation("Command response {r}", res);

            await telClient.StartAsync("+");

            telClient.OnTelemetry = (id,m) =>
            {
                _logger.LogInformation("Telemetry from {id} workingSet {m}", id, m);
            };
        }
    }
}