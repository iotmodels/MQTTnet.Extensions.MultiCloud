using MQTTnet.Extensions.MultiCloud;
using MQTTnet.Extensions.MultiCloud.AwsIoTClient;
using MQTTnet.Extensions.MultiCloud.Connections;

namespace aws_sample
{
    public class Device : BackgroundService
    {
        private readonly ILogger<Device> _logger;
        private readonly IConfiguration _configuration;

        public Device(ILogger<Device> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ConnectionSettings cs =  new (_configuration.GetConnectionString("cs"));
            var mqtt = await AwsClientFactory.CreateFromConnectionSettingsAsync(cs, false, stoppingToken);
            Console.WriteLine(mqtt.IsConnected);
            Console.WriteLine(AwsClientFactory.ComputedSettings);
            var client = new AwsMqttClient(mqtt);
            var shadow = await client.GetShadowAsync(stoppingToken);
            Console.WriteLine(shadow);

            var res = await client.UpdateShadowAsync(new { myProp = "hello 123" }, stoppingToken);
            Console.WriteLine(res);
            shadow = await client.GetShadowAsync(stoppingToken);
            Console.WriteLine(shadow.Contains("myProp"));

            WritableProperty<string> wp = new(mqtt, "myWProp")
            {
                OnMessage = async m =>
                {
                    Console.WriteLine(m);
                    return await Task.FromResult(new Ack<string> { Value = m });
                }
            };


            await wp.InitPropertyAsync(shadow, "my default val", stoppingToken);


            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}