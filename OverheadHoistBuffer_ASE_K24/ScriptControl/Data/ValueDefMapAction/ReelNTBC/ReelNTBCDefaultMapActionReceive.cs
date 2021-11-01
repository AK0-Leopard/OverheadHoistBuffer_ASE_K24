using com.mirle.AK0.ProtocolFormat;
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ReelNTB;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Data.VO;
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
        ReelNTB eqpt = null;
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public event ReelNTBEvents.ReelNTBTranCmdReqEventHandler TransferCommandRequest;
        SCApplication scApp = null;
        public ReelNTBCDefaultMapActionReceive()
        {
            scApp = SCApplication.getInstance();
        }

        public override Task<TransferCommandAck> CarrierTransferRequest(TransferCommandRequset request, ServerCallContext context)
        {
            LogHelper.RecordHostReportInfo(request, method: "CarrierTransferRequest");
            TransferCommandAck ask = new TransferCommandAck();
            string resvice_message = request.ToString();
            Console.WriteLine(resvice_message);
            ask.ReplyCode = Ack.Ok;
            LogHelper.RecordHostReportInfoAsk(ask, method: "CarrierTransferRequest");

            TransferCommandRequest?.Invoke(this, new ReelNTBTranCmdReqEventArgs(eqpt, request));

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
            string ask_reel_cst_id = request.CarrierReelId;
            var get_resutl = tryGetReelTransferState(ask_reel_cst_id);
            ask.CarrierReelId = ask_reel_cst_id;
            ask.ReplyCode = get_resutl.isExcute ? Ack.Ok : Ack.Ng;
            ask.ReasonForRejection = get_resutl.reason;
            ask.Scenario = get_resutl.state;
            if (get_resutl.isToEq)
                ask.McsTransferToEqPortCommandId = get_resutl.mcsCmdID;
            else
                ask.McsTransferToNtbCommand = get_resutl.mcsCmdID;

            Console.WriteLine(resvice_message);
            ask.ReplyCode = Ack.Ok;
            LogHelper.RecordHostReportInfoAsk(ask, method: "GetReelState");
            return Task.FromResult(ask);
        }

        private (bool isExcute, ReelTransferState state, bool isToEq, string mcsCmdID, string reason) tryGetReelTransferState(string reelCSTID)
        {
            try
            {
                CassetteData reel_cst_data = scApp.CassetteDataBLL.loadCassetteDataByBoxID(reelCSTID);
                ACMD_MCS reel_cst_id_mcs_cmd = scApp.CMDBLL.getByCstBoxID(reelCSTID);
                string cmd_mcs_id = "";
                if (reel_cst_id_mcs_cmd != null)
                {
                    cmd_mcs_id = SCUtility.Trim(reel_cst_id_mcs_cmd.CMD_ID, true);
                }
                bool has_tran_cmd_excute = reel_cst_id_mcs_cmd != null;
                if (reel_cst_data == null)
                {
                    return (false, ReelTransferState.RemovedFromNtbPort, false, "", "cst not exist");
                }
                else
                {
                    if (SCUtility.isMatche(reel_cst_data.Carrier_LOC, eqpt.Real_ID))
                    {
                        if (reel_cst_data.CSTState == E_CSTState.Installed)
                        {
                            return (true, ReelTransferState.ReciveNtbcTransferRequest, false, "", "");
                        }
                        else //Wait in、transferring
                        {
                            if (reel_cst_id_mcs_cmd == null)
                            {
                                return (true, ReelTransferState.SendMcsWaitIn, false, "", "");
                            }
                            else
                            {
                                if (SCUtility.isMatche(reel_cst_id_mcs_cmd.HOSTDESTINATION, eqpt.Real_ID))
                                {
                                    return (true, ReelTransferState.ReciveMcsTransferToNtbCommand, false, cmd_mcs_id, "");
                                }
                                else
                                {
                                    return (true, ReelTransferState.ReciveMcsTransferToEqPortCommand, true, cmd_mcs_id, "");
                                }
                            }
                        }
                    }
                    else if (scApp.TransferService.isUnitType(reel_cst_data.Carrier_LOC, Enum.UnitType.CRANE))
                    {
                        if (reel_cst_id_mcs_cmd == null)
                        {
                            return (false, default(ReelTransferState), false, cmd_mcs_id, "cst is on vh,don't know what's next");
                        }
                        else
                        {
                            if (SCUtility.isMatche(reel_cst_id_mcs_cmd.HOSTDESTINATION, eqpt.Real_ID))
                            {
                                return (true, ReelTransferState.TransferringToNtbPort, false, cmd_mcs_id, "");
                            }
                            else
                            {
                                return (true, ReelTransferState.TransferringToEqPort, true, cmd_mcs_id, "");
                            }
                        }
                    }
                    else //在EQ上
                    {
                        if (has_tran_cmd_excute)
                        {
                            if (reel_cst_data.CSTState == E_CSTState.Installed)
                            {
                                return (true, ReelTransferState.ReciveMcsTransferToNtbCommand, false, cmd_mcs_id, "");
                            }
                            else
                            {
                                return (true, ReelTransferState.TransferringToEqPort, false, cmd_mcs_id, "");
                            }
                        }
                        else
                        {
                            return (false, default(ReelTransferState), false, "", "cst on eq,but no transfer command.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
                return (false, default(ReelTransferState), false, "", "Exception happend!");
            }
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

        }

        public void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            try
            {
                switch (runLevel)
                {
                    case BCFAppConstants.RUN_LEVEL.ZERO:

                        break;
                    case BCFAppConstants.RUN_LEVEL.ONE:
                        break;
                    case BCFAppConstants.RUN_LEVEL.TWO:
                        break;
                    case BCFAppConstants.RUN_LEVEL.NINE:
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
            }
        }

        public string getIdentityKey()
        {
            return this.GetType().Name;
        }

        public void setContext(BaseEQObject baseEQ)
        {
            this.eqpt = baseEQ as ReelNTB;
        }

        public void unRegisterEvent()
        {
        }
    }
}