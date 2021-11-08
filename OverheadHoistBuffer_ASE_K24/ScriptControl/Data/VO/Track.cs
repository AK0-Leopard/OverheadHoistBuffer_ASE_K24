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
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public RailChangerProtocol.TrackBlock TrackBlock { get; private set; }
        public bool IsBlocking { get { return TrackBlock == RailChangerProtocol.TrackBlock.Block; } }
        public bool IsAlive { get; private set; }
        public TrackDir TrackDir { get; private set; }
        public RailChangerProtocol.TrackStatus TrackStatus { get; private set; }
        public int AlarmCode { get; private set; }
        public UInt32 ResetCount { get; private set; }
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
            ProtocolFormat.OHTMessage.TrackDir trackDir = convert(trackInfo.Dir);

            if (hasDifferent(trackInfo))
            {
                logger.Debug(trackInfo.ToString());
            }

            TrackDir = trackDir;
            TrackStatus = trackInfo.Status;
            AlarmCode = trackInfo.AlarmCode;
            TrackBlock = trackInfo.IsBlock;
            IsAlive = trackInfo.Alive;

            stopwatch.Restart();
        }

        private bool hasDifferent(RailChangerProtocol.TrackInfo trackInfo)
        {
            ProtocolFormat.OHTMessage.TrackDir trackDir = convert(trackInfo.Dir);

            if (TrackDir != trackDir) return true;
            else if (TrackStatus != trackInfo.Status) return true;
            else if (AlarmCode != trackInfo.AlarmCode) return true;
            else if (TrackBlock != trackInfo.IsBlock) return true;
            else if (IsAlive != trackInfo.Alive) return true;
            else return false;
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
