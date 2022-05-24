using com.mirle.ibg3k0.bc.winform.App;
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
            //Bay
            List<string> bay_ids = new List<string>();
            bay_ids.Add("");
            List<string> current_bay_ids = shelfDefs.Select(s => s.BayID).Distinct().OrderBy(s => s).ToList();
            bay_ids.AddRange(current_bay_ids);
            cmb_bay_id.DataSource = bay_ids;

            //Zone
            List<string> zone_ids = new List<string>();
            zone_ids.Add("");
            List<string> current_zone_ids = shelfDefs.Select(s => s.ZoneID).Distinct().OrderBy(s => s).ToList();
            zone_ids.AddRange(current_zone_ids);
            cmb_zoneID.DataSource = zone_ids;

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
            timer1.Stop();
            MainForm.removeForm(nameof(ShelfMaintenanceForm));
        }

        private void cmb_bay_id_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshDataGridView();
        }

        private void refreshDataGridView()
        {
            string selected_bay_id = cmb_bay_id.Text;
            string selected_zone_id = cmb_zoneID.Text;
            string selected_shelf_id = txt_shelfID.Text;
            showShelfDefs = shelfDefs;
            if (!sc.Common.SCUtility.isEmpty(selected_shelf_id))
            {
                //showShelfDefs = showShelfDefs.Where(s => shelf_ids.Contains(s.ShelfID)).ToList();
                showShelfDefs = showShelfDefs.Where(s => searchShelfForFuzzy(s.ShelfID, selected_shelf_id)).ToList();
            }
            else
            {
                if (!sc.Common.SCUtility.isEmpty(selected_bay_id))
                {
                    showShelfDefs = showShelfDefs.Where(s => sc.Common.SCUtility.isMatche(s.BayID, selected_bay_id)).ToList();
                }
                if (!sc.Common.SCUtility.isEmpty(selected_zone_id))
                {
                    showShelfDefs = showShelfDefs.Where(s => sc.Common.SCUtility.isMatche(s.ZoneID, selected_zone_id)).ToList();
                }
            }
            //if (sc.Common.SCUtility.isEmpty(selected_bay_id))
            //{
            //    showShelfDefs = shelfDefs;
            //}
            //else
            //{
            //    var shelfs_temp = shelfDefs.Where(s => sc.Common.SCUtility.isMatche(s.BayID, selected_bay_id)).ToList();
            //    showShelfDefs = shelfs_temp;
            //}
            dgv_shelfData.DataSource = showShelfDefs;
            dgv_shelfData.Refresh();
        }
        private bool searchShelfForFuzzy(string shelfID, string searchShelfIDs)
        {
            List<string> shelf_ids = new List<string>();
            if (searchShelfIDs.Contains(","))
            {
                shelf_ids = searchShelfIDs.Split(',').ToList();
            }
            else
            {
                shelf_ids.Add(searchShelfIDs);
            }
            foreach (string s in shelf_ids)
            {
                if (shelfID.Contains(s))
                {
                    return true;
                }
            }
            return false;
        }

        const int SHELF_ENABLE_DISABLE_CLOUMN_INDEX_ENABLE = 2;
        private void dgv_shelfData_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dgv_shelfData.Rows.Count <= e.RowIndex) return;
            if (e.RowIndex < 0) return;
            var enable_status = dgv_shelfData.Rows[e.RowIndex].Cells[SHELF_ENABLE_DISABLE_CLOUMN_INDEX_ENABLE].Value;
            if (!(enable_status is string))
                return;
            string enable = enable_status as string;
            DataGridViewRow row = dgv_shelfData.Rows[e.RowIndex];
            if (sc.Common.SCUtility.isMatche(enable, sc.App.SCAppConstants.YES_FLAG))
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.Yellow;
                row.DefaultCellStyle.ForeColor = Color.Red;
                if (row.Selected)
                {
                    row.DefaultCellStyle.SelectionBackColor = Color.SkyBlue;
                    row.DefaultCellStyle.SelectionForeColor = Color.Red;
                }
            }
        }

        private void cmb_zoneID_SelectedIndexChanged(object sender, EventArgs e)
        {
            refreshDataGridView();
        }

        private void txt_shelfID_TextChanged(object sender, EventArgs e)
        {
            refreshDataGridView();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            dgv_shelfData.Refresh();
        }

        private void ShelfMaintenanceForm_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private async void btn_EnableAll_Click(object sender, EventArgs e)
        {
            try
            {
                tableLayoutPanel1.Enabled = false;
                string zone_id = sc.Common.SCUtility.Trim(cmb_zoneID.Text, true);
                if (sc.Common.SCUtility.isEmpty(zone_id))
                {
                    MessageBox.Show("請選擇要開啟的Zone ID.");
                    return;
                }

                string message = $"確認是否要開啟 [{zone_id}] 所有的儲位?";
                DialogResult confirmResult = MessageBox.Show(this, message,
                    BCApplication.getMessageString("CONFIRM"), MessageBoxButtons.YesNo);
                if (confirmResult != DialogResult.Yes)
                {
                    return;
                }
                string result = await Task.Run(() => doBatchEnableDisableShelf(zone_id, true));
                UpdateShelfData();
                MessageBox.Show($"開啟 [{zone_id}] 所有的儲位,結果:{result}");
            }
            catch (Exception ex)
            {

            }
            finally
            {
                tableLayoutPanel1.Enabled = true;
            }
        }

        private string doBatchEnableDisableShelf(string zoneID, bool isEnable)
        {
            try
            {
                bool is_success = BCApp.SCApplication.TransferService.Manual_ShelfEnableByZone(zoneID, isEnable, "UI");

                return is_success ? "成功" : "失敗";
            }
            catch (Exception ex)
            {
                return $"例外發生";
            }
        }

        private async void btn_DisableAll_Click(object sender, EventArgs e)
        {
            try
            {
                tableLayoutPanel1.Enabled = false;
                string zone_id = sc.Common.SCUtility.Trim(cmb_zoneID.Text, true);
                if (sc.Common.SCUtility.isEmpty(zone_id))
                {
                    MessageBox.Show("請選擇要關閉的Zone ID.");
                    return;
                }

                string message = $"確認是否要關閉 [{zone_id}] 所有的儲位?";
                DialogResult confirmResult = MessageBox.Show(this, message,
                    BCApplication.getMessageString("CONFIRM"), MessageBoxButtons.YesNo);
                if (confirmResult != DialogResult.Yes)
                {
                    return;
                }
                string result = await Task.Run(() => doBatchEnableDisableShelf(zone_id, false));
                UpdateShelfData();
                MessageBox.Show($"關閉 [{zone_id}] 所有的儲位,結果:{result}");
            }
            catch (Exception ex)
            {

            }
            finally
            {
                tableLayoutPanel1.Enabled = true;
            }
        }
    }
}
