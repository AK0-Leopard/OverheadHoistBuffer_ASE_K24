using com.mirle.ibg3k0.sc.Common;
using Grpc.Core;
using NLog;
using RailChangerProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.WebAPI
{
    public class TrackInfoClient
    {
        protected static Logger logger = LogManager.GetCurrentClassLogger();
        sc.App.SCApplication scApp = null;
        Channel channel = null;
        Greeter.GreeterClient client = null;
        public TrackInfoClient(sc.App.SCApplication _scApp)
        {
            scApp = _scApp;
            string s_grpc_client_ip = scApp.getString("gRPCClientIP", "127.0.0.1");
            string s_grpc_client_port = scApp.getString("gRPCClientPort", "6060");
            int.TryParse(s_grpc_client_port, out int i_grpc_client_port);
            channel = new Channel(s_grpc_client_ip, i_grpc_client_port, ChannelCredentials.Insecure);
            client = new Greeter.GreeterClient(channel);
        }
        public (bool isGetSuccess, List<TrackInfo> trackInfos) getTrackInfos()
        {
            bool is_success = true;
            List<TrackInfo> trackInfos = null;
            try
            {
                var ask = client.RequestTracksInfo(new Empty());
                trackInfos = ask.TracksInfo.ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                is_success = false;
            }
            return (is_success, trackInfos);
        }
        public async Task<bool> ResetBlockAsync(string trackID)
        {
            try
            {
                if (sc.Common.SCUtility.isEmpty(trackID))
                    return false;
                string num = trackID.Replace("R", "");
                var block_reset_req = new blockRstRequest()
                {
                    RailChangerNumber = num
                };
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(TrackInfoClient), Device: "OHx",
                   Data: $"Notify track:{trackID} num:{num} block reset...");
                var ask = await client.blockRstAsync(block_reset_req);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(TrackInfoClient), Device: "OHx",
                   Data: $"Notify track:{trackID} num:{num} block reset finish,result:{ask}");
                return true;

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }
    }
}
