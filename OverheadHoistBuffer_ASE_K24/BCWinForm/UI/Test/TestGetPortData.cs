using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.Data.Enum;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.bc.winform
{
    public partial class TestGetPortData : Form
    {
        private App.BCApplication BCApp;
        private ALINE line = null;
        private List<PortDef> portList = null;

        private TransferService transferService = null;
        private DateTime openTime = new DateTime();

        public TestGetPortData()
        {
            InitializeComponent();
        }

        private void TestGetPortData_Load(object sender, EventArgs e)
        {
            transferService = BCApp.SCApplication.TransferService;
            line = BCApp.SCApplication.getEQObjCacheManager().getLine();
            portList = BCApp.SCApplication.PortDefBLL.GetOHB_CVPortData(line.LINE_ID);

            comboBox1.Items.Clear();
            comboBox2.Items.Clear();

            foreach (var v in portList)
            {
                if (transferService.isCVPort(v.PLCPortID) && transferService.isAGVZone(v.PLCPortID) == false)
                {
                    comboBox1.Items.Add(v.PLCPortID);
                }
            }

            comboBox1.SelectedIndex = 0;

            comboBox2.Items.Add("In");
            comboBox2.Items.Add("Out");
            comboBox2.SelectedIndex = 0;

            #region dataGridView2

            dataGridView2.Columns.Add("中文說明", "中文說明");
            dataGridView2.Columns.Add("訊號名稱", "訊號名稱");
            dataGridView2.Columns.Add("狀態", "狀態");

            dataGridView2.Rows.Add("運轉狀態", "RUN", "");  //0
            dataGridView2.Rows.Add("自動模式", "IsAutoMode", "");    //1
            dataGridView2.Rows.Add("異常狀態", "ErrorBit", "");
            dataGridView2.Rows.Add("異常代碼", "ErrorCode", "");
            dataGridView2.Rows.Add("流向", "", "");
            dataGridView2.Rows.Add("是否能切換流向", "IsModeChangable", "");   //5
            dataGridView2.Rows.Add("流向:Port 往 OHT", "IsInputMode", "");
            dataGridView2.Rows.Add("流向:OHT 往 Port", "IsOutputMode", "");
            dataGridView2.Rows.Add("投出入說明", "", "");
            dataGridView2.Rows.Add("Port 是否能搬入 BOX ", "IsReadyToLoad", "");
            dataGridView2.Rows.Add("Port 是否能搬出 BOX ", "IsReadyToUnload", ""); //10
            dataGridView2.Rows.Add("等待說明", "", "");
            dataGridView2.Rows.Add("等待 OHT 搬走", "PortWaitIn", "");
            dataGridView2.Rows.Add("等待從 Port 搬走", "PortWaitOut", "");
            dataGridView2.Rows.Add("狀態說明", "", "");
            dataGridView2.Rows.Add("PLC 離線狀態", "CIM_ON", "");              //15
            dataGridView2.Rows.Add("PLC 預先入料完成", "PreLoadOK", "");
            dataGridView2.Rows.Add("PLC 異常編號", "PLC_AlarmIndex", "");
            dataGridView2.Rows.Add("異常碼", "AlarmCode", "");                 //18

            dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView2.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            #endregion dataGridView2

            #region dataGridView3

            dataGridView3.Columns.Add("中文說明", "中文說明");                 //0
            dataGridView3.Columns.Add("訊號名稱", "訊號名稱");                 //1
            dataGridView3.Columns.Add("狀態", "狀態");                         //2
            dataGridView3.Columns.Add("BOXID", "BOXID");                       //3

            dataGridView3.Rows.Add("帳移除", "Remove", "");                    //0
            dataGridView3.Rows.Add("", "", "");                                //1
            dataGridView3.Rows.Add("盒子 BCR 讀取狀態", "BCRReadDone", "");    //2
            dataGridView3.Rows.Add("盒子ID", "BoxID", "");                     //3
            dataGridView3.Rows.Add("", "", "");                                //4
            dataGridView3.Rows.Add("節數 1 是否有盒子", "LoadPosition1", "");  //5
            dataGridView3.Rows.Add("節數 2 是否有盒子", "LoadPosition2", "");  //6
            dataGridView3.Rows.Add("節數 3 是否有盒子", "LoadPosition3", "");  //7
            dataGridView3.Rows.Add("節數 4 是否有盒子", "LoadPosition4", "");  //8
            dataGridView3.Rows.Add("節數 5 是否有盒子", "LoadPosition5", "");  //9
            dataGridView3.Rows.Add("節數 6 是否有盒子", "LoadPosition6", "");  //10
            dataGridView3.Rows.Add("節數 7 是否有盒子", "LoadPosition7", "");  //11

            dataGridView3.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView3.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            #endregion dataGridView3

            #region dataGridView4

            dataGridView4.Columns.Add("中文說明", "中文說明");                 //0
            dataGridView4.Columns.Add("訊號名稱", "訊號名稱");                 //1
            dataGridView4.Columns.Add("狀態", "狀態");                        //2

            dataGridView4.Rows.Add("BCR 讀取結果", "CarrierIdReadResult");       //0
            dataGridView4.Rows.Add("CST Type", "CarrierType");                  //1
            dataGridView4.Rows.Add("是否開門中", "Door Open");                  //2
            dataGridView4.Rows.Add("心跳", "Heartbeat");                  //2

            dataGridView4.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView4.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            #endregion dataGridView4

            cmb_moveBackReason.DataSource = Enum.GetValues(typeof(MoveBackReasons)).Cast<MoveBackReasons>();

            GetPortData();
            openTime = DateTime.Now;
        }

        public void SetApp(App.BCApplication app)
        {
            BCApp = app;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);

            PortPLCInfo portData = port.getPortPLCInfo();

            #region dataGridView2再Manual Port

            dataGridView2.Rows[0].Cells[2].Value = portData.OpAutoMode.ToString();
            dataGridView2.Rows[1].Cells[2].Value = portData.IsAutoMode.ToString(); //
            dataGridView2.Rows[2].Cells[2].Value = portData.OpError.ToString();
            dataGridView2.Rows[3].Cells[2].Value = portData.ErrorCode.ToString();
            //dataGridView2.Rows[4].Cells[2].Value = 流向說明;
            dataGridView2.Rows[5].Cells[2].Value = portData.IsModeChangable.ToString();
            dataGridView2.Rows[6].Cells[2].Value = portData.IsInputMode.ToString();
            dataGridView2.Rows[7].Cells[2].Value = portData.IsOutputMode.ToString();
            //dataGridView2.Rows[8].Cells[2].Value = 投出入說明;
            dataGridView2.Rows[9].Cells[2].Value = portData.IsReadyToLoad.ToString();
            dataGridView2.Rows[10].Cells[2].Value = portData.IsReadyToUnload.ToString();
            //dataGridView2.Rows[11].Cells[2].Value = 等待說明;
            dataGridView2.Rows[12].Cells[2].Value = portData.PortWaitIn.ToString();
            dataGridView2.Rows[13].Cells[2].Value = portData.PortWaitOut.ToString();
            //dataGridView2.Rows[14].Cells[2].Value = 等待說明;
            dataGridView2.Rows[15].Cells[2].Value = portData.cim_on.ToString();
            dataGridView2.Rows[16].Cells[2].Value = portData.preLoadOK.ToString();

            var manualPort = port as MANUAL_PORTSTATION;
            var manualPortData = manualPort.getManualPortPLCInfo();

            dataGridView2.Rows[17].Cells[2].Value = manualPortData.ErrorIndex.ToString();
            dataGridView2.Rows[18].Cells[2].Value = manualPortData.AlarmCode.ToString();

            #endregion dataGridView2再Manual Port

            #region dataGridView3

            dataGridView3.Rows[0].Cells[2].Value = portData.CstRemoveCheck.ToString();
            //dataGridView3.Rows[1].Cells[2].Value = portData.IsAutoMode.ToString();
            dataGridView3.Rows[2].Cells[2].Value = portData.BCRReadDone.ToString();
            dataGridView3.Rows[3].Cells[2].Value = portData.BoxID;
            //dataGridView3.Rows[4].Cells[2].Value = "";
            dataGridView3.Rows[5].Cells[2].Value = portData.LoadPosition1.ToString();
            dataGridView3.Rows[5].Cells[3].Value = portData.LoadPositionBOX1.ToString();

            dataGridView3.Rows[6].Cells[2].Value = portData.LoadPosition2.ToString();
            dataGridView3.Rows[6].Cells[3].Value = portData.LoadPositionBOX2.ToString();

            dataGridView3.Rows[7].Cells[2].Value = portData.LoadPosition3.ToString();
            dataGridView3.Rows[7].Cells[3].Value = portData.LoadPositionBOX3.ToString();

            dataGridView3.Rows[8].Cells[2].Value = portData.LoadPosition4.ToString();
            dataGridView3.Rows[8].Cells[3].Value = portData.LoadPositionBOX4.ToString();

            dataGridView3.Rows[9].Cells[2].Value = portData.LoadPosition5.ToString();
            dataGridView3.Rows[9].Cells[3].Value = portData.LoadPositionBOX5.ToString();

            dataGridView3.Rows[10].Cells[2].Value = portData.LoadPosition6.ToString();
            dataGridView3.Rows[11].Cells[2].Value = portData.LoadPosition7.ToString();

            #endregion dataGridView3

            #region dataGridView4

            var read_result = "";
            var carrier_type = "";
            var isDoorOpen = "";
            var isHeartbeatOn = "";

            if (port is MANUAL_PORTSTATION)
            {
                var manual_port_info = (port as MANUAL_PORTSTATION).getManualPortPLCInfo();
                read_result = manual_port_info.CarrierIdReadResult;
                carrier_type = manual_port_info.CarrierType.ToString();
                isDoorOpen = manual_port_info.IsDoorOpen.ToString();
                isHeartbeatOn = manual_port_info.IsHeartBeatOn.ToString();
            }

            dataGridView4.Rows[0].Cells[2].Value = read_result;
            dataGridView4.Rows[1].Cells[2].Value = carrier_type;
            dataGridView4.Rows[2].Cells[2].Value = isDoorOpen;
            dataGridView4.Rows[3].Cells[2].Value = isHeartbeatOn;

            #endregion dataGridView4

            dataGridView5.DataSource = BCApp.SCApplication.VehicleBLL.cache
                .loadVhs()
                .Select(data => new
                {
                    data.VEHICLE_ID,
                    data.HAS_CST,
                    data.BOX_ID,
                    data.CST_ID,
                    data.ACT_STATUS,
                    data.MCS_CMD,
                    data.CMD_CST_ID
                }).ToList();

            TimeSpan timeOut = DateTime.Now - openTime;
            if (timeOut.Minutes > 5)
            {
                timer1.Enabled = false;
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            transferService.PortCommanding(comboBox1.Text, true);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            transferService.PortCommanding(comboBox1.Text, false);
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            string port_id = comboBox1.Text;
            E_PortType mode = (E_PortType)comboBox2.SelectedIndex;
            DialogResult confirmResult = MessageBox.Show(this, $"Do you want to change port:{port_id} to {mode} mode?",
                App.BCApplication.getMessageString("CONFIRM"), MessageBoxButtons.YesNo);

            if (confirmResult != DialogResult.Yes)
                return;

            await Task.Run(() => transferService.PortTypeChange(port_id, mode, "測試用 UI"));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            transferService.toAGV_Mode(comboBox1.Text);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            transferService.toMGV_Mode(comboBox1.Text);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            transferService.SetPortRun(comboBox1.Text);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            transferService.SetPortStop(comboBox1.Text);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            transferService.SetAGV_PortBCR_Read(comboBox1.Text);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            transferService.SetAGV_PortOpenBOX(comboBox1.Text, "工程UI_TestGetPortData");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            transferService.RstAGV_PortBCR_Read(comboBox1.Text);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            transferService.PortAlarrmReset(comboBox1.Text);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            GetPortData();
        }

        public void GetPortData()
        {
            dataGridView1.DataSource = BCApp.SCApplication.PortDefBLL.GetOHB_CVPortData(line.LINE_ID);
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell v in dataGridView1.SelectedCells)
            {
                string portName = dataGridView1.Rows[v.RowIndex].Cells["PLCPortID"].Value.ToString();
                //transferService.UpdateIgnoreModeChange(portName, "Y");
            }
            GetPortData();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell v in dataGridView1.SelectedCells)
            {
                string portName = dataGridView1.Rows[v.RowIndex].Cells["PLCPortID"].Value.ToString();
                //transferService.UpdateIgnoreModeChange(portName, "N");
            }
            GetPortData();
        }

        private void button31_Click(object sender, EventArgs e)
        {
            transferService.OpenAGV_Station(comboBox1.Text, true, "UI_TestGetPortData");
        }

        private void button19_Click(object sender, EventArgs e)
        {
            transferService.OpenAGV_Station(comboBox1.Text, false, "UI_TestGetPortData");
        }

        private void button17_Click(object sender, EventArgs e)
        {
            foreach (var v in portList)
            {
                if (transferService.isUnitType(v.PLCPortID, UnitType.AGV))
                {
                    transferService.OpenAGV_Station(v.PLCPortID, true, "UI_TestGetPortData");
                }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            foreach (var v in portList)
            {
                if (transferService.isUnitType(v.PLCPortID, UnitType.AGV))
                {
                    transferService.OpenAGV_Station(v.PLCPortID, false, "UI_TestGetPortData");
                }
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            PortPLCInfo plcInfo = transferService.GetPLC_PortData(comboBox1.Text);

            if (plcInfo.LoadPosition1)
            {
                transferService.PLC_ReportPortWaitIn(plcInfo, "TestGetPortData");
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            transferService.OpenAGV_AutoPortType(comboBox1.Text, true);
        }

        private void button24_Click(object sender, EventArgs e)
        {
            transferService.OpenAGV_AutoPortType(comboBox1.Text, false);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            foreach (var v in portList)
            {
                if (transferService.isUnitType(v.PLCPortID, UnitType.AGV))
                {
                    transferService.OpenAGV_AutoPortType(v.PLCPortID, true);
                }
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            foreach (var v in portList)
            {
                if (transferService.isUnitType(v.PLCPortID, UnitType.AGV))
                {
                    transferService.OpenAGV_AutoPortType(v.PLCPortID, false);
                }
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell v in dataGridView1.SelectedCells)
            {
                string portName = dataGridView1.Rows[v.RowIndex].Cells["PLCPortID"].Value.ToString();
                transferService.PortInOutService(portName, E_PORT_STATUS.InService, "TestGetPortData");
            }
            GetPortData();
        }

        private void button27_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell v in dataGridView1.SelectedCells)
            {
                string portName = dataGridView1.Rows[v.RowIndex].Cells["PLCPortID"].Value.ToString();
                transferService.PortInOutService(portName, E_PORT_STATUS.OutOfService, "TestGetPortData");
            }
            GetPortData();
        }

        private void button30_Click(object sender, EventArgs e)
        {
            BCApp.SCApplication.TransferService.AlliniPortData();
            List<PortDef> portDefList = BCApp.SCApplication.PortDefBLL.GetOHB_CVPortData(line.LINE_ID);
            foreach (PortDef portDefData in portDefList)
            {
                if (portDefData.State == E_PORT_STATUS.InService)
                {
                    BCApp.SCApplication.ReportBLL.ReportPortInService(portDefData.PLCPortID);
                }
                else if (portDefData.State == E_PORT_STATUS.OutOfService)
                {
                    BCApp.SCApplication.ReportBLL.ReportPortOutOfService(portDefData.PLCPortID);
                }
            }
            dataGridView1.DataSource = portDefList;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void button25_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewCell v in dataGridView5.SelectedCells)
                {
                    string portName = dataGridView5.Rows[v.RowIndex].Cells["VEHICLE_ID"].Value.ToString();
                    transferService.iniOHTData(portName, "UI_TestGetPortData");
                }
            }
            catch
            {
            }
        }

        private void button37_Click(object sender, EventArgs e)
        {
            transferService.PortBCR_Enable(comboBox1.Text, true);
        }

        private void button38_Click(object sender, EventArgs e)
        {
            transferService.PortBCR_Enable(comboBox1.Text, false);
        }

        private void button41_Click(object sender, EventArgs e)
        {
            transferService.doUpdateTimeOutForAutoUD(comboBox1.Text, (int)numericUpDown1.Value);
        }

        private void moveBack_Click_1(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);
            if (port is MANUAL_PORTSTATION)
            {
                var manual_port = port as MANUAL_PORTSTATION;
                manual_port.MoveBackAsync();
                MessageBox.Show($"Port ID:{comboBox1.Text} 已執行 Moveback", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"Port ID:{comboBox1.Text} 並不是 Manual Port，無法進行 Move Back", "Message",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);
            if (port is MANUAL_PORTSTATION)
            {
                Enum.TryParse<MoveBackReasons>(cmb_moveBackReason.SelectedValue.ToString(), out MoveBackReasons moveBackReasons);
                var manual_port = port as MANUAL_PORTSTATION;
                manual_port.SetMoveBackReasonAsync(moveBackReasons);
                MessageBox.Show($"Port ID:{comboBox1.Text} 已執行 設定 Moveback 的原因", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"Port ID:{comboBox1.Text} 並不是 Manual Port，無法設定 Moveback 的原因", "Message",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void button_ShowPLCMonitor_Click(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);
            if (port is MANUAL_PORTSTATION)
            {
                var manual_port = port as MANUAL_PORTSTATION;
                manual_port.ShowReadyToWaitOutCarrierOnMonitorAsync(textBox_ReadyToWaitOutCarrierID1.Text, textBox_ReadyToWaitOutCarrierID2.Text);
                manual_port.ShowComingOutCarrierOnMonitorAsync(textBox_ComingOutCarrierID.Text);
                MessageBox.Show($"Port ID:{comboBox1.Text} 已執行顯示準備出庫的 Carrier ID", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"Port ID:{comboBox1.Text} 並不是 Manual Port，無法指定顯示正要出庫的 Carrier ID", "Message",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void button_TimeCalibration_Click(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);
            if (port is MANUAL_PORTSTATION)
            {
                var manual_port = port as MANUAL_PORTSTATION;
                manual_port.TimeCalibrationAsync();
                MessageBox.Show($"Port ID:{comboBox1.Text} 已執行 對時", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"Port ID:{comboBox1.Text} 並不是 Manual Port，無法執行對時", "Message",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void button_Commanding_ON_Click(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);
            if (port is MANUAL_PORTSTATION)
            {
                var manual_port = port as MANUAL_PORTSTATION;
                manual_port.SetCommandingOnAsync();
                MessageBox.Show($"Port ID:{comboBox1.Text} 已執行 預約方向", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"Port ID:{comboBox1.Text} 並不是 Manual Port，無法執行 預約方向", "Message",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void button_Commanding_OFF_Click(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);
            if (port is MANUAL_PORTSTATION)
            {
                var manual_port = port as MANUAL_PORTSTATION;
                manual_port.SetCommandingOffAsync();
                MessageBox.Show($"Port ID:{comboBox1.Text} 已執行 解預約方向", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"Port ID:{comboBox1.Text} 並不是 Manual Port，無法執行 解預約方向", "Message",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void button_StopBuzzer_Click(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);

            port.StopBuzzer();

            MessageBox.Show($"Port ID:{comboBox1.Text} 已執行 關閉蜂鳴器", "Message", MessageBoxButtons.OK);
        }

        private void button_ManualPortHeartBeat_On_Click(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);
            if (port is MANUAL_PORTSTATION)
            {
                var manual_port = port as MANUAL_PORTSTATION;
                manual_port.UpdateHeartBeat(setOn: true);
                MessageBox.Show($"Port ID:{comboBox1.Text} 已執行 [ON] HeatBeat", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"Port ID:{comboBox1.Text} 並不是 Manual Port，無法執行 [ON] HeatBeat", "Message",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void button_ManualPortHeartBeat_Off_Click(object sender, EventArgs e)
        {
            var port = BCApp.SCApplication.PortStationBLL.OperateCatch.getPortStation(comboBox1.Text);
            if (port is MANUAL_PORTSTATION)
            {
                var manual_port = port as MANUAL_PORTSTATION;
                manual_port.UpdateHeartBeat(setOn: false);
                MessageBox.Show($"Port ID:{comboBox1.Text} 已執行 [OFF] HeatBeat", "Message", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show($"Port ID:{comboBox1.Text} 並不是 Manual Port，無法執行 [OFF] HeatBeat", "Message",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }
    }
}