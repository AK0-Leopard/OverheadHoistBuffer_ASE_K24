//*********************************************************************************
//      frm_TipMessage.cs
//*********************************************************************************
// File Name: uc_ConnectionControl.cs
// Description: 連線控制
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date                      Author                  Request No.        Tag                        Description
// ---------------     ---------------     ---------------     ---------------     ------------------------------
// 2020/10/07         Boan Chen           N/A                       N/A                       Initial。
//*********************************************************************************

using NLog;
using System;
using System.Windows;


namespace VehicleControl_Viewer.frm_Help
{


    /// <summary>
    /// frm_TipMessage.xaml 的互動邏輯
    /// </summary>
    public partial class frm_TipMessage_YesNo : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public bool bResult { get; set; }


        public frm_TipMessage_YesNo()
        {
            try
            {
                InitializeComponent();
                btn_Yes.Focus();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        public frm_TipMessage_YesNo(string msg)
        {
            try
            {
                InitializeComponent();
                tbk_Message.Text = msg;
                btn_Yes.Focus();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private void btn_Yes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.bResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private void btn_No_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.bResult = false;
                this.Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private void DragMoveFrom_FromContent(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        public void setMessage(string msg)
        {
            try
            {
                tbk_Message.Text = msg;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

    }
}
