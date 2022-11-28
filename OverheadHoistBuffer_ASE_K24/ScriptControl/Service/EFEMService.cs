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

            foreach (var port in ports)
            {
                port.OnAlarmHappen += Port_OnAlarmHappen;
                port.OnAlarmClear += Port_OnAlarmClear;

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
                WriteLog($"Process eq:{port_station.EQPT_ID} Port:{efem_valus_map_action.PortName} alarm report,alarm code:{string.Join(",", alarm_codes)}.");
                foreach (var alarm_code in alarm_codes)
                {
                    if (alarm_code == 0)
                        continue;
                    string s_alarm_code = alarm_code.ToString();
                    scApp.TransferService.OHBC_AlarmSet(port_station.EQPT_ID, s_alarm_code);
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
        private long SyncPointe = 0;
        public void checkIsNeedToNotifyEFEMEqHasCSTWillIn()
        {
            if (DebugParameter.IsOpenByPassEFEMStatus)
            {
                WriteLog($"目前開啟By pass EFEM 狀態，不主動觸發通知PLC取貨訊號.");
                return;
            }
            if (Interlocked.Exchange(ref SyncPointe, 1) == 0)
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
                    Interlocked.Exchange(ref SyncPointe, 0);
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
    }
}