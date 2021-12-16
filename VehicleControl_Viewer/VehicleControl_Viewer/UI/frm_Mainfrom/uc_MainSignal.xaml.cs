
using com.mirle.AK0.ProtocolFormat;
using GenericParsing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using VehicleControl_Viewer.App;
using VehicleControl_Viewer.Data.Interface;
using VehicleControl_Viewer.frm_Help;
using VehicleControl_Viewer.UI.Components;
using VehicleControl_Viewer.Vo;
using VehicleControl_Viewer.Vo.ValueObjToShow;
namespace VehicleControl_Viewer.frm_Mainfrom
{
    /// <summary>
    /// uc_MainSignal.xaml 的互動邏輯
    /// </summary>
    public partial class uc_MainSignal : UserControl
    {
        #region 全域變數
        private static System.Windows.Threading.DispatcherTimer positionUpdat3Timer;
        #endregion 全域變數

        //建構子
        List<TextBlock> OHB_ZoneList= new List<TextBlock>();
        private readonly WindownApplication app;

        List<TextBlock> zoneData = new List<TextBlock>();
        public uc_MainSignal(WindownApplication _app)
        {
            app = _app;
            initialTimer();
            InitializeComponent();
            setFakeZoneData(); //為了表演所以讓ZONE DATA秀假資料
        }


        private void Home_Click(object sender, RoutedEventArgs e)
        {
            //this.CIMLinkStatus.Fill = new SolidColorBrush(Colors.Green);
            //this.MGV_PLC.Fill = new SolidColorBrush(Colors.Green);
            //this.RAIL_PLC1.Fill = new SolidColorBrush(Colors.Green);
            //this.RAIL_PLC2.Fill = new SolidColorBrush(Colors.Green);
            //this.OHB_Zone01_value.Foreground = new SolidColorBrush(Colors.Green);
        }
        private void WIPView_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Startup_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Loginout_Click(object sender, RoutedEventArgs e)
        {
        }

        public void SetCIMConnStatus(bool CIMConnectionStatus)
        {
        }

        //更新CIM連線狀態(1: 連線/0: 斷線)
        public void SetMPLCConnStatus(bool CIMConnectionStatus)
        {
        }


        public void SetLineID(string lineID)
        {
        }

        private void initialTimer()
        {
            positionUpdat3Timer = new System.Windows.Threading.DispatcherTimer();
            positionUpdat3Timer.Tick += new EventHandler(timeCycle);
            positionUpdat3Timer.Interval = new TimeSpan(0, 0, 0, 1);
            positionUpdat3Timer.Start();
        }
        public void timeCycle(object sender, EventArgs e)
        {
            this.CIMLinkStatus.Fill = (app.objCacheManager.LineInfo.IsConnectionWithHOST) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            this.MGV_PLC.Fill = (app.objCacheManager.LineInfo.IsConnectionWithPLCMANUAL) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            this.RAIL_PLC1.Fill = (app.objCacheManager.LineInfo.IsConnectionWithPLCTRACK1) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
            this.RAIL_PLC2.Fill = (app.objCacheManager.LineInfo.IsConnectionWithPLCTRACK2) ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }

        private void setFakeZoneData()
        {
            OHB_Zone01_value.Text = "95%";
            OHB_Zone02_value.Text = "42%";
            OHB_Zone03_value.Text = "-";
            OHB_Zone04_value.Text = "-";
            OHB_Zone05_value.Text = "47%";
            OHB_Zone06_value.Text = "-";
            OHB_Zone07_value.Text = "0%";
            OHB_Zone08_value.Text = "16%";
            OHB_Zone09_value.Text = "-";
            OHB_Zone10_value.Text = "-";
            OHB_Zone11_value.Text = "-";
            OHB_Zone12_value.Text = "0%";
            OHB_Zone13_value.Text = "0%";
            OHB_Zone14_value.Text = "-";
            OHB_Zone15_value.Text = "-";
            OHB_Zone16_value.Text = "-";
            OHB_Zone17_value.Text = "-";
            OHB_Zone18_value.Text = "4%";
            OHB_Zone19_value.Text = "18%";
            OHB_Zone20_value.Text = "-";
            OHB_Zone21_value.Text = "1%";
            OHB_Zone22_value.Text = "-";
            OHB_Zone23_value.Text = "0%";
            OHB_Zone24_value.Text = "0%";
        }

    }
}
