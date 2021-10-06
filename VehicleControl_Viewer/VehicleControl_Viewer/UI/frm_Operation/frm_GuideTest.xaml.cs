//*********************************************************************************
//      frm_Login.cs
//*********************************************************************************
// File Name: frm_Login.cs
// Description: 用戶登入
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date                      Author                  Request No.        Tag                        Description
// ---------------     ---------------     ---------------     ---------------     ------------------------------
// 2020/11/02         Boan Chen           N/A                       N/A                       Initial。
//*********************************************************************************

using com.mirle.AK0.ProtocolFormat;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using VehicleControl_Viewer.Data;

namespace VehicleControl_Viewer
{
    public partial class frm_GuideTest : Window
    {
        #region 全域變數
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private VehicleControlService VehicleControlService { get; }
        #endregion 全域變數
        #region Public
        public EventHandler<IEnumerable<string>> GuideSearchComplete;
        #endregion Public


        #region 建構子
        public frm_GuideTest(VehicleControlService vehicleControlService)
        {
            InitializeComponent();
            VehicleControlService = vehicleControlService;
        }

        #endregion 建構子

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            var search_info = new SearchInfo()
            {
                StartAdr = txb_startAdr.Text,
                EndAdr = txb_endAdr.Text
            };
            var guide_info = await VehicleControlService.RequestGuideInfo(search_info);
            if (guide_info == null) return;
            GuideSearchComplete?.Invoke(this, guide_info.SecIds);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            GuideSearchComplete = null;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        #region 介面移動事件
        private void DragMoveFrom_FromContent(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }
        #endregion 介面移動事件

    }
}
