﻿using com.mirle.ibg3k0.sc.BLL._191204Test.Extensions;
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
    public class ManualPortEventService : IManualPortEventService
    {
        private Logger logger = LogManager.GetLogger("ManualPortLogger");

        private string now { get => DateTime.Now.ToString("HH:mm:ss.fff"); }

        private ConcurrentDictionary<string, IManualPortValueDefMapAction> manualPorts { get; set; }

        private IManualPortReportBLL reportBll;
        private IManualPortCMDBLL commandBLL;
        private IManualPortAlarmBLL alarmBLL;
        private IManualPortDefBLL portDefBLL;
        private IManualPortShelfDefBLL shelfDefBLL;
        private IManualPortCassetteDataBLL cassetteDataBLL;
        private IManualPortTransferService transferService;

        private const string LITE_CASSETTE = "LC";
        private const string FOUP = "BE";

        public ManualPortEventService()
        {
            WriteLog("");
            WriteLog("");
            WriteLog("");
            WriteLog($"New ManualPortEventService");
        }

        public void Start(IEnumerable<IManualPortValueDefMapAction> ports,
                          IManualPortReportBLL reportBll,
                          IManualPortDefBLL portDefBLL,
                          IManualPortShelfDefBLL shelfDefBLL,
                          IManualPortCassetteDataBLL cassetteDataBLL,
                          IManualPortCMDBLL commandBLL,
                          IManualPortAlarmBLL alarmBLL,
                          IManualPortTransferService transferService)
        {
            this.reportBll = reportBll;
            this.portDefBLL = portDefBLL;
            this.shelfDefBLL = shelfDefBLL;
            this.cassetteDataBLL = cassetteDataBLL;
            this.commandBLL = commandBLL;
            this.alarmBLL = alarmBLL;
            this.transferService = transferService;

            WriteLog($"ManualPortEventService Start");

            RegisterEvent(ports);
        }

        private void RegisterEvent(IEnumerable<IManualPortValueDefMapAction> ports)
        {
            manualPorts = new ConcurrentDictionary<string, IManualPortValueDefMapAction>();

            foreach (var port in ports)
            {
                port.OnLoadPresenceChanged += Port_OnLoadPresenceChanged;
                port.OnWaitIn += Port_OnWaitIn;
                port.OnBcrReadDone += Port_OnBcrReadDone;
                port.OnWaitOut += Port_OnWaitOut;
                port.OnCstRemoved += Port_OnCstRemoved;
                port.OnDirectionChanged += Port_OnDirectionChanged;
                port.OnInServiceChanged += Port_OnInServiceChanged;
                port.OnAlarmHappen += Port_OnAlarmHappen;
                port.OnAlarmClear += Port_OnAlarmClear;
                port.OnDoorOpen += Port_OnDoorOpen;

                manualPorts.TryAdd(port.PortName, port);
                WriteLog($"Add Manual Port Event Success ({port.PortName})");
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

        #region LoadPresenceChanged

        private void Port_OnLoadPresenceChanged(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

                if (info.LoadPosition1)
                {
                    var logTitle = $"PortName[{args.PortName}] LoadPresenceChanged -> Stage1 ON => ";
                    WriteEventLog($"{logTitle} CarrierIdOfStage1[{stage1CarrierId}]");

                    StagePresenceOnProcess(logTitle, args.PortName, info);
                }
                else
                {
                    var logTitle = $"PortName[{args.PortName}] LoadPresenceChanged -> Stage1 OFF => ";
                    WriteEventLog($"{logTitle} CarrierIdOfStage1[{stage1CarrierId}]");

                    StagePresenceOffProcess(logTitle, args.PortName, info);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        private void StagePresenceOnProcess(string logTitle, string portName, ManualPortPLCInfo info)
        {
            if (info.Direction == DirectionType.InMode)
            {
                WriteEventLog($"{logTitle} The port direction is InMode. Do nothing.");
                return;
            }

            if (cassetteDataBLL.GetCarrierByPortName(portName, stage: 1, out var cassetteData) == false)
                WriteEventLog($"{logTitle} The port direction is OutMode. Cannot find carrier data at this port.");

            if (cassetteData == null)
                return;

            if (reportBll.ReportCarrierWaitOut(cassetteData))
                WriteEventLog($"{logTitle} Report MCS carrier wait out success.");
            else
                WriteEventLog($"{logTitle} Report MCS carrier wait out failed.");
        }

        private void StagePresenceOffProcess(string logTitle, string portName, ManualPortPLCInfo info)
        {
            if (info.Direction == DirectionType.InMode)
            {
                WriteEventLog($"{logTitle} The port direction is InMode. Do nothing.");
                return;
            }

            if (cassetteDataBLL.GetCarrierByPortName(portName, stage: 1, out var cassetteData) == false)
            {
                WriteEventLog($"{logTitle} The port direction is OutMode but cannot find carrier data at this port. Normal should have data.");
                return;
            }

            WriteEventLog($"{logTitle} The port direction is OutMode. Find a carrier data [{cassetteData.BOXID}] at this port.");

            if (reportBll.ReportCarrierRemoveFromManualPort(cassetteData))
                WriteEventLog($"{logTitle} Report MCS carrier remove From manual port success.");
            else
                WriteEventLog($"{logTitle} Report MCS carrier remove From manual port Failed.");

            cassetteDataBLL.Delete(cassetteData.BOXID);
            WriteEventLog($"{logTitle} Delete carrier data [{cassetteData.BOXID}].");
        }

        #endregion LoadPresenceChanged

        #region Wait In
        public void WaitInTest()
        {
            //ManualPortEventArgs args = new ManualPortEventArgs(new ManualPortPLCInfo() { CarrierIdOfStage1 = "12BEAA", EQ_ID = "B6_OHB01_M06" });
            //Port_OnWaitIn(null, args);
        }
        private void Port_OnWaitIn(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var readResult = info.CarrierIdReadResult == null ? "" : info.CarrierIdReadResult.Trim();
                var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

                var logTitle = $"PortName[{args.PortName}] WaitIn => ";

                WriteEventLog($"{logTitle} ReadResult[{readResult}] CarrierIdOfStage1[{stage1CarrierId}] CstType[{info.CstTypes}]({info.CarrierType})");

                if (HasCstTypeMismatch(logTitle, info))
                {
                    if (manualPorts.TryGetValue(args.PortName, out var plcPort))
                    {
                        plcPort.SetMoveBackReasonAsync(MoveBackReasons.TypeMismatch);
                        plcPort.MoveBackAsync();
                        WriteEventLog($"{logTitle} Move Back  Reason:(TypeMismatch).");
                    }
                    else
                    {
                        WriteEventLog($"{logTitle} Cannot find [IManualPortValueDefMapAction]. Cannot execute Move Back.");
                    }
                    return;
                }

                //if (IsFOUPTypeCSTWaitIn(logTitle, info))
                //{
                //    WriteEventLog($"{logTitle} 由於在測試階段，因此暫時強制拒絕Foup進入.");
                //    if (manualPorts.TryGetValue(args.PortName, out var plcPort))
                //    {
                //        plcPort.SetMoveBackReasonAsync(MoveBackReasons.Other);
                //        plcPort.MoveBackAsync();
                //        WriteEventLog($"{logTitle} Move Back  Reason:(Other).");
                //    }
                //    else
                //    {
                //        WriteEventLog($"{logTitle} Cannot find [IManualPortValueDefMapAction]. Cannot execute Move Back.");
                //    }

                //    return;
                //}

                if (cassetteDataBLL.GetCarrierByBoxId(stage1CarrierId, out var duplicateCarrierId))
                {
                    if (duplicateCarrierId.Carrier_LOC != args.PortName)
                        WaitInDuplicateProcess(logTitle, args.PortName, info, duplicateCarrierId);
                    else
                        WaitInNormalProcess(logTitle, args.PortName, info);
                }
                else
                    WaitInNormalProcess(logTitle, args.PortName, info);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        private bool IsFOUPTypeCSTWaitIn(string logTitle, ManualPortPLCInfo info)
        {
            var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();
            var subCarrierID = stage1CarrierId.Substring(2, 2);
            var plcType = info.CarrierType;
            if (subCarrierID == FOUP || plcType == CstType.A)
            {
                WriteEventLog($"{logTitle} 判斷到 Foup CST Wait in CST ID:{stage1CarrierId}, subCarrierID:{subCarrierID},PLC type:{plcType}.");
                return true;
            }
            return false;
        }

        public bool HasCstTypeMismatch(string logTitle, ManualPortPLCInfo info)
        {

            var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();
            if (stage1CarrierId.ToUpper().Contains("CIM"))
            {
                //如果是CIM開頭則不進行Box ID的檢查
                return false;
            }
            if (stage1CarrierId.Length <= 2)
            {
                WriteEventLog($"{logTitle} stage 1 carrier ID is [{stage1CarrierId}]. 長度過短.  Mismatch.  MoveBack.");
                return true;
            }

            var subCarrierID = stage1CarrierId.Substring(2, 2);
            var plcType = info.CarrierType;

            if (subCarrierID == LITE_CASSETTE)
            {
                if (plcType == CstType.B)
                {
                    WriteEventLog($"{logTitle} stage 1 carrier ID is [{stage1CarrierId}]. The third and fourth characters are [{LITE_CASSETTE}], which means it is a (Lite cassette). PLC Sensor is (Lite cassette) too.");
                    return false;
                }
                else
                {
                    WriteEventLog($"{logTitle} stage 1 carrier ID is [{stage1CarrierId}]. The third and fourth characters are [{LITE_CASSETTE}], which means it is a (Lite cassette). PLC Sensor is ({plcType}). Type mismtach.  Execute Moveback ! ");
                    return true;
                }
            }
            else if (subCarrierID == FOUP)
            {
                if (plcType == CstType.A)
                {
                    WriteEventLog($"{logTitle} stage 1 carrier ID is [{stage1CarrierId}]. The third and fourth characters are [{FOUP}], which means it is a (Foup). PLC Sensor is (Foup) too.");
                    return false;
                }
                else
                {
                    WriteEventLog($"{logTitle} stage 1 carrier ID is [{stage1CarrierId}]. The third and fourth characters are [{FOUP}], which means it is a (Foup). PLC Sensor is ({plcType}). Type mismtach.  Execute Moveback ! ");
                    return true;
                }
            }
            else
            {
                WriteEventLog($"{logTitle} stage 1 carrier ID is [{stage1CarrierId}]. The third and fourth characters are neither [{LITE_CASSETTE}] nor [{FOUP}]. ");
                return true;
            }
        }



        private void WaitInDuplicateProcess(string logTitle, string portName, ManualPortPLCInfo info, CassetteData duplicateCarrierData)
        {
            WriteEventLog($"{logTitle} Duplicate Happened. Duplication location is [{duplicateCarrierData.Carrier_LOC}]");

            if (portDefBLL.GetPortDef(duplicateCarrierData.Carrier_LOC, out var duplicateLocation))
            {
                if (duplicateLocation.ToUnitType().IsShlef())
                    WaitInDuplicateAtShelfProcess(logTitle, portName, info, duplicateCarrierData);
                else
                    WaitInDuplicateAtPortProcess(logTitle, portName, info, duplicateCarrierData, duplicateLocation);
            }
            else
            {
                WaitInDuplicateAtOhtProcess(logTitle, portName, info, duplicateCarrierData);
            }
        }

        public string GetDuplicateUnknownId(string carrierId)
        {
            var year = DateTime.Now.Year % 100;
            var date = string.Format("{0}{1:00}{2:00}{3:00}{4:00}", year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
            var id = "UNKD" + carrierId + date + string.Format("{0:00}", DateTime.Now.Second);

            return id;
        }

        private void WaitInDuplicateAtShelfProcess(string logTitle, string portName, ManualPortPLCInfo info, CassetteData duplicateCarrierData)
        {
            WriteEventLog($"{logTitle} Duplicate at shelf ({duplicateCarrierData.Carrier_LOC}).");

            CheckDuplicateCarrier(logTitle, duplicateCarrierData, out var needRemoveDuplicateShelf, out var unknownId);

            var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

            if (needRemoveDuplicateShelf)
            {
                cassetteDataBLL.Install(portName, stage1CarrierId, info.CarrierType);
                WriteEventLog($"{logTitle} Install cassette data [{stage1CarrierId}] Type[{info.CarrierType}] at this port.");
            }
            else
            {
                cassetteDataBLL.Install(portName, unknownId, info.CarrierType);
                WriteEventLog($"{logTitle} Install cassette data [{unknownId}] Type[{info.CarrierType}] at this port.");
            }

            cassetteDataBLL.GetCarrierByPortName(portName, 1, out var cassetteData);

            ReportIDRead(logTitle, cassetteData, isDuplicate: true);

            cassetteDataBLL.GetCarrierByPortName(duplicateCarrierData.Carrier_LOC, 1, out var duplicateData);

            if (needRemoveDuplicateShelf)
            {
                ReportForcedCarrierRemove(logTitle, duplicateCarrierData);
                ReportInstallCarrier(logTitle, duplicateData);
            }

            ReportWaitIn(logTitle, cassetteData);
        }

        private void CheckDuplicateCarrier(string logTitle, CassetteData duplicateCarrierData, out bool needRemoveDuplicateShelf, out string unknownId)
        {
            needRemoveDuplicateShelf = true;

            unknownId = GetDuplicateUnknownId(duplicateCarrierData.BOXID);
            var duplicateCarrierhasNoCommand = commandBLL.GetCommandByBoxId(duplicateCarrierData.BOXID, out var command) == false;
            if (duplicateCarrierhasNoCommand)
            {
                ChageDuplicateLocationCarrierIdToUnknownId(logTitle, duplicateCarrierData, unknownId);
                return;
            }

            WriteEventLog($"{logTitle} Duplicate carrier has command [{command.CMD_ID}] now.");

            if (command.TRANSFERSTATE == E_TRAN_STATUS.Queue)
            {
                WriteEventLog($"{logTitle} Command state is queue.");

                commandBLL.Delete(duplicateCarrierData.BOXID);
                WriteEventLog($"{logTitle} Delete Command.");

                ChageDuplicateLocationCarrierIdToUnknownId(logTitle, duplicateCarrierData, unknownId);

                return;
            }

            needRemoveDuplicateShelf = false;

            WriteEventLog($"{logTitle} Command state is not queue.  Install UnknownID[{unknownId}] on this wait in port.");
        }

        private void ChageDuplicateLocationCarrierIdToUnknownId(string logTitle, CassetteData duplicateCarrierData, string unknownId)
        {
            cassetteDataBLL.Delete(duplicateCarrierData.BOXID);
            WriteEventLog($"{logTitle} Delete duplicate carrier.");

            shelfDefBLL.SetStored(duplicateCarrierData.Carrier_LOC);
            WriteEventLog($"{logTitle} Set shelf stage of duplicate shelf[{duplicateCarrierData.Carrier_LOC}] to stored.");

            cassetteDataBLL.Install(duplicateCarrierData.Carrier_LOC, unknownId, duplicateCarrierData.CSTType.ToCstType());
            WriteEventLog($"{logTitle} Install UnknownID[{unknownId}] on shelf[{duplicateCarrierData.Carrier_LOC}].");
        }

        private void WaitInDuplicateAtPortProcess(string logTitle, string portName, ManualPortPLCInfo info, CassetteData duplicateCarrierData, PortDef duplicatePort)
        {
            WriteEventLog($"{logTitle} Duplicate at Port ({duplicatePort.PLCPortID}).");

            if (commandBLL.GetCommandByBoxId(duplicateCarrierData.BOXID, out var command))
            {
                bool is_excute_normal_duplocate = duplicatePort.ToUnitType().IsEQPort() &&
                                                  IsExcuteNormalDuplicateProcess(logTitle, duplicateCarrierData, command);
                if (is_excute_normal_duplocate)
                {
                    //not thing...
                }
                else
                {
                    WriteEventLog($"{logTitle} Duplicate carrier has command [{command.CMD_ID}] now.");

                    var unknownId = GetDuplicateUnknownId(duplicateCarrierData.BOXID);
                    cassetteDataBLL.Install(portName, unknownId, info.CarrierType);
                    WriteEventLog($"{logTitle} Install cassette data [{unknownId}] Type[{info.CarrierType}] at this port.");

                    cassetteDataBLL.GetCarrierByPortName(portName, 1, out var cassetteData);

                    ReportIDRead(logTitle, cassetteData, isDuplicate: true);
                    ReportWaitIn(logTitle, cassetteData);
                    return;
                }
            }


            bool is_delete_success = cassetteDataBLL.Delete(duplicateCarrierData.BOXID);
            WriteEventLog($"{logTitle} Delete duplicate cassette data [{duplicateCarrierData.BOXID}],result:{is_delete_success}.");

            var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

            cassetteDataBLL.Install(portName, stage1CarrierId, info.CarrierType);
            WriteEventLog($"{logTitle} Install cassette data [{stage1CarrierId}] Type[{info.CarrierType}] at this port.");

            cassetteDataBLL.GetCarrierByPortName(portName, 1, out var cassetteData2);

            ReportIDRead(logTitle, cassetteData2, isDuplicate: true);

            if (is_delete_success)
                ReportForcedCarrierRemove(logTitle, duplicateCarrierData);

            ReportWaitIn(logTitle, cassetteData2);
        }

        const int MAX_WAITTING_CANCEL_TIME_WHEN_DUPLICATE_HAPPEND_MS = 30_000;
        private bool IsExcuteNormalDuplicateProcess(string logTitle, CassetteData duplicateCarrierData, ACMD_MCS command)
        {
            WriteEventLog($"{logTitle} cst: [{duplicateCarrierData.BOXID}] is duplicate .");

            if (command.IsQueue)
            {
                WriteEventLog($"{logTitle} has transfer command :{sc.Common.SCUtility.Trim(command.CMD_ID, true)} in queue, direct force finish it.");

                var result = transferService.ForceFinishMCSCmd
                    (command, duplicateCarrierData, nameof(IsExcuteNormalDuplicateProcess), ACMD_MCS.ResultCode.OtherErrors);
                return sc.Common.SCUtility.isMatche(result, "OK");
            }
            else
            {
                if (!command.IsLoadArriveBefore)
                {
                    WriteEventLog($"{logTitle} has transfer command :{sc.Common.SCUtility.Trim(command.CMD_ID, true)}, 但狀態已經在load arrive 之後,不進行處理");
                    return false;
                }
                WriteEventLog($"{logTitle} has transfer command :{sc.Common.SCUtility.Trim(command.CMD_ID, true)}, OHT:{command.CRANE}前往搬送中,準備將其結束命令...");

                bool is_sned_cancel_success = transferService.tryCancelMCSCmd(command);
                if (is_sned_cancel_success)
                {
                    //開始等待命令結束...
                    bool is_cmd_cancel_complete = SpinWait.SpinUntil(() => isCancelSuccess(command.CMD_ID),
                                                                           MAX_WAITTING_CANCEL_TIME_WHEN_DUPLICATE_HAPPEND_MS);
                    if (is_cmd_cancel_complete)
                    {
                        WriteEventLog($"{logTitle} has transfer command :{sc.Common.SCUtility.Trim(command.CMD_ID, true)}, OHT:{command.CRANE}前往搬送中,等待結束命令完成。");
                        return true;
                    }
                    else
                    {
                        WriteEventLog($"{logTitle} has transfer command :{sc.Common.SCUtility.Trim(command.CMD_ID, true)}, OHT:{command.CRANE}前往搬送中,等待結束命令超時。");
                        return false;
                    }
                }
                else
                {
                    WriteEventLog($"{logTitle} has transfer command :{sc.Common.SCUtility.Trim(command.CMD_ID, true)}, OHT:{command.CRANE}前往搬送中,結束命令失敗");
                    return false;
                }
            }
        }
        private bool isCancelSuccess(string cmdMCSID)
        {
            string cmd_mcs_id = sc.Common.SCUtility.Trim(cmdMCSID, true);
            bool try_get_success = ACMD_MCS.MCS_CMD_InfoList.TryGetValue(cmd_mcs_id, out var waittingCancelCmd);
            if (try_get_success)
            {
                if (waittingCancelCmd.TRANSFERSTATE == E_TRAN_STATUS.TransferCompleted)
                {
                    return true;
                }
                else
                {
                    SpinWait.SpinUntil(() => false, 1_000);
                    return false;
                }
            }
            else
            {
                //代表命令已經結束
                return true;
            }
        }

        private void WaitInDuplicateAtOhtProcess(string logTitle, string portName, ManualPortPLCInfo info, CassetteData duplicateCarrierData)
        {
            WriteEventLog($"{logTitle} Duplicate at OHT ({duplicateCarrierData.Carrier_LOC}).");

            var unknownId = GetDuplicateUnknownId(duplicateCarrierData.BOXID);
            cassetteDataBLL.Install(portName, unknownId, info.CarrierType);
            WriteEventLog($"{logTitle} Install cassette data [{unknownId}] Type[{info.CarrierType}] at this port.");

            cassetteDataBLL.GetCarrierByPortName(portName, 1, out var cassetteData);

            ReportIDRead(logTitle, cassetteData, isDuplicate: true);
            ReportWaitIn(logTitle, cassetteData);
        }

        private void WaitInNormalProcess(string logTitle, string portName, ManualPortPLCInfo info)
        {
            WriteEventLog($"{logTitle} Normal Process.");

            CheckResidualCassetteProcess(logTitle, portName);

            var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

            cassetteDataBLL.Install(portName, stage1CarrierId, info.CarrierType);
            WriteEventLog($"{logTitle} Install cassette data [{stage1CarrierId}] Type[{info.CarrierType}] at this port.");

            cassetteDataBLL.GetCarrierByPortName(portName, stage: 1, out var cassetteData);

            ReportIDRead(logTitle, cassetteData, isDuplicate: false);
            ReportWaitIn(logTitle, cassetteData);
        }

        private void CheckResidualCassetteProcess(string logTitle, string portName)
        {
            cassetteDataBLL.GetCarrierByPortName(portName, stage: 1, out var residueCassetteData);
            var hasNoResidueData = residueCassetteData == null;
            if (hasNoResidueData)
                return;

            var residueCarrierId = residueCassetteData.BOXID;
            WriteEventLog($"{logTitle} There is residual cassette data [{residueCarrierId}] on the port.");

            if (commandBLL.GetCommandByBoxId(residueCarrierId, out var residueCommand))
            {
                WriteEventLog($"{logTitle} There is residual command [{residueCommand.CMD_ID}] cassette data [{residueCarrierId}].");

                commandBLL.Delete(residueCarrierId);
                WriteEventLog($"{logTitle} Delete residual command [{residueCommand.CMD_ID}].");

                ReportForcedTransferComplete(logTitle, residueCommand, residueCassetteData);
            }

            cassetteDataBLL.Delete(residueCarrierId);
            WriteEventLog($"{logTitle} Delete residual cassette data.");

            ReportForcedCarrierRemove(logTitle, residueCassetteData);
        }

        private void ReportForcedTransferComplete(string logTitle, ACMD_MCS command, CassetteData cassetteData)
        {
            if (reportBll.ReportTransferCompleted(command, cassetteData, ResultCode.OtherErrors))
                WriteEventLog($"{logTitle} Report MCS TransferComplete  ResultCod -> OtherErrors  Success.");
            else
                WriteEventLog($"{logTitle} Report MCS TransferComplete  ResultCode -> OtherErrors  Failed.");
        }

        private void ReportForcedCarrierRemove(string logTitle, CassetteData cassetteData)
        {
            if (reportBll.ReportForcedRemoveCarrier(cassetteData))
                WriteEventLog($"{logTitle} Report MCS CarrierRemoveComplete Success. CarrierId[{cassetteData.BOXID}]");
            else
                WriteEventLog($"{logTitle} Report MCS CarrierRemoveComplete Failed.  CarrierId[{cassetteData.BOXID}]");
        }

        private void ReportInstallCarrier(string logTitle, CassetteData cassetteData)
        {
            if (reportBll.ReportCarrierInstall(cassetteData))
                WriteEventLog($"{logTitle} Report MCS CarrierInstall Success. CarrierId[{cassetteData.BOXID}]");
            else
                WriteEventLog($"{logTitle} Report MCS CarrierInstall Failed.  CarrierId[{cassetteData.BOXID}]");
        }

        private void ReportIDRead(string logTitle, CassetteData cassetteData, bool isDuplicate)
        {
            if (reportBll.ReportCarrierIDRead(cassetteData, isDuplicate))
                WriteEventLog($"{logTitle} Report MCS CarrierIDRead Success. CarrierId[{cassetteData.BOXID}] isDuplicate[{isDuplicate}]");
            else
                WriteEventLog($"{logTitle} Report MCS CarrierIDRead Failed.  CarrierId[{cassetteData.BOXID}] isDuplicate[{isDuplicate}]");
        }

        private void ReportWaitIn(string logTitle, CassetteData cassetteData)
        {
            if (reportBll.ReportCarrierWaitIn(cassetteData))
                WriteEventLog($"{logTitle} Report MCS WaitIn Success.");
            else
                WriteEventLog($"{logTitle} Report MCS WaitIn Failed.");
        }

        #endregion Wait In

        private void Port_OnBcrReadDone(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var readResult = info.CarrierIdReadResult.Trim();
                var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

                WriteEventLog($"PortName[{args.PortName}] BcrReadDone => ReadResult[{readResult}] CarrierIdOfStage1[{stage1CarrierId}]");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        private void Port_OnWaitOut(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

                WriteEventLog($"PortName[{args.PortName}] WaitOut => CarrierIdOfStage1[{stage1CarrierId}]");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        private void Port_OnCstRemoved(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

                WriteEventLog($"PortName[{args.PortName}] CstRemoved Bit ON => CarrierIdOfStage1[{stage1CarrierId}]");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        private void Port_OnDirectionChanged(object sender, ManualPortEventArgs args)
        {
            try
            {
                var newDirection = "";
                if (args.ManualPortPLCInfo.Direction == DirectionType.InMode)
                    newDirection += "DirectionChangeTo_InMode";
                else
                    newDirection += "DirectionChangeTo_OutMode";

                var logTitle = $"PortName[{args.PortName}] {newDirection} => ";

                var info = args.ManualPortPLCInfo;
                var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

                WriteEventLog($"{logTitle} CarrierIdOfStage1[{stage1CarrierId}]");

                CheckResidualCassetteProcess(logTitle, args.PortName);

                if (args.ManualPortPLCInfo.Direction == DirectionType.InMode)
                {
                    reportBll.ReportPortDirectionChanged(args.PortName, newDirectionIsInMode: true);
                    WriteEventLog($"{logTitle} Report MCS PortTypeChange InMode");

                    portDefBLL.ChangeDirectionToInMode(args.PortName);
                    WriteEventLog($"{logTitle} PortDef change direction to InMode");

                    if (manualPorts.TryGetValue(args.PortName, out var plcPort))
                    {
                        plcPort.ChangeToInModeAsync(isOn: false);
                        WriteEventLog($"{logTitle} OFF ChangeToInMode Signal");
                    }
                    else
                        WriteEventLog($"{logTitle} Cannot OFF ChangeToInMode Signal. Because cannot find IManualPortValueDefMapAction by portName[{args.PortName}]");
                }
                else
                {
                    reportBll.ReportPortDirectionChanged(args.PortName, newDirectionIsInMode: false);
                    WriteEventLog($"{logTitle} Report MCS PortTypeChange OutMode");

                    portDefBLL.ChangeDirectionToOutMode(args.PortName);
                    WriteEventLog($"{logTitle} PortDef change direction to OutMode");

                    if (manualPorts.TryGetValue(args.PortName, out var plcPort))
                    {
                        plcPort.ChangeToOutModeAsync(isOn: false);
                        WriteEventLog($"{logTitle} OFF ChangeToOutMode Signal");
                    }
                    else
                        WriteEventLog($"{logTitle} Cannot OFF ChangeToOutMode Signal. Because cannot find IManualPortValueDefMapAction by portName[{args.PortName}]");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        #region Alarm

        private void Port_OnAlarmHappen(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var portName = args.PortName;
                var logTitle = $"PortName[{args.PortName}] AlarmHappen => ";
                WriteEventLog($"{logTitle} AlarmIndex[{info.ErrorIndex}] AlarmCode[{info.AlarmCode}] IsRun[{info.IsRun}] IsDown[{info.IsDown}] IsAlarm[{info.IsAlarm}]");

                var alarmCode = info.AlarmCode.ToString().Trim();
                var commandOfPort = GetCommandOfPort(info);

                if (alarmBLL.SetAlarm(portName, alarmCode, commandOfPort, out var alarmReport, out var reasonOfAlarmSetFailed) == false)
                {
                    WriteEventLog($"{logTitle} AlarmCode[{info.AlarmCode}] Set Alarm failed. ({reasonOfAlarmSetFailed}) Cannot report MCS Alarm.");
                    return;
                }

                if (alarmReport.ALAM_LVL == E_ALARM_LVL.Error)
                {
                    reportBll.ReportAlarmSet(alarmReport);
                    WriteEventLog($"{logTitle} AlarmCode[{info.AlarmCode}] Alarm level is (Error). Report alarm set.");
                }
                else if (alarmReport.ALAM_LVL == E_ALARM_LVL.Warn)
                {
                    reportBll.ReportUnitAlarmSet(alarmReport);
                    WriteEventLog($"{logTitle} AlarmCode[{info.AlarmCode}] Alarm level is (Warn). Report unit alarm set.");
                }
                else
                    WriteEventLog($"{logTitle} AlarmCode[{info.AlarmCode}] Not reported because the alarm level is (None).  Should be (Error) or (Warn).");

                UpdateOHBCErrorIndex(logTitle, args);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        private void Port_OnAlarmClear(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var portName = args.PortName;
                var logTitle = $"PortName[{args.PortName}] AlarmClear => ";
                WriteEventLog($"{logTitle} AlarmIndex[{info.ErrorIndex}] AlarmCode[{info.AlarmCode}] IsRun[{info.IsRun}] IsDown[{info.IsDown}] IsAlarm[{info.IsAlarm}]");

                var alarmCode = info.AlarmCode.ToString().Trim();
                var commandOfPort = GetCommandOfPort(info);

                if (alarmBLL.ClearAllAlarm(portName, commandOfPort, out var alarmReports, out var reasonOfAlarmClearFailed) == false)
                {
                    WriteEventLog($"{logTitle} Clear all Alarm failed. ({reasonOfAlarmClearFailed}). Cannot report MCS Alarm.");
                    return;
                }

                foreach (var alarm in alarmReports)
                {
                    if (alarm.ALAM_LVL == E_ALARM_LVL.Error)
                    {
                        reportBll.ReportAlarmClear(alarm);
                        WriteEventLog($"{logTitle} AlarmCode[{info.AlarmCode}] Alarm level is (Error). Report alarm clear.");
                    }
                    else if (alarm.ALAM_LVL == E_ALARM_LVL.Warn)
                    {
                        reportBll.ReportUnitAlarmClear(alarm);
                        WriteEventLog($"{logTitle} AlarmCode[{info.AlarmCode}] Alarm level is (Warn). Report unit alarm clear.");
                    }
                    else
                        WriteEventLog($"{logTitle} AlarmCode[{info.AlarmCode}] Not reported because the alarm level is (None).  Should be (Error) or (Warn).");
                }

                UpdateOHBCErrorIndex(logTitle, args);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        private void UpdateOHBCErrorIndex(string logTitle, ManualPortEventArgs args)
        {
            var info = args.ManualPortPLCInfo;

            if (manualPorts.TryGetValue(args.PortName, out var plcAction))
            {
                plcAction.SetControllerErrorIndexAsync(info.ErrorIndex);
                WriteEventLog($"{logTitle} Set OHBC AlarmIndex to [{info.ErrorIndex}]");
            }
            else
                WriteEventLog($"{logTitle} Set OHBC AlarmIndex to [{info.ErrorIndex}] Failed. Cannot find IManualPortValueDefMapAction by PortName[{args.PortName}]");
        }

        private ACMD_MCS GetCommandOfPort(ManualPortPLCInfo info)
        {
            var stage1CarrierId = info.CarrierIdOfStage1 == null ? string.Empty : info.CarrierIdOfStage1.Trim();

            var hasCommand = commandBLL.GetCommandByBoxId(stage1CarrierId, out var commandOfPort);
            if (hasCommand == false)
            {
                commandOfPort = new ACMD_MCS();
                commandOfPort.CMD_ID = "";
                commandOfPort.BOX_ID = "";
            }

            return commandOfPort;
        }

        #endregion Alarm

        private void Port_OnInServiceChanged(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var logTitle = $"PortName[{args.PortName}] InServiceChanged => ";

                WriteEventLog($"{logTitle} IsRun[{info.IsRun}] IsDown[{info.IsDown}] IsAlarm[{info.IsAlarm}]");
                E_PORT_STATUS port_status = E_PORT_STATUS.NoDefinition;
                if (args.ManualPortPLCInfo.IsRun)
                {
                    reportBll.ReportPortInServiceChanged(args.PortName, newStateIsInService: true);
                    WriteEventLog($"{logTitle} Report MCS PortInService");
                    port_status = E_PORT_STATUS.InService;
                }
                else
                {
                    reportBll.ReportPortInServiceChanged(args.PortName, newStateIsInService: false);
                    WriteEventLog($"{logTitle} Report MCS PortOutOfService");
                    port_status = E_PORT_STATUS.OutOfService;
                }
                portDefBLL.UpdataPortService(args.PortName, port_status);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }

        private void Port_OnDoorOpen(object sender, ManualPortEventArgs args)
        {
            try
            {
                var info = args.ManualPortPLCInfo;
                var logTitle = $"PortName[{args.PortName}] DoorOpenChanged => ";

                if (info.IsDoorOpen)
                    WriteEventLog($"{logTitle} Door Open");
                else
                    WriteEventLog($"{logTitle} Door Close");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "");
                WriteEventLog($"Port[{args.PortName}]  {MethodBase.GetCurrentMethod()}  Exception ! 【{ex}】");
            }
        }
    }
}