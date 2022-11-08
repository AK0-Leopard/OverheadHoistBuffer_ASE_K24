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
        public SoundPlayer oht11_error;
        public SoundPlayer oht12_error;
        public SoundPlayer oht13_error;
        public SoundPlayer oht14_error;
        public SoundPlayer oht15_error;
        public SoundPlayer oht16_error;
        public SoundPlayer oht17_error;

        public SoundPlayer oht1_disconnection;
        public SoundPlayer oht2_disconnection;
        public SoundPlayer oht3_disconnection;
        public SoundPlayer oht5_disconnection;
        public SoundPlayer oht6_disconnection;
        public SoundPlayer oht7_disconnection;
        public SoundPlayer oht8_disconnection;
        public SoundPlayer oht9_disconnection;
        public SoundPlayer oht10_disconnection;
        public SoundPlayer oht11_disconnection;
        public SoundPlayer oht12_disconnection;
        public SoundPlayer oht13_disconnection;
        public SoundPlayer oht14_disconnection;
        public SoundPlayer oht15_disconnection;
        public SoundPlayer oht16_disconnection;
        public SoundPlayer oht17_disconnection;

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
            oht11_error = new SoundPlayer($"{executablePath}alarm\\11.wav");
            oht12_error = new SoundPlayer($"{executablePath}alarm\\12.wav");
            oht13_error = new SoundPlayer($"{executablePath}alarm\\13.wav");
            oht14_error = new SoundPlayer($"{executablePath}alarm\\14.wav");
            oht15_error = new SoundPlayer($"{executablePath}alarm\\15.wav");
            oht16_error = new SoundPlayer($"{executablePath}alarm\\16.wav");
            oht17_error = new SoundPlayer($"{executablePath}alarm\\17.wav");

            oht1_disconnection = new SoundPlayer($"{executablePath}disconnection\\1.wav");
            oht2_disconnection = new SoundPlayer($"{executablePath}disconnection\\2.wav");
            oht3_disconnection = new SoundPlayer($"{executablePath}disconnection\\3.wav");
            oht5_disconnection = new SoundPlayer($"{executablePath}disconnection\\5.wav");
            oht6_disconnection = new SoundPlayer($"{executablePath}disconnection\\6.wav");
            oht7_disconnection = new SoundPlayer($"{executablePath}disconnection\\7.wav");
            oht8_disconnection = new SoundPlayer($"{executablePath}disconnection\\8.wav");
            oht9_disconnection = new SoundPlayer($"{executablePath}disconnection\\9.wav");
            oht10_disconnection = new SoundPlayer($"{executablePath}disconnection\\10.wav");
            oht11_disconnection = new SoundPlayer($"{executablePath}disconnection\\11.wav");
            oht12_disconnection = new SoundPlayer($"{executablePath}disconnection\\12.wav");
            oht13_disconnection = new SoundPlayer($"{executablePath}disconnection\\13.wav");
            oht14_disconnection = new SoundPlayer($"{executablePath}disconnection\\14.wav");
            oht15_disconnection = new SoundPlayer($"{executablePath}disconnection\\15.wav");
            oht15_disconnection = new SoundPlayer($"{executablePath}disconnection\\16.wav");
            oht17_disconnection = new SoundPlayer($"{executablePath}disconnection\\17.wav");

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
            oht11_error.LoadAsync();
            oht12_error.LoadAsync();
            oht13_error.LoadAsync();
            oht14_error.LoadAsync();
            oht15_error.LoadAsync();
            oht16_error.LoadAsync();
            oht17_error.LoadAsync();

            oht1_disconnection.LoadAsync();
            oht2_disconnection.LoadAsync();
            oht3_disconnection.LoadAsync();
            oht5_disconnection.LoadAsync();
            oht6_disconnection.LoadAsync();
            oht7_disconnection.LoadAsync();
            oht8_disconnection.LoadAsync();
            oht9_disconnection.LoadAsync();
            oht10_disconnection.LoadAsync();
            oht11_disconnection.LoadAsync();
            oht12_disconnection.LoadAsync();
            oht13_disconnection.LoadAsync();
            oht14_disconnection.LoadAsync();
            oht15_disconnection.LoadAsync();
            oht15_disconnection.LoadAsync();
            oht17_disconnection.LoadAsync();

            vhHasCmdNoAction.LoadAsync();

        }
    }
}
