using com.mirle.ibg3k0.sc.App;
using System.Linq;
using NLog;

namespace com.mirle.ibg3k0.sc.Service
{
    public class TrackService
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private SCApplication scApp = null;

        public TrackService()
        {

        }
        public void Start(SCApplication _app)
        {
            scApp = _app;
        }

        public void RefreshTrackStatus()
        {
            var get_track_info_result = scApp.TrackInfoClient.getTrackInfos();
            if (get_track_info_result.isGetSuccess)
            {
                var all_track = scApp.UnitBLL.cache.GetALLTracks();
                foreach (var track in all_track)
                {
                    var track_info = get_track_info_result.trackInfos.
                        Where(t => Common.SCUtility.isMatche(t.TrackId, track.UNIT_ID)).
                        FirstOrDefault();
                    if (track_info == null)
                    {
                        Common.LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TrackService), Device: "OHx",
                           Data: $"Want to update track:{track.UNIT_ID} but get result not exist.");
                        continue;
                    }
                    else
                    {
                        Common.LogHelper.Log(logger: logger, LogLevel: LogLevel.Trace, Class: nameof(TrackService), Device: "OHx",
                           Data: $"Track id:{track.UNIT_ID}: dir:{track.TrackDir} last updata time(ms):{track.LastUpdataStopwatch.ElapsedMilliseconds}.");
                    }
                    track.setTrackInfo(track_info);
                }
            }
        }


    }
}
