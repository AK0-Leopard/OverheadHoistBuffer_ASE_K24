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


    }
}
