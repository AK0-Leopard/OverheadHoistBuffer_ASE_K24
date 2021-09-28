using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO.EntityFramework;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class UnitBLL
    {
        private SCApplication scApp = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public Cache cache { get; private set; }

        public UnitBLL()
        {

        }
        public void start(SCApplication app)
        {
            scApp = app;
            cache = new Cache(scApp.getEQObjCacheManager());
        }

        public class Cache
        {
            EQObjCacheManager cacheManager;
            public Cache(EQObjCacheManager _cacheManager)
            {
                cacheManager = _cacheManager;
            }
            public List<Track> GetALLTracks()
            {
                var tracks = cacheManager.getAllUnit().Where(u => u is Track).Select(u => u as Track).ToList();
                return tracks;
            }

            public Track GetTrack(string id)
            {
                var track = cacheManager.getAllUnit().Where(u => u is Track && SCUtility.isMatche(u.UNIT_ID, id))
                                                      .Select(u => u as Track)
                                                      .FirstOrDefault();
                return track;
            }

        }


    }

}
