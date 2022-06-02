using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.Data.DAO;
using NLog;
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.App;
using System.Transactions;
using com.mirle.ibg3k0.sc.Service;
using com.mirle.ibg3k0.sc.BLL.Interface;

namespace com.mirle.ibg3k0.sc.BLL
{
    public partial class ShelfDefBLL
    {
        private SCApplication scApp = null;
        private ShelfDefDao shelfdefDao = null;
        private ZoneDefDao zonedefDao = null;
        private CassetteDataDao cassetteDataDao = null;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void start(SCApplication scApp)
        {
            this.scApp = scApp;
            shelfdefDao = scApp.ShelfDefDao;
            zonedefDao = scApp.ZoneDefDao;
            cassetteDataDao = scApp.CassetteDataDao;
        }

        public bool addShelfData(ShelfDef shelfDef)
        {
            bool is_success = true;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    shelfdefDao.insertShelfDef(con, shelfDef);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                is_success = false;
            }
            return is_success;
        }

        public ShelfDef GetShelfDataByID(string shelfid)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    return shelfdefDao.GetShelfByID(con, shelfid);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }

        public List<ShelfDef> LoadShelf()
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    return shelfdefDao.LoadShelfDef(con);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }

        public List<ShelfDef> LoadEnableShelf()
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    return shelfdefDao.LoadEnableShelf(con);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }

        public bool UpdateEnableByID(string shelfid, bool enable, string remark)
        {
            bool isSuccsess = true;
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    ShelfDef shelf = shelfdefDao.GetShelfByID(con, shelfid);
                    shelf.Enable = enable == true ? "Y" : "N";
                    shelf.Remark = remark;
                    if (enable)
                    {
                        shelf.DISABLE_TIME = null;
                    }
                    else
                    {
                        shelf.DISABLE_TIME = DateTime.Now;
                    }

                    shelfdefDao.UpdateShelfDef(con, shelf);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                isSuccsess = false;
            }
            return isSuccsess;
        }

        public bool isExist(string name)
        {
            try
            {
                //return scApp.TransferService.isLocExist(name);
                return scApp.TransferService.isShelfPort(name);
                //using (DBConnection_EF con = DBConnection_EF.GetUContext())
                //{
                //    return (shelfdefDao.LoadShelfDef(con).Where(x => x.ShelfID == name).Count() > 0);
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }
        public bool isEnable(string shelfID)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    return shelfdefDao.IsEnable(con, shelfID);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
        }

        public bool updatePriority(string shelf_id, int priority)
        {
            try
            {
                //ShelfDef shelf_def = new ShelfDef();
                //shelf_def.ShelfID = shelf_id;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    var shelf_def = con.ShelfDef.Where(x => x.ShelfID == shelf_id).FirstOrDefault();
                    shelf_def.SelectPriority = priority;
                    //shelf_def.SelectPriority = priority;

                    con.Entry(shelf_def).Property(p => p.SelectPriority).IsModified = true;

                    shelfdefDao.UpdateShelfDef(con, shelf_def);
                    //con.Entry(port_statino).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }

        internal bool updateRemark(string shelf_id, string remark)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    var shelf_def = con.ShelfDef.Where(x => x.ShelfID == shelf_id).FirstOrDefault();
                    shelf_def.Remark = remark;
                    //shelf_def.SelectPriority = priority;

                    con.Entry(shelf_def).Property(p => p.Remark).IsModified = true;

                    shelfdefDao.UpdateShelfDef(con, shelf_def);
                    //con.Entry(port_statino).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }

        public bool updateStatus(string shelf_id, string status)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    var shelf_def = con.ShelfDef.Where(x => x.ShelfID == shelf_id).FirstOrDefault();
                    shelf_def.ShelfState = status;
                    con.Entry(shelf_def).Property(p => p.ShelfState).IsModified = true;
                    shelfdefDao.UpdateShelfDef(con, shelf_def);
                }

                scApp.TransferService.TransferServiceLogger.Info
                (
                    DateTime.Now.ToString("HH:mm:ss.fff ") + "OHB >> DB|"
                    + "更新儲位狀態:  shelf_id: " + shelf_id
                    + " status: " + status
                );

                if (status == ShelfDef.E_ShelfState.EmptyShelf)
                {
                    scApp.TransferService.OHBC_AlarmCleared(scApp.getEQObjCacheManager().getLine().LINE_ID, ((int)AlarmLst.LINE_NotEmptyShelf).ToString());
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }

            return true;
        }

        public bool updateEnable(string shelf_id, bool enable)
        {
            try
            {
                //ShelfDef shelf_def = new ShelfDef();
                //shelf_def.ShelfID = shelf_id;
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    string en = enable == true ? "Y" : "N";
                    var shelf_def = con.ShelfDef.Where(x => x.ShelfID == shelf_id).FirstOrDefault();
                    shelf_def.Enable = en;
                    //shelf_def.SelectPriority = priority;

                    con.Entry(shelf_def).Property(p => p.SelectPriority).IsModified = true;

                    shelfdefDao.UpdateShelfDef(con, shelf_def);
                    //con.Entry(port_statino).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }
        public bool updateShelfEnableByZoneID(string zoneID)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    shelfdefDao.UpdateShelfEnableByZone(con, zoneID);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }
        public bool updateShelfDisableByZoneID(string zoneID, string remark)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    shelfdefDao.UpdateShelfDisableByZone(con, zoneID, remark);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }
        public UInt16 getCurrentShelfEnableCount()
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return (UInt16)shelfdefDao.LoadEnableShelf(con).Count;
            }
        }

        public UInt16 getCurrentShelfDisableCount()
        {
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                return (UInt16)shelfdefDao.LoadDisableShelf(con).Count;
            }
        }


        public bool updateOccupied(string shelf_id, bool occupied)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    string isEmpty = occupied == true ? "0" : "1";
                    var shelf_def = con.ShelfDef.Where(x => x.ShelfID == shelf_id).FirstOrDefault();
                    shelf_def.EmptyBlockFlag = isEmpty;

                    con.Entry(shelf_def).Property(p => p.EmptyBlockFlag).IsModified = true;

                    shelfdefDao.UpdateShelfDef(con, shelf_def);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return false;
            }
            return true;
        }

        public List<ShelfDef> GetEmptyAndEnableShelf()
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    return shelfdefDao.GetEmptyAndEnableShelf(con);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }

        public List<ShelfDef> GetEmptyAndEnableShelfByZone(string zoneID)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    return shelfdefDao.GetEmptyAndEnableShelfByZone(con, zoneID);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }

        public List<ShelfDef> GetReserveShelf()
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    return shelfdefDao.GetReserveShelf(con);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }

        public int GetDistance(string shelfID, string targetAddress)
        {
            ShelfDef targetShelf = GetShelfDataByID(shelfID);
            return scApp.GuideBLL.GetDistance(targetShelf.ADR_ID, targetAddress);
        }
        public List<ShelfDef> loadHasChangeShelfDefByAfterDateTime(string afterDateTime)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {

                    return shelfdefDao.loadHasChangeShelfDefByAfterDateTime(con, afterDateTime);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return null;
            }
        }
    }

    public partial class ShelfDefBLL : IManualPortShelfDefBLL
    {
        public void SetEmpty(string shelfId)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    var shelf = con.ShelfDef.Where(x => x.ShelfID == shelfId).FirstOrDefault();

                    shelf.ShelfState = ShelfDef.E_ShelfState.EmptyShelf;

                    shelfdefDao.UpdateShelfDef(con, shelf);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        public void SetStored(string shelfId)
        {
            try
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    var shelf = con.ShelfDef.Where(x => x.ShelfID == shelfId).FirstOrDefault();

                    shelf.ShelfState = ShelfDef.E_ShelfState.Stored;

                    shelfdefDao.UpdateShelfDef(con, shelf);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }
    }
}