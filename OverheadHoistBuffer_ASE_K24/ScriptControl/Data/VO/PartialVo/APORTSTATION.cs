using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using System.Diagnostics;
using System.Threading;

namespace com.mirle.ibg3k0.sc
{
    public partial class APORTSTATION : BaseEQObject
    {

        public APORTSTATION()
        {
            eqptObjectCate = SCAppConstants.EQPT_OBJECT_PORT_STATION;
        }

        public string CST_ID { get; set; }
        public string EQPT_ID { get; set; }
        public string CARRIER_CST_TYPE { get; set; }

        public override void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            foreach (IValueDefMapAction action in valueDefMapActionDic.Values)
            {
                action.doShareMemoryInit(runLevel);
            }
        }

        public override string ToString()
        {
            return $"{PORT_ID} ({ADR_ID})";
        }

        public virtual PortPLCInfo getPortPLCInfo()
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return null;
            return portValueDefMapAction.GetPortState() as PortPLCInfo;
        }

        protected virtual ICommonPortInfoValueDefMapAction getICommonPortInfoValueDefMapAction()
        {
            ICommonPortInfoValueDefMapAction portValueDefMapAction =
                getMapActionByIdentityKey(typeof(Data.ValueDefMapAction.PortValueDefMapAction).Name) as ICommonPortInfoValueDefMapAction;
            return portValueDefMapAction;
        }

        public virtual void ChangeToInMode(bool isOn)
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.ChangeToInModeAsync(isOn);
        }

        private long syncInModePoint = 0;
        public void ChangeToInModeAndAutoReset()
        {
            if (Interlocked.Exchange(ref syncInModePoint, 1) == 0)
            {
                NLog.LogManager.GetLogger("TransferServiceLogger").Info($"進行port id {PORT_ID} In Mode Change流程...");
                try
                {
                    var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
                    if (portValueDefMapAction == null) return;
                    portValueDefMapAction.ChangeToInModeAsync(true);
                    System.Threading.SpinWait.SpinUntil(() => false, 5_000);
                    portValueDefMapAction.ChangeToInModeAsync(false);
                }
                finally
                {
                    Interlocked.Exchange(ref syncInModePoint, 0);
                }
            }
            else
            {
                NLog.LogManager.GetLogger("TransferServiceLogger").Warn($"port id {PORT_ID} 正在進行In Mode Change流程中，無法再次下達");
            }
        }
        public void ChangeToOutMode(bool isOn)
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.ChangeToOutModeAsync(isOn);
        }

        public void ResetAlarm()
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.ResetAlarmAsync();
        }

        public void StopBuzzer()
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.StopBuzzerAsync();
        }

        public void SetRun()
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.SetRunAsync();
        }

        public void SetStop()
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.SetStopAsync();
        }

        public void SetCommanding(bool isCommanding)
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.SetCommandingAsync(isCommanding);
        }
        public void SetHeartBeat(bool setOn)
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.HeartBeatAsync(setOn);
        }

        public void SetControllerErrorIndex(int index)
        {
            var portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.SetControllerErrorIndexAsync(index);
        }

        public bool IsEqPort(BLL.EquipmentBLL equipmentBLL)
        {
            var eq = equipmentBLL.cache.getEqpt(EQPT_ID);
            if (eq == null)
            {
                NLog.LogManager.GetCurrentClassLogger().Warn($"EQPT_ID:{EQPT_ID} no define");
                return false;
            }
            return eq.Type == SCAppConstants.EqptType.Equipment;
        }

        const int MAX_ALIVE_TIME_OUT_MILLISECION = 10_000;
        public Stopwatch AliveStopwatch = new Stopwatch();
        public bool IsAlive
        {
            get
            {
                if (AliveStopwatch.IsRunning)
                {
                    return AliveStopwatch.ElapsedMilliseconds < MAX_ALIVE_TIME_OUT_MILLISECION;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}