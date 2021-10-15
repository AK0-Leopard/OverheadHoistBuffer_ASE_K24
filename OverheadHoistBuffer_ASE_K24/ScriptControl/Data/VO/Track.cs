using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class Track : AUNIT
    {
        public TrackDir TrackDir { get; set; }
        public RailChangerProtocol.TrackStatus TrackStatus { get; set; }
        public string AlarmCode { get; set; }
        public UInt32 ResetCount { get; set; }
        public Stopwatch stopwatch { get; private set; } = new Stopwatch();
        public string LastUpdateTime
        {
            get
            {
                return stopwatch.ElapsedMilliseconds.ToString();
            }
        }
        public void setTrackDir(TrackDir _trackDir)
        {
            TrackDir = _trackDir;
            stopwatch.Restart();
        }
        public void setTrackInfo(RailChangerProtocol.TrackInfo trackInfo)
        {
            TrackDir = convert(trackInfo.Dir);
            TrackStatus = trackInfo.Status;
            AlarmCode = trackInfo.AlarmCode;
            stopwatch.Restart();
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

        public void ResetBlock(WebAPI.TrackInfoClient trackInfoClient)
        {
            if (trackInfoClient == null) return;
            bool is_success = trackInfoClient.ResetBlockAsync(UNIT_ID).Result;
            if (is_success)
            {
                ResetCount++;
            }
        }
    }
}
