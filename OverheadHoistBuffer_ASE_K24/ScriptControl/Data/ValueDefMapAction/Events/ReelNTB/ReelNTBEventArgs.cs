﻿using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using System;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ReelNTB
{
    public class ReelNTBEventArgs : EventArgs
    {
        public com.mirle.ibg3k0.sc.Data.VO.ReelNTB ReelNTB;
        public ReelNTBEventArgs()
        {
        }
    }
    public class ReelNTBTranCmdReqEventArgs : ReelNTBEventArgs
    {
        Mirle.U332MA30.Grpc.OhbcNtbcConnect.TransferCommandRequset transferCommandRequset;
        public string CarrierReelId { get => transferCommandRequset?.CarrierReelId; }
        public string SourcePortName { get => transferCommandRequset?.SourcePortName; }
        public string DestinationEqPortName { get => transferCommandRequset?.DestinationEqPortName; }
        public string DestinationEqPortId { get => transferCommandRequset?.DestinationEqPortId; }
        public ReelNTBTranCmdReqEventArgs(VO.ReelNTB _ReelNTB, Mirle.U332MA30.Grpc.OhbcNtbcConnect.TransferCommandRequset _transferCommandRequset)
        {
            ReelNTB = _ReelNTB;
            transferCommandRequset = _transferCommandRequset;
        }
    }

}