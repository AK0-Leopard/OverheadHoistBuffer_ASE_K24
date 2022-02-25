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
using VehicleControl_Viewer.Protots;
using VehicleControl_Viewer.Vo;

namespace VehicleControl_Viewer.UI.Components
{
    /// <summary>
    /// TrackInfo.xaml 的互動邏輯
    /// </summary>
    public partial class TrackInfo : UserControl
    {
        SolidColorBrush alarmSignal = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        SolidColorBrush blockSignal = new SolidColorBrush(Color.FromRgb(0, 0, 255));
        SolidColorBrush normalSignal = new SolidColorBrush(Color.FromRgb(0, 255, 0));
        public Track track { get; private set; }
        public TrackInfo()
        {
            InitializeComponent();
        }
        public void setTrack(Track _track)
        {
            track = _track;
            this.ShowName.Text = _track.id;
            this.Margin = new Thickness(_track.Position_X, _track.Position_Y, 0, 0);
            
        }

        public void refreshTrack()
        {
            if (track.status != Track.TrackStatus.TrackStatus_Auto)
                this.ShowSignal.Fill = alarmSignal;
            else if(track.block != Track.TrackBlock.TrackBlock_NonBlock)
                this.ShowSignal.Fill = blockSignal;
            else
                this.ShowSignal.Fill = normalSignal;

            this.ShowInfo.Text = "";
            this.ShowInfo.Text += "ID:" + track.id + "\n";
            this.ShowInfo.Text += "Status:" + track.status.ToString() + "\n";
            this.ShowInfo.Text += "Block:" + track.block.ToString() + "\n";
            this.ShowInfo.Text += "Alarm code:" + track.alarmCode.ToString() + "\n";
            this.ShowInfo.Text += "Dir:" + track.dir.ToString() + "\n";
        }

        private void ShowPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowPopup.IsOpen = true;
        }

        private void ShowPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            ShowPopup.IsOpen = false;
        }
    }
}
