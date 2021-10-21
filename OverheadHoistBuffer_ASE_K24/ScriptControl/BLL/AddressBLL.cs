using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
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
        }
    }
}
