using NLog;
using System;
using System.Windows;


namespace VehicleControl_Viewer.frm_Help
{


    /// <summary>
    /// frm_TipMessage.xaml 的互動邏輯
    /// </summary>
    public partial class frm_TipMessage_OK : Window
    {

        private static Logger logger = LogManager.GetCurrentClassLogger();


        public frm_TipMessage_OK()
        {
            InitializeComponent();
            btn_Yes.Focus();
        }

        public frm_TipMessage_OK(string msg)
        {
            InitializeComponent();
            tbk_Message.Text = msg;
            btn_Yes.Focus();
        }

        public void setMessage(string msg)
        {
            tbk_Message.Text = msg;
        }

        private void btn_Yes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btn_No_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
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
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }
}
