using com.mirle.ibg3k0.sc.BLL._191204Test.Extensions;
using com.mirle.ibg3k0.sc.BLL.Interface;
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
                WriteLog($"Process Port:{efem_valus_map_action.PortName} alarm all clear.");
                scApp.TransferService.OHBC_AlarmAllCleared(efem_valus_map_action.PortName);
                WriteLog($"End process Port:{efem_valus_map_action.PortName} alarm all clear.");
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
                UInt16[] alarm_codes = args.efemPortInfo.AlarmCodes;
                WriteLog($"Process Port:{efem_valus_map_action.PortName} alarm report,alarm code:{string.Join(",", alarm_codes)}.");
                foreach (var alarm_code in alarm_codes)
                {
                    string s_alarm_code = alarm_code.ToString();
                    scApp.TransferService.OHBC_AlarmSet(efem_valus_map_action.PortName, s_alarm_code);
                }
                WriteLog($"set error index:{args.efemPortInfo.ErrorIndex} to Port:{efem_valus_map_action.PortName}.");
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


    }
}