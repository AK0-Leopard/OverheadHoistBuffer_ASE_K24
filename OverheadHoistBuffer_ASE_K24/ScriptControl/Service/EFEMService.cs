using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL._191204Test.Extensions;
using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Extension;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.EFEM;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Service.Interface;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace com.mirle.ibg3k0.sc.Service
{
    public class EFEMService
    {
        private Logger logger = LogManager.GetLogger("EFEMLogger");

        private string now { get => DateTime.Now.ToString("HH:mm:ss.fff"); }
        private ConcurrentDictionary<string, IEFEMValueDefMapAction> efemPorts { get; set; }
        private ConcurrentDictionary<string, string> comingOutCarrierOfEFEMPorts { get; set; }
        private ConcurrentDictionary<string, string> lastComingOutCarrierOfEFEMPorts { get; set; }

        sc.App.SCApplication scApp = null;

        public EFEMService()
        {
            WriteLog("");
            WriteLog("");
            WriteLog("");
            WriteLog($"New EFEMService");
        }

        public void Start(sc.App.SCApplication _scApp, IEnumerable<IEFEMValueDefMapAction> ports)
        {
            scApp = _scApp;
            WriteLog($"EFEMService Start");

            RegisterEvent(ports);
        }

        private void RegisterEvent(IEnumerable<IEFEMValueDefMapAction> ports)
        {
            comingOutCarrierOfEFEMPorts = new ConcurrentDictionary<string, string>();
            lastComingOutCarrierOfEFEMPorts = new ConcurrentDictionary<string, string>();
            efemPorts = new ConcurrentDictionary<string, IEFEMValueDefMapAction>();
            foreach (var port in ports)
            {
                port.OnAlarmHappen += Port_OnAlarmHappen;
                port.OnAlarmClear += Port_OnAlarmClear;


                efemPorts.TryAdd(port.PortName, port);

                comingOutCarrierOfEFEMPorts.TryAdd(port.PortName, string.Empty);
                lastComingOutCarrierOfEFEMPorts.TryAdd(port.PortName, string.Empty);

                WriteLog($"Add EFEM Event Success ({port.PortName})");
            }
        }

        private void Port_OnAlarmClear(object sender, EFEMEventArgs args)
        {
            try
            {
                IEFEMValueDefMapAction efem_valus_map_action = sender as IEFEMValueDefMapAction;
                var port_station = scApp.PortStationBLL.OperateCatch.getPortStationByID(efem_valus_map_action.PortName);

                WriteLog($"Process eq:{port_station.EQPT_ID} Port:{efem_valus_map_action.PortName} alarm all clear.");
                scApp.TransferService.OHBC_AlarmAllCleared(port_station.EQPT_ID);
                WriteLog($"End process eq:{port_station.EQPT_ID} Port:{efem_valus_map_action.PortName} alarm all clear.");
                efem_valus_map_action.SetControllerErrorIndexAsync(args.efemPortInfo.ErrorIndex);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        private void Port_OnAlarmHappen(object sender, EFEMEventArgs args)
        {
            try
            {
                IEFEMValueDefMapAction efem_valus_map_action = sender as IEFEMValueDefMapAction;
                var port_station = scApp.PortStationBLL.OperateCatch.getPortStationByID(efem_valus_map_action.PortName);
                UInt16[] alarm_codes = args.efemPortInfo.AlarmCodes;
                string on_eq_cst_id = args.efemPortInfo.CarrierIdReadResult;

                WriteLog($"Process eq:{port_station.EQPT_ID} Port:{efem_valus_map_action.PortName} alarm report,alarm code:{string.Join(",", alarm_codes)}, cst id:{on_eq_cst_id}.");
                foreach (var alarm_code in alarm_codes)
                {
                    if (alarm_code == 0)
                        continue;
                    string s_alarm_code = alarm_code.ToString();
                    scApp.TransferService.OHBC_AlarmSet(port_station.EQPT_ID, s_alarm_code, "", on_eq_cst_id);
                }
                WriteLog($"set error index:{args.efemPortInfo.ErrorIndex} to eq:{port_station.EQPT_ID} Port:{efem_valus_map_action.PortName}.");
                efem_valus_map_action.SetControllerErrorIndexAsync(args.efemPortInfo.ErrorIndex);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        #region Log

        private void WriteLog(string message)
        {
            var logMessage = $"[{now}] {message}";
            logger.Info(logMessage);
        }

        private void WriteEventLog(string message)
        {
            var logMessage = $"[{now}] PLC Event | {message}";
            logger.Info(logMessage);
        }

        #endregion Log
        private long SyncPointe_NeedNotyfyWaitIn = 0;
        public void checkIsNeedToNotifyEFEMEqHasCSTWillIn()
        {
            if (DebugParameter.IsOpenByPassEFEMStatus)
            {
                WriteLog($"目前開啟By pass EFEM 狀態，不主動觸發通知PLC取貨訊號.");
                return;
            }
            if (Interlocked.Exchange(ref SyncPointe_NeedNotyfyWaitIn, 1) == 0)
            {
                try
                {
                    var efem_ports = scApp.PortStationBLL.OperateCatch.loadAllEFEMPortStation();
                    var current_cmd_mcs = ACMD_MCS.tryGetMCSCommandList();
                    foreach (var port in efem_ports)
                    {
                        var efem_port_plc_info = port.getEFEMPortPLCInfo();
                        bool is_true_on_notify = checkIsNeedNotify(current_cmd_mcs, port);
                        if (is_true_on_notify)
                        {
                            if (efem_port_plc_info.IsNotifyAcquireStarted == false)
                            {
                                WriteLog($"開啟[IsNotifyAcquireStarted]訊號.");
                                port.ChangeToInMode(true);
                            }
                        }
                        else
                        {
                            if (efem_port_plc_info.IsNotifyAcquireStarted == true)
                            {
                                WriteLog($"關閉[IsNotifyAcquireStarted]訊號.");
                                port.ChangeToInMode(false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    Interlocked.Exchange(ref SyncPointe_NeedNotyfyWaitIn, 0);
                }
            }
        }

        private bool checkIsNeedNotify(List<ACMD_MCS> current_cmd_mcs, EFEM_PORTSTATION port)
        {
            var cmd_from_efem = current_cmd_mcs.Where(cmd => SCUtility.isMatche(port.PORT_ID, cmd.HOSTSOURCE)).
                                                FirstOrDefault();
            if (cmd_from_efem == null)
                return false;
            if (cmd_from_efem.IsQueue)
            {
                return true;
            }
            else
            {
                if (cmd_from_efem.IsTransferring)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private long SyncPointe_EfemHearBeat = 0;
        private bool LAST_HEART_BEAT_STATUS = false;
        private void EfemPortHearbeatPulse()
        {
            if (Interlocked.Exchange(ref SyncPointe_EfemHearBeat, 1) == 0)
            {
                bool current_set_heart_beat_status = LAST_HEART_BEAT_STATUS ? false : true;
                try
                {
                    var efem_ports = scApp.PortStationBLL.OperateCatch.loadAllEFEMPortStation();
                    foreach (var efem_port in efem_ports)
                    {
                        efem_port.SetHeartBeat(current_set_heart_beat_status);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    Interlocked.Exchange(ref SyncPointe_EfemHearBeat, 0);
                    LAST_HEART_BEAT_STATUS = current_set_heart_beat_status;
                }
            }
        }
        private long SyncPointe_ReflashComingOutCarrier = 0;
        private void ReflashComingOutCarrier()
        {
            if (Interlocked.Exchange(ref SyncPointe_ReflashComingOutCarrier, 1) == 0)
            {
                try
                {
                    var commandsOfOHT = ACMD_OHTC.CMD_OHTC_InfoList;
                    var efem_ports = scApp.PortStationBLL.OperateCatch.loadAllEFEMPortStation();
                    foreach (var portItem in efem_ports)
                    {
                        var portName = portItem.PORT_ID;
                        comingOutCarrierOfEFEMPorts[portName] = string.Empty;

                        var cmds = commandsOfOHT.Where(c => SCUtility.isMatche(c.Value.DESTINATION.Trim(), portName) &&
                                                            c.Value.CMD_STAUS >= E_CMD_STATUS.Execution
                                                            ).Select(c => c.Value).ToList();

                        var cmd = cmds.FirstOrDefault();
                        if (cmd == null)
                            continue;

                        comingOutCarrierOfEFEMPorts[portName] = cmd.BOX_ID.Trim();
                    }

                    foreach (var item in comingOutCarrierOfEFEMPorts)
                    {
                        var carrierId = item.Value;

                        if (lastComingOutCarrierOfEFEMPorts[item.Key] == carrierId)
                            continue;

                        lastComingOutCarrierOfEFEMPorts[item.Key] = carrierId;

                        if (SCUtility.isEmpty(item.Value))
                        {
                            WriteLog($"{item.Key} Has no carrier coming out. 保持上筆紀錄");
                        }
                        else
                        {
                            WriteLog($"{item.Key} Has carrier coming out. Show PLC Monitor ({carrierId}).  有「正在出庫的」的 ID");

                            efemPorts[item.Key].ShowComingOutCarrierOnMonitorAsync(carrierId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                    WriteLog($"{MethodBase.GetCurrentMethod()}, Exception Happen: (( {ex} ))");
                }
                finally
                {
                    Interlocked.Exchange(ref SyncPointe_ReflashComingOutCarrier, 0);
                }
            }
        }


        public void ReflashState()
        {
            try
            {
                checkIsNeedToNotifyEFEMEqHasCSTWillIn();
                EfemPortHearbeatPulse();
                ReflashComingOutCarrier();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

    }
}