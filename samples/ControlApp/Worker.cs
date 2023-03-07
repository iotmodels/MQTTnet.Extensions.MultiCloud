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
            TelemetryClient<double> telClient = new TelemetryClient<double>(mqttClient, "workingSet");
            await telClient.Start("+");
            telClient.OnTelemetry = m =>
            {
                _logger.LogInformation(m.ToString());
            };
        }
    }
}