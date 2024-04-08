
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo.Server.Protos;

namespace GrpcDemo.ClientDevice.Services
{
    public class HealthCheckBackgroundService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new Random();
            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress("https://localhost:7119", new GrpcChannelOptions { HttpHandler = httpHandler });
            var client = new TelemetryService.TelemetryServiceClient(channel);

            AsyncClientStreamingCall<PulseMessage, Empty> stream = client.KeepAlive();

            var keepAliveTask = Task.Run(async () => 
            {
                while (!stoppingToken.IsCancellationRequested) 
                {
                    await stream.RequestStream.WriteAsync(new PulseMessage 
                    {
                        ClientStatus = ClientStatus.Active, 
                        ClientId = random.Next(1,20), 
                        MessageDate = Timestamp.FromDateTime(DateTime.UtcNow)
                    });

                    await Task.Delay(5000);
                }
            });

            return Task.Delay(1000, stoppingToken);
        }
    }
}
