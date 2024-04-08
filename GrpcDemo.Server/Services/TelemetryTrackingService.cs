using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcDemo.Server.Protos;
using System.Text.Json;

namespace GrpcDemo.Server.Services
{
    public class TelemetryTrackingService : TelemetryService.TelemetryServiceBase
    {
        private readonly ILogger _logger;

        public TelemetryTrackingService(ILogger<TelemetryTrackingService> logger)
        {
            _logger = logger;
        }

        public override Task<TrackingResponse> SendDeviceTracking(TrackingMessage request, ServerCallContext context)
        {
            try
            {
                _logger.LogInformation($"Grpc server received at {request.MessageDate.ToDateTime().ToString("dd/MMM/yy")} body: {JsonSerializer.Serialize(request)}");

                return Task.FromResult(new TrackingResponse
                {
                    Success = true,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"TelemetryTrackingService error {JsonSerializer.Serialize(ex)}");
                return Task.FromResult(new TrackingResponse
                {
                    Success = false
                });
            }
        }

        public override async Task<Empty> KeepAlive(IAsyncStreamReader<PulseMessage> requestStream, ServerCallContext context)
        {   
            //create new thread. avoid using the main thread as we might need it for other implementation
            var task = Task.Run(async () => {
                //wait for each message the client send and once you receive a message start processing
                await foreach (var item in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"keep alive message received from {item.ClientId} with status: {item.ClientStatus.ToString()} at {item.MessageDate.ToDateTime()}");
                }
            });

            await task;
            return new Empty();
        }

        public override async Task SubscribeNotification(NotificationMessage request, IServerStreamWriter<NotificationResponse> responseStream, ServerCallContext context)
        {
            Random random = new Random();
            var task = Task.Run(async() =>
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    await responseStream.WriteAsync(new NotificationResponse
                    {
                        NotificationHeader = $"The header of notification no. {random.Next()}",
                        NotificationBody = $"The body of notification no. {random.Next()}",
                        NotificationDate = Timestamp.FromDateTime(DateTime.UtcNow)
                    });

                    await Task.Delay(10000);
                }
            });

            await task;
        }
    }
}
