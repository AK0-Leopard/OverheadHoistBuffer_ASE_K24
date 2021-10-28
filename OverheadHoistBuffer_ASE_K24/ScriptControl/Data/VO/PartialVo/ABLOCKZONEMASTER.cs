using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ObjectRelay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class ABLOCKZONEMASTER
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public event EventHandler<string> VehicleLeave;
        public event EventHandler<string> VehicleEntry;
        public List<Track> RelatedTracks { get; private set; } = new List<Track>();
        public void setRelatedTracks(List<Track> tracks)
        {
            RelatedTracks = tracks;
        }

        private void onSectinoLeave(string vh_id)
        {
            VehicleLeave?.Invoke(this, vh_id);
        }
        private void onSectinoEntry(string vh_id)
        {
            VehicleEntry?.Invoke(this, vh_id);
        }

        public void Leave(string vh_id)
        {
            onSectinoLeave(vh_id);
        }
        public void Entry(string vh_id)
        {
            onSectinoEntry(vh_id);
        }


        List<string> BlockZoneDetailSectionIDs;
        public void SetBlockDetailList(BLL.MapBLL mapBLL)
        {
            BlockZoneDetailSectionIDs = mapBLL.loadBlockZoneDetailSecIDsByEntrySecID(ENTRY_SEC_ID);
        }
        public List<string> GetBlockZoneDetailSectionIDs()
        {
            if (BlockZoneDetailSectionIDs == null)
            {
                return new List<string>();
            }
            else
            {
                return BlockZoneDetailSectionIDs;
            }
        }
        const int INTERVAL_TIME = 2_500;
        public bool IsAllTrackReadyStraight()
        {
            if (RelatedTracks == null || RelatedTracks.Count == 0)
                return false;
            foreach (var related_track in RelatedTracks)
            {
                if (related_track == null)
                {
                    LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                       Data: $"Block:{ENTRY_SEC_ID} of related track is null ,return not ready.");
                    return false;
                }
                if (!related_track.stopwatch.IsRunning || related_track.stopwatch.ElapsedMilliseconds > INTERVAL_TIME)
                {
                    LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                       Data: $"Block:{ENTRY_SEC_ID} of related track:{related_track.UNIT_ID} stop watch no work or over time out {INTERVAL_TIME}ms, return not ready");
                    return false;
                }
                if (related_track.TrackDir != ProtocolFormat.OHTMessage.TrackDir.Straight)
                {
                    LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                       Data: $"Block:{ENTRY_SEC_ID} of related track:{related_track.UNIT_ID} current status:{related_track.TrackDir} not Straight, return not ready");
                    return false;
                }
            }
            return true;
        }

        public bool IsAllTrackBlockReady()
        {
            if (DebugParameter.IsPassTrackBlockStatus)
            {
                LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                   Data: $"Block:{ENTRY_SEC_ID} request,but force pass track block status is open ,return block is ready.");
                return true;
            }


            if (RelatedTracks == null || RelatedTracks.Count == 0)
            {
                LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                   Data: $"Block:{ENTRY_SEC_ID} of no related track ,return block is ready.");
                return true;
            }

            foreach (var related_track in RelatedTracks)
            {

                if (related_track == null)
                {
                    continue;
                }


                if (!related_track.stopwatch.IsRunning || related_track.stopwatch.ElapsedMilliseconds > INTERVAL_TIME)
                {
                    LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                       Data: $"Block:{ENTRY_SEC_ID} of related track:{related_track.UNIT_ID} stop watch no work or over time out {INTERVAL_TIME}ms,  return block not ready");
                    return false;
                }
                //if (!related_track.IsAlive)
                //{
                //    LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                //       Data: $"Block:{ENTRY_SEC_ID} of related track:{related_track.UNIT_ID} no alive, force return block ready ");
                //    return true;
                //}
                if (related_track.IsBlocking)
                {
                    LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                       Data: $"Block:{ENTRY_SEC_ID} of related track:{related_track.UNIT_ID} current blocking:{related_track.IsBlocking}, return block not ready");
                    return false;
                }

                ProtocolFormat.OHTMessage.TrackDir trackDir = related_track.TrackDir;
                if (trackDir == ProtocolFormat.OHTMessage.TrackDir.None)
                {
                    LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                       Data: $"Block:{ENTRY_SEC_ID} of related track:{related_track.UNIT_ID} current dir:{trackDir}, return block not ready");
                    return false;
                }
            }
            return true;
        }

    }

}
