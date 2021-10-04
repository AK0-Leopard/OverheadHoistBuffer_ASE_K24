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
        public uc_MainSignal()
        {
            InitializeComponent();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
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
