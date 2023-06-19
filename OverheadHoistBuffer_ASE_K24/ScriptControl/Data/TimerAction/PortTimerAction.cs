using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    public class PortTimerAction : ITimerAction
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected SCApplication scApp = null;
        protected MPLCSMControl smControl;

        public PortTimerAction(string name, long intervalMilliSec) : base(name, intervalMilliSec)
        {
        }

        public override void initStart()
        {
            scApp = SCApplication.getInstance();
        }

        public override void doProcess(object obj)
        {
            try
            {
                if (SystemParameter.IsOpenReelNTBPortStatusAsk)
                    Task.Run(() => scApp.ReelNTBEventService.RefreshReelNTBPortSignal());
                scApp.ManualPortControlService?.ReflashState();
                scApp.EFEMService?.ReflashState();

                //EFEM_PORT_HEARBEAT_PULSE();
                //scApp.EFEMService.checkIsNeedToNotifyEFEMEqHasCSTWillIn();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private long EFEMPortHeartBeatSyncPointe = 0;
        private bool LAST_HEART_BEAT_STATUS = false;
        private void EFEM_PORT_HEARBEAT_PULSE()
        {
            if (Interlocked.Exchange(ref EFEMPortHeartBeatSyncPointe, 1) == 0)
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
                    Interlocked.Exchange(ref EFEMPortHeartBeatSyncPointe, 0);
                    LAST_HEART_BEAT_STATUS = current_set_heart_beat_status;
                }
            }
        }
    }
}