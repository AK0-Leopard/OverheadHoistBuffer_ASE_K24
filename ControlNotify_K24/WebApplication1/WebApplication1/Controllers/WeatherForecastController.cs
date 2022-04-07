using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        private readonly ILogger<WeatherForecastController> _logger;
        static int come_count = 0;


        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {

            _logger = logger;

        }

        static object lock_obj = new object();
        [HttpGet("alarm/{id}")]
        public String Happened(string id, [FromServices] WeatherForecast service, [FromServices] SerialPortService serialPort)

        {

            //SpeechSynthesizer synth = new SpeechSynthesizer();

            // Configure the audio output.   
            //synth.SetOutputToDefaultAudioDevice();

            // Speak a string.  
            //synth.Speak("This example demonstrates a basic use of Speech Synthesizer");
            lock (lock_obj)
            {
                //if (!service.passReportVh.Contains(id))
                //    serialPort.openPort();
                switch (id)
                {
                    case "1":
                        _logger.LogInformation("1號車發生異常");
                        service.oht1_error.PlaySync();
                        break;
                    case "2":
                        _logger.LogInformation("2號車發生異常");
                        service.oht2_error.PlaySync();
                        break;
                    case "3":
                        _logger.LogInformation("3號車發生異常");
                        service.oht3_error.PlaySync();
                        break;
                    case "5":
                        _logger.LogInformation("5號車發生異常");
                        service.oht5_error.PlaySync();
                        break;
                    case "6":
                        _logger.LogInformation("6號車發生異常");
                        service.oht6_error.PlaySync();
                        break;
                    case "7":
                        _logger.LogInformation("7號車發生異常");
                        service.oht7_error.PlaySync();
                        break;
                    case "8":
                        _logger.LogInformation("8號車發生異常");
                        service.oht8_error.PlaySync();
                        break;
                    case "9":
                        _logger.LogInformation("9號車發生異常");
                        service.oht9_error.PlaySync();
                        break;
                    case "10":
                        _logger.LogInformation("10號車發生異常");
                        service.oht10_error.PlaySync();
                        break;
                }
                //if (!service.passReportVh.Contains(id))
                //    serialPort.closePort();
            }

            return "ok";
        }
        [HttpGet("disconnected/{id}")]
        public String Disconnected(string id, [FromServices] WeatherForecast service, [FromServices] SerialPortService serialPort)

        {

            lock (lock_obj)
            {
                switch (id)
                {
                    case "1":
                        _logger.LogInformation("1號車發生斷線");
                        service.oht1_disconnection.PlaySync();
                        break;
                    case "2":
                        _logger.LogInformation("2號車發生斷線");
                        service.oht2_disconnection.PlaySync();
                        break;
                    case "3":
                        _logger.LogInformation("3號車發生斷線");
                        service.oht3_disconnection.PlaySync();
                        break;
                    case "5":
                        _logger.LogInformation("5號車發生斷線");
                        service.oht5_disconnection.PlaySync();
                        break;
                    case "6":
                        _logger.LogInformation("6號車發生斷線");
                        service.oht6_disconnection.PlaySync();
                        break;
                    case "7":
                        _logger.LogInformation("7號車發生斷線");
                        service.oht7_disconnection.PlaySync();
                        break;
                    case "8":
                        _logger.LogInformation("8號車發生斷線");
                        service.oht8_disconnection.PlaySync();
                        break;
                    case "9":
                        _logger.LogInformation("9號車發生斷線");
                        service.oht9_disconnection.PlaySync();
                        break;
                    case "10":
                        _logger.LogInformation("10號車發生斷線");
                        service.oht10_disconnection.PlaySync();
                        break;
                }
            }

            return "ok";
        }
        [HttpGet("VhHasCmdNoAction/{id}")]
        public String VhHasCmdNoAction(string id, [FromServices] WeatherForecast service, [FromServices] SerialPortService serialPort)

        {

            lock (lock_obj)
            {
                _logger.LogInformation("天車無法移動");
                service.vhHasCmdNoAction.PlaySync();
            }

            return "ok";
        }

    }
}
