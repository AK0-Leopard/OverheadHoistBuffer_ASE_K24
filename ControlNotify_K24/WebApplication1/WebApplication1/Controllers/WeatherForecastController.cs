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
                    case "11":
                        _logger.LogInformation("11號車發生異常");
                        service.oht11_error.PlaySync();
                        break;
                    case "12":
                        _logger.LogInformation("12號車發生異常");
                        service.oht12_error.PlaySync();
                        break;
                    case "13":
                        _logger.LogInformation("13號車發生異常");
                        service.oht13_error.PlaySync();
                        break;
                    case "14":
                        _logger.LogInformation("14號車發生異常");
                        service.oht14_error.PlaySync();
                        break;
                    case "15":
                        _logger.LogInformation("15號車發生異常");
                        service.oht15_error.PlaySync();
                        break;
                    case "16":
                        _logger.LogInformation("16號車發生異常");
                        service.oht16_error.PlaySync();
                        break;
                    case "17":
                        _logger.LogInformation("17號車發生異常");
                        service.oht17_error.PlaySync();
                        break;
                    case "18":
                        _logger.LogInformation("18號車發生異常");
                        service.oht18_error.PlaySync();
                        break;
                    case "19":
                        _logger.LogInformation("19號車發生異常");
                        service.oht19_error.PlaySync();
                        break;
                    case "20":
                        _logger.LogInformation("20號車發生異常");
                        service.oht20_error.PlaySync();
                        break;
                    case "21":
                        _logger.LogInformation("21號車發生異常");
                        service.oht21_error.PlaySync();
                        break;
                    case "22":
                        _logger.LogInformation("22號車發生異常");
                        service.oht22_error.PlaySync();
                        break;
                    case "23":
                        _logger.LogInformation("23號車發生異常");
                        service.oht23_error.PlaySync();
                        break;
                    case "24":
                        _logger.LogInformation("24號車發生異常");
                        service.oht24_error.PlaySync();
                        break;
                    case "25":
                        _logger.LogInformation("25號車發生異常");
                        service.oht25_error.PlaySync();
                        break;
                    case "26":
                        _logger.LogInformation("26號車發生異常");
                        service.oht26_error.PlaySync();
                        break;
                    case "27":
                        _logger.LogInformation("27號車發生異常");
                        service.oht27_error.PlaySync();
                        break;
                    case "28":
                        _logger.LogInformation("28號車發生異常");
                        service.oht28_error.PlaySync();
                        break;
                    case "29":
                        _logger.LogInformation("29號車發生異常");
                        service.oht29_error.PlaySync();
                        break;
                    case "30":
                        _logger.LogInformation("30號車發生異常");
                        service.oht30_error.PlaySync();
                        break;
                    case "31":
                        _logger.LogInformation("31號車發生異常");
                        service.oht31_error.PlaySync();
                        break;
                    case "32":
                        _logger.LogInformation("32號車發生異常");
                        service.oht32_error.PlaySync();
                        break;
                    case "33":
                        _logger.LogInformation("33號車發生異常");
                        service.oht33_error.PlaySync();
                        break;
                    case "34":
                        _logger.LogInformation("34號車發生異常");
                        service.oht34_error.PlaySync();
                        break;
                    case "35":
                        _logger.LogInformation("35號車發生異常");
                        service.oht35_error.PlaySync();
                        break;
                    case "36":
                        _logger.LogInformation("36號車發生異常");
                        service.oht36_error.PlaySync();
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
                    case "11":
                        _logger.LogInformation("11號車發生斷線");
                        service.oht11_disconnection.PlaySync();
                        break;
                    case "12":
                        _logger.LogInformation("12號車發生斷線");
                        service.oht12_disconnection.PlaySync();
                        break;
                    case "13":
                        _logger.LogInformation("13號車發生斷線");
                        service.oht13_disconnection.PlaySync();
                        break;
                    case "14":
                        _logger.LogInformation("14號車發生斷線");
                        service.oht14_disconnection.PlaySync();
                        break;
                    case "15":
                        _logger.LogInformation("15號車發生斷線");
                        service.oht15_disconnection.PlaySync();
                        break;
                    case "16":
                        _logger.LogInformation("16號車發生斷線");
                        service.oht16_disconnection.PlaySync();
                        break;
                    case "17":
                        _logger.LogInformation("17號車發生斷線");
                        service.oht17_disconnection.PlaySync();
                        break;
                    case "18":
                        _logger.LogInformation("18號車發生斷線");
                        service.oht18_disconnection.PlaySync();
                        break;
                    case "19":
                        _logger.LogInformation("19號車發生斷線");
                        service.oht19_disconnection.PlaySync();
                        break;
                    case "20":
                        _logger.LogInformation("20號車發生斷線");
                        service.oht20_disconnection.PlaySync();
                        break;
                    case "21":
                        _logger.LogInformation("21號車發生斷線");
                        service.oht21_disconnection.PlaySync();
                        break;
                    case "22":
                        _logger.LogInformation("22號車發生斷線");
                        service.oht22_disconnection.PlaySync();
                        break;
                    case "23":
                        _logger.LogInformation("23號車發生斷線");
                        service.oht23_disconnection.PlaySync();
                        break;
                    case "24":
                        _logger.LogInformation("24號車發生斷線");
                        service.oht24_disconnection.PlaySync();
                        break;
                    case "25":
                        _logger.LogInformation("25號車發生斷線");
                        service.oht25_disconnection.PlaySync();
                        break;
                    case "26":
                        _logger.LogInformation("26號車發生斷線");
                        service.oht26_disconnection.PlaySync();
                        break;
                    case "27":
                        _logger.LogInformation("27號車發生斷線");
                        service.oht27_disconnection.PlaySync();
                        break;
                    case "28":
                        _logger.LogInformation("28號車發生斷線");
                        service.oht28_disconnection.PlaySync();
                        break;
                    case "29":
                        _logger.LogInformation("29號車發生斷線");
                        service.oht29_disconnection.PlaySync();
                        break;
                    case "30":
                        _logger.LogInformation("30號車發生斷線");
                        service.oht30_disconnection.PlaySync();
                        break;
                    case "31":
                        _logger.LogInformation("31號車發生斷線");
                        service.oht31_disconnection.PlaySync();
                        break;
                    case "32":
                        _logger.LogInformation("32號車發生斷線");
                        service.oht32_disconnection.PlaySync();
                        break;
                    case "33":
                        _logger.LogInformation("33號車發生斷線");
                        service.oht33_disconnection.PlaySync();
                        break;
                    case "34":
                        _logger.LogInformation("34號車發生斷線");
                        service.oht34_disconnection.PlaySync();
                        break;
                    case "35":
                        _logger.LogInformation("35號車發生斷線");
                        service.oht35_disconnection.PlaySync();
                        break;
                    case "36":
                        _logger.LogInformation("36號車發生斷線");
                        service.oht36_disconnection.PlaySync();
                        break;
                }
            }

            return "ok";
        }
        private long NoActionSyncPoint = 0;
        [HttpGet("VhHasCmdNoAction/{id}")]
        public String VhHasCmdNoAction(string id, [FromServices] WeatherForecast service, [FromServices] SerialPortService serialPort)

        {

            if (System.Threading.Interlocked.Exchange(ref NoActionSyncPoint, 1) == 0)
            {
                try
                {

                    _logger.LogInformation("天車無法移動");
                    service.vhHasCmdNoAction.PlaySync();
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref NoActionSyncPoint, 0);
                }
            }

            return "ok";
        }

    }
}
