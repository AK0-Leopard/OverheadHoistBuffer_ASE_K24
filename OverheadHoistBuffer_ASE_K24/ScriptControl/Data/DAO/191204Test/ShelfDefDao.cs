﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.bcf.Data;
using NLog;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class ShelfDefDao : DaoBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public void insertShelfDef(DBConnection_EF conn, ShelfDef shelfdef)
        {
            try
            {
                conn.ShelfDef.Add(shelfdef);
                conn.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public void DeleteShelfDef(DBConnection_EF conn, ShelfDef shelfdef)
        {
            try
            {
                conn.ShelfDef.Remove(shelfdef);
                conn.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public void UpdateShelfDef(DBConnection_EF conn, ShelfDef shelfdef)
        {
            try
            {
                shelfdef.TrnDT = DateTime.Now.ToString(sc.App.SCAppConstants.TimestampFormat_19);
                conn.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public void UpdateShelfEnableByZone(DBConnection_EF con, string zoneID)
        {
            string trn_dt = DateTime.Now.ToString(sc.App.SCAppConstants.TimestampFormat_19);
            string sql = "Update [ShelfDef] SET [Enable] = 'Y' ,[TrnDT] = {0} ,[Remark] = {1} ,[DISABLE_TIME] = {2} WHERE [ZoneID] = {3} AND [ADR_ID] <> '99999'";
            con.Database.ExecuteSqlCommand(sql, trn_dt, "", null, zoneID);
        }
        public void UpdateShelfDisableByZone(DBConnection_EF con, string zoneID, string remark)
        {
            string trn_dt = DateTime.Now.ToString(sc.App.SCAppConstants.TimestampFormat_19);
            string disable_dt = DateTime.Now.ToString(sc.App.SCAppConstants.DateTimeFormat_22);
            string sql = "Update [ShelfDef] SET [Enable] = 'N' ,[TrnDT] = {0} ,[Remark] = {1} ,[DISABLE_TIME] = {2} WHERE [ZoneID] = {3} AND [ADR_ID] <> '99999'";
            con.Database.ExecuteSqlCommand(sql, trn_dt, remark, disable_dt, zoneID);
        }


        public List<ShelfDef> LoadShelfDef(DBConnection_EF conn)
        {
            try
            {
                var port = from a in conn.ShelfDef
                           select a;
                return port.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public IQueryable getQueryAllSQL(DBConnection_EF conn)
        {
            try
            {
                var port = from a in conn.ShelfDef
                           select a;
                return port;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }


        public List<ShelfDef> LoadDisableShelf(DBConnection_EF conn, string zoneid)
        {
            try
            {
                var result = from a in conn.ShelfDef
                             where a.Enable == "N" && a.ZoneID == zoneid
                             select a;
                return result.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<ShelfDef> LoadEnableShelf(DBConnection_EF conn)
        {
            try
            {
                var result = from a in conn.ShelfDef
                             where a.Enable == "Y"
                             select a;
                return result.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<ShelfDef> LoadDisableShelf(DBConnection_EF conn)
        {
            try
            {
                var result = from a in conn.ShelfDef
                             where a.Enable == "N"
                             select a;
                return result.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<ShelfDef> LoadEnableShelfByZone(DBConnection_EF conn, string zoneid)
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.Enable == "Y" && x.ZoneID == zoneid)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<string> LoadEnableShelfIDsByZone(DBConnection_EF conn, string zoneid)
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.Enable == "Y" && x.ZoneID.Trim() == zoneid.Trim())
                    .Select(shelf => shelf.ShelfID.Trim())
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public int LoadEnableShelfCountByZone(DBConnection_EF conn, string zoneid)
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.Enable == "Y" && x.ZoneID.Trim() == zoneid.Trim())
                    .Count();

                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public ShelfDef GetShelfByID(DBConnection_EF conn, string shelfid)
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.ShelfID == shelfid)
                    .FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public List<ShelfDef> GetEmptyAndEnableShelfByZone(DBConnection_EF conn, string zoneID)
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.ZoneID.Trim() == zoneID.Trim() &&
                                x.ShelfState == ShelfDef.E_ShelfState.EmptyShelf &&
                                x.Enable == "Y")
                    .OrderByDescending(x => x.ShelfID).ToList();
                //.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public List<ShelfDef> GetEmptyAndEnableShelf(DBConnection_EF conn)
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.ShelfState == ShelfDef.E_ShelfState.EmptyShelf &&
                                x.Enable == "Y")
                    .OrderByDescending(x => x.ShelfID).ToList();
                //.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public List<ShelfDef> loadHasChangeShelfDefByAfterDateTime(DBConnection_EF conn, string afterDateTime)  //取得不是有改變狀態的儲位
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.TrnDT != "1" &&
                                x.TrnDT.CompareTo(afterDateTime) >= 0)
                    .ToList();
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public List<ShelfDef> GetReserveShelf(DBConnection_EF conn)  //取得不是空儲位的所有儲位
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.ShelfState != ShelfDef.E_ShelfState.EmptyShelf)
                    .OrderByDescending(x => x.ShelfID).ToList();
                return result;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }
        public bool IsEnable(DBConnection_EF conn, string shelfID)
        {
            try
            {
                var result = conn.ShelfDef
                    .Where(x => x.ShelfID.Trim() == shelfID.Trim() && x.Enable == "Y")
                    .Count();
                return result > 0;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
