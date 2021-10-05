using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VehicleControl_Viewer.frm_Mainfrom
{
    /// <summary>
    /// uc_MainSignal.xaml 的互動邏輯
    /// </summary>
    public partial class uc_MainSignal : UserControl
    {
        #region 全域變數
        #endregion 全域變數

        //建構子
        List<TextBlock> OHB_ZoneList= new List<TextBlock>();
        public uc_MainSignal()
        {
            InitializeComponent();
        }


        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.CIMLinkStatus.Fill = new SolidColorBrush(Colors.Green);
            this.MGV_PLC.Fill = new SolidColorBrush(Colors.Green);
            this.RAIL_PLC1.Fill = new SolidColorBrush(Colors.Green);
            this.RAIL_PLC2.Fill = new SolidColorBrush(Colors.Green);
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





    }
}
