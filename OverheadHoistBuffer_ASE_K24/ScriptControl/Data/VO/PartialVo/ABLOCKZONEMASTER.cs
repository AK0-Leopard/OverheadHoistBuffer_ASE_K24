﻿using com.mirle.ibg3k0.bcf.App;
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
        public bool IsAllTrackReadyStraight()
        {
            if (RelatedTracks == null || RelatedTracks.Count == 0)
                return false;
            foreach (var related_track in RelatedTracks)
            {
                if (related_track.TrackDir != ProtocolFormat.OHTMessage.TrackDir.Straight)
                {
                    LogHelper.Log(logger: logger, LogLevel: NLog.LogLevel.Debug, Class: nameof(ABLOCKZONEMASTER), Device: "OHx",
                       Data: $"Block:{ENTRY_SEC_ID} of related track:{related_track.UNIT_ID} current status:{related_track.TrackDir} not Straight, return not ready");
                    return false;
                }
            }
            return true;
        }
    }

}
