﻿using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;

namespace com.mirle.ibg3k0.sc
{
    public partial class MGV_PORTSTATION : APORTSTATION
    {
        public MGV_PORTSTATION() : base()
        {

        }
        public IManualPortValueDefMapAction getExcuteMapAction()
        {
            IManualPortValueDefMapAction mapAction = this.getMapActionByIdentityKey(typeof(MGVDefaultValueDefMapAction).Name) as IManualPortValueDefMapAction;
            return mapAction;
        }
        public ManualPortPLCInfo getManualPortPLCInfo()
        {
            ICommonPortInfoValueDefMapAction portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return null;

            var manual_port_info = portValueDefMapAction.GetPortState() as ManualPortPLCInfo;
            return manual_port_info;
        }
        public override PortPLCInfo getPortPLCInfo()
        {
            ICommonPortInfoValueDefMapAction portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return null;

            var mgv_port_info = portValueDefMapAction.GetPortState() as ManualPortPLCInfo;

            return MgvPortInfoToPortInfo(mgv_port_info);
        }
        protected override ICommonPortInfoValueDefMapAction getICommonPortInfoValueDefMapAction()
        {
            ICommonPortInfoValueDefMapAction portValueDefMapAction =
                getMapActionByIdentityKey(typeof(Data.ValueDefMapAction.MGVDefaultValueDefMapAction).Name) as ICommonPortInfoValueDefMapAction;
            return portValueDefMapAction;
        }
        private IManualPortValueDefMapAction getIManualPortValueDefMapAction()
        {
            IManualPortValueDefMapAction portValueDefMapAction =
                getMapActionByIdentityKey(typeof(Data.ValueDefMapAction.MGVDefaultValueDefMapAction).Name) as IManualPortValueDefMapAction;
            return portValueDefMapAction;
        }
        private PortPLCInfo MgvPortInfoToPortInfo(Data.PLC_Functions.MGV.ManualPortPLCInfo mgvPortInfo)
        {
            return new PortPLCInfo()
            {
                OpAutoMode = mgvPortInfo.IsRun,
                OpManualMode = mgvPortInfo.IsDown,
                OpError = mgvPortInfo.IsAlarm,
                IsInputMode = mgvPortInfo.IsInMode,
                IsOutputMode = mgvPortInfo.IsOutMode,
                IsModeChangable = mgvPortInfo.IsDirectionChangable,
                IsAGVMode = false,
                IsMGVMode = false,
                PortWaitIn = mgvPortInfo.IsWaitIn,
                PortWaitOut = mgvPortInfo.IsWaitOut,
                IsAutoMode = mgvPortInfo.RunEnable,
                IsReadyToLoad = mgvPortInfo.IsLoadOK,
                IsReadyToUnload = mgvPortInfo.IsUnloadOK,
                LoadPosition1 = mgvPortInfo.LoadPosition1,
                LoadPosition2 = mgvPortInfo.LoadPosition2,
                LoadPosition3 = mgvPortInfo.LoadPosition3,
                LoadPosition4 = mgvPortInfo.LoadPosition4,
                LoadPosition5 = mgvPortInfo.LoadPosition5,
                LoadPosition7 = false,
                LoadPosition6 = false,
                IsCSTPresence = false,
                AGVPortReady = false,
                CanOpenBox = false,
                IsBoxOpen = false,
                BCRReadDone = mgvPortInfo.IsBcrReadDone,
                CSTPresenceMismatch = false,
                IsTransferComplete = mgvPortInfo.IsTransferComplete,
                CstRemoveCheck = mgvPortInfo.IsRemoveCheck,
                //ErrorCode = mgvPortInfo.IsBcrReadDone,
                BoxID = mgvPortInfo.CarrierIdOfStage1,
                LoadPositionBOX1 = "",
                LoadPositionBOX2 = "",
                LoadPositionBOX3 = "",
                LoadPositionBOX4 = "",
                LoadPositionBOX5 = "",
                CassetteID = "",
                FireAlarm = false,
                cim_on = false,
                preLoadOK = false,
                //ErrorCode = mgvPortInfo.AlarmCode

            };
        }


        #region Control Port
        public void MoveBackAsync()
        {
            var manualPortValueDefMapAction = getIManualPortValueDefMapAction();
            if (manualPortValueDefMapAction == null) return;
            manualPortValueDefMapAction.MoveBackAsync();
        }
        public void SetMoveBackReasonAsync(MoveBackReasons moveBackReasons)
        {
            var manualPortValueDefMapAction = getIManualPortValueDefMapAction();
            if (manualPortValueDefMapAction == null) return;
            manualPortValueDefMapAction.SetMoveBackReasonAsync(moveBackReasons);
        }
        #endregion Control Port

    }

}
