﻿using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.RouteKit;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class GuideBLL
    {
        SCApplication scApp;
        Logger logger = LogManager.GetCurrentClassLogger();

        public void start(SCApplication _scApp)
        {
            scApp = _scApp;
        }

        public (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
        getGuideInfoForMtx(string startAddress, string targetAddress)
        {
            return getGuideInfoInternalUse(startAddress, targetAddress, null);
        }
        public (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
        getGuideInfo(string startAddress, string targetAddress, List<string> byPassSectionIDs = null)
        {
            List<string> by_pass_section_ids = new List<string>();
            if (byPassSectionIDs != null && byPassSectionIDs.Any())
            {
                by_pass_section_ids.AddRange(byPassSectionIDs);
            }

            List<string> mtx_section_ids = scApp.MTLService.All_Mtx_Devic_Section_Ids;
            by_pass_section_ids.AddRange(mtx_section_ids);
            if (!DebugParameter.IsPaassErrorVhAndTrackStatus)
            {
                List<string> blocked_section_ids = loadBLockedSectionIDs(scApp.VehicleBLL, scApp.UnitBLL);
                by_pass_section_ids.AddRange(blocked_section_ids);
            }
            else
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(GuideBLL), Device: "OHx",
                   Data: $"Pass error vh and tarck status");
            }
            return getGuideInfoInternalUse(startAddress, targetAddress, by_pass_section_ids);
        }

        private (bool isSuccess, List<string> guideSegmentIds, List<string> guideSectionIds, List<string> guideAddressIds, int totalCost)
        getGuideInfoInternalUse(string startAddress, string targetAddress, List<string> byPassSectionIDs)
        {
            if (SCUtility.isMatche(startAddress, targetAddress))
            {
                return (true, new List<string>(), new List<string>(), new List<string>(), 0);
            }

            int.TryParse(startAddress, out int i_start_address);
            int.TryParse(targetAddress, out int i_target_address);

            List<RouteInfo> stratFromRouteInfoList = null;
            if (byPassSectionIDs == null || byPassSectionIDs.Count == 0)
            {
                stratFromRouteInfoList = scApp.NewRouteGuide.getFromToRoutesAddrToAddr(i_start_address, i_target_address);
            }
            else
            {
                stratFromRouteInfoList = scApp.NewRouteGuide.getFromToRoutesAddrToAddr(i_start_address, i_target_address, byPassSectionIDs);
            }
            RouteInfo min_stratFromRouteInfo = null;
            if (stratFromRouteInfoList != null && stratFromRouteInfoList.Count > 0)
            {
                min_stratFromRouteInfo = stratFromRouteInfoList.First();
                return (true, null, min_stratFromRouteInfo.GetSectionIDs(), min_stratFromRouteInfo.GetAddressesIDs(), min_stratFromRouteInfo.total_cost);
            }
            else
            {
                return (false, null, null, null, int.MinValue);
            }

        }


        public (bool isSuccess, int distance) IsRoadWalkableForMTx(string startAddress, string targetAddress)
        {
            try
            {
                if (SCUtility.isMatche(startAddress, targetAddress))
                    return (true, 0);

                var guide_info = getGuideInfoForMtx(startAddress, targetAddress);
                if (guide_info.isSuccess)
                {
                    return (true, guide_info.totalCost);
                }
                else
                {
                    return (false, int.MaxValue);
                }
            }
            catch
            {
                return (false, int.MaxValue);
            }
        }
        public (bool isSuccess, int distance) IsRoadWalkable(string startAddress, string targetAddress, List<string> byPassSectionIDs = null)
        {
            try
            {
                if (SCUtility.isMatche(startAddress, targetAddress))
                    return (true, 0);

                var guide_info = getGuideInfo(startAddress, targetAddress, byPassSectionIDs);
                //if ((guide_info.guideAddressIds != null && guide_info.guideAddressIds.Count != 0) &&
                //    ((guide_info.guideSectionIds != null && guide_info.guideSectionIds.Count != 0)))
                if (guide_info.isSuccess)
                {
                    return (true, guide_info.totalCost);
                }
                else
                {
                    return (false, int.MaxValue);
                }
            }
            catch
            {
                return (false, int.MaxValue);
            }
        }
        public int GetDistance(string startAddress, string targetAddress)
        {
            try
            {
                if (SCUtility.isMatche(startAddress, targetAddress))
                    return 0;

                var guide_info = getGuideInfo(startAddress, targetAddress);
                //if ((guide_info.guideAddressIds != null && guide_info.guideAddressIds.Count != 0) &&
                //    ((guide_info.guideSectionIds != null && guide_info.guideSectionIds.Count != 0)))
                if (guide_info.isSuccess)
                {
                    return guide_info.totalCost;
                }
                else
                {
                    return int.MaxValue;
                }
            }
            catch
            {
                return int.MaxValue;
            }
        }

        public ASEGMENT OpenSegment(string strSegCode, ASEGMENT.DisableType disableType)
        {
            ASEGMENT seg_vo = null;
            ASEGMENT seg_do = null;
            try
            {
                seg_vo = scApp.SegmentBLL.cache.GetSegment(strSegCode);
                lock (seg_vo)
                {
                    seg_do = scApp.MapBLL.EnableSegment(strSegCode, disableType);
                    //unbanRouteTwoDirect(strSegCode);
                    unbanRouteOneDirect(strSegCode);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return seg_do;
        }
        public ASEGMENT CloseSegment(string strSegCode, ASEGMENT.DisableType disableType)
        {
            ASEGMENT seg_vo = null;
            ASEGMENT seg_do = null;
            try
            {
                seg_vo = scApp.SegmentBLL.cache.GetSegment(strSegCode);
                lock (seg_vo)
                {
                    seg_do = scApp.MapBLL.DisableSegment(strSegCode, disableType);
                    //banRouteTwoDirect(strSegCode);
                    banRouteOneDirect(strSegCode);

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            return seg_do;
        }
        public ASEGMENT banRouteTwoDirect(string segmentID)
        {
            ASEGMENT segment = null;
            ASEGMENT segment_vo = scApp.SegmentBLL.cache.GetSegment(segmentID);
            if (segment_vo != null)
            {
                foreach (var sec in segment_vo.Sections)
                    scApp.NewRouteGuide.banRouteTwoDirect(sec.SEC_ID);
            }
            segment = scApp.MapBLL.DisableSegment(segmentID);
            return segment;
        }
        public ASEGMENT banRouteOneDirect(string segmentID)
        {
            ASEGMENT segment = null;
            ASEGMENT segment_vo = scApp.SegmentBLL.cache.GetSegment(segmentID);
            if (segment_vo != null)
            {
                foreach (var sec in segment_vo.Sections)
                {
                    int.TryParse(sec.FROM_ADR_ID, out int ifrom);
                    int.TryParse(sec.TO_ADR_ID, out int ito);
                    scApp.NewRouteGuide.banRouteOneDirect(ifrom, ito);
                }
            }
            segment = scApp.MapBLL.DisableSegment(segmentID);
            return segment;
        }
        public ASEGMENT unbanRouteTwoDirect(string segmentID)
        {
            ASEGMENT segment_do = null;
            ASEGMENT segment_vo = scApp.SegmentBLL.cache.GetSegment(segmentID);
            if (segment_vo != null)
            {
                foreach (var sec in segment_vo.Sections)
                    scApp.NewRouteGuide.unbanRouteTwoDirect(sec.SEC_ID);
            }
            segment_do = scApp.MapBLL.EnableSegment(segmentID);
            return segment_do;
        }
        public ASEGMENT unbanRouteOneDirect(string segmentID)
        {
            ASEGMENT segment_do = null;
            ASEGMENT segment_vo = scApp.SegmentBLL.cache.GetSegment(segmentID);
            if (segment_vo != null)
            {
                foreach (var sec in segment_vo.Sections)
                {
                    int.TryParse(sec.FROM_ADR_ID, out int ifrom);
                    int.TryParse(sec.TO_ADR_ID, out int ito);
                    scApp.NewRouteGuide.unbanRouteOneDirect(ifrom, ito);
                }
            }
            segment_do = scApp.MapBLL.EnableSegment(segmentID);
            return segment_do;
        }

        public List<string> loadBLockedSectionIDs(BLL.VehicleBLL vehicleBLL, BLL.UnitBLL unitBLL)
        {
            List<string> blocked_section_ids = new List<string>();
            try
            {
                var alarm_vhs = vehicleBLL.cache.loadAlarmVhs();
                //blocked_section_ids.AddRange(alarm_vhs.Select(v => v.getVIEW_SEC_ID(scApp.SectionBLL)));

                foreach (var vh in alarm_vhs)
                {
                    string current_sec_id = vh.getVIEW_SEC_ID(scApp.SectionBLL);
                    if (SCUtility.isEmpty(current_sec_id))
                    {
                        continue;
                    }
                    blocked_section_ids.Add(current_sec_id);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(GuideBLL), Device: "OHx",
                       Data: $"vh:{vh.VEHICLE_ID} error at section:{current_sec_id}");
                }

                var no_auto_tracks = unitBLL.cache.loadNoAutoTrack();
                foreach (var track in no_auto_tracks)
                {
                    blocked_section_ids.AddRange(track.RelatedSection);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(GuideBLL), Device: "OHx",
                       Data: $"Track:{track.UNIT_ID} status:{track.TrackStatus},will pass section id:{track.sRelatedSection}");
                }
                return blocked_section_ids;
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "Exception:");
                return blocked_section_ids;
            }
        }
    }

}

