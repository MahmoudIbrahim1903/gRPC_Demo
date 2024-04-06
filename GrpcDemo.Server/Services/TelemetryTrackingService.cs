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
    }
}
