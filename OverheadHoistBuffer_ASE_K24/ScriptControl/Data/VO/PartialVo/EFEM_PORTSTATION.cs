using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.EFEM;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using Mirle.U332MA30.Grpc.OhbcNtbcConnect;
using System;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public class EFEM_PORTSTATION : APORTSTATION
    {
        public EFEM_PORTSTATION() : base()
        {
        }
        public IEFEMValueDefMapAction getExcuteMapAction()
        {
            IEFEMValueDefMapAction mapAction = this.getMapActionByIdentityKey(typeof(EFEMPortStationDefaultValueDefMapAction).Name) as IEFEMValueDefMapAction;
            return mapAction;
        }

        public override PortPLCInfo getPortPLCInfo()
        {
            ICommonPortInfoValueDefMapAction portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return null;

            var mgv_port_info = portValueDefMapAction.GetPortState() as EFEMPortPLCInfo;

            return EFEMPortInfoToPortInfo(mgv_port_info);
        }
        public EFEMPortPLCInfo getEFEMPortPLCInfo()
        {
            ICommonPortInfoValueDefMapAction portValueDefMapAction = getICommonPortInfoValueDefMapAction();
            if (portValueDefMapAction == null) return null;

            var efem_port_info = portValueDefMapAction.GetPortState() as EFEMPortPLCInfo;

            return efem_port_info;
        }

        public override void ChangeToInMode(bool isOn)
        {
            var portValueDefMapAction = getExcuteMapAction();
            if (portValueDefMapAction == null) return;
            portValueDefMapAction.NotifyAcquireStartedFromEQPortAsync(isOn);
        }
        private PortPLCInfo EFEMPortInfoToPortInfo(EFEMPortPLCInfo efemPortInfo)
        {
            return new PortPLCInfo()
            {
                EQ_ID = efemPortInfo.EQ_ID,
                OpAutoMode = efemPortInfo.IsRun,
                OpManualMode = efemPortInfo.IsDown,
                OpError = false,
                IsInputMode = efemPortInfo.IsInMode,
                IsOutputMode = efemPortInfo.IsOutMode,
                IsModeChangable = false,
                IsAGVMode = false,
                IsMGVMode = false,
                PortWaitIn = false,
                PortWaitOut = false,
                IsAutoMode = false,
                IsReadyToLoad = efemPortInfo.IsLoadOK,
                IsReadyToUnload = efemPortInfo.IsUnloadOK,
                LoadPosition1 = efemPortInfo.LoadPosition1,
                LoadPosition2 = false,
                LoadPosition3 = false,
                LoadPosition4 = false,
                LoadPosition5 = false,
                LoadPosition7 = false,
                LoadPosition6 = false,
                IsCSTPresence = false,
                AGVPortReady = false,
                CanOpenBox = false,
                IsBoxOpen = false,
                BCRReadDone = false,
                CSTPresenceMismatch = false,
                IsTransferComplete = false,
                CstRemoveCheck = false,
                BoxID = "",
                LoadPositionBOX1 = "",
                LoadPositionBOX2 = "",
                LoadPositionBOX3 = "",
                LoadPositionBOX4 = "",
                LoadPositionBOX5 = "",
                CassetteID = "",
                FireAlarm = false,
                cim_on = false,
                preLoadOK = false,
            };
        }

        protected override ICommonPortInfoValueDefMapAction getICommonPortInfoValueDefMapAction()
        {
            ICommonPortInfoValueDefMapAction portValueDefMapAction =
                getMapActionByIdentityKey(typeof(Data.ValueDefMapAction.EFEMPortStationDefaultValueDefMapAction).Name) as ICommonPortInfoValueDefMapAction;
            return portValueDefMapAction;
        }


        public PortState state;
        public DirectionType direction;
        public RequestType requestType;
        public string CarrierReelId;

    }
}