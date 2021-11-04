using com.mirle.ibg3k0.sc.BLL._191204Test.Extensions;
using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Extension;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Service.Interface;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using static com.mirle.ibg3k0.sc.ACMD_MCS;

namespace com.mirle.ibg3k0.sc.Service
{
    public class ReelNTBEventService
    {
        private Logger logger = LogManager.GetLogger("ReelNTBLogger");

        private string now { get => DateTime.Now.ToString("HH:mm:ss.fff"); }

        private ConcurrentDictionary<string, IReelNTBValueDefMapAction> manualPorts { get; set; }

        private sc.App.SCApplication scApp;
        private IReelNTBReportBLL reportBll;
        private IReelNTBEquipmentBLL equipmentBLL;
        private IReelNTBPortStationBLL portStationBLL;

        public ReelNTBEventService()
        {
            WriteLog("");
            WriteLog("");
            WriteLog("");
            WriteLog($"New ReelNTBEventService");
        }

        public void Start(sc.App.SCApplication _scApp, IReelNTBReportBLL reportBll, IReelNTBEquipmentBLL equipmentBLL, IReelNTBPortStationBLL portStationBLL)
        {
            scApp = _scApp;
            this.reportBll = reportBll;
            this.equipmentBLL = equipmentBLL;
            this.portStationBLL = portStationBLL;

            WriteLog($"ReelNTBEventService Start");

            RegisterEvent();
        }

        private void RegisterEvent()
        {
            var reel_ntbs = equipmentBLL.loadReelNTBs();
            foreach (var ntb in reel_ntbs)
            {
                ntb.getReelNTBCDefaultMapActionReceive().TransferCommandRequest += Eq_TransferCommandRequest;

                ntb.RelatedReelCSTReceiveMCSCmd += Ntb_RelatedReelCSTReceiveMCSCmd;
                ntb.RelatedReelCSTTransferring += Ntb_RelatedReelCSTTransferring;
                ntb.RelatedReelCSTTransfeFail += Ntb_RelatedReelCSTTransfeFail;
                ntb.RelatedReelCSTArrived += Ntb_RelatedReelCSTArrived;
            }
        }

        private void Ntb_RelatedReelCSTArrived(object sender, ACMD_MCS cmd_mcs)
        {
            try
            {
                var reel_ntb = sender as Data.VO.ReelNTB;
                string cst_id = sc.Common.SCUtility.Trim(cmd_mcs.BOX_ID, true);
                string mcs_cmd_id = sc.Common.SCUtility.Trim(cmd_mcs.CMD_ID, true);
                bool is_to_ntb = cmd_mcs.IsDestReelNTB(scApp.TransferService);
                Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState reelTransferState =
                    is_to_ntb ?
                    Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.ArrivedNtbPort :
                    Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.ArrivedEqPort;

                reel_ntb.ReelStateUpdate(cst_id, reelTransferState, is_to_ntb, mcs_cmd_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private void Ntb_RelatedReelCSTTransfeFail(object sender, ACMD_MCS cmd_mcs)
        {
            try
            {
                var reel_ntb = sender as Data.VO.ReelNTB;
                string cst_id = sc.Common.SCUtility.Trim(cmd_mcs.BOX_ID, true);
                string mcs_cmd_id = sc.Common.SCUtility.Trim(cmd_mcs.CMD_ID, true);
                bool is_to_ntb = cmd_mcs.IsDestReelNTB(scApp.TransferService);
                Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState reelTransferState =
                    is_to_ntb ?
                    Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.FailedWhenTransferringToNtbPort :
                    Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.FailedWhenTransferringToEqPort;

                reel_ntb.ReelStateUpdate(cst_id, reelTransferState, is_to_ntb, mcs_cmd_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private void Ntb_RelatedReelCSTTransferring(object sender, ACMD_MCS cmd_mcs)
        {
            try
            {
                var reel_ntb = sender as Data.VO.ReelNTB;
                string cst_id = sc.Common.SCUtility.Trim(cmd_mcs.BOX_ID, true);
                string mcs_cmd_id = sc.Common.SCUtility.Trim(cmd_mcs.CMD_ID, true);
                bool is_to_ntb = cmd_mcs.IsDestReelNTB(scApp.TransferService);
                Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState reelTransferState =
                    is_to_ntb ?
                    Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.TransferringToNtbPort :
                    Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.TransferringToEqPort;

                reel_ntb.ReelStateUpdate(cst_id, reelTransferState, is_to_ntb, mcs_cmd_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private void Ntb_RelatedReelCSTReceiveMCSCmd(object sender, ACMD_MCS cmd_mcs)
        {
            try
            {
                var reel_ntb = sender as Data.VO.ReelNTB;
                string cst_id = sc.Common.SCUtility.Trim(cmd_mcs.BOX_ID, true);
                string mcs_cmd_id = sc.Common.SCUtility.Trim(cmd_mcs.CMD_ID, true);
                bool is_to_ntb = cmd_mcs.IsDestReelNTB(scApp.TransferService);
                Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState reelTransferState =
                    is_to_ntb ?
                    Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.ReciveMcsTransferToNtbCommand :
                    Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.ReciveMcsTransferToEqPortCommand;

                reel_ntb.ReelStateUpdate(cst_id, reelTransferState, is_to_ntb, mcs_cmd_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private void Eq_TransferCommandRequest(object sender, Data.ValueDefMapAction.Events.ReelNTB.ReelNTBTranCmdReqEventArgs args)
        {
            try
            {
                var reel_ntb = args.ReelNTB;
                string cst_id = args.CarrierReelId;

                reel_ntb.ReelStateUpdate(cst_id, Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.ReciveNtbcTransferRequest, false, "");
                reportBll.ReportCarrierTransferRequest(args);
                reel_ntb.ReelStateUpdate(cst_id, Mirle.U332MA30.Grpc.OhbcNtbcConnect.ReelTransferState.SendMcsWaitIn, false, "");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private long syncPoint = 0;
        public void RefreshReelNTBPortSignal()
        {
            if (Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
                {
                    var reel_ntbs = equipmentBLL.loadReelNTBs();
                    foreach (var eq in reel_ntbs)
                    {
                        var port_stations = portStationBLL.loadReelNTBPortStations(eq.EQPT_ID);
                        foreach (var port in port_stations)
                        {
                            var port_signal = eq.GetOhtPortSignal(port.PORT_ID);
                            if (port_signal != null)
                            {
                                port.state = port_signal.State;
                                port.direction = port_signal.Direction;
                                port.requestType = port_signal.RequestState;
                                port.CarrierReelId = port_signal.CarrierReelId;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                finally
                {
                    Interlocked.Exchange(ref syncPoint, 0);
                }
            }
        }


        #region Log

        private void WriteLog(string message)
        {
            var logMessage = $"[{now}] {message}";
            logger.Info(logMessage);
        }

        private void WriteEventLog(string message)
        {
            var logMessage = $"[{now}] PLC Event | {message}";
            logger.Info(logMessage);
        }

        #endregion Log

    }
}