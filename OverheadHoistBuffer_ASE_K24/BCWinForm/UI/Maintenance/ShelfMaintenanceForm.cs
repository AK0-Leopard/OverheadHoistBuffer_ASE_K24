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
    public partial class ShelfMaintenanceForm : Form
    {
        public BCMainForm MainForm { get; }
        public App.BCApplication BCApp;
        List<sc.ShelfDef> shelfDefs = null;
        List<sc.ShelfDef> showShelfDefs = null;
        public ShelfMaintenanceForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            BCApp = _mainForm.BCApp;
            dgv_shelfData.AutoGenerateColumns = false;
            initialShelfData();
            UpdateShelfData();
            MainForm = _mainForm;
        }

        private void initialComboBox()
        {
            List<string> bay_ids = new List<string>();
            bay_ids.Add("");
            List<string> current_bay_ids = shelfDefs.Select(s => s.BayID).Distinct().OrderBy(s => s).ToList();
            bay_ids.AddRange(current_bay_ids);
            cmb_bay_id.DataSource = bay_ids;
        }

        private async void initialShelfData()
        {
            var shelfs = await Task.Run(() => BCApp.SCApplication.ShelfDefBLL.LoadShelf());
            shelfDefs = shelfs.OrderBy(s => s.ZoneID).ThenBy(s => s.SeqNo).ToList();
            showShelfDefs = shelfDefs;
            dgv_shelfData.DataSource = showShelfDefs;

            initialComboBox();
        }

        private async void UpdateShelfData()
        {
            try
            {
                var shelfs = await Task.Run(() => BCApp.SCApplication.ShelfDefBLL.LoadShelf());
                shelfs.ForEach(shelf => setNewData(shelf));
                dgv_shelfData.Refresh();
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "Exception");
            }
        }

        void setNewData(sc.ShelfDef shelf)
        {
            if (shelf == null) return;
            var source_shelf_data = shelfDefs.Where(s => sc.Common.SCUtility.isMatche(s.ShelfID, shelf.ShelfID)).FirstOrDefault();
            if (source_shelf_data == null) return;
            source_shelf_data.put(shelf);
        }

        const int CELL_INDEX_SHELFID = 0;


        private async void btn_enable_Click(object sender, EventArgs e)
        {
            var selected_rows = dgv_shelfData.SelectedRows;
            if (selected_rows.Count <= 0)
            {
                MessageBox.Show("Please select want to disable shelf.");
                return;
            }
            var first_selected_row = selected_rows[0];
            string shelf_id = first_selected_row.Cells[CELL_INDEX_SHELFID].Value.ToString();
            string result = await Task.Run(() => BCApp.SCApplication.TransferService.Manual_ShelfEnable(shelf_id, true));
            UpdateShelfData();
            MessageBox.Show($"Shelf:{shelf_id} enable {result}");
        }

        private async void btn_disable_Click(object sender, EventArgs e)
        {
            var selected_rows = dgv_shelfData.SelectedRows;
            if (selected_rows.Count <= 0)
            {
                MessageBox.Show("Please select want to enable shelf.");
                return;
            }
            var first_selected_row = selected_rows[0];
            string shelf_id = first_selected_row.Cells[CELL_INDEX_SHELFID].Value.ToString();
            string result = await Task.Run(() => BCApp.SCApplication.TransferService.Manual_ShelfEnable(shelf_id, false));
            UpdateShelfData();
            MessageBox.Show($"Shelf:{shelf_id} disable {result}");

        }

        private void ShelfMaintenanceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MainForm.removeForm(nameof(ShelfMaintenanceForm));
        }

        private void cmb_bay_id_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshDataGridView();
        }

        private void refreshDataGridView()
        {
            string selected_bay_id = cmb_bay_id.Text;
            if (sc.Common.SCUtility.isEmpty(selected_bay_id))
            {
                showShelfDefs = shelfDefs;
            }
            else
            {
                var shelfs_temp = shelfDefs.Where(s => sc.Common.SCUtility.isMatche(s.BayID, selected_bay_id)).ToList();
                showShelfDefs = shelfs_temp;
            }
            dgv_shelfData.DataSource = showShelfDefs;
            dgv_shelfData.Refresh();
        }

        const int SHELF_ENABLE_DISABLE_CLOUMN_INDEX_ENABLE = 1;
        private void dgv_shelfData_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dgv_shelfData.Rows.Count <= e.RowIndex) return;
            if (e.RowIndex < 0) return;
            var enable_status = dgv_shelfData.Rows[e.RowIndex].Cells[SHELF_ENABLE_DISABLE_CLOUMN_INDEX_ENABLE].Value;
            if (!(enable_status is string))
                return;
            string enable = enable_status as string;
            if (sc.Common.SCUtility.isMatche(enable, sc.App.SCAppConstants.YES_FLAG))
            {
                //not thing...
            }
            else
            {
                DataGridViewRow row = dgv_shelfData.Rows[e.RowIndex];
                row.DefaultCellStyle.BackColor = Color.Yellow;
                row.DefaultCellStyle.ForeColor = Color.Red;
                if (row.Selected)
                {
                    row.DefaultCellStyle.SelectionBackColor = Color.SkyBlue;
                    row.DefaultCellStyle.SelectionForeColor = Color.Red;
                }
            }
        }
    }
}
