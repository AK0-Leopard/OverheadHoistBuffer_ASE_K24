using System;
using System.IO.Ports;
using System.Media;


namespace WebApplication1
{
    public class WeatherForecast
    {
        public SoundPlayer oht1_error;
        public SoundPlayer oht2_error;
        public SoundPlayer oht3_error;
        public SoundPlayer oht5_error;
        public SoundPlayer oht6_error;
        public SoundPlayer oht7_error;
        public SoundPlayer oht8_error;
        public SoundPlayer oht9_error;
        public SoundPlayer oht10_error;

        public SoundPlayer oht1_disconnection;
        public SoundPlayer oht2_disconnection;
        public SoundPlayer oht3_disconnection;
        public SoundPlayer oht5_disconnection;
        public SoundPlayer oht6_disconnection;
        public SoundPlayer oht7_disconnection;
        public SoundPlayer oht8_disconnection;
        public SoundPlayer oht9_disconnection;
        public SoundPlayer oht10_disconnection;

        public SoundPlayer vhHasCmdNoAction;


        public string passReportVh = "";




        public WeatherForecast(string passReporVh)
        {
            this.passReportVh = passReporVh;
            var executablePath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            oht1_error = new SoundPlayer($"{executablePath}alarm\\1.wav");
            oht2_error = new SoundPlayer($"{executablePath}alarm\\2.wav");
            oht3_error = new SoundPlayer($"{executablePath}alarm\\3.wav");
            oht5_error = new SoundPlayer($"{executablePath}alarm\\5.wav");
            oht6_error = new SoundPlayer($"{executablePath}alarm\\6.wav");
            oht7_error = new SoundPlayer($"{executablePath}alarm\\7.wav");
            oht8_error = new SoundPlayer($"{executablePath}alarm\\8.wav");
            oht9_error = new SoundPlayer($"{executablePath}alarm\\9.wav");
            oht10_error = new SoundPlayer($"{executablePath}alarm\\10.wav");

            oht1_disconnection = new SoundPlayer($"{executablePath}disconnection\\1.wav");
            oht2_disconnection = new SoundPlayer($"{executablePath}disconnection\\2.wav");
            oht3_disconnection = new SoundPlayer($"{executablePath}disconnection\\3.wav");
            oht5_disconnection = new SoundPlayer($"{executablePath}disconnection\\5.wav");
            oht6_disconnection = new SoundPlayer($"{executablePath}disconnection\\6.wav");
            oht7_disconnection = new SoundPlayer($"{executablePath}disconnection\\7.wav");
            oht8_disconnection = new SoundPlayer($"{executablePath}disconnection\\8.wav");
            oht9_disconnection = new SoundPlayer($"{executablePath}disconnection\\9.wav");
            oht10_disconnection = new SoundPlayer($"{executablePath}disconnection\\10.wav");

            vhHasCmdNoAction = new SoundPlayer($"{executablePath}vh\\OHT_Not_Move.wav");



            oht1_error.LoadAsync();
            oht2_error.LoadAsync();
            oht3_error.LoadAsync();
            oht5_error.LoadAsync();
            oht6_error.LoadAsync();
            oht7_error.LoadAsync();
            oht8_error.LoadAsync();
            oht9_error.LoadAsync();
            oht10_error.LoadAsync();

            oht1_disconnection.LoadAsync();
            oht2_disconnection.LoadAsync();
            oht3_disconnection.LoadAsync();
            oht5_disconnection.LoadAsync();
            oht6_disconnection.LoadAsync();
            oht7_disconnection.LoadAsync();
            oht8_disconnection.LoadAsync();
            oht9_disconnection.LoadAsync();
            oht10_disconnection.LoadAsync();

            vhHasCmdNoAction.LoadAsync();

        }
    }
}
