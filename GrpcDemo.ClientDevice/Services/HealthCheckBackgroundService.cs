
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo.Server.Protos;
using System.Text.Json;
using static GrpcDemo.Server.Protos.TelemetryService;

namespace GrpcDemo.ClientDevice.Services
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        private readonly ILogger _logger;
        public TelemetryServiceClient Client { get; set; }

        public HealthCheckBackgroundService(ILogger<HealthCheckBackgroundService> logger)
        {
            _logger = logger;

            if (Client == null)
            {
                var httpHandler = new HttpClientHandler();
                httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                var channel = GrpcChannel.ForAddress("https://localhost:7119", new GrpcChannelOptions { HttpHandler = httpHandler });
                Client = new TelemetryService.TelemetryServiceClient(channel);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new Random();

            var healthCheckTask = SendHeathCheckRequest(random, Client, stoppingToken);
            Task notificationTask = SubscribeNotification(random, Client, stoppingToken);

            await Task.WhenAll(healthCheckTask, notificationTask);
        }

        private async Task SendHeathCheckRequest(Random random, TelemetryService.TelemetryServiceClient client, CancellationToken stoppingToken)
        {
            AsyncClientStreamingCall<PulseMessage, Empty> stream = client.KeepAlive();

            var keepAliveTask = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await stream.RequestStream.WriteAsync(new PulseMessage
                    {
                        ClientStatus = ClientStatus.Active,
                        ClientId = random.Next(1, 20),
                        MessageDate = Timestamp.FromDateTime(DateTime.UtcNow)
                    });

                    await Task.Delay(5000);
                }
            });

            await keepAliveTask;
        }

        private Task SubscribeNotification(Random random, TelemetryService.TelemetryServiceClient client, CancellationToken stoppingToken)
        {
            var responseStream = client.SubscribeNotification(new NotificationMessage
            {
                DeviceId = random.Next(),
                SubscriptionDate = Timestamp.FromDateTime(DateTime.UtcNow)
            });

            var task = Task.Run(async () =>
            {
                //wait until you reveive a new message
                while (await responseStream.ResponseStream.MoveNext(stoppingToken))
                {
                    var msg = responseStream.ResponseStream.Current;

                    _logger.LogInformation($"notification message received from the server: {JsonSerializer.Serialize(msg)}");
                }
            });
            return task;
        }
    }
}
