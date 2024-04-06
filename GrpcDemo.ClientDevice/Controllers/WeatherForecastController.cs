using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcDemo.Server.Protos;
using Microsoft.AspNetCore.Mvc;

namespace GrpcDemo.ClientDevice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
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
            
            client.SendDeviceTracking(message);

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
