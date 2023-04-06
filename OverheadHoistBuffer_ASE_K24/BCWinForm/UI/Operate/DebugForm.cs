// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
// 2020/05/24    Jason Wu       N/A            A0.01   新增DebugParameter.ignore136UnloadComplete 開關
//**********************************************************************************
using com.mirle.ibg3k0.bc.winform.App;
using com.mirle.ibg3k0.bc.winform.Common;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static com.mirle.ibg3k0.sc.App.SCAppConstants;

namespace com.mirle.ibg3k0.bc.winform.UI
{
    public partial class DebugForm : Form
    {

        BCMainForm mainForm;
        BCApplication bcApp;
        List<RadioButton> radioButtons = new List<RadioButton>();
        List<BLOCKZONEQUEUE> blocked_queues = null;
        AEQPT MTSMTL = null;
        public DebugForm(BCMainForm _mainForm)
        {
            InitializeComponent();
            mainForm = _mainForm;
            bcApp = mainForm.BCApp;

            cb_StartGenAntoCmd.Checked = DebugParameter.CanAutoRandomGeneratesCommand;
            cb_FroceReservePass.Checked = DebugParameter.isForcedPassReserve;
            cb_FroceReserveReject.Checked = DebugParameter.isForcedRejectBlockControl;
            ck_retry.Checked = DebugParameter.Is_136_retry_test;
            List<string> lstVh = new List<string>();
            lstVh.Add(string.Empty);
            lstVh.AddRange(bcApp.SCApplication.getEQObjCacheManager().getAllVehicle().Select(vh => vh.VEHICLE_ID).ToList());
            string[] allVh = lstVh.ToArray();
            BCUtility.setComboboxDataSource(cmb_tcpipctr_Vehicle, allVh);
            BCUtility.setComboboxDataSource(cmb_plcctr_Vehicle, allVh.ToArray());
            BCUtility.setComboboxDataSource(cmb_car_out_vh, allVh.ToArray());
            BCUtility.setComboboxDataSource(cmb_cycleRunVhId, allVh.ToArray());
            BCUtility.setComboboxDataSource(cb_testHIDAbnormalVh, allVh.ToArray());
            //var cst_data = CassetteData.CassetteData_InfoList;
            //if (cst_data != null && cst_data.Count() != 0)
            //{
            //    var cst_data_ids = new List<string>();
            //    cst_data_ids.Add("");
            //    cst_data_ids.AddRange(cst_data.Select(cst => cst.BOXID).ToList());
            //    BCUtility.setComboboxDataSource(cmb_cycleCstID, cst_data_ids.ToArray());
            //}

            List<AADDRESS> allAddress_obj = bcApp.SCApplication.MapBLL.loadAllAddress();
            string[] allAdr_ID = allAddress_obj.Select(adr => adr.ADR_ID).ToArray();
            BCUtility.setComboboxDataSource(cmb_teach_from_adr, allAdr_ID);
            BCUtility.setComboboxDataSource(cmb_teach_to_adr, allAdr_ID.ToArray());


            List<AEQPT> ohcvDevices = bcApp.SCApplication.EquipmentBLL.cache.loadOHCVDevices();
            string[] ohcv_devices_id = ohcvDevices.Select(eq => eq.EQPT_ID).ToArray();
            BCUtility.setComboboxDataSource(cb_cv_ids, ohcv_devices_id.ToArray());

            var shelfDefs = bcApp.SCApplication.ShelfDefBLL.LoadShelf();
            List<string> current_bay_ids = shelfDefs.Select(s => s.BayID).Distinct().OrderBy(s => s).ToList();
            cmb_cycleRunBayID.DataSource = current_bay_ids;


            numericUpDown1.Value = DebugParameter.PreDriveOutDistance_MM;
            cb_passDriveOutByAreaSensor.Checked = DebugParameter.isPassDriveOutByAreaSensor;
            cmb_cycleRunVhId.SelectedItem = DebugParameter.cycleRunVh;
            cmb_cycleRunBayID.SelectedItem = DebugParameter.cycleRunBay;
            //cmb_cycleCstID.SelectedValue = DebugParameter.cycleRunCST;

            cb_ForceStraightPass.Checked = DebugParameter.IsForceStraightPass;
            cb_ForceNonStraightPass.Checked = DebugParameter.IsForceNonStraightPass;
            cb_ignoreNTBPort.Checked = DebugParameter.IsIgnoreNTBPortStatus;
            cb_paassErrorVhAndTrackStatus.Checked = DebugParameter.IsPaassErrorVhAndTrackStatus;
            txt_cycleCstID.Text = DebugParameter.cycleRunCSTs;
            cb_IsSameBayAfterWay.Checked = DebugParameter.IsSameByAfterWay;
            ch_IsAutoDriveOut.Checked = DebugParameter.IsAutoDriveOut;
            cb_isCheckHIDStatus.Checked = DebugParameter.IsCheckHIDStatus;
            cb_testHIDAbnormalVh.SelectedItem = DebugParameter.TestHIDAbnormalVhID;
            cb_autoUnloadOnVh.Checked = DebugParameter.IsAutoUnloadOnvh;
            cb_openSpecifyVh.Checked = DebugParameter.IsSpecifyVhTransfer;
            cb_openGuideSectionChange.Checked = DebugParameter.IsOpneChangeGuideSection;
            cb_SyncWithOHTWhenConnection.Checked = DebugParameter.IsSyncWhenConnectionEvent;
            cb_cloesCorrectGuideInfoWhenSameLoadAdr.Checked = DebugParameter.IsCloseCorrectGuideInfoWhenSameLoadAdr;
            cb_openContinueNotifyFun.Checked = sc.App.SystemParameter.IsOpenContinueNotifyWhenVehicleTimeout;
            cb_isOpenReelNTBPortStatusAsk.Checked = sc.App.SystemParameter.IsOpenReelNTBPortStatusAsk;
            cb_openTrackResetByVhBlockRelease.Checked = DebugParameter.IsOpenTrackResetByVhBlockRelease;
            cb_checkTrackAlive.Checked = DebugParameter.IsOpenCheckTrackAlive;
            cb_IsOpenParkingZoneControlFunction.Checked = DebugParameter.IsOpenParkingZoneControlFunction;
            cb_IsOpenParkingZoneAutoPull.Checked = DebugParameter.IsOpenParkingZoneAutoPull;


            cb_OperMode.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.OperatingVHMode));
            cb_PwrMode.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.OperatingPowerMode));
            cmb_pauseEvent.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.PauseEvent));
            cmb_pauseType.DataSource = Enum.GetValues(typeof(OHxCPauseType));
            cb_Abort_Type.DataSource = Enum.GetValues(typeof(sc.ProtocolFormat.OHTMessage.CMDCancelType));
            combox_cycle_type.DataSource = Enum.GetValues(typeof(DebugParameter.CycleRunType));
            cmbVhType.DataSource = Enum.GetValues(typeof(E_VH_TYPE));

            combox_cycle_type.SelectedItem = DebugParameter.cycleRunType;
            cb_passTrack.Checked = DebugParameter.IsPassTrackBlockStatus;

            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR01", cb_unloadArrivePassReply01);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR02", cb_unloadArrivePassReply02);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR03", cb_unloadArrivePassReply03);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR05", cb_unloadArrivePassReply05);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR06", cb_unloadArrivePassReply06);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR07", cb_unloadArrivePassReply07);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR08", cb_unloadArrivePassReply08);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR09", cb_unloadArrivePassReply09);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR10", cb_unloadArrivePassReply10);
            setUnloadArrivePassReplyCheckBox("B6_OHB01_CR11", cb_unloadArrivePassReply11);


            radioButtons.Add(radio_bit0);
            radioButtons.Add(radio_bit1);
            radioButtons.Add(radio_bit2);
            radioButtons.Add(radio_bit3);
            radioButtons.Add(radio_bit4);
            radioButtons.Add(radio_bit5);
            radioButtons.Add(radio_bit6);
            radioButtons.Add(radio_bit7);
            radioButtons.Add(radio_bit8);
            radioButtons.Add(radio_bit9);
            radioButtons.Add(radio_bita);
            radioButtons.Add(radio_bitb);
            radioButtons.Add(radio_bitc);
            radioButtons.Add(radio_bitd);
            radioButtons.Add(radio_bite);
            radioButtons.Add(radio_bitf);



            cb_Cache_data_Name.Items.Add("");
            cb_Cache_data_Name.Items.Add("APORTSTATION");
            dgv_cache_object_data.AutoGenerateColumns = false;

            var hids = bcApp.SCApplication.EquipmentBLL.cache.loadHID();
            if (hids.Count > 0)
            {
                string[] hid_ids = hids.Select(h => h.EQPT_ID).ToArray();
                BCUtility.setComboboxDataSource(comboBox_HID_control, hid_ids.ToArray());
            }
            comboBox_HID_control.SelectedIndex = 0;



        }

        private void DebugForm_Load(object sender, EventArgs e)
        {
            DebugParameter.IsDebugMode = true;
        }

        private void DebugForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            TrunOffAllVhPLCControl();
            DebugParameter.IsDebugMode = false;
            mainForm.removeForm(typeof(DebugForm).Name);
        }



        private void cb_FroceReservePass_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isForcedPassBlockControl = cb_FroceReservePass.Checked;
        }


        AVEHICLE noticeCar = null;
        string vh_id = null;
        private void cmb_Vehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            vh_id = cmb_tcpipctr_Vehicle.Text.Trim();

            noticeCar = bcApp.SCApplication.getEQObjCacheManager().getVehicletByVHID(vh_id);
            lbl_id_37_cmdID_value.Text = noticeCar?.OHTC_CMD;
            lbl_install_status.Text = noticeCar?.IS_INSTALLED.ToString();
            lbl_listening_status.Text = noticeCar?.IsTcpIpListening(bcApp.SCApplication.getBCFApplication()).ToString();
            cmbVhType.SelectedItem = noticeCar?.VEHICLE_TYPE;
        }

        private void uctl_Btn1_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.HostBasicVersionReport);
            //asyExecuteAction(noticeCar.sned_S1);
        }
        private void uctl_SendFun11_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.BasicInfoReport);
            //asyExecuteAction(noticeCar.sned_S11);
        }
        private void uctl_SendFun13_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.TavellingDataReport);
            //asyExecuteAction(noticeCar.sned_S13);
        }
        private void uctl_SendFun15_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.SectionDataReport);
            //asyExecuteAction(noticeCar.sned_S15);
        }
        private void uctl_SendFun17_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.AddressDataReport);
            //asyExecuteAction(noticeCar.sned_S17);
        }

        private void uctl_SendFun19_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.ScaleDataReport);
            //asyExecuteAction(noticeCar.sned_S19);
        }

        private void uctl_SendFun21_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.ControlDataReport);
            //asyExecuteAction(noticeCar.sned_S21);
        }

        private void uctl_SendFun23_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.GuideDataReport);
            //asyExecuteAction(noticeCar.sned_S23);
        }

        private void asyExecuteAction(Func<string, bool> act)
        {
            Task.Run(() =>
            {
                act(vh_id);
            });
        }

        private void uctl_SendAllFun_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.doDataSysc);
            //asyExecuteAction(noticeCar.sned_ALL);
        }

        private void uctl_Send_Fun_71_Click(object sender, EventArgs e)
        {
            string from_adr = cmb_teach_from_adr.Text;
            string to_adr = cmb_teach_to_adr.Text;
            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.TeachingRequest(vh_id, from_adr, to_adr);
                //noticeCar.send_Str71(from_adr, to_adr);
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.IndividualUploadRequest);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.IndividualChangeRequest);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sc.ProtocolFormat.OHTMessage.OperatingVHMode operatiogMode;
            Enum.TryParse(cb_OperMode.SelectedValue.ToString(), out operatiogMode);

            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.ModeChangeRequest(vh_id, operatiogMode);
            });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sc.ProtocolFormat.OHTMessage.OperatingPowerMode operatiogPowerMode;
            Enum.TryParse(cb_PwrMode.SelectedValue.ToString(), out operatiogPowerMode);

            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.PowerOperatorRequest(vh_id, operatiogPowerMode);
            });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            asyExecuteAction(bcApp.SCApplication.VehicleService.AlarmResetRequest);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (noticeCar == null)
            {
                MessageBox.Show($"Please select vh first.", "Force finish fail.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string message = $"Do you want to force finish vh:{noticeCar.VEHICLE_ID} command?";
            DialogResult confirmResult = MessageBox.Show(this, message,
                BCApplication.getMessageString("CONFIRM"), MessageBoxButtons.YesNo);

            if (noticeCar.ACT_STATUS == sc.ProtocolFormat.OHTMessage.VHActionStatus.Commanding)
            {
                MessageBox.Show($"vh:{noticeCar.VEHICLE_ID} current status:{noticeCar.ACT_STATUS} can't excute force finish.",
                                 "Force finish fail.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BCUtility.recordAction(bcApp, this.Name, message, confirmResult.ToString());
            if (confirmResult != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }
            Task.Run(() =>
            {
                bcApp.SCApplication.CMDBLL.forceUpdataCmdStatus2FnishByVhID(vh_id);
            });
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                //bcApp.SCApplication.VehicleService.forceResetVHStatus(vh_id);
                bcApp.SCApplication.VehicleService.VehicleStatusRequest(vh_id, true);
            });
        }

        private void cb_StartGenAntoCmd_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            DebugParameter.CanAutoRandomGeneratesCommand = cb.Checked;

            if (!cb.Checked)
            {
                Task.Run(() =>
                {
                    var mcs_cmds = bcApp.SCApplication.CMDBLL.loadMCS_Command_Queue();
                    //foreach (var cmd in mcs_cmds)
                    //{
                    //    //bcApp.SCApplication.CMDBLL.updateCMD_MCS_TranStatus2Complete(cmd.CMD_ID, E_TRAN_STATUS.Canceling);
                    //}
                });
            }
        }

        private void btn_forceReleaseALLBlock_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.forceReleaseBlockControl();
            });
        }

        private void btn_ForceReleaseBlock_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.forceReleaseBlockControl(vh_id);
            });
        }

        private void btn_pause_Click(object sender, EventArgs e)
        {
            sc.ProtocolFormat.OHTMessage.PauseEvent pauseEvent;
            OHxCPauseType pauseType;
            Enum.TryParse(cmb_pauseEvent.SelectedValue.ToString(), out pauseEvent);
            Enum.TryParse(cmb_pauseType.SelectedValue.ToString(), out pauseType);
            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.PauseRequest(vh_id, pauseEvent, pauseType);
            });

        }




        private void label17_Click(object sender, EventArgs e)
        {

        }



        AVEHICLE plcctrAVEHICLE = null;
        string event_id = "DebugFrom";
        private void cmb_plcctr_Vehicle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (plcctrAVEHICLE != null)
            {
                plcctrAVEHICLE.removeEventHandler(event_id);
                plcctrAVEHICLE = null;
            }
            if (string.IsNullOrWhiteSpace(cmb_plcctr_Vehicle.Text))
            {
                ck_PLC_control_flag.Checked = false;
                ck_PLC_control_flag.Enabled = false;
                return;
            }
            else
            {
                ck_PLC_control_flag.Enabled = true;
            }
            plcctrAVEHICLE = bcApp.SCApplication.VehicleBLL.getVehicleByID(cmb_plcctr_Vehicle.Text);
            plcctrAVEHICLE.addEventHandler(event_id
                                , BCFUtility.getPropertyName(() => plcctrAVEHICLE.Status_Info_PLC)
                                , (s1, e1) => { updateVehicleStatusInfo(); });
            updateVehicleControlItem();
            updateVehicleStatusInfo();
        }

        private void updateVehicleControlItem()
        {
            if (plcctrAVEHICLE == null) return;
            ck_PLC_control_flag.Checked = plcctrAVEHICLE.isPLCInControl;
            if (plcctrAVEHICLE.VehicleControlItemForPLC == null) return;
            for (int i = 0; i < radioButtons.Count(); i++)
            {
                radioButtons[i].Checked = plcctrAVEHICLE.VehicleControlItemForPLC[i];
            }
        }

        private void ck_PLC_control_flag_CheckedChanged(object sender, EventArgs e)
        {
            if (plcctrAVEHICLE == null) return;
            bool isControl = (sender as CheckBox).Checked;
            tlp_PLCControl.Enabled = isControl;
            if (isControl)
            {
                bcApp.SCApplication.VehicleService.PLC_Control_TrunOn(plcctrAVEHICLE.VEHICLE_ID);
            }
            else
            {
                foreach (var radio in radioButtons)
                    radio.Checked = false;
                bcApp.SCApplication.VehicleService.PLC_Control_TrunOff(plcctrAVEHICLE.VEHICLE_ID);

            }
        }

        private void radio_bitX_Click(object sender, EventArgs e)
        {
            (sender as RadioButton).Checked = !(sender as RadioButton).Checked;
        }

        private void radio_bitX_CheckedChanged(object sender, EventArgs e)
        {
            Boolean[] bools = radioButtons.Select(radio => radio.Checked).ToArray();
            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.SetVehicleControlItemForPLC(plcctrAVEHICLE.VEHICLE_ID, bools);
            });
        }

        private void updateVehicleStatusInfo()
        {
            if (plcctrAVEHICLE == null | plcctrAVEHICLE.Status_Info_PLC == null) return;
            Adapter.Invoke((obj) =>
            {
                VH_ID_Value.Text = plcctrAVEHICLE.Status_Info_PLC.vh_id.ToString();
                CUR_SEC_ID_Value.Text = plcctrAVEHICLE.Status_Info_PLC.cur_sec_id.ToString();
                CUR_SEC_DIST_Value.Text = plcctrAVEHICLE.Status_Info_PLC.CUR_SEC_DIST.ToString();
                CUR_ADR_DIST_Value.Text = plcctrAVEHICLE.Status_Info_PLC.cur_adr_id.ToString();
                TRAN_CMD_ID_Value.Text = plcctrAVEHICLE.Status_Info_PLC.tran_cmd_id.ToString();
                ACTION_STATUS_Value.Text = plcctrAVEHICLE.Status_Info_PLC.ACTION_STATUS.ToString();
                HAS_CST_Value.Text = plcctrAVEHICLE.Status_Info_PLC.HAS_CST.ToString();
                CST_ID_Value.Text = plcctrAVEHICLE.Status_Info_PLC.CST_ID?.ToString();
                OBS_PAUSE_Value.Text = plcctrAVEHICLE.Status_Info_PLC.OBS_PAUSE.ToString();
                BLOCK_PAUSE_Value.Text = plcctrAVEHICLE.Status_Info_PLC.BLOCK_PAUSE.ToString();
                NORMAL_PAUSE_Value.Text = plcctrAVEHICLE.Status_Info_PLC.NORMAL_PAUSE.ToString();
                HID_PAUSE_Value.Text = plcctrAVEHICLE.Status_Info_PLC.HID_PAUSE.ToString();
                ERROR_PAUSE_Value.Text = plcctrAVEHICLE.Status_Info_PLC.ERROR_PAUSE.ToString();
                CUR_BLOCK_ID_Value.Text = plcctrAVEHICLE.Status_Info_PLC.cur_block_id.ToString();
                CUR_HID_ID_Value.Text = plcctrAVEHICLE.Status_Info_PLC.cur_hid_id.ToString();
                VH_MODE_STATUS_Value.Text = plcctrAVEHICLE.Status_Info_PLC.VH_MODE_STATUS.ToString();
                VH_SPEED_MIN_Value.Text = plcctrAVEHICLE.Status_Info_PLC.VH_SPEED_MIN.ToString();
                VH_ENCODER_VALUE_Value.Text = plcctrAVEHICLE.Status_Info_PLC.VH_ENCODER_VALUE.ToString();
                VH_MAG_VALUE_Value.Text = plcctrAVEHICLE.Status_Info_PLC.VH_MAG_VALUE.ToString();
                SPEED_LIMIT_Value.Text = plcctrAVEHICLE.Status_Info_PLC.SPEED_LIMIT.ToString();
                LEFT_GUIDE_STATUS_Value.Text = plcctrAVEHICLE.Status_Info_PLC.LEFT_GUIDE_STATUS.ToString();
                RIGHT_GUIDE_STATUS_Value.Text = plcctrAVEHICLE.Status_Info_PLC.RIGHT_GUIDE_STATUS.ToString();
                SEC_DIST_DEV_Value.Text = plcctrAVEHICLE.Status_Info_PLC.SEC_DIST_DEV.ToString();
                DEV_SEC_ID_Value.Text = plcctrAVEHICLE.Status_Info_PLC.dev_sec_id.ToString();
                Power_Mode_Value.Text = plcctrAVEHICLE.Status_Info_PLC.POWER_STATUS.ToString();
                ACC_SEC_DIST_Value.Text = plcctrAVEHICLE.Status_Info_PLC.ACC_SEC_DIST.ToString();
                plc_time_Value.Text = plcctrAVEHICLE.Status_Info_PLC.PLC_Datetime.ToString(SCAppConstants.DateTimeFormat_23);

            }, null);

        }

        private void TrunOffAllVhPLCControl()
        {
            var vhs = bcApp.SCApplication.getEQObjCacheManager().getAllVehicle();

            foreach (var vh in vhs)
            {
                vh.PLC_Control_TrunOff();
            }

        }

        private void cb_FroceReserveReject_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isForcedRejectBlockControl = cb_FroceReserveReject.Checked;
        }

        private void button8_Click(object sender, EventArgs e)
        {
        }

        private void button9_Click(object sender, EventArgs e)
        {
            sc.ProtocolFormat.OHTMessage.CMDCancelType type;
            Enum.TryParse(cb_Abort_Type.SelectedValue.ToString(), out type);

            Task.Run(() =>
            {
                noticeCar.sned_Str37(noticeCar.OHTC_CMD, type);
            });

        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            //Task.Run(() =>
            //{
            //    var mapAction = bcApp.SCApplication.getEQObjCacheManager().getLine().getMapActionByIdentityKey(nameof(sc.Data.ValueDefMapAction.MCSDefaultMapAction)) as sc.Data.ValueDefMapAction.MCSDefaultMapAction;
            //    mapAction.s2f35Test();
            //});
        }


        private void ck_test_carrierinterface_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isTestCarrierInterfaceError = ck_test_carrierinterface_error.Checked;
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            //Task.Run(() =>
            //{
            //    var mapAction = bcApp.SCApplication.getEQObjCacheManager().getLine().getMapActionByIdentityKey(nameof(sc.Data.ValueDefMapAction.MCSDefaultMapAction)) as sc.Data.ValueDefMapAction.MCSDefaultMapAction;
            //    mapAction.removeCmdTest(txt_remove_cst_id.Text, txt_remove_loc_id.Text);
            //});
            // bcApp.SCApplication.FlexsimCommandDao.setCommandToFlexsimDB("OHT01", "30103","1", "20311","1","CST01","0");
        }



        private async void ck_autoTech_Click(object sender, EventArgs e)
        {
            //sc.App.SystemParameter.AutoTeching = ck_autoTech.Checked;
            //if (!ck_autoTech.Checked) return;
            //string vh_id = cmb_tcpipctr_Vehicle.Text;
            //await Task.Run(() =>
            // {
            //     bcApp.SCApplication.VehicleService.AutoTeaching(vh_id);
            //     SpinWait.SpinUntil(() => !sc.App.SystemParameter.AutoTeching);
            // });
            //ck_autoTech.Checked = sc.App.SystemParameter.AutoTeching;

        }

        private void btn_reset_teach_result_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                List<ASECTION> sections = bcApp.SCApplication.CatchDataFromDB_Section;
                foreach (var sec in sections)
                {
                    if (bcApp.SCApplication.MapBLL.resetSecTechingTime(sec.SEC_ID))
                    {
                        sec.LAST_TECH_TIME = null;
                    }

                }
            });
        }

        private void btn_cmd_override_test_Click(object sender, EventArgs e)
        {
            //string vh_id = cmb_tcpipctr_Vehicle.Text;
            //bool is_need_pause_first = cb_pauseFirst.Checked;
            //Task.Run(() =>
            //{
            //    bcApp.SCApplication.VehicleService.VhicleChangeThePath(vh_id, is_need_pause_first);
            //});
        }

        private void uctl_SendFun2_Click(object sender, EventArgs e)
        {
            //asyExecuteAction(bcApp.SCApplication.VehicleService.BasicInfoVersionReport);
            asyExecuteAction(bcApp.SCApplication.VehicleService.HostBasicVersionReport);


        }

        private void cb_Cache_data_Name_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected_name = (sender as ComboBox).SelectedItem as string;
            if (selected_name == "APORTSTATION")
            {
                var aportation = bcApp.SCApplication.getEQObjCacheManager().getALLPortStation();
                dgv_cache_object_data.DataSource = aportation;
            }
        }

        private void dgv_cache_object_data_EditModeChanged(object sender, EventArgs e)
        {

        }


        #region MTL Test


        private void btn_mtl_dateTimeSync_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var mtl_mapaction = maintainEQ.
                    getMapActionByIdentityKey(nameof(com.mirle.ibg3k0.sc.Data.ValueDefMapAction.MTxValueDefMapActionBase)) as
                    com.mirle.ibg3k0.sc.Data.ValueDefMapAction.MTxValueDefMapActionBase;
                mtl_mapaction.DateTimeSyncCommand(DateTime.Now);
            }
            );
        }
        #endregion MTL Test

        private void btn_mtl_message_download_Click(object sender, EventArgs e)
        {
            string msg = txt_mtlMessage.Text;
            Task.Run(() =>
            {
                var mtl_mapaction = maintainEQ.
                    getMapActionByIdentityKey(nameof(com.mirle.ibg3k0.sc.Data.ValueDefMapAction.MTxValueDefMapActionBase)) as
                    com.mirle.ibg3k0.sc.Data.ValueDefMapAction.MTxValueDefMapActionBase;
                mtl_mapaction.OHxCMessageDownload(msg);
            }
            );
        }

        private void btn_mtl_vh_realtime_info_Click(object sender, EventArgs e)
        {
            UInt16 car_id = UInt16.Parse(txt_mtl_car_id.Text);
            UInt16 action_mode = UInt16.Parse(txt_mtl_action_mode.Text);
            UInt16 cst_exist = UInt16.Parse(txt_mtl_cst_exist.Text);
            UInt16 current_section_id = UInt16.Parse(txt_mtl_current_sec_id.Text);
            UInt32 current_address_id = UInt32.Parse(txt_mtl_current_adr_id.Text);
            UInt32 buffer_distance = UInt32.Parse(txt_mtl_buffer_distance.Text);
            UInt16 speed = UInt16.Parse(txt_mtl_speed.Text);
            Task.Run(() =>
            {
                var mtl_mapaction = maintainEQ.
                    getMapActionByIdentityKey(nameof(com.mirle.ibg3k0.sc.Data.ValueDefMapAction.MTxValueDefMapActionBase)) as
                    com.mirle.ibg3k0.sc.Data.ValueDefMapAction.MTxValueDefMapActionBase;
                mtl_mapaction.CarRealtimeInfo(car_id, action_mode, cst_exist, current_section_id, current_address_id, buffer_distance, speed);
            }
            );
        }

        private void btn_mtl_car_out_notify_Click(object sender, EventArgs e)
        {
        }

        private void btn_rename_cst_id_Click(object sender, EventArgs e)
        {
            string cst_id = txt_rename_cst_id.Text;

            Task.Run(() =>
            {
                bcApp.SCApplication.VehicleService.CarrierIDRenameRequset(vh_id, noticeCar.CST_ID, cst_id);
            });
        }

        private void btn_mtl_o2m_u2d_CarOutInterface(object sender, EventArgs e)
        {
        }

        private void btn_mtl_o2m_d2u_CarInInterfaece(object sender, EventArgs e)
        {
        }

        private void btn_mtl2ohxc_carinterface_refresh_Click(object sender, EventArgs e)
        {

        }

        private void btn_SendHIDControl_Click(object sender, EventArgs e)
        {
            SCApplication scApp = SCApplication.getInstance();
            AEQPT eqpt_HID = scApp.getEQObjCacheManager().getEquipmentByEQPTID("HID");
            HIDValueDefMapAction mapAction = (eqpt_HID.getMapActionByIdentityKey("HIDValueDefMapAction") as HIDValueDefMapAction);
            bool signal = comboBox_HID_control.SelectedIndex == 0 ? true : false;
            mapAction.HID_Control(signal);

        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            SCApplication scApp = SCApplication.getInstance();
            Task.Run(() => scApp.LineService.OnlineWithHostOp());
        }

        private void ck_CycleRunTest_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsCycleRun = ck_CycleRunTest.Checked;
        }


        private void uctlButton1_Click(object sender, EventArgs e)
        {

        }

        private async void btn_carOutTest_Click(object sender, EventArgs e)
        {
            try
            {
                string vh_id = cmb_tcpipctr_Vehicle.Text;
                btn_carOutTest.Enabled = false;
                bool isSuccess = false;
                string result = "";
                await Task.Run(() =>
                {
                    //var r = bcApp.SCApplication.MTLService.carOutRequset(vh_id);
                    //isSuccess = r.isSuccess;
                    //result = r.result;

                });
                if (!isSuccess)
                {
                    MessageBox.Show(result);
                }
            }
            catch { }
            finally
            {
                btn_carOutTest.Enabled = true;
            }
        }

        private void btn_cauout_cancel_Click(object sender, EventArgs e)
        {
            //bcApp.SCApplication.MTLService.carOutRequestCancle();
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void btn_changeToAutoRemote_Click(object sender, EventArgs e)
        {
            bcApp.SCApplication.VehicleService.VehicleAutoModeCahnge(vh_id, sc.ProtocolFormat.OHTMessage.VHModeStatus.AutoRemote);
        }
        private void btn_changeToAutoLocal_Click(object sender, EventArgs e)
        {
            bcApp.SCApplication.VehicleService.VehicleAutoModeCahnge(vh_id, sc.ProtocolFormat.OHTMessage.VHModeStatus.AutoMts);
        }
        private void btn_changeToAutoMTL_Click(object sender, EventArgs e)
        {
            bcApp.SCApplication.VehicleService.VehicleAutoModeCahnge(vh_id, sc.ProtocolFormat.OHTMessage.VHModeStatus.AutoMtl);
        }
        private void btn_changeToAutoLocal_Click_1(object sender, EventArgs e)
        {
            bcApp.SCApplication.VehicleService.VehicleAutoModeCahnge(vh_id, sc.ProtocolFormat.OHTMessage.VHModeStatus.AutoLocal);
        }


        private void btn_hid_info_Click(object sender, EventArgs e)
        {
            setHIDNormalStatus(lbl_isHID1Normal, "HID1");
            setHIDNormalStatus(lbl_isHID2Normal, "HID2");
            setHIDNormalStatus(lbl_isHID3Normal, "HID3");
            setHIDNormalStatus(lbl_isHID4Normal, "HID4");
            setHIDNormalStatus(lbl_isHID5Normal, "HID5");

            //AEQPT eqpt_HID = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID(comboBox_HID_control.Text);
            //if (eqpt_HID == null) return;
            ////var hid_info = eqpt_HID.HID_Info;
            ////if (hid_info == null) return;
            //Adapter.Invoke((obj) =>
            //{
            //    //lbl_hour_sigma_word_value.Text = hid_info.Hour_Sigma_Converted.ToString();
            //    //lbl_vr_value.Text = hid_info.VR_Converted.ToString();
            //    //lbl_vs_value.Text = hid_info.VS_Converted.ToString();
            //    //lbl_vt_value.Text = hid_info.VT_Converted.ToString();

            //    //lbl_ar_value.Text = hid_info.AR_Converted.ToString();
            //    //lbl_as_value.Text = hid_info.AS_Converted.ToString();
            //    //lbl_at_value.Text = hid_info.AT_Converted.ToString();

            //    //lbl_sigma_w_value.Text = hid_info.Sigma_W_Converted.ToString();
            //    lbl_isHID1Normal.Text = eqpt_HID.IsNormal.ToString();

            //}, null);
        }

        private void setHIDNormalStatus(Label lbl, string eqID)
        {
            AEQPT eqpt_HID = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID(eqID);
            if (eqpt_HID == null) return;
            Adapter.Invoke((obj) =>
            {
                lbl.Text = eqpt_HID.IsNormal.ToString();

            }, null);
        }

        private void btn_mtl_info_refresh_Click(object sender, EventArgs e)
        {
            //var eQPT = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID("MTS") as sc.Data.VO.MaintainSpace;
            Adapter.Invoke((obj) =>
            {
                lbl_mtl_current_car_id.Text = maintainEQ.CurrentCarID?.ToString();
                lbl_mtl_has_vh.Text = maintainEQ.HasVehicle.ToString();
                lbl_mtl_stop_single.Text = maintainEQ.StopSingle.ToString();
                lbl_mtl_mode.Text = maintainEQ.MTxMode.ToString();
                lbl_mtl_location.Text = maintainEQ.MTLLocation.ToString();
                lbl_mtl_moving_status.Text = maintainEQ.MTLMovingStatus.ToString();
                lbl_mtl_encoder.Text = maintainEQ.Encoder.ToString();
                lbl_mtl_in_position.Text = maintainEQ.VhInPosition.ToString();

            }, null);
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private async void btn_carOutTest_Click_1(object sender, EventArgs e)
        {

        }

        private async void btn_cauout_cancel_Click_1(object sender, EventArgs e)
        {
            try
            {
                btn_cauout_cancel.Enabled = false;
                string maintain_eq = cmb_maintain_device.Text;
                sc.Data.VO.Interface.IMaintainDevice maintainDevice = bcApp.SCApplication.EquipmentBLL.cache.getMaintainDevice(maintain_eq);
                await Task.Run(() => bcApp.SCApplication.MTLService.carOutRequestCancle(maintainDevice));
            }
            catch { }
            finally
            {
                btn_cauout_cancel.Enabled = true;
            }
        }

        private void btn_refresh_carout_info_Click(object sender, EventArgs e)
        {

        }
        AEQPT maintainEQ = null;
        private void cmb_maintain_device_SelectedIndexChanged(object sender, EventArgs e)
        {
            string device_id = cmb_maintain_device.Text;
            maintainEQ = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID(device_id) as AEQPT;
        }

        private async void btn_changeToRemove_Click(object sender, EventArgs e)
        {
            try
            {

                if (!noticeCar.IS_INSTALLED)
                {
                    MessageBox.Show($"{vh_id} is removed ready!");
                    return;
                }
                btn_changeToRemove.Enabled = false;
                await Task.Run(() => bcApp.SCApplication.VehicleService.Remove(vh_id));
                MessageBox.Show($"{vh_id} remove ok");
                lbl_install_status.Text = noticeCar?.IS_INSTALLED.ToString();
            }
            finally
            {
                btn_changeToRemove.Enabled = true;
            }
        }

        private async void btn_changeToInstall_Click(object sender, EventArgs e)
        {
            try
            {
                if (noticeCar.IS_INSTALLED)
                {
                    MessageBox.Show($"{vh_id} is install ready!");
                    return;
                }

                btn_changeToInstall.Enabled = false;
                await Task.Run(() => bcApp.SCApplication.VehicleService.Install(vh_id));
                MessageBox.Show($"{vh_id} install ok");
                lbl_install_status.Text = noticeCar?.IS_INSTALLED.ToString();
            }
            finally
            {
                btn_changeToInstall.Enabled = true;
            }
        }

        private void cb_test_duplicate_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.TestDuplicate = cb_test_duplicate.Checked;
        }




        private void tabPage6_Click(object sender, EventArgs e)
        {

        }

        private void LifterPosition_cb_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void set_MTLMTS_btn_Click(object sender, EventArgs e)
        {
            string eq_id = MTLMTS_cb_box.Text;
            MTSMTL = bcApp.SCApplication.getEQObjCacheManager().getEquipmentByEQPTID(eq_id);
        }

        private void plc_link_btn_Click(object sender, EventArgs e)
        {
            if (MTSMTL != null)
            {
                if (MTSMTL.Plc_Link_Stat == LinkStatus.LinkOK)
                {
                    MTSMTL.Plc_Link_Stat = LinkStatus.LinkFail;
                    return;
                }
                if (MTSMTL.Plc_Link_Stat == LinkStatus.LinkFail)
                {
                    MTSMTL.Plc_Link_Stat = LinkStatus.LinkOK;
                    return;
                }
            }
        }

        private void set_Alive_btn_Click(object sender, EventArgs e)
        {
            if (MTSMTL != null)
            {
                int alive = Convert.ToInt32(alive_numeric.Value);
                //MTSMTL.Is_Eq_Alive = !MTSMTL.Is_Eq_Alive;
                MTSMTL.Eq_Alive_Last_Change_time = DateTime.Now;
            }
        }

        private void mode_change_btn_Click(object sender, EventArgs e)
        {
            if (MTSMTL != null)
            {
                if (MTSMTL.MTxMode == sc.ProtocolFormat.OHTMessage.MTxMode.Auto)
                {
                    MTSMTL.MTxMode = sc.ProtocolFormat.OHTMessage.MTxMode.Manual;
                    return;
                }
                if (MTSMTL.MTxMode == sc.ProtocolFormat.OHTMessage.MTxMode.Manual)
                {
                    MTSMTL.MTxMode = sc.ProtocolFormat.OHTMessage.MTxMode.Auto;
                    return;
                }
            }
        }

        private void OHTC_send_interlock_btn_Click(object sender, EventArgs e)
        {
            //if (MTSMTL.EQPT_ID.StartsWith("MTL"))
            //{
            //    (MTSMTL as MaintainLift).CarOutInterlock = !(MTSMTL as MaintainLift).CarOutInterlock;
            //    return;
            //}
            //if (MTSMTL.EQPT_ID.StartsWith("MTL"))
            //{
            //    (MTSMTL as MaintainLift).CarOutInterlock = !(MTSMTL as MaintainLift).CarOutInterlock;
            //}
        }

        private void set_VehicleID_btn_Click(object sender, EventArgs e)
        {
            if (MTSMTL != null)
            {
                MTSMTL.CurrentCarID = vehicleID_textbox.Text;
            }
        }

        private void set_Lifterposition_btn_Click(object sender, EventArgs e)
        {
            if (LifterPosition_cb.Text == "UP")
            {
                MTSMTL.MTLLocation = sc.ProtocolFormat.OHTMessage.MTLLocation.Upper;
            }
            else if (LifterPosition_cb.Text == "DOWN")
            {
                MTSMTL.MTLLocation = sc.ProtocolFormat.OHTMessage.MTLLocation.Bottorn;
            }
            else if (LifterPosition_cb.Text == "NONE")
            {
                MTSMTL.MTLLocation = sc.ProtocolFormat.OHTMessage.MTLLocation.None;
            }
        }

        private void button7_Click_2(object sender, EventArgs e)
        {
            SCApplication scApp = SCApplication.getInstance();
            AEQPT eqpt_HID = scApp.getEQObjCacheManager().getEquipmentByEQPTID("HID");
            HIDValueDefMapAction mapAction = (eqpt_HID.getMapActionByIdentityKey("HIDValueDefMapAction") as HIDValueDefMapAction);
            mapAction.PowerAlarm(null, null);
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            SCApplication scApp = SCApplication.getInstance();
            AEQPT eqpt_HID = scApp.getEQObjCacheManager().getEquipmentByEQPTID("HID");
            HIDValueDefMapAction mapAction = (eqpt_HID.getMapActionByIdentityKey("HIDValueDefMapAction") as HIDValueDefMapAction);
            mapAction.TempAlarm(null, null);

        }

        private void set_Distance_btn_Click(object sender, EventArgs e)
        {

        }

        private async void btn_open_tcp_port_Click(object sender, EventArgs e)
        {
            bool is_success = false;
            await Task.Run(() =>
            {
                is_success = bcApp.SCApplication.VehicleService.startVehicleTcpIpServer(vh_id);
            });
            MessageBox.Show(is_success ? "OK" : "NG");
        }

        private async void btn_close_tcp_port_Click(object sender, EventArgs e)
        {
            bool is_success = false;
            await Task.Run(() =>
            {
                is_success = bcApp.SCApplication.VehicleService.stopVehicleTcpIpServer(vh_id);
            });
            MessageBox.Show(is_success ? "OK" : "NG");
        }

        private void lbl_install_status_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private void lbl_listening_status_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (ModifierKeys & Keys.Control) == Keys.Control)
            {
                btn_open_tcp_port.Visible = true;
                btn_close_tcp_port.Visible = true;
            }
        }

        private void ck_SaftyCheckComplete_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            bcf.Controller.ValueRead vr = bcApp.SCApplication.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_EQPT, "CV31_A", "SAFETY_CHECK_COMPLETE");
            vr.Value = new int[] { ck.Checked ? 1 : 0 };
        }

        private void ck_SaftyCheckRequest_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            bcf.Controller.ValueRead vr = bcApp.SCApplication.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_EQPT, "CV31_A", "SAFETY_CHECK_REQUEST");
            vr.Value = new int[] { ck.Checked ? 1 : 0 };
        }

        private void ck_DoorClosed_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            bcf.Controller.ValueRead vr = bcApp.SCApplication.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_EQPT, "CV31_A", "DOOR_CLOSE");
            vr.Value = new int[] { ck.Checked ? 1 : 0 };
        }

        private void ck_Alive_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            bcf.Controller.ValueRead vr = bcApp.SCApplication.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_EQPT, "CV31_A", "OHCV_TO_OHTC_ALIVE");
            vr.Value = new int[] { ck.Checked ? 1 : 0 };
        }




        private void ck_DoorClosed_B_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            bcf.Controller.ValueRead vr = bcApp.SCApplication.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_EQPT, "CV31_B", "DOOR_CLOSE");
            vr.Value = new int[] { ck.Checked ? 1 : 0 };
        }


        private void ck_SaftyCheckRequest_B_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            bcf.Controller.ValueRead vr = bcApp.SCApplication.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_EQPT, "CV31_B", "SAFETY_CHECK_REQUEST");
            vr.Value = new int[] { ck.Checked ? 1 : 0 };
        }

        private void ck_SaftyCheckComplete_B_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            bcf.Controller.ValueRead vr = bcApp.SCApplication.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_EQPT, "CV31_B", "SAFETY_CHECK_COMPLETE");
            vr.Value = new int[] { ck.Checked ? 1 : 0 };
        }

        private void ck_Alive_B_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ck = sender as CheckBox;
            bcf.Controller.ValueRead vr = bcApp.SCApplication.getBCFApplication().getReadValueEvent(SCAppConstants.EQPT_OBJECT_CATE_EQPT, "CV31_B", "OHCV_TO_OHTC_ALIVE");
            vr.Value = new int[] { ck.Checked ? 1 : 0 };
        }

        OHCV selectedOHCV = null;
        private void cb_cv_ids_SelectedIndexChanged(object sender, EventArgs e)
        {
            string cv_id = (sender as ComboBox).Text;
            selectedOHCV = bcApp.SCApplication.EquipmentBLL.cache.getOHCV(cv_id);
        }

        private void btn_port_test_Click(object sender, EventArgs e)
        {
            //var portValueDefMapAction = bcApp.SCApplication.getEQObjCacheManager().getPortByPortID("OHB100T01").getMapActionByIdentityKey(typeof(PortValueDefMapAction).Name) as PortValueDefMapAction;
            //portValueDefMapAction.Port_ChangeToOutput(true);
        }

        private void ck_retry_CheckedChanged(object sender, EventArgs e)
        {
            //DebugParameter.Is_136_empty_double_retry = ((CheckBox)sender).Checked;
            DebugParameter.Is_136_retry_test = ((CheckBox)sender).Checked;
        }

        private void cb_FroceReservePass_ChangeUICues(object sender, UICuesEventArgs e)
        {

        }

        private void num_section_dis_ValueChanged(object sender, EventArgs e)
        {
            string current_sec_id = txt_current_sec_id.Text;
            uint distance = (uint)num_section_dis.Value;
            bcApp.SCApplication.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh_id, current_sec_id, "", distance, 0, 0);
        }

        private void combox_cycle_type_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        //*************************************
        //A0.01
        private void Ignore136UnloadComplete_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.ignore136UnloadComplete = Ignore136UnloadComplete.Checked;
        }

        private void num_vh_y_ValueChanged(object sender, EventArgs e)
        {
            string current_sec_id = txt_current_sec_id.Text;
            uint distance = (uint)num_section_dis.Value;
            double x_axis = (int)num_vh_x.Value;
            double y_axis = (int)num_vh_y.Value;
            bcApp.SCApplication.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh_id, current_sec_id, "", distance, x_axis, y_axis);
        }

        private void num_vh_x_ValueChanged(object sender, EventArgs e)
        {
            string current_sec_id = txt_current_sec_id.Text;
            uint distance = (uint)num_section_dis.Value;
            double x_axis = (int)num_vh_x.Value;
            double y_axis = (int)num_vh_y.Value;
            bcApp.SCApplication.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh_id, current_sec_id, "", distance, x_axis, y_axis);
        }


        private void btn_bcrReadMismatch_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.Bcrread;
            McsReportEventTest(report_event, sc.ProtocolFormat.OHTMessage.BCRReadResult.BcrMisMatch);
        }

        private void btn_bcrReadError_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.Bcrread;
            McsReportEventTest(report_event, sc.ProtocolFormat.OHTMessage.BCRReadResult.BcrReadFail);
        }

        private void btn_cmpIdMismatch_Click(object sender, EventArgs e)
        {
            var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusIdcsttypeMismatch;
            McsCommandCompleteTest(completeStatus);
        }

        private void btn_idReadError_Click(object sender, EventArgs e)
        {
            var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusIdreadFailed;
            McsCommandCompleteTest(completeStatus);
        }
        private void McsReportEventTest(sc.ProtocolFormat.OHTMessage.EventType report_event,
            sc.ProtocolFormat.OHTMessage.BCRReadResult bCRReadResult = sc.ProtocolFormat.OHTMessage.BCRReadResult.BcrNormal)
        {
            string cst_id = txt_mcsReportTestCstID.Text;
            string cmd_id = txt_mcsReportTestCmdID.Text;
            AVEHICLE test_report_vh = bcApp.SCApplication.VehicleBLL.cache.getVhByID(cmb_mcsReportTestVHID.Text);
            var id_136 = new sc.ProtocolFormat.OHTMessage.ID_136_TRANS_EVENT_REP()
            {
                EventType = report_event,
                BOXID = cst_id,
                BCRReadResult = bCRReadResult,
            };
            var bcfApp = bcApp.SCApplication.getBCFApplication();
            Task.Run(() =>
            {
                dynamic recive_processor = bcApp.SCApplication.VehicleService;
                //bcApp.SCApplication.VehicleService.Receive.TranEventReport(bcfApp, test_report_vh, id_136, 0);
                recive_processor.TranEventReport(bcfApp, test_report_vh, id_136, 0);
            });
        }
        private void McsCommandCompleteTest(sc.ProtocolFormat.OHTMessage.CompleteStatus completeStatus)
        {
            string cmd_id = txt_mcsReportTestCmdID.Text;
            string cst_id = txt_mcsReportTestCstID.Text;
            AVEHICLE test_report_vh = bcApp.SCApplication.VehicleBLL.cache.getVhByID(cmb_mcsReportTestVHID.Text);
            var id_132 = new sc.ProtocolFormat.OHTMessage.ID_132_TRANS_COMPLETE_REPORT()
            {
                CmdID = cmd_id,
                CSTID = cst_id,
                CmpStatus = completeStatus
            };
            var bcfApp = bcApp.SCApplication.getBCFApplication();
            Task.Run(() => bcApp.SCApplication.VehicleService.CommandCompleteReport("", bcfApp, test_report_vh, id_132, 0));
        }

        private void btn_cmp_vh_abort_Click(object sender, EventArgs e)
        {
            var completeStatus = sc.ProtocolFormat.OHTMessage.CompleteStatus.CmpStatusVehicleAbort;
            McsCommandCompleteTest(completeStatus);
        }

        private void btn_unloadComplete_Click(object sender, EventArgs e)
        {

        }

        private void btn_vhloading_Click(object sender, EventArgs e)
        {

        }

        private void btn_initial_Click(object sender, EventArgs e)
        {
            var report_event = sc.ProtocolFormat.OHTMessage.EventType.UnloadArrivals;
            McsReportEventTest(report_event, sc.ProtocolFormat.OHTMessage.BCRReadResult.BcrMisMatch);
        }

        private void cmb_cycleRunBayID_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void cb_passDriveOutByAreaSensor_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.isPassDriveOutByAreaSensor = cb_passDriveOutByAreaSensor.Checked;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            DebugParameter.PreDriveOutDistance_MM = (int)numericUpDown1.Value;
        }

        private void cmb_cycleRunVhId_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void cmb_cycleRunVhId_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DebugParameter.cycleRunVh = cmb_cycleRunVhId.SelectedValue as string;
        }

        private void cmb_cycleRunBayID_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DebugParameter.cycleRunBay = cmb_cycleRunBayID.SelectedValue as string;
        }

        private void cb_ForceStraightPass_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsForceStraightPass = cb_ForceStraightPass.Checked;
        }

        private void cb_ForceNonStraightPass_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsForceNonStraightPass = cb_ForceNonStraightPass.Checked;
        }

        private void combox_cycle_type_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Enum.TryParse(combox_cycle_type.SelectedValue.ToString(), out DebugParameter.CycleRunType type);

            DebugParameter.cycleRunType = type;

            if (type == DebugParameter.CycleRunType.shelfByOrder ||
                type == DebugParameter.CycleRunType.DemoRun)
            {
                gb_cycleRunInfo.Visible = true;
            }
            else
            {
                gb_cycleRunInfo.Visible = false;
            }
        }

        private async void btn_reloadParkingAdr_Click(object sender, EventArgs e)
        {
            try
            {
                string message = $"Do you reload parking addresses?";
                DialogResult confirmResult = MessageBox.Show(this, message,
                    BCApplication.getMessageString("CONFIRM"), MessageBoxButtons.YesNo);

                BCUtility.recordAction(bcApp, this.Name, message, confirmResult.ToString());
                if (confirmResult != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                btn_reloadParkingAdr.Enabled = false;
                await Task.Run(() => bcApp.SCApplication.AddressBLL.reloadAddressType());
            }
            finally
            {
                btn_reloadParkingAdr.Enabled = true;

            }

        }

        private async void bt_ntb_hbReq_Click(object sender, EventArgs e)
        {
            var ntb = bcApp.SCApplication.EquipmentBLL.loadReelNTBs().FirstOrDefault();
            if (ntb == null) return;
            await Task.Run(() => ntb.getReelNTBCDefaultMapActionSend().SendHeartBeatRequest());
        }

        private async void bt_IoPortSignalQuery_Click(object sender, EventArgs e)
        {
            //var ntb = bcApp.SCApplication.EquipmentBLL.loadReelNTBs().FirstOrDefault();
            //if (ntb == null) return;
            //await Task.Run(() => ntb.getReelNTBCDefaultMapActionSend().IoPortSignalQuery());
        }

        private async void bt_ReelStateUpdate_Click(object sender, EventArgs e)
        {
            var ntb = bcApp.SCApplication.EquipmentBLL.loadReelNTBs().FirstOrDefault();
            if (ntb == null) return;
            await Task.Run(() => ntb.getReelNTBCDefaultMapActionSend().ReelStateUpdate());
        }

        private void cb_passTrack_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsPassTrackBlockStatus = cb_passTrack.Checked;
        }

        private async void btn_receiveTraReq_Click(object sender, EventArgs e)
        {
            var ntb = bcApp.SCApplication.EquipmentBLL.loadReelNTBs().FirstOrDefault();
            if (ntb == null) return;

            await Task.Run(() => ntb.CarrierTransferRequestTest("CST03", "P01", "E0081367", "TestPort_P01"));
        }

        private void cb_ignoreNTBPort_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsIgnoreNTBPortStatus = cb_ignoreNTBPort.Checked;
        }

        private void cb_paassErrorVhAndTrackStatus_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsPaassErrorVhAndTrackStatus = cb_paassErrorVhAndTrackStatus.Checked;
        }

        private void txt_cycleCstID_TextChanged(object sender, EventArgs e)
        {
            string scycle_run_csts = txt_cycleCstID.Text;
            DebugParameter.cycleRunCSTs = scycle_run_csts;

        }

        private void cb_IsSameBayAfterWay_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsSameByAfterWay = cb_IsSameBayAfterWay.Checked;
        }

        private void ch_IsAutoDriveOut_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsAutoDriveOut = ch_IsAutoDriveOut.Checked;
        }

        private void cb_isCheckHIDStatus_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsCheckHIDStatus = cb_isCheckHIDStatus.Checked;
        }

        private void btn_allVhContinue_Click(object sender, EventArgs e)
        {
            Task.Run(() => bcApp.SCApplication.VehicleService.AllVhContinue());
        }

        private void cb_testHIDAbnormalVh_SelectionChangeCommitted(object sender, EventArgs e)
        {
            DebugParameter.TestHIDAbnormalVhID = cb_testHIDAbnormalVh.SelectedValue as string;
        }

        private void cb_autoUnloadOnVh_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsAutoUnloadOnvh = cb_autoUnloadOnVh.Checked;
        }

        private void cb_unloadArrivePassReply01_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR01", cb_unloadArrivePassReply01.Checked);
        }
        private void setUnloadArrivePassReplyFlag(string vhID, bool isOpen)
        {
            var vh1 = bcApp.SCApplication.VehicleBLL.cache.getVhByID(vhID);
            if (vh1 == null) return;
            vh1.IsUnloadArriveByPassReply = isOpen;
        }
        private void setUnloadArrivePassReplyCheckBox(string vhID, CheckBox checkBox)
        {
            var vh1 = bcApp.SCApplication.VehicleBLL.cache.getVhByID(vhID);
            if (vh1 == null) return;
            checkBox.Checked = vh1.IsUnloadArriveByPassReply;
        }

        private void cb_unloadArrivePassReply02_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR02", cb_unloadArrivePassReply02.Checked);
        }

        private void cb_unloadArrivePassReply03_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR03", cb_unloadArrivePassReply03.Checked);
        }

        private void cb_unloadArrivePassReply05_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR05", cb_unloadArrivePassReply05.Checked);
        }

        private void cb_unloadArrivePassReply06_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR06", cb_unloadArrivePassReply06.Checked);
        }

        private void cb_unloadArrivePassReply07_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR07", cb_unloadArrivePassReply07.Checked);
        }

        private void cb_unloadArrivePassReply08_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR08", cb_unloadArrivePassReply08.Checked);
        }

        private void cb_unloadArrivePassReply09_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR09", cb_unloadArrivePassReply09.Checked);
        }

        private void cb_unloadArrivePassReply10_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR10", cb_unloadArrivePassReply10.Checked);
        }

        private void cb_unloadArrivePassReply11_CheckedChanged(object sender, EventArgs e)
        {
            setUnloadArrivePassReplyFlag("B6_OHB01_CR11", cb_unloadArrivePassReply11.Checked);
        }

        private void cb_openSpecifyVh_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsSpecifyVhTransfer = cb_openSpecifyVh.Checked;
        }

        private async void btnVhTypeUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                E_VH_TYPE vh_type;
                Enum.TryParse(cmbVhType.SelectedValue.ToString(), out vh_type);
                btnVhTypeUpdate.Enabled = false;
                await Task.Run(() => bcApp.SCApplication.VehicleService.updateVhType(vh_id, vh_type));
                cmbVhType.SelectedItem = noticeCar.VEHICLE_TYPE;
                MessageBox.Show($"update vh:[{vh_id}] to type:[{vh_type}] is success.");
            }
            finally
            {
                btnVhTypeUpdate.Enabled = true;
            }
        }

        private void cb_openGuideSectionChange_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsOpneChangeGuideSection = cb_openGuideSectionChange.Checked;
        }

        private void btn_manualPortDuplicatTest_Click(object sender, EventArgs e)
        {
            Task.Run(() => bcApp.SCApplication.ManualPortEventService.WaitInTest());
        }

        private void cb_SyncWithOHTWhenConnection_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsSyncWhenConnectionEvent = cb_SyncWithOHTWhenConnection.Checked;
        }

        private void cb_cloesCorrectGuideInfoWhenSameLoadAdr_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsCloseCorrectGuideInfoWhenSameLoadAdr = cb_cloesCorrectGuideInfoWhenSameLoadAdr.Checked;
        }

        private void cb_openContinueNotifyFun_CheckedChanged(object sender, EventArgs e)
        {
            sc.App.SystemParameter.setIsOpenContinueNotifyWhenVehicleTimeout(cb_openContinueNotifyFun.Checked);
        }

        private void cb_isOpenReelNTBPortStatusAsk_CheckedChanged(object sender, EventArgs e)
        {
            sc.App.SystemParameter.setIsOpenReelNTBPortStatusAsk(cb_isOpenReelNTBPortStatusAsk.Checked);
        }

        private void cb_openTrackResetByVhBlockRelease_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsOpenTrackResetByVhBlockRelease = cb_openTrackResetByVhBlockRelease.Checked;
        }

        private void cb_checkTrackAlive_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsOpenCheckTrackAlive = cb_checkTrackAlive.Checked;
        }

        private void cb_IsOpenParkingZoneControlFunction_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsOpenParkingZoneControlFunction = cb_IsOpenParkingZoneControlFunction.Checked;
        }

        private void cb_IsOpenParkingZoneAutoPull_CheckedChanged(object sender, EventArgs e)
        {
            DebugParameter.IsOpenParkingZoneAutoPull = cb_IsOpenParkingZoneAutoPull.Checked;

        }
    }
}
