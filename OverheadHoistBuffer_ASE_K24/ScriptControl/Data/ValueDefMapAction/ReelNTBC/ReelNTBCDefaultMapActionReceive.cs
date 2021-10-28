using com.mirle.AK0.ProtocolFormat;
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ReelNTB;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using Grpc.Core;
using Mirle.U332MA30.Grpc.OhbcNtbcConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.ReelNTBC
{
    public class ReelNTBCDefaultMapActionReceive : Mirle.U332MA30.Grpc.OhbcNtbcConnect.NtbcToOhbcService.NtbcToOhbcServiceBase, IReelNTBValueDefMapAction
    {
        public event ReelNTBEvents.ReelNTBTranCmdReqEventHandler TransferCommandRequest;

        public override Task<TransferCommandAck> CarrierTransferRequest(TransferCommandRequset request, ServerCallContext context)
        {
            LogHelper.RecordHostReportInfo(request, method: "CarrierTransferRequest");
            TransferCommandAck ask = new TransferCommandAck();
            string resvice_message = request.ToString();
            Console.WriteLine(resvice_message);
            ask.ReplyCode = Ack.Ok;
            LogHelper.RecordHostReportInfoAsk(ask, method: "CarrierTransferRequest");

            TransferCommandRequest?.Invoke(this, new ReelNTBTranCmdReqEventArgs(request));

            return Task.FromResult(ask);
        }


        public override Task<OhtPortStateAck> GetOhtPortState(OhtPortQuery request, ServerCallContext context)
        {
            LogHelper.RecordHostReportInfo(request, method: "GetOhtPortState");
            OhtPortStateAck ask = new OhtPortStateAck();
            string resvice_message = request.ToString();
            Console.WriteLine(resvice_message);
            ask.ReplyCode = Ack.Ok;
            LogHelper.RecordHostReportInfoAsk(ask, method: "GetOhtPortState");
            return Task.FromResult(ask);
        }

        public override Task<ReelStateAck> GetReelState(ReelStateQuery request, ServerCallContext context)
        {
            LogHelper.RecordHostReportInfo(request, method: "GetReelState");
            ReelStateAck ask = new ReelStateAck();
            string resvice_message = request.ToString();
            Console.WriteLine(resvice_message);
            ask.ReplyCode = Ack.Ok;
            LogHelper.RecordHostReportInfoAsk(ask, method: "GetReelState");
            return Task.FromResult(ask);
        }

        public override Task<HeartBeatAck> HeartBeatRequest(HeartBeatReq request, ServerCallContext context)
        {
            LogHelper.RecordHostReportInfo(request, method: "HeartBeatRequest");
            HeartBeatAck ask = new HeartBeatAck();
            string resvice_message = request.ToString();
            Console.WriteLine(resvice_message);
            ask.ReplyCode = Ack.Ok;
            LogHelper.RecordHostReportInfoAsk(ask, method: "HeartBeatRequest");
            return Task.FromResult(ask);
        }

        public void doInit()
        {
            throw new NotImplementedException();
        }

        public void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            throw new NotImplementedException();
        }

        public string getIdentityKey()
        {
            throw new NotImplementedException();
        }

        public void setContext(BaseEQObject baseEQ)
        {
            throw new NotImplementedException();
        }

        public void unRegisterEvent()
        {
            throw new NotImplementedException();
        }
    }
}