using com.mirle.ibg3k0.sc.Data.VO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class TrackMaintenanceForm : Form
    {
        public BCMainForm MainForm { get; }
        public App.BCApplication BCApp;
        List<Track> tracks = null;
        List<Track> showTracks = null;
        public TrackMaintenanceForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            BCApp = _mainForm.BCApp;
            dgv_trackData.AutoGenerateColumns = false;
            MainForm = _mainForm;
            initialTrackData();
        }

        private void initialTrackData()
        {
            var t = BCApp.SCApplication.UnitBLL.cache.GetALLTracks();
            tracks = t;
            showTracks = tracks;
            dgv_trackData.DataSource = showTracks;
        }

        const int CELL_INDEX_SHELFID = 0;
        private void TrackMaintenanceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
            MainForm.removeForm(nameof(TrackMaintenanceForm));
        }

        const int TRACK_DATA_TRACKSTATUS = 2;
        const int TRACK_DATA_ALARM = 3;
        private void dgv_shelfData_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            //if (dgv_trackData.Rows.Count <= e.RowIndex) return;
            //if (e.RowIndex < 0) return;
            //var track_status = dgv_trackData.Rows[e.RowIndex].Cells[TRACK_DATA_TRACKSTATUS].Value;
            //var alarm_code = dgv_trackData.Rows[e.RowIndex].Cells[TRACK_DATA_ALARM].Value;
            //if (!(track_status is string))
            //    return;
            //if (!(alarm_code is string))
            //    return;
            //string status = track_status as string;
            //string alarm = track_status as string;
            //if (sc.Common.SCUtility.isMatche(status, sc.App.SCAppConstants.YES_FLAG))
            //{
            //    //not thing...
            //}
            //else
            //{
            //    DataGridViewRow row = dgv_trackData.Rows[e.RowIndex];
            //    row.DefaultCellStyle.BackColor = Color.GreenYellow;
            //    row.DefaultCellStyle.ForeColor = Color.Black;
            //    if (row.Selected)
            //    {
            //        row.DefaultCellStyle.SelectionBackColor = Color.SkyBlue;
            //        row.DefaultCellStyle.SelectionForeColor = Color.Black;
            //    }
            //}
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dgv_trackData.Refresh();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            refreshDataGridView();
        }
        private void refreshDataGridView()
        {
            string track = txt_tracks.Text;
            if (sc.Common.SCUtility.isEmpty(track))
            {
                showTracks = tracks;
            }
            else
            {
                List<string> tracks_id = new List<string>();
                if (track.Contains(","))
                {
                    tracks_id = track.Split(',').ToList();
                }
                else
                {
                    tracks_id.Add(track);
                }

                showTracks = tracks.Where(t => tracks_id.Contains(t.UNIT_ID)).ToList();
            }
            dgv_trackData.DataSource = showTracks;
            dgv_trackData.Refresh();

        }

        private void TrackMaintenanceForm_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }


    }
}
