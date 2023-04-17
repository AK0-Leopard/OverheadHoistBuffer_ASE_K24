//*********************************************************************************
//      EQObjCacheManager.cs
//*********************************************************************************
// File Name: EQObjCacheManager.cs
// Description: Equipment Cache Manager
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.ConfigHandler;
using com.mirle.ibg3k0.bcf.Data.FlowRule;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.VO;
using NLog;
using com.mirle.ibg3k0.sc.ConfigHandler;
using com.mirle.ibg3k0.sc.Data;

namespace com.mirle.ibg3k0.sc.Common
{

    public class CommObjCacheManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static CommObjCacheManager instance = null;
        private static Object _lock = new Object();
        private SCApplication scApp = null;
        //Cache Object
        //Section
        private List<AADDRESS> Addresses;
        private List<ASECTION> Sections;
        //Segment
        private List<ASEGMENT> Segments;
        private List<PortDef> PortDefs;
        private List<ReserveEnhanceInfo> ReserveEnhanceInfos;
        private CommonInfo CommonInfo;
        private List<ABLOCKZONEMASTER> BlockZoneMasters;
        private List<ParkingZone> ParkingZones = null;

        private CommObjCacheManager() { }
        public static CommObjCacheManager getInstance()
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = new CommObjCacheManager();
                }
                return instance;
            }
        }


        public void setContext()
        {
        }

        public void start(SCApplication _app)
        {
            scApp = _app;

            Addresses = scApp.MapBLL.loadAllAddress();
            Segments = scApp.MapBLL.loadAllSegments();
            Sections = scApp.MapBLL.loadAllSection();
            BlockZoneMasters = scApp.MapBLL.loadAllBlockZoneMaster();
            ParkingZones = scApp.ParkingZoneBLL.getAllParkingZoneData();
            ReserveEnhanceInfos = scApp.ReserveEnhanceInfoDao.loadReserveInfos(scApp);
            foreach (ASEGMENT segment in Segments)
            {
                segment.SetSectionList(scApp.SectionBLL);
            }

            foreach (ABLOCKZONEMASTER block_zone_master in BlockZoneMasters)
            {
                block_zone_master.SetBlockDetailList(scApp.MapBLL);
            }

            foreach (var add in Addresses)
            {
                add.updateAddressType();
            }


            CommonInfo = new CommonInfo();
        }

        private List<Track> GetTracksByBlock(string entrySectionID)
        {
            var block_track_map = scApp.BlockTrackMapDao.getBlockTrackInfo(scApp, entrySectionID);
            if (block_track_map == null)
            {
                return new List<Track>();
            }
            List<Track> tracks = new List<Track>();
            foreach (var track_id in block_track_map.TRACKS_ID)
            {
                Track track = scApp.UnitBLL.cache.GetTrack(track_id);
                if (track == null)
                {
                    logger.Warn($"track id:{block_track_map.TRACKS_ID} not exist.");
                    continue;
                }
                tracks.Add(track);
            }
            return tracks;
        }

        public void setPortDefsInfo()
        {
            //PortDefs = scApp.PortDefBLL.GetOHB_CVPortData(scApp.getEQObjCacheManager().getLine().LINE_ID);
            PortDefs = scApp.PortDefBLL.GetOHB_PortData(scApp.getEQObjCacheManager().getLine().LINE_ID);
        }
        public void setBlockAndTrack()
        {
            foreach (ABLOCKZONEMASTER block_zone_master in BlockZoneMasters)
            {
                block_zone_master.setRelatedTracks(GetTracksByBlock(block_zone_master.ENTRY_SEC_ID));
            }
        }


        public void stop()
        {
            clearCache();
        }


        private void clearCache()
        {
            Sections.Clear();
        }


        private void removeFromDB()
        {
            //not implement yet.
        }

        #region 取得各種EQ Object的方法
        //Section
        public ASECTION getSection(string sec_id)
        {
            return Sections.Where(z => z.SEC_ID.Trim() == sec_id.Trim()).FirstOrDefault();
        }
        public ASECTION getSection(string adr1, string adr2)
        {
            return Sections.Where(s => (s.FROM_ADR_ID.Trim() == adr1.Trim() && s.TO_ADR_ID.Trim() == adr2.Trim())
                                    || (s.FROM_ADR_ID.Trim() == adr2.Trim() && s.TO_ADR_ID.Trim() == adr1.Trim())).FirstOrDefault();
        }
        public List<AADDRESS> getAddresses()
        {
            return Addresses.ToList();
        }

        public List<ASECTION> getSections()
        {
            return Sections.ToList();
        }
        //Segment
        public List<ASEGMENT> getSegments()
        {
            return Segments.ToList();
        }
        public List<PortDef> getPortDefs()
        {
            return PortDefs.ToList();
        }
        public List<ReserveEnhanceInfo> getReserveEnhanceInfos()
        {
            return ReserveEnhanceInfos;
        }

        #endregion


        private void setValueToPropety<T>(ref T sourceObj, ref T destinationObj)
        {
            BCFUtility.setValueToPropety(ref sourceObj, ref destinationObj);
        }
        public List<ABLOCKZONEMASTER> getBlockMasterZone()
        {
            return BlockZoneMasters;
        }
        public List<ParkingZone> GetAllParkingZonesInfos()
        {
            return ParkingZones;
        }

        #region 將最新物件資料，放置入Cache的方法
        //NotImplemented
        #endregion


        #region 從DB取得最新EQ Object，並更新Cache
        //NotImplemented
        #endregion



    }
}
