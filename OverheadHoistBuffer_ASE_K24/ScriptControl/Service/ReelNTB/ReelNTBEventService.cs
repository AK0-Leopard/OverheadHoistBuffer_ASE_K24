using com.mirle.ibg3k0.sc.BLL._191204Test.Extensions;
using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Extension;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Service.Interface;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using static com.mirle.ibg3k0.sc.ACMD_MCS;

namespace com.mirle.ibg3k0.sc.Service
{
    public class ReelNTBEventService : IManualPortEventService
    {
        private Logger logger = LogManager.GetLogger("ManualPortLogger");

        private string now { get => DateTime.Now.ToString("HH:mm:ss.fff"); }

        private ConcurrentDictionary<string, IManualPortValueDefMapAction> manualPorts { get; set; }

        private IManualPortReportBLL reportBll;
        private IManualPortCMDBLL commandBLL;
        private IManualPortAlarmBLL alarmBLL;
        private IManualPortDefBLL portDefBLL;
        private IManualPortShelfDefBLL shelfDefBLL;
        private IManualPortCassetteDataBLL cassetteDataBLL;

        private const string LITE_CASSETTE = "LC";
        private const string FOUP = "BE";

        public ReelNTBEventService()
        {
            WriteLog("");
            WriteLog("");
            WriteLog("");
            WriteLog($"New ManualPortEventService");
        }

        public void Start(IEnumerable<IManualPortValueDefMapAction> ports,
                          IManualPortReportBLL reportBll,
                          IManualPortDefBLL portDefBLL,
                          IManualPortShelfDefBLL shelfDefBLL,
                          IManualPortCassetteDataBLL cassetteDataBLL,
                          IManualPortCMDBLL commandBLL,
                          IManualPortAlarmBLL alarmBLL)
        {
            this.reportBll = reportBll;
            this.portDefBLL = portDefBLL;
            this.shelfDefBLL = shelfDefBLL;
            this.cassetteDataBLL = cassetteDataBLL;
            this.commandBLL = commandBLL;
            this.alarmBLL = alarmBLL;

            WriteLog($"ReelNTBEventService Start");

            RegisterEvent(ports);
        }

        private void RegisterEvent(IEnumerable<IManualPortValueDefMapAction> ports)
        {
            
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