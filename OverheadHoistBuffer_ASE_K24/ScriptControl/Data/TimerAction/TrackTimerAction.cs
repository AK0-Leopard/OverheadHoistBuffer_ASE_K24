using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using NLog;
using System;
using System.Linq;
namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    public class TrackTimerAction : ITimerAction
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        protected SCApplication scApp = null;
        protected MPLCSMControl smControl;

        public TrackTimerAction(string name, long intervalMilliSec) : base(name, intervalMilliSec)
        {
        }

        public override void initStart()
        {
            scApp = SCApplication.getInstance();
        }
        private long syncPoint = 0;
        public override void doProcess(object obj)
        {
            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
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
                                Common.LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(TrackTimerAction), Device: "OHx",
                                   Data: $"Want to update track:{track.UNIT_ID} but get result not exist.");
                                continue;
                            }
                            else
                            {
                                Common.LogHelper.Log(logger: logger, LogLevel: LogLevel.Trace, Class: nameof(TrackTimerAction), Device: "OHx",
                                   Data: $"Track id:{track.UNIT_ID}: dir:{track.TrackDir} last updata time(ms):{track.stopwatch.ElapsedMilliseconds}.");
                            }
                            track.setTrackInfo(track_info);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                }
            }
        }

        private ProtocolFormat.OHTMessage.TrackDir convert(RailChangerProtocol.TrackDir dir)
        {
            switch (dir)
            {
                case RailChangerProtocol.TrackDir.None:
                    return ProtocolFormat.OHTMessage.TrackDir.None;
                case RailChangerProtocol.TrackDir.Straight:
                    return ProtocolFormat.OHTMessage.TrackDir.Straight;
                case RailChangerProtocol.TrackDir.Curve:
                    return ProtocolFormat.OHTMessage.TrackDir.Curve;
                default:
                    return ProtocolFormat.OHTMessage.TrackDir.None;
            }
        }
    }
}