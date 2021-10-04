using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.AK0.ProtocolFormat;
using Grpc.Core;
using VehicleControl_Viewer.Data.Interface;

namespace VehicleControl_Viewer.Data
{
    public class VehicleControlService : IVehicleCommand
    {
        Channel channel = null;
        VehicleControlFun.VehicleControlFunClient client;
        public VehicleControlService()
        {
            channel = new Channel("127.0.0.1", 7001, ChannelCredentials.Insecure);
            client = new VehicleControlFun.VehicleControlFunClient(channel);
        }
        public List<SegmentInfo> GetSegmentInfos()
        {
            try
            {
                var replySegmentData = client.RequestSegmentData(new Empty());
                return replySegmentData.SegmentInfos.ToList();
            }
            catch (Exception ex)
            {
                return new List<SegmentInfo>();
            }
        }
        public async Task<ReplyTrnsfer> RequestTrnsferAsync(VehicleCommandInfo commandInfo)
        {
            try
            {
                var replySegmentData = await client.RequestTrnsferAsync(commandInfo);
                return replySegmentData;
            }
            catch (Exception ex)
            {
                var reply = new ReplyTrnsfer();
                reply.Result = Result.Ng;
                reply.Reason = "Service no reply";
                return reply;
            }
        }
        public async Task<ReplyGuideInfo> RequestGuideInfo(SearchInfo guideInfo)
        {
            try
            {
                var reply_guide_info = await client.RequestGuideInfoAsync(guideInfo);
                return reply_guide_info;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public (bool isRequestSunncess, ReplyVehicleSummary vhSummary) RequestVehicleSummary()
        {
            try
            {
                var reply_guide_info = client.RequestVehicleSummary(new Empty());
                return (true, reply_guide_info);
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }
        public (bool isRequestSunncess, ReplyPortsInfo replyResult) RequestPortsInfo()
        {
            try
            {
                var infos = client.RequestPortsInfo(new Empty());
                return (true, infos);
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }
    }
}
