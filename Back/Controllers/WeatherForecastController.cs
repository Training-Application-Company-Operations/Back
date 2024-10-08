using Back.Interfaces;
using Back.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Back.Controllers
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
        private readonly ITextEncryptionService _messageEncryption;
        private readonly IKeyVaultService _keyVaultService;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger
            ,ITextEncryptionService messageEncryption
            ,IKeyVaultService keyVaultService)
        {
            _logger = logger;
            _messageEncryption = messageEncryption;
            _keyVaultService = keyVaultService;
        }

        [HttpGet("GetWeather", Name = "GetWeather")]
        public ActionResult GetWeather()
        {
            return Ok(
                Enumerable
                    .Range(1, 5)
                    .Select(
                        index => new WeatherForecast {
                            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            TemperatureC = Random.Shared.Next(-20, 55),
                            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                        }
                    )
                    .ToArray()
            );
        }

        [HttpGet("GetWeatherForDay", Name = "GetWeatherForDay")]
        public ActionResult GetWeatherForDay(int day)
        {
            if (day <= 0 || day > 30)
            {
                return BadRequest("Value of 'day' should be between 1 and 30");
            }

            return Ok(new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(day)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });
        }

        [HttpGet("DoesTheWeatherMatch", Name = "DoesTheWeatherMatch")]
        public ActionResult DoesTheWeatherMatch(string encryptedWeather)
        {
            if (string.IsNullOrEmpty(encryptedWeather))
            {
                return BadRequest("Encrypted payload is missing.");
            }

            try
            {
                var jsonString =  _messageEncryption.Decrypt(encryptedWeather);

                var weatherForecast = JsonConvert.DeserializeObject<WeatherForecast>(jsonString);

                if (weatherForecast == null)
                {
                    return BadRequest("Decrypted payload is invalid.");
                }

                return Ok(_messageEncryption.Encrypt(JsonConvert.SerializeObject(new DoesTheWeatherMatchResponse() { IsSuccess = true })));
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetWeatherSecret", Name = "GetWeatherSecret")]
        public async Task<ActionResult> GetWeatherSecret()
        {
            return Ok(await _keyVaultService.GetSecretAsync("Prueba"));
        }
    }
}
