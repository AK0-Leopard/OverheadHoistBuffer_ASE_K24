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
        public SoundPlayer oht18_error;
        public SoundPlayer oht19_error;
        public SoundPlayer oht20_error;
        public SoundPlayer oht21_error;
        public SoundPlayer oht22_error;
        public SoundPlayer oht23_error;
        public SoundPlayer oht24_error;
        public SoundPlayer oht25_error;
        public SoundPlayer oht26_error;
        public SoundPlayer oht27_error;
        public SoundPlayer oht28_error;

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
        public SoundPlayer oht18_disconnection;
        public SoundPlayer oht19_disconnection;
        public SoundPlayer oht20_disconnection;
        public SoundPlayer oht21_disconnection;
        public SoundPlayer oht22_disconnection;
        public SoundPlayer oht23_disconnection;
        public SoundPlayer oht24_disconnection;
        public SoundPlayer oht25_disconnection;
        public SoundPlayer oht26_disconnection;

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
            oht18_error = new SoundPlayer($"{executablePath}alarm\\18.wav");
            oht19_error = new SoundPlayer($"{executablePath}alarm\\19.wav");
            oht20_error = new SoundPlayer($"{executablePath}alarm\\20.wav");
            oht21_error = new SoundPlayer($"{executablePath}alarm\\21.wav");
            oht22_error = new SoundPlayer($"{executablePath}alarm\\22.wav");
            oht23_error = new SoundPlayer($"{executablePath}alarm\\23.wav");
            oht24_error = new SoundPlayer($"{executablePath}alarm\\24.wav");
            oht25_error = new SoundPlayer($"{executablePath}alarm\\25.wav");
            oht26_error = new SoundPlayer($"{executablePath}alarm\\26.wav");
            oht27_error = new SoundPlayer($"{executablePath}alarm\\27.wav");
            oht28_error = new SoundPlayer($"{executablePath}alarm\\28.wav");

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
            oht18_disconnection = new SoundPlayer($"{executablePath}disconnection\\18.wav");
            oht19_disconnection = new SoundPlayer($"{executablePath}disconnection\\19.wav");
            oht20_disconnection = new SoundPlayer($"{executablePath}disconnection\\20.wav");
            oht21_disconnection = new SoundPlayer($"{executablePath}disconnection\\21.wav");
            oht22_disconnection = new SoundPlayer($"{executablePath}disconnection\\22.wav");
            oht23_disconnection = new SoundPlayer($"{executablePath}disconnection\\23.wav");
            oht24_disconnection = new SoundPlayer($"{executablePath}disconnection\\24.wav");
            oht25_disconnection = new SoundPlayer($"{executablePath}disconnection\\25.wav");
            oht26_disconnection = new SoundPlayer($"{executablePath}disconnection\\26.wav");

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
            oht18_error.LoadAsync();
            oht19_error.LoadAsync();
            oht20_error.LoadAsync();
            oht21_error.LoadAsync();
            oht22_error.LoadAsync();
            oht23_error.LoadAsync();
            oht24_error.LoadAsync();
            oht25_error.LoadAsync();
            oht26_error.LoadAsync();
            oht27_error.LoadAsync();
            oht28_error.LoadAsync();

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
            oht18_disconnection.LoadAsync();
            oht19_disconnection.LoadAsync();
            oht20_disconnection.LoadAsync();
            oht21_disconnection.LoadAsync();
            oht22_disconnection.LoadAsync();
            oht23_disconnection.LoadAsync();
            oht24_disconnection.LoadAsync();
            oht25_disconnection.LoadAsync();
            oht26_disconnection.LoadAsync();

            vhHasCmdNoAction.LoadAsync();

        }
    }
}
