using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo.Server.Protos;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GrpcDemo.ClientDevice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("send_device_tracking")]
        public TrackingResponse? SendDeviceTracking()
        {
            //let's send a message to the server
            var message = new TrackingMessage
            {
                DeviceId = 21,
                Speed = 142,
                Location = new Location { Lat = 30.25, Log = 29.56 },
                MessageDate = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            message.Sensors.Add(new Sensor { Key = "sensor 1", Value = "value of sensor 1" });
            message.Sensors.Add(new Sensor { Key = "sensor 2", Value = "value of sensor 2" });
            message.Sensors.Add(new Sensor { Key = "sensor 3", Value = "value of sensor 3" });
            message.Sensors.Add(new Sensor { Key = "sensor 4", Value = "value of sensor 4" });

            var httpHandler = new HttpClientHandler();
            httpHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress("https://localhost:7119", new GrpcChannelOptions { HttpHandler = httpHandler });
            var client  = new TelemetryService.TelemetryServiceClient(channel);
            var grpcResponse = client.SendDeviceTracking(message);
            
            if (grpcResponse is not null)
                _logger.LogInformation($"server response: {JsonSerializer.Serialize(grpcResponse)}");
            else
                _logger.LogError($"something went wrong with the gRPC server!");

            return grpcResponse;
        }
    }
}
