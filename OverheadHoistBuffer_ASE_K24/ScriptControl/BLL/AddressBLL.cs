using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class AddressBLL
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public SCApplication scApp;
        public Database dataBase { get; private set; }
        public Cache cache { get; private set; }
        public AddressBLL()
        {
        }
        public void start(SCApplication _app)
        {
            scApp = _app;
            dataBase = new Database(scApp.AddressDao);
            cache = new Cache(scApp.getCommObjCacheManager());

        }
        public void reloadAddressType()
        {
            try
            {
                var all_address = dataBase.loadAllAddress();
                cache.upDataAddressType(all_address);
                var avoid_adr = cache.loadCanAvoidAddresses().Where(a => a.IsAvoid).Select(a => a.ADR_ID).ToList();
                Console.WriteLine($"avoid:{string.Join(",", avoid_adr)}");
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(AddressBLL), Device: "OHx",
                   Data: $"current avoid adr id:{string.Join(",", avoid_adr)}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }
        public class Database
        {
            ADDRESSDao addressDao = null;
            public Database(ADDRESSDao dao)
            {
                addressDao = dao;
            }
            public List<AADDRESS> loadAllAddress()
            {
                List<AADDRESS> adrs = null;
                using (DBConnection_EF con = new DBConnection_EF())
                {
                    adrs = addressDao.loadAll(con);
                }
                return adrs;
            }

        }
        public class Cache
        {
            CommObjCacheManager CommObjCacheManager = null;
            public Cache(CommObjCacheManager commObjCacheManager)
            {
                CommObjCacheManager = commObjCacheManager;
            }
            public AADDRESS GetAddress(string id)
            {
                return CommObjCacheManager.getAddresses().
                                           Where(a => Common.SCUtility.isMatche(a.ADR_ID, id)).
                                           FirstOrDefault();
            }

            public List<AADDRESS> loadCanAvoidAddresses()
            {
                return CommObjCacheManager.getAddresses().
                                           Where(a => a.IsAvoid).
                                           ToList();
            }
            public void upDataAddressType(List<AADDRESS> inputAdrData)
            {
                var addresses = CommObjCacheManager.getAddresses();
                foreach (var db_adr in inputAdrData)
                {
                    var cache_adr = addresses.Where(a => SCUtility.isMatche(a.ADR_ID, db_adr.ADR_ID)).FirstOrDefault();
                    if (cache_adr == null) continue;
                    cache_adr.ADRTYPE = db_adr.ADRTYPE;
                    cache_adr.updateAddressType();
                }
            }

            public List<AADDRESS> LoadCanAvoidAddresses()
            {
                return CommObjCacheManager.getAddresses().
                    Where(adr => adr.IsAvoid).
                    ToList();
            }

        }
    }
}
