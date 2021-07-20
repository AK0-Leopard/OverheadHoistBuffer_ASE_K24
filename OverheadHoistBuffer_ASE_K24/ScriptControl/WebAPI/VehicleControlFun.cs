using com.mirle.AK0.ProtocolFormat;
using com.mirle.ibg3k0.sc.App;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.WebAPI
{
    public class VehicleControlFun : com.mirle.AK0.ProtocolFormat.VehicleControlFun.VehicleControlFunBase
    {
        #region RequestSegmentData
        public override Task<ReplySegmentData> RequestSegmentData(Empty request, ServerCallContext context)
        {
            var app = sc.App.SCApplication.getInstance();
            ReplySegmentData replySegmentData = new ReplySegmentData();
            var segments = app.SegmentBLL.cache.GetSegments();
            List<SegmentInfo> segmentInfos = new List<SegmentInfo>();
            foreach (var seg in segments)
            {
                var info = new SegmentInfo();
                info.ID = seg.SEG_NUM;
                info.Note = seg.NOTE;
                info.Status = converTo(seg.STATUS);
                var sec_ids = seg.Sections.Select(s => s.SEC_ID);
                info.SecIds.AddRange(sec_ids);
                segmentInfos.Add(info);
            }
            replySegmentData.SegmentInfos.AddRange(segmentInfos);
            return Task.FromResult(replySegmentData);
        }
        private SegmentStatus converTo(E_SEG_STATUS status)
        {
            switch (status)
            {
                case E_SEG_STATUS.Active:
                    return SegmentStatus.Active;
                case E_SEG_STATUS.Closed:
                    return SegmentStatus.Closed;
                case E_SEG_STATUS.Inactive:
                    return SegmentStatus.Inactive;
                default:
                    throw new Exception();
            }
        }
        #endregion RequestSegmentData
        #region RequestTranser
        public override Task<ReplyTrnsfer> RequestTrnsfer(VehicleCommandInfo request, ServerCallContext context)
        {
            var scApp = sc.App.SCApplication.getInstance();
            switch (request.Type)
            {
                case CommandEventType.Move:
                    return CommandMove(request, scApp);
                default:
                    return Task.FromResult(TransferResult(Result.Ng, $"No action"));
            }
        }

        private Task<ReplyTrnsfer> CommandMove(VehicleCommandInfo request, SCApplication scApp)
        {
            var assign_vh = scApp.VehicleBLL.cache.getVhByID(request.VhId);
            if (assign_vh == null)
            {
                return Task.FromResult(TransferResult(Result.Ng, $"vh id:{request.VhId} not exist"));
            }

            scApp.CMDBLL.doCreatTransferCommand(request.VhId, out var cmd_obj,
                                        cmd_type: converTo(request.Type),
                                        source: "",
                                        destination: "",
                                        box_id: request.CarrierId,
                                        destination_address: request.ToPortId,
                                        gen_cmd_type: SCAppConstants.GenOHxCCommandType.Manual);
            sc.BLL.CMDBLL.OHTCCommandCheckResult check_result_info =
                                sc.BLL.CMDBLL.getCallContext<sc.BLL.CMDBLL.OHTCCommandCheckResult>
                               (sc.BLL.CMDBLL.CALL_CONTEXT_KEY_WORD_OHTC_CMD_CHECK_RESULT);
            bool isSuccess = check_result_info.IsSuccess;
            string result = check_result_info.ToString();
            if (isSuccess)
            {
                isSuccess = scApp.VehicleService.doSendOHxCCmdToVh(assign_vh, cmd_obj);
                if (isSuccess)
                {
                    return Task.FromResult(TransferResult(Result.Ok, $"sned command to vh:{request.VhId} success"));
                }
                else
                {
                    return Task.FromResult(TransferResult(Result.Ok, $"Send command to vh:{request.VhId} failed!"));
                }
            }
            else
            {
                return Task.FromResult(TransferResult(Result.Ng, $"Creat command vh:{request.VhId} failed,reason:{result}"));
            }
        }

        private static ReplyTrnsfer TransferResult(Result result, string reason)
        {
            ReplyTrnsfer replyTrnsfer = new ReplyTrnsfer()
            {
                Result = result,
                Reason = reason
            };
            return replyTrnsfer;
        }

        private E_CMD_TYPE converTo(CommandEventType status)
        {
            switch (status)
            {
                case CommandEventType.Move:
                    return E_CMD_TYPE.Move;
                case CommandEventType.Load:
                    return E_CMD_TYPE.Load;
                case CommandEventType.Unload:
                    return E_CMD_TYPE.Unload;
                case CommandEventType.LoadUnload:
                    return E_CMD_TYPE.LoadUnload;
                default:
                    throw new Exception();
            }
        }

        #endregion RequestTranser
        #region GuideInfo Request
        public override Task<ReplyGuideInfo> RequestGuideInfo(SearchInfo request, ServerCallContext context)
        {
            var app = sc.App.SCApplication.getInstance();
            var guide_infos = app.GuideBLL.getGuideInfo(request.StartAdr, request.EndAdr);

            ReplyGuideInfo reply = new ReplyGuideInfo();
            reply.SecIds.AddRange(guide_infos.guideSectionIds);
            reply.AdrIds.AddRange(guide_infos.guideAddressIds);
            return Task.FromResult(reply);
        }
        private int intConvert(string s)
        {
            int.TryParse(s, out int i);
            return i;
        }
        #endregion GuideInfo Request
    }
}