using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.AK0.ProtocolFormat;
using Grpc.Core;
using VehicleControl_Viewer.Data.Interface;
using VehicleControl_Viewer.Vo;
using System.Timers;

namespace VehicleControl_Viewer.Data
{
    internal class TrackService
    {
        Channel channel = null;
        com.mirle.AK0.ProtocolFormat.Track.Greeter.GreeterClient client;
        App.WindownApplication app = null;
        Timer timer = null;
        public TrackService()
        {
            channel = new Channel($"localhost", 6060, ChannelCredentials.Insecure);
            client = new com.mirle.AK0.ProtocolFormat.Track.Greeter.GreeterClient(channel);
            app = App.WindownApplication.getInstance();
        }
        public TrackService(App.WindownApplication _app)
        {
            app = _app;
            channel = new Channel($"localhost", 6060, ChannelCredentials.Insecure);
            client = new com.mirle.AK0.ProtocolFormat.Track.Greeter.GreeterClient(channel);
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += refreshAllTrackStatus;
            //timer.Start();
        }
        public void refreshAllTrackStatus(object obj, ElapsedEventArgs e)
        {
            var tracks = client.RequestTracksInfo(new com.mirle.AK0.ProtocolFormat.Track.Empty());
            UI.Components.TrackInfo trackInfo;
            foreach (var temp in tracks.TracksInfo)
            {
                trackInfo = null;
                app.objCacheManager.TrackInfo.TryGetValue(temp.TrackId, out trackInfo) ;
                if(trackInfo != null)
                {
                    //更新狀態
                    trackInfo.track.status = (Track.TrackStatus)((int)temp.Status);
                    //更新block
                    trackInfo.track.block = (Track.TrackBlock)((int)temp.IsBlock);
                    //更新alarmCode
                    trackInfo.track.alarmCode = temp.AlarmCode;
                    //更新方向
                    trackInfo.track.dir = (Track.TrackDir)((int)temp.Dir);
                }
            }
        }
    }
}
