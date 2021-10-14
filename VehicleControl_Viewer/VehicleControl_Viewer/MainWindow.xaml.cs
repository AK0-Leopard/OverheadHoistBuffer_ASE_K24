using VehicleControl_Viewer.frm_Mainfrom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VehicleControl_Viewer.App;
using VehicleControl_Viewer;

namespace VehicleControl_Vierwer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        uc_VehicleMainform uc_VehicleMainform = null;
        uc_MainSignal uc_MainSignal = null;
        frm_GuideTest frm_guideTest = null;


        WindownApplication app = null;
        public MainWindow()
        {
            InitializeComponent();
            app = WindownApplication.getInstance();
            app.Start();
            uc_VehicleMainform = new uc_VehicleMainform(app);
            placeToShowChildForm.Children.Add(uc_VehicleMainform);

            uc_MainSignal = new uc_MainSignal(app);
            ucMainSignal.Children.Add(uc_MainSignal);

        }

        private void FormLoad(object sender, RoutedEventArgs e)
        {
            
        }

        private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Form_Close(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void mi_GuideTest_Click(object sender, RoutedEventArgs e)
        {
            if (frm_guideTest == null)
            {
                frm_guideTest = new frm_GuideTest(app.VehicleControlService);
                frm_guideTest.Show();
                frm_guideTest.GuideSearchComplete += frm_guideTest_GuideSearchComplete;

            }
            else
            {
                frm_guideTest.Show();
            }
        }
        private void frm_guideTest_GuideSearchComplete(object sender, IEnumerable<string> e)
        {
            uc_VehicleMainform.setTestGideRail(e);
        }
    }
}
