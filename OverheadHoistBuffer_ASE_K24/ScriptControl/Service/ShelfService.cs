using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.SECS.ASE;
using CommonMessage.ProtocolFormat.ShelfFun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace com.mirle.ibg3k0.sc.Service
{
    public class ShelfService
    {
        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private SCApplication scApp = null;
        private ShelfDefBLL shelfDefBLL = null;
        private bool AlreadyNotify_UNKE_Happend = false;
        public event EventHandler<bool> UNKE_Happend;

        public ShelfService()
        {

        }
        public void start(SCApplication _app)
        {
            scApp = _app;
            shelfDefBLL = _app.ShelfDefBLL;
        }


        public bool doUpdatePriority(string shelf_id, int priority)
        {
            bool isSuccess = true;
            string result = string.Empty;
            try
            {
                if (isSuccess)
                {
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            isSuccess = shelfDefBLL.updatePriority(shelf_id, priority);
                            if (isSuccess)
                            {
                                tx.Complete();
                                //scApp.PortStationBLL.OperateCatch.updatePriority(shelf_id, priority);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error(ex, "Execption:");
            }
            return isSuccess;
        }

        public bool doUpdateEnable(string shelf_id, bool enable)
        {
            bool isSuccess = true;
            string result = string.Empty;
            try
            {
                if (isSuccess)
                {
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            isSuccess = shelfDefBLL.updateEnable(shelf_id, enable);
                            if (isSuccess)
                            {
                                tx.Complete();
                                //scApp.PortStationBLL.OperateCatch.updatePriority(shelf_id, priority);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error(ex, "Execption:");
            }
            return isSuccess;
        }

		internal bool doUpdateState(string shelf_id, string state)
		{
            bool isSuccess = true;
            string result = string.Empty;
            try
            {
                if (isSuccess)
                {
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            isSuccess = shelfDefBLL.updateStatus(shelf_id, state);
                            if (isSuccess)
                            {
                                tx.Complete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error(ex, "Execption:");
            }
            return isSuccess;
        }

        internal bool doUpdateRemark(string shelf_id, string remark)
        {
            bool isSuccess = true;
            string result = string.Empty;
            try
            {
                if (isSuccess)
                {
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            isSuccess = shelfDefBLL.updateRemark(shelf_id, remark);
                            if (isSuccess)
                            {
                                tx.Complete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                logger.Error(ex, "Execption:");
            }
            return isSuccess;

        }


        public void doCheckShelfWhenUNKE()
        {
            bool hasUNKE = scApp.CassetteDataBLL.LoadCassetteDataByCSTID_UNKandOnShelf()
                .Any(carrier => carrier.CSTID.StartsWith("UNKE"));

            if (hasUNKE && !AlreadyNotify_UNKE_Happend)
            {
                UNKE_Happend.Invoke(this, true); //如果有UNKE但沒記錄他已經發生過，表示 false=>true
                scApp.ReportBLL.ReportAlarmHappend(ProtocolFormat.OHTMessage.ErrorStatus.ErrSet, "88888", "EmptyRetrieval");
            }
            else if (!hasUNKE && AlreadyNotify_UNKE_Happend)
            {
                UNKE_Happend.Invoke(this, false); //如果沒有UNKE帳但系統曾發過已發生是建，表示true => false
                scApp.ReportBLL.ReportAlarmHappend(ProtocolFormat.OHTMessage.ErrorStatus.ErrReset, "88888", "EmptyRetrieval");
            }
        }
    }
}
