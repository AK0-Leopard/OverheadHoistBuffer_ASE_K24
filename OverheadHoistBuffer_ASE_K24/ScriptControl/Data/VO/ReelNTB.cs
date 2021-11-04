using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class ReelNTB : AEQPT
    {
        public event EventHandler<ACMD_MCS> RelatedReelCSTReceiveMCSCmd;
        public event EventHandler<ACMD_MCS> RelatedReelCSTTransferring;
        public event EventHandler<ACMD_MCS> RelatedReelCSTTransfeFail;
        public event EventHandler<ACMD_MCS> RelatedReelCSTArrived;

        public Data.ValueDefMapAction.ReelNTBC.ReelNTBCDefaultMapActionSend getReelNTBCDefaultMapActionSend()
        {
            Data.ValueDefMapAction.ReelNTBC.ReelNTBCDefaultMapActionSend portValueDefMapAction =
                getMapActionByIdentityKey(typeof(Data.ValueDefMapAction.ReelNTBC.ReelNTBCDefaultMapActionSend).Name) as
                Data.ValueDefMapAction.ReelNTBC.ReelNTBCDefaultMapActionSend;
            return portValueDefMapAction;
        }
        public Data.ValueDefMapAction.ReelNTBC.ReelNTBCDefaultMapActionReceive getReelNTBCDefaultMapActionReceive()
        {
            Data.ValueDefMapAction.ReelNTBC.ReelNTBCDefaultMapActionReceive portValueDefMapAction =
                getMapActionByIdentityKey(typeof(Data.ValueDefMapAction.ReelNTBC.ReelNTBCDefaultMapActionReceive).Name) as
                Data.ValueDefMapAction.ReelNTBC.ReelNTBCDefaultMapActionReceive;
            return portValueDefMapAction;
        }

        public Mirle.U332MA30.Grpc.OhbcNtbcConnect.OhtPortSignal GetOhtPortSignal(string portName)
        {
            var map_action_send = getReelNTBCDefaultMapActionSend();
            return map_action_send.IoPortSignalQuery(portName);
        }
        public bool ReelStateUpdate(string cstID, Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState state, bool isToEQ, string mcsCmdID)
        {
            var map_action_send = getReelNTBCDefaultMapActionSend();
            return map_action_send.ReelStateUpdate(cstID, state, isToEQ, mcsCmdID);
        }

        public void onRelatedReelCSTReceiveMCSCmd(ACMD_MCS _acmdMCS)
        {
            RelatedReelCSTReceiveMCSCmd?.Invoke(this, _acmdMCS);
        }
        public void onRelatedReelCSTTransferring(ACMD_MCS _acmdMCS)
        {
            RelatedReelCSTTransferring?.Invoke(this, _acmdMCS);
        }
        public void onRelatedReelCSTTransfeFail(ACMD_MCS _acmdMCS)
        {
            RelatedReelCSTTransfeFail?.Invoke(this, _acmdMCS);
        }
        public void onRelatedReelCSTArrived(ACMD_MCS _acmdMCS)
        {
            RelatedReelCSTArrived?.Invoke(this, _acmdMCS);
        }


        public void CarrierTransferRequestTest(string cstID, string destEqPortID, string destinationEqPortName, string sourcePortName)
        {
            Mirle.U332MA30.Grpc.OhbcNtbcConnect.TransferCommandRequset r = new Mirle.U332MA30.Grpc.OhbcNtbcConnect.TransferCommandRequset()
            {
                CarrierReelId = cstID,
                DestinationEqPortId = destEqPortID,
                DestinationEqPortName = destinationEqPortName,
                SourcePortName = sourcePortName
            };
            getReelNTBCDefaultMapActionReceive().CarrierTransferRequest(r, null);
        }
    }

}
