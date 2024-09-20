//*********************************************************************************
//      MESDefaultMapAction.cs
//*********************************************************************************
// File Name: MESDefaultMapAction.cs
// Description: 與EAP通訊的劇本
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
// 2020/02/23    Kevin Wei      N/A            B0.01   功能增加，為了加入Cycle run時，需要額外去更新Carrier location
// 2020/02/27    Kevin Wei      N/A            B0.02   加入多個Reserve詢問的功能。
// 2020/04/17    Jason Wu       N/A            B0.03   加入Vehicle Abort, BCR Read Fail 與InterLock Error 做一次Alarm Set 與 Alarm Clear 以記錄在 MCS
// 2020/04/21    Jason Wu       N/A            B0.04   修改對OHT 31 cmd 之 load unload 命令路徑判定(主要是針對有原地取貨或原地放貨情況)
// 2020/05/04    Jason Wu       N/A            B0.05   新增BoxID更新，但尚未開啟，因為這部分OHT部分之回報要先有修正後才能開啟
// 2020/05/24    Jason Wu       N/A            B0.06   修改回報136 unload complete及 132 command complete 時會判定是否上報，shelf 上報，port 不上報。
// 2020/05/24    Jason Wu       N/A            B0.07   新增funtion "GetVehicleIDByPortID(string portID)" 讓上層能呼叫出目前port ID address 上的車輛ID
// 2020/05/27    Jason Wu       N/A            B0.08   新增funtion "GetVehicleDataByVehicleID(string vehicleID)" 讓上層能呼叫出目前vehicle ID 的vehicle cache 實時資料
// 2020/05/27    Jason Wu       N/A            B0.08.0 新增Task Run 在 132 命令完成之後，會處發TransferRun，使MCS命令可以在多車情形下早於趕車CMD下達。
// 2020/08/27    Kevin Wei      N/A            B0.09   修改選擇避車點邏輯。原本是找目前in mode的CV，改成固定找被標記的CV Port。(目前是固定設定在LOOP-T01、T0A)
// 2020/08/27    Kevin Wei      N/A            B0.10   修改針對OHT進行 data initial的時機。
//                                                     原:一有連線事件觸發，就直接進行
//                                                     改:在連線事件後且詢問143狀態成功更新後。
// 2020/09/05    Kevin Wei      N/A            B0.11   加入在命令結束時，如果沒有MCS命令要準備派送，將會讓他到停等點待命
//**********************************************************************************
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data;
using com.mirle.ibg3k0.sc.Data.Enum;
using com.mirle.ibg3k0.sc.Data.PLC_Functions;
using com.mirle.ibg3k0.sc.Data.SECS.ASE;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Google.Protobuf.Collections;
using KingAOP;
using Mirle.Hlts.Utils;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using static com.mirle.ibg3k0.sc.App.SCAppConstants;

namespace com.mirle.ibg3k0.sc.Service
{
    public enum AlarmLst
    {
        OHT_INTERLOCK_ERROR = 100010,
        OHT_VEHICLE_ABORT = 100011,
        OHT_BCR_READ_FAIL = 100012,
        PORT_BOXID_READ_FAIL = 100013,
        PORT_CSTID_READ_FAIL = 100014,
        OHT_IDLE_HasCMD_TimeOut = 100015,
        OHT_QueueCmdTimeOut = 100016,
        AGV_HasCmdsAccessTimeOut = 100017,
        AGVStation_DontHaveEnoughEmptyBox = 100018,
        PORT_CIM_OFF = 100019,
        PORT_DOWN = 100020,
        BOX_NumberIsNotEnough = 100021,
        OHT_IDMismatchUNKU = 100022,
        LINE_NotEmptyShelf = 100023,
        PORT_OP_WaitOutTimeOut = 100024,
        PORT_BP1_WaitOutTimeOut = 100025,
        PORT_BP2_WaitOutTimeOut = 100026,
        PORT_BP3_WaitOutTimeOut = 100027,
        PORT_BP4_WaitOutTimeOut = 100028,
        PORT_BP5_WaitOutTimeOut = 100029,
        PORT_LP_WaitOutTimeOut = 100030,
        OHT_CommandNotFinishedInTime = 100031,
        OHT_BlockingTimeOut = 100032,
        OHT_ObstaclingTimeOut = 100033,
        OHT_TransferringCmdFinishTimeOut = 100034,
        OHT_HasUnknowCstDataHappend = 100035,
        ServiceWatchDog_NatsDisConnection = 100036,
        ServiceWatchDog_RedisDisConnection = 100037,
    }

    public class VehicleService : IDynamicMetaObjectProvider
    {
        public const string DEVICE_NAME_OHx = "OHx";
        private Logger logger = LogManager.GetCurrentClassLogger();
        private TransferService transferService = null;
        private SCApplication scApp = null;

        public VehicleService()
        {
        }

        public void Start(SCApplication app)
        {
            scApp = app;
            //SubscriptionPositionChangeEvent();
            scApp.getEQObjCacheManager().getLine().HasHIDPowerAlarmHappendChange += VehicleService_HasHIDPowerAlarmHappendChange;

            List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();

            foreach (var vh in vhs)
            {
                vh.addEventHandler(nameof(VehicleService), nameof(vh.isTcpIpConnect), PublishVhInfo);
                vh.addEventHandler(nameof(VehicleService), vh.VhPositionChangeEvent, PublishVhInfo);
                vh.addEventHandler(nameof(VehicleService), vh.VhExcuteCMDStatusChangeEvent, PublishVhInfo);
                vh.addEventHandler(nameof(VehicleService), vh.VhStatusChangeEvent, PublishVhInfo);
                vh.LocationChange += Vh_LocationChange;
                vh.SegmentChange += Vh_SegementChange;
                vh.AssignCommandFailOverTimes += Vh_AssignCommandFailOverTimes;
                vh.StatusRequestFailOverTimes += Vh_StatusRequestFailOverTimes;
                vh.LongTimeNoCommuncation += Vh_LongTimeNoCommuncation;
                vh.LongTimeInaction += Vh_LongTimeInaction;
                //Blocking event
                vh.LongTimeBlocking += Vh_LongTimeBlocking;
                vh.LongTimeBlockingKeepHappening += Vh_LongTimeBlockingKeepHappening;
                vh.LongTimeBlockFinish += Vh_LongTimeBlockFinish;
                //Obstacling event
                vh.LongTimeObstacling += Vh_LongTimeObstacling;
                vh.LongTimeObstaclingKeepHappening += Vh_LongTimeObstaclingKeepHappening;
                vh.LongTimeObstacleFinish += Vh_LongTimeObstacleFinish;

                vh.ErrorStatusChange += (s1, e1) => Vh_ErrorStatusChange(s1, e1);

                vh.OHTCCommandResidualHappend += Vh_OHTCCommandResidualHappend;
                vh.TimerActionStart();
            }

            transferService = app.TransferService;
            //註冊trackAlarmHappend
            //var v = app.getEQObjCacheManager().getAllUnit().
            foreach (Track t in scApp.UnitBLL.cache.GetALLTracks())
            {
                t.alarmCodeChange += trackAlarmHappend;
            }
        }

        private void Vh_OHTCCommandResidualHappend(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                TransferServiceLogger.Info(DateTime.Now.ToString("HH:mm:ss.fff ") +
                                           $"系統判斷到OHTC殘留命令發生，進行強制清除命令...");
                bool is_success = scApp.CMDBLL.forceUpdataCmdStatus2FnishByVhID(vh.VEHICLE_ID);
                TransferServiceLogger.Info(DateTime.Now.ToString("HH:mm:ss.fff ") +
                                           $"系統判斷到OHTC殘留命令發生，進行強制清除命令，Rssult:{is_success}");
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }

        private void Vh_LongTimeObstacling(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process vehicle long time obstacling",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                Task.Run(() => scApp.VehicleBLL.web.vehicleHasCmdNoAction(vh.Num));
                scApp.TransferService.OHBC_AlarmSet(vh.VEHICLE_ID, ((int)AlarmLst.OHT_ObstaclingTimeOut).ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }
        private void Vh_LongTimeObstaclingKeepHappening(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                if (!SystemParameter.IsOpenContinueNotifyWhenVehicleTimeout)
                    return;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process vehicle long time obstacling (Keep Happening)",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                Task.Run(() => scApp.VehicleBLL.web.vehicleHasCmdNoAction(vh.Num));
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }
        private void Vh_LongTimeObstacleFinish(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process vehicle long time obstacle finish",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                scApp.TransferService.OHBC_AlarmCleared(vh.VEHICLE_ID, ((int)AlarmLst.OHT_ObstaclingTimeOut).ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }

        public void AllVhContinue()
        {
            try
            {
                List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
                foreach (var vh in vhs)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"Start Process continue vh:{vh.VEHICLE_ID} by manual",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            bool is_success = PauseRequest(vh.VEHICLE_ID, PauseEvent.Continue, OHxCPauseType.Normal);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"End Process continue vh:{vh.VEHICLE_ID} by manual,Result:{is_success}",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex, "Exception:");
                        }
                    }
                    );
                    SpinWait.SpinUntil(() => false, 200);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        private void VehicleService_HasHIDPowerAlarmHappendChange(object sender, bool e)
        {
            try
            {

                if (e)
                {
                    List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
                    foreach (var vh in vhs)
                    {
                        if (SCUtility.isEmpty(DebugParameter.TestHIDAbnormalVhID))
                        {
                            //not thing...
                        }
                        else
                        {
                            if (!SCUtility.isMatche(DebugParameter.TestHIDAbnormalVhID, vh.VEHICLE_ID))
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"目前測試HID Abmormal vh:{DebugParameter.TestHIDAbnormalVhID} ,不對vh:{vh.VEHICLE_ID} 下暫停",
                                   VehicleID: vh.VEHICLE_ID,
                                   CarrierID: vh.CST_ID);
                                continue;
                            }
                        }
                        Task.Run(() =>
                        {
                            try
                            {
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"Start Process paused vh:{vh.VEHICLE_ID} by HID Alarm happend",
                                   VehicleID: vh.VEHICLE_ID,
                                   CarrierID: vh.CST_ID);
                                bool is_success = PauseRequest(vh.VEHICLE_ID, PauseEvent.Pause, OHxCPauseType.Normal);
                                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"End Process paused vh:{vh.VEHICLE_ID} by HID Alarm happend,Result:{is_success}",
                                   VehicleID: vh.VEHICLE_ID,
                                   CarrierID: vh.CST_ID);
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "Exception:");
                            }
                        }
                        );
                        SpinWait.SpinUntil(() => false, 200);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }



        private void Vh_LongTimeBlocking(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process vehicle long time block",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                scApp.TransferService.OHBC_AlarmSet(vh.VEHICLE_ID, ((int)AlarmLst.OHT_BlockingTimeOut).ToString());
                //Task.Run(() => scApp.VehicleBLL.web.vehicleHasCmdNoAction(vh.Num));
                scApp.VehicleBLL.web.vehicleHasCmdNoAction(vh.Num);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }
        private void Vh_LongTimeBlockingKeepHappening(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                if (!SystemParameter.IsOpenContinueNotifyWhenVehicleTimeout)
                    return;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process vehicle long time block (Keep Happening)",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                //Task.Run(() => scApp.VehicleBLL.web.vehicleHasCmdNoAction(vh.Num));
                scApp.VehicleBLL.web.vehicleHasCmdNoAction(vh.Num);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }
        private void Vh_LongTimeBlockFinish(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process vehicle long time block finish",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                scApp.TransferService.OHBC_AlarmCleared(vh.VEHICLE_ID, ((int)AlarmLst.OHT_BlockingTimeOut).ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }

        private void Vh_ErrorStatusChange(object sender, VhStopSingle vhStopSingle)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                if (vhStopSingle == VhStopSingle.StopSingleOn)
                {
                    //如果OHT不為Install且為AutoLocal模式就不用呼叫異常通知
                    if (!vh.IS_INSTALLED && vh.MODE_STATUS == VHModeStatus.AutoLocal)
                        return;
                    Task.Run(() => scApp.VehicleBLL.web.errorHappendNotify(vh.Num));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }

        private void Vh_AssignCommandFailOverTimes(object sender, int failTimes)
        {
            AVEHICLE vh = (sender as AVEHICLE);
            if (vh.MODE_STATUS == VHModeStatus.AutoRemote)
            {
                scApp.TransferService.OHBC_AlarmSet(vh.VEHICLE_ID,
                    SCAppConstants.SystemAlarmCode.OHT_Issue.RejectCommandAlarm);

                VehicleAutoModeCahnge(vh.VEHICLE_ID, VHModeStatus.AutoLocal);
                string message = $"vh:{vh.VEHICLE_ID}, assign command fail times:{failTimes}, change to auto local mode";
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: message,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                BCFApplication.onWarningMsg(message);
            }
        }

        private void Vh_LongTimeNoCommuncation(object sender, EventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            //當發生很久沒有通訊的時候，就會發送143去進行狀態的詢問，確保Control還與Vehicle連線著
            bool is_success = VehicleStatusRequest(vh.VEHICLE_ID);
            //如果連續三次 都沒有得到回覆時，就將Port關閉在重新打開
            if (!is_success)
            {
                //vh.StatusRequestFailTimes++;
                vh.StatusRequestFailTimes = vh.StatusRequestFailTimes + 1;
            }
            else
            {
                vh.StatusRequestFailTimes = 0;
            }
        }

        private void Vh_LongTimeInaction(object sender, string cmdID)
        {
            AVEHICLE vh = sender as AVEHICLE;
            if (vh == null) return;
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process vehicle long time inaction, cmd id:{cmdID}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                //vh.Stop();
                //上報Alamr Rerport給MCS
                scApp.TransferService.OHBC_AlarmSet(scApp.getEQObjCacheManager().getLine().LINE_ID, ((int)AlarmLst.OHT_CommandNotFinishedInTime).ToString());
                //Task.Run(() => scApp.VehicleBLL.web.vehicleLongTimeNoAction(scApp));
                //Task.Run(() => scApp.VehicleBLL.web.vehicleHasCmdNoAction(vh.Num));

                //scApp.LineService.ProcessAlarmReport(
                //    vh.NODE_ID, vh.VEHICLE_ID, vh.Real_ID, "",
                //    SCAppConstants.SystemAlarmCode.OHT_Issue.OHTLongInaction,
                //    ProtocolFormat.OHTMessage.ErrorStatus.ErrSet);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
            }
        }

        private void Vh_StatusRequestFailOverTimes(object sender, int e)
        {
            try
            {
                AVEHICLE vh = sender as AVEHICLE;
                //vh.StatusRequestFailTimes = 0;

                //1.當Status要求失敗超過3次時，要將對應的Port關閉再開啟。
                //var endPoint = vh.getIPEndPoint(scApp.getBCFApplication());
                int port_num = vh.getPortNum(scApp.getBCFApplication());
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Over {AVEHICLE.MAX_STATUS_REQUEST_FAIL_TIMES} times request status fail, begin force close tcpip section...",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                vh.StopTcpIpConnection(scApp.getBCFApplication());

                //stopVehicleTcpIpServer(vh);
                //SpinWait.SpinUntil(() => false, 2000);
                //startVehicleTcpIpServer(vh);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex);
            }
        }

        public bool stopVehicleTcpIpServer(string vhID)
        {
            AVEHICLE vh = scApp.VehicleBLL.cache.getVhByID(vhID);
            return stopVehicleTcpIpServer(vh);
        }

        private bool stopVehicleTcpIpServer(AVEHICLE vh)
        {
            if (!vh.IsTcpIpListening(scApp.getBCFApplication()))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vh.VEHICLE_ID} of tcp/ip server already stopped!,IsTcpIpListening:{vh.IsTcpIpListening(scApp.getBCFApplication())}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }

            int port_num = vh.getPortNum(scApp.getBCFApplication());
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Stop vh:{vh.VEHICLE_ID} of tcp/ip server, port num:{port_num}",
               VehicleID: vh.VEHICLE_ID,
               CarrierID: vh.CST_ID);
            scApp.stopTcpIpServer(port_num);
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Stop vh:{vh.VEHICLE_ID} of tcp/ip server finish, IsTcpIpListening:{vh.IsTcpIpListening(scApp.getBCFApplication())}",
               VehicleID: vh.VEHICLE_ID,
               CarrierID: vh.CST_ID);
            return true;
        }

        public bool startVehicleTcpIpServer(string vhID)
        {
            AVEHICLE vh = scApp.VehicleBLL.cache.getVhByID(vhID);
            return startVehicleTcpIpServer(vh);
        }

        private bool startVehicleTcpIpServer(AVEHICLE vh)
        {
            if (vh.IsTcpIpListening(scApp.getBCFApplication()))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vh.VEHICLE_ID} of tcp/ip server already listening!,IsTcpIpListening:{vh.IsTcpIpListening(scApp.getBCFApplication())}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }

            int port_num = vh.getPortNum(scApp.getBCFApplication());
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Start vh:{vh.VEHICLE_ID} of tcp/ip server, port num:{port_num}",
               VehicleID: vh.VEHICLE_ID,
               CarrierID: vh.CST_ID);
            scApp.startTcpIpServerListen(port_num);
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Start vh:{vh.VEHICLE_ID} of tcp/ip server finish, IsTcpIpListening:{vh.IsTcpIpListening(scApp.getBCFApplication())}",
               VehicleID: vh.VEHICLE_ID,
               CarrierID: vh.CST_ID);
            return true;
        }

        private void Vh_LocationChange(object sender, LocationChangeEventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            ASECTION entry_section = scApp.SectionBLL.cache.GetSection(e.EntrySection);
            if (entry_section == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vh.VEHICLE_ID} entry section is null,don't process location change .",
                   VehicleID: vh.VEHICLE_ID);
                return;
            }
            string leave_section_id = e.LeaveSection;
            ASECTION leave_section = scApp.SectionBLL.cache.GetSection(leave_section_id);
            if (leave_section == null)
            {
                string pre_section_id = vh.PRE_SEC_ID;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vh.VEHICLE_ID} leave section is null,try get pre section id:{pre_section_id}.",
                   VehicleID: vh.VEHICLE_ID);
                leave_section = scApp.SectionBLL.cache.GetSection(pre_section_id);
                leave_section_id = SCUtility.Trim(pre_section_id, true);
            }
            leave_section?.Leave(vh.VEHICLE_ID);
            entry_section?.Entry(vh.VEHICLE_ID);

            if (leave_section != null)
            {
                if (leave_section == entry_section)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vh.VEHICLE_ID} leave section:{leave_section.SEC_ID} equals entry section,don't remove reserved.",
                       VehicleID: vh.VEHICLE_ID);
                }
                else
                {
                    scApp.ReserveBLL.RemoveManyReservedSectionsByVIDSID(vh.VEHICLE_ID, leave_section.SEC_ID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vh.VEHICLE_ID} leave section {leave_section.SEC_ID},remove reserved.",
                       VehicleID: vh.VEHICLE_ID);
                }
            }
            var entry_sec_related_blocks = scApp.BlockControlBLL.cache.loadBlockZoneMasterBySectionID(e.EntrySection);
            //var leave_sec_related_blocks = scApp.BlockControlBLL.cache.loadBlockZoneMasterBySectionID(e.LeaveSection);
            var leave_sec_related_blocks = scApp.BlockControlBLL.cache.loadBlockZoneMasterBySectionID(leave_section_id);
            var entry_blocks = entry_sec_related_blocks.Except(leave_sec_related_blocks);
            foreach (var entry_block in entry_blocks)
            {
                entry_block.Entry(vh.VEHICLE_ID);
            }
            var leave_blocks = leave_sec_related_blocks.Except(entry_sec_related_blocks);
            foreach (var leave_block in leave_blocks)
            {
                leave_block.Leave(vh.VEHICLE_ID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vh.VEHICLE_ID} leave block {leave_block.ENTRY_SEC_ID},remove reserved.",
                   VehicleID: vh.VEHICLE_ID);
            }
        }
        const int ADVANCE_DRIVE_OUT_DISTANCE_IN_GUIDE_SECTION_MM = 20000;
        const int ADVANCE_DRIVE_OUT_DISTANCE_NOT_IN_GUIDE_SECTION_MM = 2000;


        public void tryDriveOutTheVhByAdvance(AVEHICLE vh)
        {
            try
            {
                if (vh.WillPassSectionID == null || vh.WillPassSectionID.Count == 0)
                {
                    string next_find_from_adr = vh.CUR_ADR_ID;
                    double checked_distance_by_unknow_guide_section = 0;
                    if (!vh.IsObstacle)
                        return;
                    do
                    {
                        var related_sections = scApp.SectionBLL.cache.GetSectionsByFromAddress(next_find_from_adr);
                        if (related_sections == null || related_sections.Count == 0 || related_sections.Count >= 2)
                        { return; }
                        ASECTION last_check_section_by_unknow_guide_section = related_sections.FirstOrDefault();
                        var reserve_result = scApp.ReserveBLL.TryAddReservedSection
                        (vh.VEHICLE_ID, last_check_section_by_unknow_guide_section.SEC_ID, isAsk: true);
                        if (!reserve_result.OK)
                        {
                            tryDriveOutTheVh(vh.VEHICLE_ID, reserve_result.VehicleID);
                            return;
                        }
                        next_find_from_adr = last_check_section_by_unknow_guide_section.TO_ADR_ID;
                        checked_distance_by_unknow_guide_section += last_check_section_by_unknow_guide_section.SEC_DIS;
                    } while (checked_distance_by_unknow_guide_section < ADVANCE_DRIVE_OUT_DISTANCE_IN_GUIDE_SECTION_MM);

                    return;
                }
                List<string> wiil_pass_section_ids = vh.WillPassSectionID.ToList();
                string current_section = "";
                if (SCUtility.isEmpty(vh.CUR_SEC_ID))
                {
                    ASECTION from_sec = scApp.SectionBLL.cache.GetSectionsByFromAddress(vh.CUR_ADR_ID).FirstOrDefault();
                    if (from_sec == null)
                    {
                        return;
                    }
                    else
                    {
                        current_section = from_sec.SEC_ID;
                    }
                }
                else
                {
                    current_section = vh.CUR_SEC_ID;
                }
                //int sec_index = vh.WillPassSectionID.IndexOf(current_section);
                int sec_index = wiil_pass_section_ids.IndexOf(current_section);
                if (sec_index < 0) return;
                double checked_distance = 0;
                ASECTION last_check_section = null;
                //for (int start_index = sec_index; checked_distance < ADVANCE_DRIVE_OUT_DISTANCE_MM; start_index++)
                for (int start_index = sec_index; checked_distance < DebugParameter.PreDriveOutDistance_MM; start_index++)
                {
                    if (start_index < wiil_pass_section_ids.Count)
                    {
                        //string sec_id = vh.WillPassSectionID[start_index];
                        string sec_id = wiil_pass_section_ids[start_index];
                        ASECTION sec_obj = scApp.SectionBLL.cache.GetSection(sec_id);
                        var vhs_on_section = scApp.VehicleBLL.cache.getVhBySections(sec_id);
                        if (vhs_on_section.Count > 0)
                        {
                            foreach (var v in vhs_on_section)
                            {
                                if (!SCUtility.isMatche(v.VEHICLE_ID, vh.VEHICLE_ID))
                                {
                                    tryDriveOutTheVh(vh.VEHICLE_ID, v.VEHICLE_ID);
                                }
                            }
                        }
                        var vhs_on_addresses = scApp.VehicleBLL.cache.getVhByAddressIDs(sec_obj.getNodeAdrs());
                        if (vhs_on_addresses.Count > 0)
                        {
                            foreach (var v in vhs_on_addresses)
                            {
                                if (!SCUtility.isMatche(v.VEHICLE_ID, vh.VEHICLE_ID))
                                {
                                    tryDriveOutTheVh(vh.VEHICLE_ID, v.VEHICLE_ID);
                                }
                            }
                        }
                        //var reserve_result = scApp.ReserveBLL.TryAddReservedSection
                        //                    (vh.VEHICLE_ID, sec_id, isAsk: true);
                        //if (!reserve_result.OK)
                        //{
                        //    tryDriveOutTheVh(vh.VEHICLE_ID, reserve_result.VehicleID);
                        //    return;
                        //}
                        last_check_section = sec_obj;
                    }
                    else
                    {
                        if (!vh.IsObstacle)
                            return;
                        if (last_check_section != null)
                        {
                            List<ASECTION> sections = scApp.SectionBLL.cache.GetSectionsByFromAddress(last_check_section.TO_ADR_ID);
                            if (sections == null || sections.Count == 0)
                                return;
                            last_check_section = sections.FirstOrDefault();
                            var reserve_result = scApp.ReserveBLL.TryAddReservedSection
                                                (vh.VEHICLE_ID, last_check_section.SEC_ID, isAsk: true);
                            if (!reserve_result.OK)
                            {
                                tryDriveOutTheVh(vh.VEHICLE_ID, reserve_result.VehicleID);
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                        if (checked_distance > ADVANCE_DRIVE_OUT_DISTANCE_NOT_IN_GUIDE_SECTION_MM)
                            return;
                    }
                    checked_distance += last_check_section.SEC_DIS;
                    if (checked_distance > ADVANCE_DRIVE_OUT_DISTANCE_IN_GUIDE_SECTION_MM)
                        return;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        private void Vh_SegementChange(object sender, SegmentChangeEventArgs e)
        {
            AVEHICLE vh = sender as AVEHICLE;
            ASEGMENT leave_segment = scApp.SegmentBLL.cache.GetSegment(e.LeaveSegment);
            ASEGMENT entry_segment = scApp.SegmentBLL.cache.GetSegment(e.EntrySegment);
            leave_segment?.Leave(vh);
            entry_segment?.Entry(vh, scApp.SectionBLL, leave_segment == null);
        }

        private void PublishVhInfo(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                string vh_id = e.PropertyValue as string;
                AVEHICLE vh = scApp.VehicleBLL.getVehicleByID(vh_id);
                if (sender == null) return;
                byte[] vh_Serialize = BLL.VehicleBLL.Convert2GPB_VehicleInfo(vh);
                RecoderVehicleObjInfoLog(vh_id, vh_Serialize);

                scApp.getNatsManager().PublishAsync
                    (string.Format(SCAppConstants.NATS_SUBJECT_VH_INFO_0, vh.VEHICLE_ID.Trim()), vh_Serialize);

                scApp.getRedisCacheManager().ListSetByIndexAsync
                    (SCAppConstants.REDIS_LIST_KEY_VEHICLES, vh.VEHICLE_ID, vh.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
            //});
        }

        private static void RecoderVehicleObjInfoLog(string vh_id, byte[] arrayByte)
        {
            string compressStr = SCUtility.CompressArrayByte(arrayByte);
            dynamic logEntry = new JObject();
            logEntry.RPT_TIME = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
            logEntry.OBJECT_ID = vh_id;
            logEntry.RAWDATA = compressStr;
            logEntry.Index = "ObjectHistoricalInfo";
            var json = logEntry.ToString(Newtonsoft.Json.Formatting.None);
            json = json.Replace("RPT_TIME", "@timestamp");
            LogManager.GetLogger("ObjectHistoricalInfo").Info(json);
        }

        public static string CompressArrayByte(byte[] arrayByte)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
            compressedzipStream.Write(arrayByte, 0, arrayByte.Length);
            compressedzipStream.Close();
            string compressStr = (string)(Convert.ToBase64String(ms.ToArray()));
            return compressStr;
        }

        public void SubscriptionPositionChangeEvent()
        {
            scApp.getRedisCacheManager().SubscriptionEvent($"{SCAppConstants.REDIS_KEY_WORD_POSITION_REPORT}#*", scApp.VehicleBLL.VehiclePositionChangeHandler);
        }

        public void UnsubscribePositionChangeEvent()
        {
            //scApp.getRedisCacheManager().UnsubscribeEvent($"{SCAppConstants.REDIS_KEY_WORD_POSITION_REPORT}_*", scApp.VehicleBLL.VehiclePositionChangeHandler);
            scApp.getRedisCacheManager().UnsubscribeEvent($"{SCAppConstants.REDIS_KEY_WORD_POSITION_REPORT}#*", scApp.VehicleBLL.VehiclePositionChangeHandler);
        }

        #region Send Message To Vehicle

        #region Tcp/Ip

        public bool HostBasicVersionReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            DateTime crtTime = DateTime.Now;
            ID_101_HOST_BASIC_INFO_VERSION_RESPONSE receive_gpp = null;
            ID_1_HOST_BASIC_INFO_VERSION_REP sned_gpp = new ID_1_HOST_BASIC_INFO_VERSION_REP()
            {
                DataDateTimeYear = "2018",
                DataDateTimeMonth = "10",
                DataDateTimeDay = "25",
                DataDateTimeHour = "15",
                DataDateTimeMinute = "22",
                DataDateTimeSecond = "50",
                CurrentTimeYear = crtTime.Year.ToString(),
                CurrentTimeMonth = crtTime.Month.ToString(),
                CurrentTimeDay = crtTime.Day.ToString(),
                CurrentTimeHour = crtTime.Hour.ToString(),
                CurrentTimeMinute = crtTime.Minute.ToString(),
                CurrentTimeSecond = crtTime.Second.ToString()
            };
            isSuccess = vh.send_Str1(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }

        public bool BasicInfoReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            DateTime crtTime = DateTime.Now;
            ID_111_BASIC_INFO_RESPONSE receive_gpp = null;
            int travel_base_data_count = 1;
            int section_data_count = 0;
            int address_data_coune = 0;
            int scale_base_data_count = 1;
            int control_data_count = 1;
            int guide_base_data_count = 1;
            section_data_count = scApp.DataSyncBLL.getCount_ReleaseVSections();
            address_data_coune = scApp.MapBLL.getCount_AddressCount();
            ID_11_BASIC_INFO_REP sned_gpp = new ID_11_BASIC_INFO_REP()
            {
                TravelBasicDataCount = travel_base_data_count,
                SectionDataCount = section_data_count,
                AddressDataCount = address_data_coune,
                ScaleDataCount = scale_base_data_count,
                ContrlDataCount = control_data_count,
                GuideDataCount = guide_base_data_count
            };
            isSuccess = vh.sned_S11(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }

        public bool TavellingDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            DateTime crtTime = DateTime.Now;
            AVEHICLE_CONTROL_100 data = scApp.DataSyncBLL.getReleaseVehicleControlData_100(vh_id);

            ID_113_TAVELLING_DATA_RESPONSE receive_gpp = null;
            ID_13_TAVELLING_DATA_REP sned_gpp = new ID_13_TAVELLING_DATA_REP()
            {
                Resolution = (UInt32)data.TRAVEL_RESOLUTION,
                StartStopSpd = (UInt32)data.TRAVEL_START_STOP_SPEED,
                MaxSpeed = (UInt32)data.TRAVEL_MAX_SPD,
                AccelTime = (UInt32)data.TRAVEL_ACCEL_DECCEL_TIME,
                SCurveRate = (UInt16)data.TRAVEL_S_CURVE_RATE,
                OriginDir = (UInt16)data.TRAVEL_HOME_DIR,
                OriginSpd = (UInt32)data.TRAVEL_HOME_SPD,
                BeaemSpd = (UInt32)data.TRAVEL_KEEP_DIS_SPD,
                ManualHSpd = (UInt32)data.TRAVEL_MANUAL_HIGH_SPD,
                ManualLSpd = (UInt32)data.TRAVEL_MANUAL_LOW_SPD,
                TeachingSpd = (UInt32)data.TRAVEL_TEACHING_SPD,
                RotateDir = (UInt16)data.TRAVEL_TRAVEL_DIR,
                EncoderPole = (UInt16)data.TRAVEL_ENCODER_POLARITY,
                PositionCompensation = (UInt16)data.TRAVEL_F_DIR_LIMIT, //TODO 要填入正確的資料
                //FLimit = (UInt16)data.TRAVEL_F_DIR_LIMIT, //TODO 要填入正確的資料
                //RLimit = (UInt16)data.TRAVEL_R_DIR_LIMIT,
                KeepDistFar = (UInt32)data.TRAVEL_OBS_DETECT_LONG,
                KeepDistNear = (UInt32)data.TRAVEL_OBS_DETECT_SHORT,
            };
            isSuccess = vh.sned_S13(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }

        public bool SectionDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            DateTime crtTime = DateTime.Now;
            List<VSECTION_100> vSecs = scApp.DataSyncBLL.loadReleaseVSections();

            ID_15_SECTION_DATA_REP send_gpp = new ID_15_SECTION_DATA_REP();
            ID_115_SECTION_DATA_RESPONSE receive_gpp = null;
            foreach (VSECTION_100 vSec in vSecs)
            {
                var secInfo = new ID_15_SECTION_DATA_REP.Types.Section()
                {
                    DriveDir = (UInt16)vSec.DIRC_DRIV,
                    GuideDir = (UInt16)vSec.DIRC_GUID,
                    AeraSecsor = (UInt16)(UInt16)(vSec.AREA_SECSOR ?? 0),
                    SectionID = vSec.SEC_ID,
                    FromAddr = vSec.FROM_ADR_ID,
                    ToAddr = vSec.TO_ADR_ID,
                    ControlTable = convertvSec2ControlTable(vSec),
                    Speed = (UInt32)vSec.SEC_SPD,
                    Distance = (UInt32)vSec.SEC_DIS,
                    ChangeAreaSensor1 = (UInt16)vSec.CHG_AREA_SECSOR_1,
                    ChangeGuideDir1 = (UInt16)vSec.CDOG_1,
                    ChangeSegNum1 = vSec.CHG_SEG_NUM_1,

                    ChangeAreaSensor2 = (UInt16)vSec.CHG_AREA_SECSOR_2,
                    ChangeGuideDir2 = (UInt16)vSec.CDOG_2,
                    ChangeSegNum2 = vSec.CHG_SEG_NUM_2,
                    AtSegment = vSec.SEG_NUM
                };
                send_gpp.Sections.Add(secInfo);
            }
            isSuccess = vh.sned_S15(send_gpp, out receive_gpp);
            // isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }

        private UInt16 convertvSec2ControlTable(VSECTION_100 vSec)
        {
            System.Collections.BitArray bitArray = new System.Collections.BitArray(16);
            bitArray[0] = SCUtility.int2Bool(vSec.PRE_BLO_REQ);
            bitArray[1] = vSec.BRANCH_FLAG;
            bitArray[2] = vSec.HID_CONTROL;
            bitArray[3] = false;
            bitArray[4] = vSec.CAN_GUIDE_CHG;
            bitArray[6] = false;
            bitArray[7] = false;
            bitArray[8] = vSec.IS_ADR_RPT;
            bitArray[9] = false;
            bitArray[10] = false;
            bitArray[11] = false;
            bitArray[12] = SCUtility.int2Bool(vSec.RANGE_SENSOR_F);
            bitArray[13] = SCUtility.int2Bool(vSec.OBS_SENSOR_F);
            bitArray[14] = SCUtility.int2Bool(vSec.OBS_SENSOR_R);
            bitArray[15] = SCUtility.int2Bool(vSec.OBS_SENSOR_L);
            return SCUtility.getUInt16FromBitArray(bitArray);
        }

        public bool AddressDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            //List<AADDRESS_DATA> adrs = scApp.DataSyncBLL.loadReleaseADDRESS_DATAs(vh_id);
            List<AADDRESS_DATA> adrs = scApp.DataSyncBLL.loadReleaseADDRESS_DATAs(sc.BLL.DataSyncBLL.COMMON_ADDRESS_DATA_INDEX);
            List<string> hid_leave_adr = scApp.HIDBLL.loadAllHIDLeaveAdr();
            string rtnMsg = string.Empty;
            ID_17_ADDRESS_DATA_REP send_gpp = new ID_17_ADDRESS_DATA_REP();
            ID_117_ADDRESS_DATA_RESPONSE receive_gpp = null;
            foreach (AADDRESS_DATA adr in adrs)
            {
                var block_master = scApp.MapBLL.loadBZMByAdrID(adr.ADR_ID.Trim());
                var adrInfo = new ID_17_ADDRESS_DATA_REP.Types.Address()
                {
                    Addr = adr.ADR_ID,
                    Resolution = adr.RESOLUTION,
                    Loaction = adr.LOACTION,
                    BlockRelease = (block_master != null && block_master.Count > 0) ? 1 : 0,
                    HIDRelease = hid_leave_adr.Contains(adr.ADR_ID.Trim()) ? 1 : 0
                };
                send_gpp.Addresss.Add(adrInfo);
            }
            isSuccess = vh.sned_S17(send_gpp, out receive_gpp);
            // isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }

        public bool ScaleDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            SCALE_BASE_DATA data = scApp.DataSyncBLL.getReleaseSCALE_BASE_DATA();

            ID_119_SCALE_DATA_RESPONSE receive_gpp = null;
            ID_19_SCALE_DATA_REP sned_gpp = new ID_19_SCALE_DATA_REP()
            {
                Resolution = (UInt32)data.RESOLUTION,
                InposArea = (UInt32)data.INPOSITION_AREA,
                InposStability = (UInt32)data.INPOSITION_STABLE_TIME,
                ScalePulse = (UInt32)data.TOTAL_SCALE_PULSE,
                ScaleOffset = (UInt32)data.SCALE_OFFSET,
                ScaleReset = (UInt32)data.SCALE_RESE_DIST,
                ReadDir = (UInt16)data.READ_DIR
            };
            isSuccess = vh.sned_S19(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }

        public bool ControlDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);

            CONTROL_DATA data = scApp.DataSyncBLL.getReleaseCONTROL_DATA();
            string rtnMsg = string.Empty;
            ID_121_CONTROL_DATA_RESPONSE receive_gpp;
            ID_21_CONTROL_DATA_REP sned_gpp = new ID_21_CONTROL_DATA_REP()
            {
                TimeoutT1 = (UInt32)data.T1,
                TimeoutT2 = (UInt32)data.T2,
                TimeoutT3 = (UInt32)data.T3,
                TimeoutT4 = (UInt32)data.T4,
                TimeoutT5 = (UInt32)data.T5,
                TimeoutT6 = (UInt32)data.T6,
                TimeoutT7 = (UInt32)data.T7,
                TimeoutT8 = (UInt32)data.T8,
                TimeoutBlock = (UInt32)data.BLOCK_REQ_TIME_OUT
            };
            isSuccess = vh.sned_S21(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }

        public bool GuideDataReport(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            AVEHICLE_CONTROL_100 data = scApp.DataSyncBLL.getReleaseVehicleControlData_100(vh_id);
            ID_123_GUIDE_DATA_RESPONSE receive_gpp;
            ID_23_GUIDE_DATA_REP sned_gpp = new ID_23_GUIDE_DATA_REP()
            {
                StartStopSpd = (UInt32)data.GUIDE_START_STOP_SPEED,
                MaxSpeed = (UInt32)data.GUIDE_MAX_SPD,
                AccelTime = (UInt32)data.GUIDE_ACCEL_DECCEL_TIME,
                SCurveRate = (UInt16)data.GUIDE_S_CURVE_RATE,
                NormalSpd = (UInt32)data.GUIDE_RUN_SPD,
                ManualHSpd = (UInt32)data.GUIDE_MANUAL_HIGH_SPD,
                ManualLSpd = (UInt32)data.GUIDE_MANUAL_LOW_SPD,
                LFLockPos = (UInt32)data.GUIDE_LF_LOCK_POSITION,
                LBLockPos = (UInt32)data.GUIDE_LB_LOCK_POSITION,
                RFLockPos = (UInt32)data.GUIDE_RF_LOCK_POSITION,
                RBLockPos = (UInt32)data.GUIDE_RB_LOCK_POSITION,
                ChangeStabilityTime = (UInt32)data.GUIDE_CHG_STABLE_TIME,
            };
            isSuccess = vh.sned_S23(sned_gpp, out receive_gpp);
            isSuccess = isSuccess && receive_gpp.ReplyCode == 0;
            return isSuccess;
        }

        public bool doDataSysc(string vh_id)
        {
            bool isSyscCmp = false;
            DateTime ohtDataVersion = new DateTime(2017, 03, 27, 10, 30, 00);
            if (BasicInfoReport(vh_id) &&
                TavellingDataReport(vh_id) &&
                SectionDataReport(vh_id) &&
                AddressDataReport(vh_id) &&
                ScaleDataReport(vh_id) &&
                ControlDataReport(vh_id) &&
                GuideDataReport(vh_id))
            {
                isSyscCmp = true;
            }
            return isSyscCmp;
        }

        //public bool CSTIDRenameRequest(string vh_id, string new_cst_id)
        //{
        //}

        public bool IndividualUploadRequest(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_161_INDIVIDUAL_UPLOAD_RESPONSE receive_gpp;
            ID_61_INDIVIDUAL_UPLOAD_REQ sned_gpp = new ID_61_INDIVIDUAL_UPLOAD_REQ()
            {
            };
            isSuccess = vh.sned_S61(sned_gpp, out receive_gpp);
            //TODO Set info 2 DB
            if (isSuccess)
            {
            }
            return isSuccess;
        }

        public bool IndividualChangeRequest(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_163_INDIVIDUAL_CHANGE_RESPONSE receive_gpp;
            ID_63_INDIVIDUAL_CHANGE_REQ sned_gpp = new ID_63_INDIVIDUAL_CHANGE_REQ()
            {
                OffsetGuideFL = 1,
                OffsetGuideRL = 2,
                OffsetGuideFR = 3,
                OffsetGuideRR = 4
            };
            isSuccess = vh.sned_S63(sned_gpp, out receive_gpp);
            return isSuccess;
        }

        /// <summary>
        /// 與Vehicle進行資料同步。(通常使用剛與Vehicle連線時)
        /// </summary>
        /// <param name="vh_id"></param>
        public void VehicleInfoSynchronize(string vh_id)
        {
            try
            {
                /*與Vehicle進行狀態同步*/
                bool ask_status_success = VehicleStatusRequest(vh_id, true);
                /*要求Vehicle進行Alarm的Reset，如果成功後會將OHxC上針對該Vh的Alarm清除*/
                if (AlarmResetRequest(vh_id))
                {
                }
                AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
                //if (vh.MODE_STATUS == VHModeStatus.Manual &&
                //    !SCUtility.isEmpty(vh.CUR_ADR_ID) &&
                //    !SCUtility.isMatche(vh.CUR_ADR_ID, MTLService.MTL_ADDRESS))
                var check_is_in_maintain_device = scApp.EquipmentBLL.cache.IsInMaintainDevice(vh.CUR_ADR_ID);
                if (vh.MODE_STATUS == VHModeStatus.Manual &&
                    !check_is_in_maintain_device.isIn)
                {
                    ModeChangeRequest(vh_id, OperatingVHMode.OperatingAuto);
                    if (SpinWait.SpinUntil(() => vh.MODE_STATUS == VHModeStatus.AutoRemote, 5000))
                    {
                        ASEGMENT vh_current_seg_obj = scApp.SegmentBLL.cache.GetSegment(vh.CUR_SEG_ID);
                        vh_current_seg_obj?.Entry(vh, scApp.SectionBLL, true);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        public bool VehicleStatusRequest(string vh_id, bool isSync = false)
        {
            bool isSuccess = false;
            try
            {
                string reason = string.Empty;
                AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
                ID_143_STATUS_RESPONSE receive_gpp;
                ID_43_STATUS_REQUEST send_gpp = new ID_43_STATUS_REQUEST()
                {
                    SystemTime = DateTime.Now.ToString(SCAppConstants.TimestampFormat_16)
                };
                LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, send_gpp, 0);
                isSuccess = vh.send_S43(send_gpp, out receive_gpp);
                if (isSuccess)
                    LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, receive_gpp, 0);
                if (isSync && isSuccess)
                {
                    string current_adr_id = receive_gpp.CurrentAdrID;
                    VHModeStatus modeStat = DecideVhModeStatus(vh.VEHICLE_ID, current_adr_id, receive_gpp.ModeStatus);
                    VHActionStatus actionStat = receive_gpp.ActionStatus;
                    VhPowerStatus powerStat = receive_gpp.PowerStatus;
                    string cstID = receive_gpp.CSTID;
                    VhStopSingle obstacleStat = receive_gpp.ObstacleStatus;
                    VhStopSingle blockingStat = receive_gpp.BlockingStatus;
                    VhStopSingle pauseStat = receive_gpp.PauseStatus;
                    VhStopSingle hidStat = receive_gpp.HIDStatus;
                    VhStopSingle errorStat = receive_gpp.ErrorStatus;
                    VhLoadCarrierStatus loadCSTStatus = receive_gpp.HasCst;
                    VhLoadCarrierStatus loadBOXStatus = receive_gpp.HasBox;
                    if (loadBOXStatus == VhLoadCarrierStatus.Exist) //B0.05
                    {
                        vh.BOX_ID = receive_gpp.CarBoxID;
                    }
                    //VhGuideStatus leftGuideStat = recive_str.LeftGuideLockStatus;
                    //VhGuideStatus rightGuideStat = recive_str.RightGuideLockStatus;
                    checkObstacleState(vh, obstacleStat);


                    if (errorStat != vh.ERROR)
                    {
                        vh.onErrorStatusChange(errorStat);
                    }

                    int obstacleDIST = receive_gpp.ObstDistance;
                    string obstacleVhID = receive_gpp.ObstVehicleID;

                    scApp.VehicleBLL.setAndPublishPositionReportInfo2Redis(vh.VEHICLE_ID, receive_gpp);
                    scApp.VehicleBLL.getAndProcPositionReportFromRedis(vh.VEHICLE_ID);
                    // 0317 Jason 此部分之loadBOXStatus 原為loadCSTStatus ，現在之狀況為暫時解法
                    if (!scApp.VehicleBLL.doUpdateVehicleStatus(vh, cstID,
                                           modeStat, actionStat,
                                           blockingStat, pauseStat, obstacleStat, hidStat, errorStat, loadBOXStatus))
                    {
                        isSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
            return isSuccess;
        }

        public bool ModeChangeRequest(string vh_id, OperatingVHMode mode)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_141_MODE_CHANGE_RESPONSE receive_gpp;
            ID_41_MODE_CHANGE_REQ sned_gpp = new ID_41_MODE_CHANGE_REQ()
            {
                OperatingVHMode = mode
            };
            SCUtility.RecodeReportInfo(vh_id, 0, sned_gpp);
            isSuccess = vh.sned_S41(sned_gpp, out receive_gpp);
            SCUtility.RecodeReportInfo(vh_id, 0, receive_gpp, isSuccess.ToString());
            return isSuccess;
        }

        public bool PowerOperatorRequest(string vh_id, OperatingPowerMode mode)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_145_POWER_OPE_RESPONSE receive_gpp;
            ID_45_POWER_OPE_REQ sned_gpp = new ID_45_POWER_OPE_REQ()
            {
                OperatingPowerMode = mode
            };
            isSuccess = vh.sned_S45(sned_gpp, out receive_gpp);
            return isSuccess;
        }

        public bool AlarmResetRequest(string vh_id)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_191_ALARM_RESET_RESPONSE receive_gpp;
            //scApp.TransferService.OHT_AlarmAllCleared(vh.VEHICLE_ID);
            ID_91_ALARM_RESET_REQUEST sned_gpp = new ID_91_ALARM_RESET_REQUEST()
            {
            };
            isSuccess = vh.sned_S91(sned_gpp, out receive_gpp);
            if (isSuccess)
            {
                isSuccess = receive_gpp?.ReplyCode == 0;
            }
            return isSuccess;
        }

        public bool PauseRequest(string vh_id, PauseEvent pause_event, OHxCPauseType ohxc_pause_type)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            PauseType pauseType = convert2PauseType(ohxc_pause_type);
            ID_139_PAUSE_RESPONSE receive_gpp;
            ID_39_PAUSE_REQUEST send_gpp = new ID_39_PAUSE_REQUEST()
            {
                PauseType = pauseType,
                EventType = pause_event
            };
            LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, send_gpp, 0);
            isSuccess = vh.sned_Str39(send_gpp, out receive_gpp);
            LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, receive_gpp, 0);
            return isSuccess;
        }

        public bool OHxCPauseRequest(string vh_id, PauseEvent pause_event, OHxCPauseType ohxc_pause_type)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    switch (ohxc_pause_type)
                    {
                        case OHxCPauseType.Earthquake:
                            scApp.VehicleBLL.updateVehiclePauseStatus
                                (vh_id, earthquake_pause: pause_event == PauseEvent.Pause);
                            break;

                        case OHxCPauseType.Obstacle:
                            scApp.VehicleBLL.updateVehiclePauseStatus
                                (vh_id, obstruct_pause: pause_event == PauseEvent.Pause);
                            break;

                        case OHxCPauseType.Safty:
                            scApp.VehicleBLL.updateVehiclePauseStatus
                                (vh_id, safyte_pause: pause_event == PauseEvent.Pause);
                            break;
                    }
                    PauseType pauseType = convert2PauseType(ohxc_pause_type);
                    ID_139_PAUSE_RESPONSE receive_gpp;
                    ID_39_PAUSE_REQUEST send_gpp = new ID_39_PAUSE_REQUEST()
                    {
                        PauseType = pauseType,
                        EventType = pause_event
                    };
                    SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, send_gpp);
                    isSuccess = vh.sned_Str39(send_gpp, out receive_gpp);
                    SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, receive_gpp, isSuccess.ToString());

                    if (isSuccess)
                    {
                        tx.Complete();
                        vh.NotifyVhStatusChange();
                    }
                }
            }
            return isSuccess;
        }

        private PauseType convert2PauseType(OHxCPauseType ohxc_pauseType)
        {
            switch (ohxc_pauseType)
            {
                case OHxCPauseType.Normal:
                case OHxCPauseType.Obstacle:
                    return PauseType.OhxC;

                case OHxCPauseType.Block:
                    return PauseType.Block;

                case OHxCPauseType.Hid:
                    return PauseType.Hid;

                case OHxCPauseType.Earthquake:
                    return PauseType.EarthQuake;
                //case OHxCPauseType.Obstruct:
                //    return PauseType.;
                case OHxCPauseType.Safty:
                    return PauseType.Safety;

                case OHxCPauseType.ManualBlock:
                    return PauseType.ManualBlock;

                case OHxCPauseType.ManualHID:
                    return PauseType.ManualHid;

                case OHxCPauseType.ALL:
                    return PauseType.All;

                default:
                    throw new AggregateException($"enum arg not exist!value: {ohxc_pauseType}");
            }
        }

        public bool doSendOHxCCmdToVh(AVEHICLE assignVH, ACMD_OHTC cmd)
        {
            ActiveType activeType = default(ActiveType);
            string[] routeSections = null;
            string[] cycleRunSections = null;
            string[] minRouteSec_Vh2From = null;
            string[] minRouteSec_From2To = null;
            string[] minRouteAdr_Vh2From = null;
            string[] minRouteAdr_From2To = null;
            bool isSuccess = false;

            //嘗試規劃該筆ACMD_OHTC的搬送路徑
            if (scApp.CMDBLL.tryGenerateCmd_OHTC_Details(cmd, out activeType, out routeSections, out cycleRunSections
                                                                         , out minRouteSec_Vh2From, out minRouteSec_From2To
                                                                         , out minRouteAdr_Vh2From, out minRouteAdr_From2To))
            {
                if (activeType == ActiveType.Scan || activeType == ActiveType.Load || activeType == ActiveType.Loadunload)
                {
                    // B0.04 補上原地取貨狀態之說明
                    // B0.04 若取貨之section address 為空 (原地取貨) 則在該guide section 與 guide address 去補上該車目前之位置資訊(因為目前新架構OHT版本需要至少一段section 去判定
                    //if (minRouteSec_Vh2From == null || minRouteAdr_Vh2From == null)
                    //{
                    //    if (assignVH.CUR_SEC_ID != null && assignVH.CUR_ADR_ID != null)
                    //    {
                    //        string start_sec_id = assignVH.CUR_SEC_ID;
                    //        if (assignVH.IsOnAdr)
                    //        {
                    //            start_sec_id = assignVH.getVIEW_SEC_ID(scApp.SectionBLL);
                    //        }

                    //        //minRouteSec_Vh2From = new string[] { assignVH.CUR_SEC_ID };
                    //        minRouteSec_Vh2From = new string[] { start_sec_id };
                    //        minRouteAdr_Vh2From = new string[] { assignVH.CUR_ADR_ID };
                    //    }
                    //    else
                    //    {
                    //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: string.Empty,
                    //           Data: $"can't generate command road data, something is null,id:{SCUtility.Trim(cmd.CMD_ID)},vh id:{SCUtility.Trim(cmd.VH_ID)} current status not allowed." +
                    //           $"assignVH.CUR_ADR_ID:{assignVH.CUR_ADR_ID }, assignVH.CUR_SEC_ID:{assignVH.CUR_SEC_ID } , current assign ohtc cmd id:{assignVH.OHTC_CMD}." +
                    //           $"assignVH.ACT_STATUS:{assignVH.ACT_STATUS}.");
                    //        return isSuccess;
                    //    }
                    //}
                    // B0.04 補上 LoadUnload 原地放貨狀態之說明 與修改
                    // B0.04 若放貨之section address 為空 (原地放貨) 則在該guide section 與 guide address 去補上該車需要之資訊
                    if (activeType == ActiveType.Loadunload)
                    {
                        //if (minRouteSec_From2To == null || minRouteAdr_From2To == null)
                        //{
                        //    // B0.04 對該string array 補上要去 load 路徑資訊的最後一段address與 section 資料
                        //    minRouteSec_From2To = new string[] { minRouteSec_Vh2From[minRouteSec_Vh2From.Length - 1] };
                        //    minRouteAdr_From2To = new string[] { minRouteAdr_Vh2From[minRouteAdr_Vh2From.Length - 1] };
                        //}
                    }
                }
                // B0.04 補上 Unload 原地放貨狀態之說明 與修改
                // B0.04 若放貨之section address 為空 (原地放貨) 則在該guide section 與 guide address 去補上該車需要之資訊
                if (activeType == ActiveType.Unload) //B0.04 若為單獨放貨命令，在該空值處補上該車當下之位置資訊。
                {
                    //if (minRouteSec_From2To == null || minRouteAdr_From2To == null)
                    //{
                    //    if (assignVH.CUR_SEC_ID != null && assignVH.CUR_ADR_ID != null)
                    //    {
                    //        string start_sec_id = assignVH.CUR_SEC_ID;
                    //        if (assignVH.IsOnAdr)
                    //        {
                    //            start_sec_id = assignVH.getVIEW_SEC_ID(scApp.SectionBLL);
                    //        }
                    //        //minRouteSec_From2To = new string[] { assignVH.CUR_SEC_ID };
                    //        minRouteSec_From2To = new string[] { start_sec_id };
                    //        minRouteAdr_From2To = new string[] { assignVH.CUR_ADR_ID };
                    //    }
                    //    else
                    //    {
                    //        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: string.Empty,
                    //           Data: $"can't generate command road data, something is null,id:{SCUtility.Trim(cmd.CMD_ID)},vh id:{SCUtility.Trim(cmd.VH_ID)} current status not allowed." +
                    //           $"assignVH.CUR_ADR_ID:{assignVH.CUR_ADR_ID }, assignVH.CUR_SEC_ID:{assignVH.CUR_SEC_ID } , current assign ohtc cmd id:{assignVH.OHTC_CMD}." +
                    //           $"assignVH.ACT_STATUS:{assignVH.ACT_STATUS}.");
                    //        return isSuccess;
                    //    }
                    //}
                }

                //產生成功，則將該命令下達給車子，並更新車子執行命令的狀態
                isSuccess = sendTransferCommandToVh(cmd, assignVH, activeType, minRouteSec_Vh2From, minRouteSec_From2To, minRouteAdr_Vh2From, minRouteAdr_From2To);
                if (isSuccess)
                {
                    assignVH.VehicleAssign();
                    scApp.SysExcuteQualityBLL.updateSysExecQity_PassSecInfo(cmd.CMD_ID_MCS, assignVH.VEHICLE_ID, assignVH.CUR_SEC_ID,
                                            minRouteSec_Vh2From, minRouteSec_From2To);
                    scApp.CMDBLL.setVhExcuteCmdToShow(cmd, assignVH, routeSections,
                                                      minRouteSec_Vh2From?.ToList(), minRouteSec_From2To?.ToList(),
                                                      cycleRunSections);
                    assignVH.sw_speed.Restart();

                    if (!SCUtility.isEmpty(cmd.CMD_ID_MCS))
                    {
                        isSuccess &= scApp.CMDBLL.updateCMD_MCS_TranStatus2Initial(cmd.CMD_ID_MCS);
                    }
                }
                else
                {
                    //如果失敗了，則要將該筆命令更新回Queue，並且AbnormalEnd該筆命令
                    //if (!SCUtility.isEmpty(cmd.CMD_ID_MCS))
                    if (cmd.IsTransferCmdByMCS)
                    {
                        //scApp.CMDBLL.updateCMD_MCS_TranStatus2Queue(cmd.CMD_ID_MCS);
                        ACMD_MCS cmd_mcs = scApp.CMDBLL.getCMD_MCSByID(cmd.CMD_ID_MCS);
                        CassetteData cmdOHT_CSTdata = scApp.CassetteDataBLL.loadCassetteDataByBoxID(cmd.BOX_ID);
                        scApp.TransferService.ForceFinishMCSCmd(cmd_mcs, cmdOHT_CSTdata, "doSendOHxCCmdToVh");
                    }
                    //scApp.CMDBLL.updateCommand_OHTC_StatusByCmdID(cmd.CMD_ID, E_CMD_STATUS.AbnormalEndByOHT);
                    scApp.CMDBLL.updateOHTCCommandToFinishByCmdID(cmd.CMD_ID, E_CMD_STATUS.AbnormalEndByOHT, CompleteStatus.CmpStatusCommandInitailFail);
                }
            }
            else
            {
                if (cmd.IsTransferCmdByMCS)
                {
                    TransferServiceLogger.Info
                    (
                        DateTime.Now.ToString("HH:mm:ss.fff ") +
                        $"發送命令失敗，cmd id:{cmd.CMD_ID} mcs cmd id:{cmd.CMD_ID_MCS} load port:{cmd.SOURCE}, unload port:{cmd.DESTINATION} 強制將命令改回Queue"
                    );
                    scApp.CMDBLL.updateCMD_MCS_CRANE(cmd.CMD_ID_MCS, "");
                    scApp.CMDBLL.updateCMD_MCS_TranStatus(cmd.CMD_ID_MCS, E_TRAN_STATUS.Queue);
                }
                //scApp.CMDBLL.updateCommand_OHTC_StatusByCmdID(cmd.CMD_ID, E_CMD_STATUS.AbnormalEndByOHTC);
                scApp.CMDBLL.updateOHTCCommandToFinishByCmdID(cmd.CMD_ID, E_CMD_STATUS.AbnormalEndByOHTC, CompleteStatus.CmpStatusCommandInitailFail);
            }
            return isSuccess;
        }

        public bool doSendOHxCOverrideCmdToVh(AVEHICLE assignVH, ACMD_OHTC cmd, bool isNeedPauseFirst)
        {
            ActiveType activeType = default(ActiveType);
            string[] routeSections = null;
            string[] cycleRunSections = null;
            string[] minRouteSeg_Vh2From = null;
            string[] minRouteSeg_From2To = null;
            bool isSuccess = false;

            throw new NotImplementedException();
            //如果失敗會將命令改成abonormal End
            //if (scApp.CMDBLL.tryGenerateCmd_OHTC_Details(cmd, out activeType, out routeSections, out cycleRunSections
            //                                                             , out minRouteSeg_Vh2From, out minRouteSeg_From2To))
            //{
            //    isSuccess = sendTransferCommandToVh(cmd, assignVH, ActiveType.Override, routeSections, cycleRunSections);

            //    if (isSuccess)
            //    {
            //        scApp.CMDBLL.setVhExcuteCmdToShow(cmd, assignVH, routeSections, cycleRunSections);
            //        if (isNeedPauseFirst)
            //            PauseRequest(assignVH.VEHICLE_ID, PauseEvent.Continue, OHxCPauseType.Normal);
            //        assignVH.sw_speed.Restart();
            //    }
            //    else
            //    {
            //    }
            //}
            return isSuccess;
        }

        public bool doCancelCommandByMCSCmdIDWithNoReport(string cancel_abort_mcs_cmd_id, CMDCancelType actType, out string ohtc_cmd_id)
        {
            ACMD_MCS mcs_cmd = scApp.CMDBLL.getCMD_MCSByID(cancel_abort_mcs_cmd_id);
            bool is_success = true;
            ohtc_cmd_id = string.Empty;
            switch (actType)
            {
                case CMDCancelType.CmdCancel:
                    //scApp.ReportBLL.newReportTransferCancelInitial(mcs_cmd, null);
                    if (mcs_cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue)
                    {
                        return false;
                    }
                    else if (mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.Queue && mcs_cmd.TRANSFERSTATE < E_TRAN_STATUS.Transferring)
                    {
                        AVEHICLE assign_vh = null;
                        assign_vh = scApp.VehicleBLL.getVehicleByExcuteMCS_CMD_ID(cancel_abort_mcs_cmd_id);
                        ohtc_cmd_id = assign_vh.OHTC_CMD;
                        is_success = doAbortCommand(assign_vh, ohtc_cmd_id, actType);
                        return is_success;
                    }
                    else if (mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.Transferring) //當狀態變為Transferring時，即代表已經是Load complete
                    {
                        return false;
                    }
                    break;

                case CMDCancelType.CmdAbort:
                    //do nothing
                    break;
            }
            return is_success;
        }

        public bool doCancelOrAbortCommandByMCSCmdID(string cancel_abort_mcs_cmd_id, CMDCancelType actType)
        {
            ACMD_MCS mcs_cmd = scApp.CMDBLL.getCMD_MCSByID(cancel_abort_mcs_cmd_id);
            var excute_vh = scApp.VehicleBLL.cache.getVehicleByMCSCmdID(mcs_cmd.CMD_ID);
            bool is_success = true;

            switch (actType)
            {
                case CMDCancelType.CmdCancel:
                    scApp.ReportBLL.ReportTransferCancelInitial(cancel_abort_mcs_cmd_id);
                    if (mcs_cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue)
                    {
                        scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.TransferCompleted);
                        scApp.ReportBLL.ReportTransferCancelCompleted(cancel_abort_mcs_cmd_id);
                        return true;
                    }
                    //如果不是在Queue且沒有車子在執行時，就直接Cancel Complete
                    if (excute_vh == null)
                    {
                        scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.TransferCompleted);
                        scApp.ReportBLL.ReportTransferCancelCompleted(cancel_abort_mcs_cmd_id);
                        return true;
                    }
                    if (mcs_cmd.isLoading || mcs_cmd.isUnloading)
                    {
                        scApp.ReportBLL.newReportTransferCancelFailed(cancel_abort_mcs_cmd_id, null);
                        return false;
                    }
                    using (var tx = SCUtility.getTransactionScope())
                    {
                        using (var con = DBConnection_EF.GetUContext())
                        {
                            is_success = scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.Canceling);
                            is_success = scApp.VehicleService.cancleOrAbortCommandByMCSCmdID(cancel_abort_mcs_cmd_id, ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);
                            if (is_success)
                            {
                                //scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.Canceling);
                                tx.Complete();
                            }
                            else
                            {
                                scApp.ReportBLL.newReportTransferCancelFailed(cancel_abort_mcs_cmd_id, null);
                            }
                        }
                    }

                    //if (mcs_cmd.TRANSFERSTATE >= E_TRAN_STATUS.Transferring)
                    //{
                    //    scApp.ReportBLL.newReportTransferCancelFailed(cancel_abort_mcs_cmd_id, null);
                    //}
                    //else
                    //{
                    //    AVEHICLE crane = scApp.VehicleBLL.getVehicleByID(mcs_cmd.CRANE.Trim());
                    //    if (crane.isTcpIpConnect)
                    //    {
                    //        is_success = scApp.VehicleService.cancleOrAbortCommandByMCSCmdID(cancel_abort_mcs_cmd_id, ProtocolFormat.OHTMessage.CMDCancelType.CmdCancel);

                    //        if (is_success)
                    //        {
                    //            scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.Canceling);
                    //        }
                    //        else
                    //        {
                    //            scApp.ReportBLL.newReportTransferCancelFailed(cancel_abort_mcs_cmd_id, null);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        scApp.ReportBLL.newReportTransferCancelFailed(cancel_abort_mcs_cmd_id, null);
                    //        //scApp.TransferService.LocalCmdCancel(cancel_abort_mcs_cmd_id, "車子不在線上");
                    //    }
                    //}
                    break;

                case CMDCancelType.CmdAbort:
                    scApp.ReportBLL.ReportTransferAbortInitiated(cancel_abort_mcs_cmd_id);
                    if (mcs_cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue)
                    {
                        scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.TransferCompleted);
                        scApp.ReportBLL.ReportTransferAbortCompleted(cancel_abort_mcs_cmd_id);
                        return false;
                    }
                    //如果不是在Queue且沒有車子在執行時，Abort Complete
                    if (excute_vh == null)
                    {
                        scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.TransferCompleted);
                        scApp.ReportBLL.ReportTransferAbortCompleted(cancel_abort_mcs_cmd_id);
                        return true;
                    }

                    if (mcs_cmd.isLoading || mcs_cmd.isUnloading)
                    {
                        scApp.ReportBLL.newReportTransferAbortFailed(cancel_abort_mcs_cmd_id, null);
                        return false;
                    }
                    using (var tx = SCUtility.getTransactionScope())
                    {
                        using (var con = DBConnection_EF.GetUContext())
                        {
                            scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.Aborting);
                            is_success = scApp.VehicleService.cancleOrAbortCommandByMCSCmdID(cancel_abort_mcs_cmd_id, ProtocolFormat.OHTMessage.CMDCancelType.CmdAbort);
                            if (is_success)
                            {
                                //scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.Aborting);
                                tx.Complete();
                            }
                            else
                            {
                                scApp.ReportBLL.newReportTransferAbortFailed(cancel_abort_mcs_cmd_id, null);
                            }
                        }
                    }


                    //bool localDelete = false;
                    //string log = "對命令: " + cancel_abort_mcs_cmd_id + " 強制結束";

                    //if (mcs_cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue)
                    //{
                    //    localDelete = true;
                    //    log = log + " mcs_cmd.TRANSFERSTATE:" + mcs_cmd.TRANSFERSTATE;
                    //}
                    //else
                    //{
                    //    AVEHICLE crane = GetVehicleDataByVehicleID(mcs_cmd.CRANE.Trim());

                    //    if (crane.isTcpIpConnect)
                    //    {
                    //        if (crane.MCS_CMD.Trim() == mcs_cmd.CMD_ID.Trim())
                    //        {
                    //            is_success = cancleOrAbortCommandByMCSCmdID(cancel_abort_mcs_cmd_id, ProtocolFormat.OHTMessage.CMDCancelType.CmdAbort);
                    //            if (is_success)
                    //            {
                    //                scApp.ReportBLL.ReportTransferAbortInitiated(cancel_abort_mcs_cmd_id);
                    //                scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.Aborting);
                    //            }
                    //            else
                    //            {
                    //                scApp.ReportBLL.newReportTransferAbortFailed(cancel_abort_mcs_cmd_id, null);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            log = log + " 命令ID不一樣";
                    //            localDelete = true;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        log = log + " " + crane.VEHICLE_ID + " 連線狀態(isTcpIpConnect) : " + crane.isTcpIpConnect;
                    //        localDelete = true;
                    //    }
                    //}

                    //if (localDelete)
                    //{
                    //    transferService.TransferServiceLogger.Info
                    //    (DateTime.Now.ToString("HH:mm:ss.fff ")
                    //        + log + "\n"
                    //        + transferService.GetCmdLog(mcs_cmd)
                    //    );

                    //    scApp.CMDBLL.updateCMD_MCS_TranStatus(cancel_abort_mcs_cmd_id, E_TRAN_STATUS.TransferCompleted);
                    //    scApp.ReportBLL.ReportTransferAbortInitiated(cancel_abort_mcs_cmd_id);
                    //    scApp.ReportBLL.ReportTransferAbortCompleted(cancel_abort_mcs_cmd_id);

                    //    //自動 Force finish cmd 可以加在這
                    //    Task.Run(() =>
                    //    {
                    //        scApp.CMDBLL.forceUpdataCmdStatus2FnishByVhID(mcs_cmd.CRANE.Trim()); // Force finish Cmd
                    //    });
                    //}
                    break;
            }
            return is_success;
        }

        public bool doPriorityUpdateCommandByMCSCmdID(string update_mcs_cmd_id, string priority)
        {
            ACMD_MCS mcs_cmd = scApp.CMDBLL.getCMD_MCSByID(update_mcs_cmd_id);
            bool is_success = true;
            int pri = Convert.ToInt32(priority);
            if (mcs_cmd.TRANSFERSTATE == E_TRAN_STATUS.Queue)
            {
                scApp.CMDBLL.updateCMD_MCS_Priority(mcs_cmd, pri);
            }
            return is_success;
        }

        public bool cancleOrAbortCommandByMCSCmdID(string mcsCmdID, CMDCancelType actType)
        {
            scApp.TransferService.TransferServiceLogger.Info(
                DateTime.Now.ToString("HH:mm:ss.fff ") + "OHB >> OHT | 對 OHT 下 MCS CmdID：" + mcsCmdID + " " + actType + " 動作");

            bool isSuccess = true;
            AVEHICLE assign_vh = null;
            try
            {
                assign_vh = scApp.VehicleBLL.getVehicleByExcuteMCS_CMD_ID(mcsCmdID);
                if (assign_vh == null)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"command interrupt by mcs command id:{mcsCmdID} fail. current no vh in excute",
                       VehicleID: assign_vh?.VEHICLE_ID,
                       CarrierID: assign_vh?.CST_ID);
                    //return false;
                    return false;
                }
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"command interrupt by mcs command id:{mcsCmdID},vh:{assign_vh.VEHICLE_ID},ohtc cmd:{assign_vh.OHTC_CMD},interrupt type:{actType}",
                   VehicleID: assign_vh?.VEHICLE_ID,
                   CarrierID: assign_vh?.CST_ID);

                CMDCancelType cancel_type_to_vh = CMDCancelType.CmdCancel;
                //if (assign_vh.HAS_BOX == 1)
                if (assign_vh.HAS_CST == 1)
                {
                    cancel_type_to_vh = CMDCancelType.CmdAbort;
                }

                string ohtc_cmd_id = SCUtility.Trim(assign_vh.OHTC_CMD);
                //A0.01 isSuccess = doAbortCommand(assign_vh, mcsCmdID, actType);
                isSuccess = doAbortCommand(assign_vh, ohtc_cmd_id, cancel_type_to_vh); //A0.01
            }
            catch (Exception ex)
            {
                isSuccess = false;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: assign_vh?.VEHICLE_ID,
                   CarrierID: assign_vh?.CST_ID,
                   Details: $"abort command fail mcs command id:{mcsCmdID}");
            }

            scApp.TransferService.TransferServiceLogger.Info(
                DateTime.Now.ToString("HH:mm:ss.fff ") + "OHB >> OHT | 對 OHT 下 MCS CmdID：" + mcsCmdID + " 回傳結果：" + isSuccess);

            return isSuccess;
        }

        public bool doInstallCommandByMCSCmdID(bool has_carrier, string carrier_id, string box_id, string carrier_loc)
        {
            bool is_success = true;
            CassetteData carrier = null;
            carrier = new CassetteData()
            {
                StockerID = "1",
                //CSTID = carrier_id,
                BOXID = box_id,
                Carrier_LOC = carrier_loc,
                CSTState = E_CSTState.Installed,
                CSTInDT = DateTime.Now.ToString("yy/MM/dd HH:mm:ss"),
            };

            if (scApp.TransferService.isUnitType(carrier_loc, UnitType.SHELF))
            {
                carrier.CSTState = E_CSTState.Completed;
            }

            if (has_carrier)
            {
                is_success &= scApp.CassetteDataBLL.UpdateCSTDataByID(carrier_id, box_id, carrier_loc);
            }
            else
            {
                is_success &= scApp.CassetteDataBLL.insertCassetteData(carrier);
            }

            is_success &= scApp.ReportBLL.ReportCarrierInstallCompleted(carrier);
            return is_success;
        }


        public bool doChgEnableShelfCommand(string shelf_id, bool enable)
        {
            string disable_reason = enable ? "" : "Disable By MCS Command";
            bool is_success = true;
            ShelfDef shelf = scApp.ShelfDefBLL.GetShelfDataByID(shelf_id);
            is_success &= scApp.ShelfDefBLL.UpdateEnableByID(shelf_id, enable, disable_reason);
            ZoneDef zone = scApp.ZoneDefBLL.loadZoneDataByID(shelf.ZoneID);
            scApp.ReportBLL.ReportShelfStatusChange(zone);
            return is_success;
        }

        public bool doAbortCommand(AVEHICLE assign_vh, string cmd_id, CMDCancelType actType)
        {
            return assign_vh.sned_Str37(cmd_id, actType);
        }

        private bool sendTransferCommandToVh(ACMD_OHTC cmd, AVEHICLE assignVH, ActiveType activeType, string[] minRouteSec_Vh2From,
                                            string[] minRouteSec_From2To, string[] minRouteAdr_Vh2From, string[] minRouteAdr_From2To)
        {
            bool isSuccess = true;
            string cmd_id = cmd.CMD_ID;
            string vh_id = cmd.VH_ID;
            try
            {
                string cst_type = getCSTType(cmd);
                List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                using (var tx = SCUtility.getTransactionScope())
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        if (activeType != ActiveType.Override)
                        {
                            isSuccess &= scApp.VehicleBLL.updateVehicleExcuteCMD(cmd.VH_ID, cmd.CMD_ID, cmd.CMD_ID_MCS);
                        }
                        isSuccess &= scApp.CMDBLL.updateCommand_OHTC_StatusByCmdID(cmd.CMD_ID, E_CMD_STATUS.Execution);
                        if (isSuccess)
                        {
                            isSuccess &= TransferRequset
                                (cmd.VH_ID, cmd.CMD_ID, cmd.CMD_ID_MCS, activeType, cmd.CARRIER_ID, cmd.BOX_ID, cmd.LOT_ID
                                , minRouteSec_Vh2From, minRouteSec_From2To, minRouteAdr_Vh2From, minRouteAdr_From2To
                                , cmd.SOURCE, cmd.DESTINATION, cmd.SOURCE_ADR, cmd.DESTINATION_ADR, cst_type);
                        }
                        if (isSuccess)
                        {
                            tx.Complete();
                        }
                    }
                }
                if (isSuccess)
                {
                    scApp.TransferService.OHT_TransferStatus(cmd_id, vh_id, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_ENROUTE);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exection:");
                isSuccess = false;
            }
            return isSuccess;
        }

        private string getCSTType(ACMD_OHTC cmd)
        {
            string cst_type = "";
            var cst_type_get_result = cmd.tryGetCSTType(scApp.PortStationBLL, scApp.EquipmentBLL, scApp.TransferService);
            if (!SCUtility.isEmpty(DebugParameter.CST_TYPE))
            {
                cst_type = SCUtility.Trim(DebugParameter.CST_TYPE, true);
            }
            else
            {
                if (cst_type_get_result.isDefine)
                {
                    cst_type = cst_type_get_result.cstType;
                }
            }

            return cst_type;
        }

        public bool TransferRequset(string vh_id, string cmd_id, string mcs_cmd_id, ActiveType activeType, string cst_id, string box_id, string lot_id,
            string[] minRouteSec_Vh2From, string[] minRouteSec_From2To, string[] minRouteAdr_Vh2From, string[] minRouteAdr_From2To,
            string fromPort_id, string toPort_id, string fromAdr, string toAdr, string cstType)
        {
            bool isSuccess = false;
            string reason = string.Empty;
            ID_131_TRANS_RESPONSE receive_gpp = null;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            isSuccess = TransferCommandCheck(activeType, cst_id, minRouteSec_Vh2From, minRouteSec_From2To, minRouteAdr_Vh2From, minRouteAdr_From2To, fromAdr, toAdr, out reason);
            if (isSuccess)
            {
                ID_31_TRANS_REQUEST send_gpb = new ID_31_TRANS_REQUEST()
                {
                    CmdID = cmd_id,
                    ActType = activeType,
                    CSTID = cst_id ?? string.Empty,
                    BOXID = box_id ?? string.Empty,
                    LOTID = lot_id ?? string.Empty,
                    LoadPortID = fromPort_id,
                    UnloadPortID = toPort_id,
                    LoadAdr = fromAdr,
                    ToAdr = toAdr,
                    CSTTYPE = cstType
                };
                if (minRouteSec_Vh2From != null)
                    send_gpb.GuideSectionsStartToLoad.AddRange(minRouteSec_Vh2From);
                if (minRouteSec_From2To != null)
                    send_gpb.GuideSectionsToDestination.AddRange(minRouteSec_From2To);
                if (minRouteAdr_Vh2From != null)
                    send_gpb.GuideAddressStartToLoad.AddRange(minRouteAdr_Vh2From);
                if (minRouteAdr_From2To != null)
                    send_gpb.GuideAddressToDestination.AddRange(minRouteAdr_From2To);
                LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, send_gpb, 0);
                isSuccess = vh.sned_Str31(send_gpb, out receive_gpp, out reason);
                LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, receive_gpp, 0);
            }
            if (isSuccess)
            {
                int reply_code = receive_gpp.ReplyCode;
                if (reply_code != 0)
                {
                    isSuccess = false;
                    var return_code_map = scApp.CMDBLL.getReturnCodeMap(vh.NODE_ID, reply_code.ToString());
                    if (return_code_map != null)
                        reason = return_code_map.DESC;
                    bcf.App.BCFApplication.onWarningMsg(string.Format("發送命令失敗,VH ID:{0}, CMD ID:{1}, Reason:{2}",
                                                              vh_id,
                                                              cmd_id,
                                                              reason));
                }
                else
                {

                }
            }
            else
            {
                bcf.App.BCFApplication.onWarningMsg(string.Format("發送命令失敗,VH ID:{0}, CMD ID:{1}, Reason:{2}",
                                          vh_id,
                                          cmd_id,
                                          reason));
                VehicleStatusRequest(vh_id, true);
            }

            return isSuccess;
        }

        public bool CarrierIDRenameRequset(string vh_id, string oldCarrierID, string newCarrierID)
        {
            bool isSuccess = true;

            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_135_CARRIER_ID_RENAME_RESPONSE receive_gpp;
            ID_35_CARRIER_ID_RENAME_REQUEST send_gpp = new ID_35_CARRIER_ID_RENAME_REQUEST()
            {
                OLDCSTID = oldCarrierID ?? string.Empty,
                NEWCSTID = newCarrierID ?? string.Empty,
            };
            SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, send_gpp);
            isSuccess = vh.sned_Str35(send_gpp, out receive_gpp);
            SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, receive_gpp, isSuccess.ToString());
            return isSuccess;
        }

        private bool TransferCommandCheck(ActiveType activeType, string cst_id,
                                        string[] minRouteSec_Vh2From, string[] minRouteSec_From2To, string[] minRouteAdr_Vh2From, string[] minRouteAdr_From2To,
                                        string fromAdr, string toAdr, out string reason)
        {
            reason = "";
            if (activeType == ActiveType.Home || activeType == ActiveType.Mtlhome)
            {
                return true;
            }

            if (activeType == ActiveType.Load || activeType == ActiveType.Unload ||
                (activeType == ActiveType.Loadunload && SCUtility.isMatche(fromAdr, toAdr)))
            {
                //not thing...
            }
            else
            {
                if (minRouteSec_Vh2From == null || minRouteSec_Vh2From.Count() == 0)
                {   //For Test Bypass 2020/01/12
                    //reason = "Pass section is empty !";
                    //return false;
                    return true;
                }
            }

            bool isOK = true;
            switch (activeType)
            {
                case ActiveType.Load:
                    if (SCUtility.isEmpty(fromAdr))
                    {
                        isOK = false;
                        reason = $"Transfer type[{activeType},from adr is empty!]";
                    }
                    break;

                case ActiveType.Unload:
                    if (SCUtility.isEmpty(toAdr))
                    {
                        isOK = false;
                        reason = $"Transfer type[{activeType},from adr is empty!]";
                    }
                    break;

                case ActiveType.Loadunload:
                    if (SCUtility.isEmpty(fromAdr))
                    {
                        isOK = false;
                        reason = $"Transfer type[{activeType},from adr is empty!]";
                    }
                    else if (SCUtility.isEmpty(toAdr))
                    {
                        isOK = false;
                        reason = $"Transfer type[{activeType},toAdr adr is empty!]";
                    }
                    break;
            }

            return isOK;
        }

        public bool TeachingRequest(string vh_id, string from_adr, string to_adr)
        {
            bool isSuccess = false;
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            ID_171_RANGE_TEACHING_RESPONSE receive_gpp;
            ID_71_RANGE_TEACHING_REQUEST send_gpp = new ID_71_RANGE_TEACHING_REQUEST()
            {
                FromAdr = from_adr,
                ToAdr = to_adr
            };

            SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, send_gpp);
            isSuccess = vh.send_Str71(send_gpp, out receive_gpp);
            SCUtility.RecodeReportInfo(vh.VEHICLE_ID, 0, receive_gpp, isSuccess.ToString());

            return isSuccess;
        }

        #endregion Tcp/Ip

        #region PLC

        public void PLC_Control_TrunOn(string vh_id)
        {
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            vh.PLC_Control_TrunOn();
        }

        public void PLC_Control_TrunOff(string vh_id)
        {
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            vh.PLC_Control_TrunOff();
        }

        public bool SetVehicleControlItemForPLC(string vh_id, Boolean[] items)
        {
            AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
            return vh.setVehicleControlItemForPLC(items);
        }

        #endregion PLC

        #endregion Send Message To Vehicle

        #region Position Report

        [ClassAOPAspect]
        public void PositionReport(BCFApplication bcfApp, AVEHICLE eqpt, ID_134_TRANS_EVENT_REP recive_str)
        {
            if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                return;
            var workItem = new com.mirle.ibg3k0.bcf.Data.BackgroundWorkItem(scApp, eqpt, recive_str);
            scApp.BackgroundWorkProcVehiclePosition.triggerBackgroundWork(eqpt.VEHICLE_ID, workItem);
        }



        private double getDistance(double x1, double y1, double x2, double y2)
        {
            double dx, dy;
            dx = x2 - x1;
            dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public void doUpdateVheiclePositionAndCmdSchedule(AVEHICLE vh,
          string current_adr_id, string current_sec_id, string current_seg_id,
          string last_adr_id, string last_sec_id, string last_seg_id,
          uint sec_dis, EventType vhPassEvent)
        {
            try
            {
                ALINE line = scApp.getEQObjCacheManager().getLine();
                scApp.VehicleBLL.updateVheiclePosition_CacheManager(vh, current_adr_id, current_sec_id, current_seg_id, sec_dis);
                var update_result = scApp.VehicleBLL.updateVheiclePositionToReserveControlModule
                    (scApp.ReserveBLL, vh, current_sec_id, current_adr_id, sec_dis, 0, 0, 1,
                     HltDirection.Forward, HltDirection.Forward);

                if (line.ServiceMode == SCAppConstants.AppServiceMode.Active)
                {
                    if (!SCUtility.isMatche(current_seg_id, last_seg_id))
                    {
                        vh.onSegmentChange(current_seg_id, last_seg_id);
                    }

                    if (!SCUtility.isMatche(current_sec_id, last_sec_id))
                    {
                        vh.onLocationChange(current_sec_id, last_sec_id);
                        //TODO 要改成查一次CMD出來然後直接帶入CMD ID
                        if (!SCUtility.isEmpty(vh.OHTC_CMD))
                        {
                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        private long syncPoint_NotifyVhAvoid = 0;

        public void CheckObstacleStatusByVehicleView()
        {
            try
            {
                List<AVEHICLE> lstVH = scApp.VehicleBLL.cache.loadVhs();
                foreach (var vh in lstVH)
                {
                    if (vh.isTcpIpConnect &&
                        (vh.MODE_STATUS != VHModeStatus.Manual) &&
                         vh.IsObstacle)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                               Data: $"start try find blocked vh...",
                                               VehicleID: vh.VEHICLE_ID);
                        ASEGMENT seg = scApp.SegmentBLL.cache.GetSegment(vh.CUR_SEG_ID);
                        AVEHICLE next_vh_on_seg = seg.GetNextVehicle(vh);
                        if (next_vh_on_seg != null)
                        {
                            tryDriveOutTheVh(vh.VEHICLE_ID, next_vh_on_seg.VEHICLE_ID);
                        }
                        else
                        {

                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"current segment id:{vh.CUR_SEG_ID},no find the next vh",
                               VehicleID: vh.VEHICLE_ID);

                            if (vh.IsOnAdr)
                            {
                                string start_search_adr = vh.CUR_ADR_ID;
                                var from_sections = scApp.SectionBLL.cache.GetSectionsByAddress(start_search_adr);
                                foreach (var sec in from_sections)
                                {
                                    var vhs = scApp.VehicleBLL.cache.getVhBySections(sec.SEC_ID);
                                    vhs.Remove(vh);
                                    if (vhs.Count != 0)
                                    {
                                        foreach (var v in vhs)
                                        {
                                            Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, v.VEHICLE_ID));
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        var vh_from = scApp.VehicleBLL.cache.getVhByAddressID(sec.FROM_ADR_ID);
                                        if (vh_from != null)
                                        {
                                            Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, vh_from.VEHICLE_ID));
                                            return;
                                        }
                                        var vh_to = scApp.VehicleBLL.cache.getVhByAddressID(sec.TO_ADR_ID);
                                        if (vh_to != null)
                                        {
                                            Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, vh_to.VEHICLE_ID));
                                            return;
                                        }
                                    }
                                }

                                var sections = scApp.SectionBLL.cache.GetSectionsByAddress(vh.CUR_ADR_ID);
                                if (sections != null && sections.Count > 0)
                                {
                                    foreach (var sec in sections)
                                    {
                                        var result = scApp.ReserveBLL.TryAddReservedSection(vh.VEHICLE_ID, sec.SEC_ID,
                                                         sensorDir: HltDirection.ForwardBackword,
                                                         isAsk: true);

                                        if (!result.OK)
                                        {
                                            if (!SCUtility.isEmpty(result.VehicleID))
                                            {
                                                //Task.Run(() => scApp.VehicleBLL.whenVhObstacle(result.VehicleID, vhID));
                                                Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, result.VehicleID));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var cur_sec = scApp.SectionBLL.cache.GetSection(vh.CUR_SEC_ID);
                                var sections_from = scApp.SectionBLL.cache.GetSectionsByAddress(cur_sec.FROM_ADR_ID);
                                var sections_to = scApp.SectionBLL.cache.GetSectionsByAddress(cur_sec.TO_ADR_ID);
                                sections_from.AddRange(sections_to);

                                foreach (var sec in sections_from)
                                {
                                    var result = scApp.ReserveBLL.TryAddReservedSection(vh.VEHICLE_ID, sec.SEC_ID,
                                                     sensorDir: HltDirection.ForwardBackword,
                                                     isAsk: true);

                                    if (!result.OK)
                                    {
                                        if (!SCUtility.isEmpty(result.VehicleID))
                                        {
                                            //Task.Run(() => scApp.VehicleBLL.whenVhObstacle(result.VehicleID, vhID));
                                            Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, result.VehicleID));
                                            return;
                                        }
                                    }
                                }

                                var vh_cur_sec = scApp.SectionBLL.cache.GetSection(vh.CUR_SEC_ID);
                                string start_search_adr = vh_cur_sec.TO_ADR_ID;
                                var to_sections = scApp.SectionBLL.cache.GetSectionsByAddress(start_search_adr);
                                foreach (var sec in to_sections)
                                {
                                    var vhs = scApp.VehicleBLL.cache.getVhBySections(sec.SEC_ID);
                                    vhs.Remove(vh);
                                    if (vhs.Count != 0)
                                    {
                                        foreach (var v in vhs)
                                        {
                                            Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, v.VEHICLE_ID));
                                        }
                                        return;
                                    }
                                    else
                                    {
                                        var vh_from = scApp.VehicleBLL.cache.getVhByAddressID(sec.FROM_ADR_ID);
                                        if (vh_from != null)
                                        {
                                            Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, vh_from.VEHICLE_ID));
                                            return;
                                        }
                                        var vh_to = scApp.VehicleBLL.cache.getVhByAddressID(sec.TO_ADR_ID);
                                        if (vh_to != null)
                                        {
                                            Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, vh_to.VEHICLE_ID));
                                            return;
                                        }
                                    }
                                }
                                var current_guide_sections = vh.WillPassSectionID;
                                if (current_guide_sections != null && current_guide_sections.Count > 0)
                                {
                                    foreach (string sec in current_guide_sections)
                                    {
                                        var result = scApp.ReserveBLL.TryAddReservedSection(vh.VEHICLE_ID, sec,
                                                         sensorDir: HltDirection.ForwardBackword,
                                                         isAsk: true);

                                        if (!result.OK)
                                        {
                                            if (!SCUtility.isEmpty(result.VehicleID))
                                            {
                                                //Task.Run(() => scApp.VehicleBLL.whenVhObstacle(result.VehicleID, vhID));
                                                Task.Run(() => tryDriveOutTheVh(vh.VEHICLE_ID, result.VehicleID));
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        public void TestDriveOutTheVh(string willPassVhID, string onTheWayVhID)
        {
            tryDriveOutTheVh(willPassVhID, onTheWayVhID);
        }
        private void tryDriveOutTheVh(string willPassVhID, string onTheWayVhID)
        {
            if (!DebugParameter.IsAutoDriveOut)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"auto drive out vh current is off,IsAutoDriveOut:{DebugParameter.IsAutoDriveOut}",
                   VehicleID: willPassVhID);
                return;
            }

            if (System.Threading.Interlocked.Exchange(ref syncPoint_NotifyVhAvoid, 1) == 0)
            {
                try
                {
                    if (DebugParameter.IsOpenParkingZoneControlFunction)
                    {
                        findTheVhOfAvoidAddressNew(willPassVhID, onTheWayVhID);
                    }
                    else
                    {
                        findTheVhOfAvoidAddress(willPassVhID, onTheWayVhID);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: ex,
                       Details: $"excute tryNotifyVhAvoid has exception happend.requestVh:{willPassVhID}");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint_NotifyVhAvoid, 0);
                }
            }
        }

        private bool findTheVhOfAvoidAddress(string willPassVhID, string inTheWayVhID)
        {
            bool is_success = false;
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"start try drive out vh:{inTheWayVhID}...",
                                   VehicleID: willPassVhID);

            //確認能否把該Vh趕走
            AVEHICLE in_the_way_vh = scApp.VehicleBLL.cache.getVhByID(inTheWayVhID);
            var check_can_creat_avoid_command = canCreatDriveOutCommand(in_the_way_vh);
            if (check_can_creat_avoid_command.is_can)
            {
                //B0.09 var find_result = findAvoidAddressNew(in_the_way_vh);
                //var find_result = findAvoidAddressForFixPort(in_the_way_vh);//B0.09
                var find_result = findAvoidAddressForAvoidTypeAdr(in_the_way_vh);//B0.09
                if (find_result.isFind)
                {
                    is_success = scApp.CMDBLL.doCreatTransferCommand(inTheWayVhID,
                                                                         cmd_type: E_CMD_TYPE.Move,
                                                                         destination_address: find_result.avoidAdr);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Try to notify vh avoid,requestVh:{willPassVhID} reservedVh:{inTheWayVhID} avoid address:{find_result.avoidAdr}," +
                             $" is success :{is_success}.",
                       VehicleID: willPassVhID);
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Can't find the avoid address.",
                       VehicleID: willPassVhID);
                }
            }
            else
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"start try drive out vh:{inTheWayVhID},but vh status not ready.",
                   VehicleID: willPassVhID);
            }
            return is_success;
        }
        private bool findTheVhOfAvoidAddressNew(string willPassVhID, string inTheWayVhID)
        {
            bool is_success = false;
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"start try drive out vh:{inTheWayVhID} (new)...",
                                   VehicleID: willPassVhID);

            //確認能否把該Vh趕走
            AVEHICLE on_the_way_vh = scApp.VehicleBLL.cache.getVhByID(inTheWayVhID);
            var check_can_excute_parking_cmd = on_the_way_vh.CanCreatParkingCommand(scApp.CMDBLL);

            if (check_can_excute_parking_cmd.is_can)
            {
                AVEHICLE will_pass_vh = scApp.VehicleBLL.cache.getVhByID(willPassVhID);
                var find_result = findNotConflictSectionAndAvoidAddressNew(will_pass_vh, on_the_way_vh);
                if (find_result.isFind)
                {
                    is_success = scApp.CMDBLL.doCreatTransferCommand(inTheWayVhID,
                                                                         cmd_type: E_CMD_TYPE.Move,
                                                                         destination_address: find_result.avoidAdr);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Try to notify vh avoid,requestVh:{willPassVhID} reservedVh:{inTheWayVhID} avoid address:{find_result.avoidAdr}," +
                             $" is success :{is_success}.",
                       VehicleID: willPassVhID);
                }
                else
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Can't find the avoid address.",
                       VehicleID: willPassVhID);
                }
            }
            else
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"start try drive out vh:{inTheWayVhID},but vh status not ready,reason:{check_can_excute_parking_cmd.result}",
                   VehicleID: willPassVhID);
            }
            return is_success;
        }
        private (bool isFind, string avoidAdr) findAvoidAddressForFixPort(AVEHICLE willDrivenAwayVh)
        {
            //1.找看看是否有設定的固定避車點。
            //List<PortDef> can_avoid_cv_port = scApp.PortDefBLL.cache.loadCanAvoidCVPortDefs();
            //List<PortDef> can_avoid_port = scApp.PortDefBLL.cache.loadCanAvoidPortDefs();
            List<PortDef> can_avoid_port = null;
            can_avoid_port = scApp.PortDefBLL.cache.loadCanAvoidPortDefs();


            //2.找出離自己最近的一個CV點且沒有車在上面沒有命令要前往的Address
            var find_result = findTheNearestCVPort(willDrivenAwayVh, can_avoid_port);


            PortDef avoid_port = null;
            if (find_result.isFind)
            {
                avoid_port = find_result.PortDef;
            }
            else
            {
                avoid_port = can_avoid_port.FirstOrDefault();
            }

            if (avoid_port != null)
            {
                return (true, avoid_port.ADR_ID);
            }
            else
            {
                return (false, "");
            }
        }


        private (bool isFind, PortDef PortDef) findTheNearestCVPort(AVEHICLE willDrivenAwayVh, IEnumerable<PortDef> all_cv_port_in_mode)
        {
            int min_distance = int.MaxValue;
            PortDef nearest_cv_port = null;
            foreach (var port_def in all_cv_port_in_mode)
            {
                var adr_obj = port_def.getAdressObj(scApp.AddressBLL);
                if (adr_obj != null)
                {
                    bool has_vh_on_or_to_adr = adr_obj.HasVhIdleOnHere(scApp.VehicleBLL) || adr_obj.HasVhWillComeHere(scApp.CMDBLL);
                    if (has_vh_on_or_to_adr)
                    {
                        continue;
                    }
                }

                if (SCUtility.isMatche(port_def.ADR_ID, willDrivenAwayVh.CUR_ADR_ID)) continue;//如果目前所在的Address與要找的CV Port 一樣的話，要濾掉

                var check_result = scApp.GuideBLL.IsRoadWalkable(willDrivenAwayVh.CUR_ADR_ID, port_def.ADR_ID);
                if (check_result.isSuccess && check_result.distance < min_distance)
                {
                    min_distance = check_result.distance;
                    nearest_cv_port = port_def;
                }
            }
            return (nearest_cv_port != null, nearest_cv_port);
        }


        private (bool isFind, string avoidAdr) findAvoidAddressForAvoidTypeAdr(AVEHICLE willDrivenAwayVh)
        {
            //1.找看看是否有設定的固定避車點。
            var avoid_addresses = scApp.AddressBLL.cache.loadCanAvoidAddresses();

            //2.找出離自己最近的一個CV點且沒有車在上面沒有命令要前往的Address
            var find_result = findTheNearestAvoidAddress(willDrivenAwayVh, avoid_addresses);


            AADDRESS avoid_adr = null;
            if (find_result.isFind)
            {
                avoid_adr = find_result.adr;
            }
            else
            {
                avoid_adr = avoid_addresses.FirstOrDefault();
            }



            if (avoid_adr != null)
            {
                //找出點位以後，將自己的位置到停等點為止是否也有停車點，有的話就改到那個位置
                var guide_info = scApp.GuideBLL.getGuideInfo(willDrivenAwayVh.CUR_ADR_ID, avoid_adr.ADR_ID);

                var guide_adrs = guide_info.guideAddressIds;
                foreach (var adr in guide_adrs)
                {
                    var adr_obj = scApp.AddressBLL.cache.GetAddress(adr);
                    if (adr_obj.IsAvoid)
                    {
                        avoid_adr = adr_obj;
                    }
                }

                return (true, avoid_adr.ADR_ID);
            }
            else
            {
                return (false, "");
            }
        }
        private (bool isFind, AADDRESS adr) findTheNearestAvoidAddress(AVEHICLE willDrivenAwayVh, List<AADDRESS> all_can_avoid_adrs)
        {
            int min_distance = int.MaxValue;
            AADDRESS nearest_address = null;
            foreach (var adr in all_can_avoid_adrs)
            {
                bool has_vh_on_or_to_adr = adr.HasVhIdleOnHere(scApp.VehicleBLL) || adr.HasVhWillComeHere(scApp.CMDBLL);
                if (has_vh_on_or_to_adr)
                {
                    continue;
                }

                if (SCUtility.isMatche(adr.ADR_ID, willDrivenAwayVh.CUR_ADR_ID)) continue;

                var check_result = scApp.GuideBLL.IsRoadWalkable(willDrivenAwayVh.CUR_ADR_ID, adr.ADR_ID);
                if (check_result.isSuccess && check_result.distance < min_distance)
                {
                    min_distance = check_result.distance;
                    nearest_address = adr;
                }
            }
            return (nearest_address != null, nearest_address);
        }


        private (bool isFind, string avoidAdr) findNotConflictSectionAndAvoidAddressNew
            (AVEHICLE willPassVh, AVEHICLE findAvoidAdrOfVh)
        {
            string needToAvoidAdr = findNeedAvoidAddress(willPassVh);
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"vh:{findAvoidAdrOfVh.VEHICLE_ID} 正在找尋停車位，需要特別避開的點位:{needToAvoidAdr}...",
                                   VehicleID: willPassVh.VEHICLE_ID);

            bool isFindNotConflict = TryFindAvoidAddress(willPassVh, findAvoidAdrOfVh, needToAvoidAdr, out string escapeAddress);
            if (isFindNotConflict)
            {
                return (true, escapeAddress);
            }
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                   Data: $"vh:{findAvoidAdrOfVh.VEHICLE_ID} 正在找尋停車位，取消需要特別避開的點位再找一次...",
                                   VehicleID: willPassVh.VEHICLE_ID);
            bool isFindClosest = TryFindAvoidAddress(willPassVh, findAvoidAdrOfVh, "", out string escapeAddressClosest);
            if (isFindClosest)
            {
                return (true, escapeAddressClosest);
            }
            return (false, "");

        }

        private string findNeedAvoidAddress(AVEHICLE willPassVh)
        {
            if (!willPassVh.IsExcuteCMD_OHTC)
                return "";
            if (willPassVh.IsExcuteCMD_MCS)
            {
                var get_mcs_cmd_resutl = scApp.CMDBLL.cache.tryGetCMD_MCS(willPassVh.MCS_CMD);
                if (get_mcs_cmd_resutl.isExist)
                {
                    var cmd_mcs = get_mcs_cmd_resutl.cmdMCS;
                    if (cmd_mcs.COMMANDSTATE < ACMD_MCS.COMMAND_STATUS_BIT_INDEX_ENROUTE)
                    {
                        var source_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(cmd_mcs.HOSTSOURCE);
                        return source_port_station == null ? string.Empty : source_port_station.ADR_ID;
                    }
                    else
                    {
                        var dest_port_station = scApp.PortStationBLL.OperateCatch.getPortStation(cmd_mcs.HOSTDESTINATION);
                        return dest_port_station == null ? string.Empty : dest_port_station.ADR_ID;
                    }
                }
                else
                {
                    return "";
                }
            }
            else
            {
                var get_ohtc_cmd_resutl = scApp.CMDBLL.cache.tryGetExcuteCmd(willPassVh.OHTC_CMD);
                if (get_ohtc_cmd_resutl.isExist)
                {
                    var cmd_ohtc = get_ohtc_cmd_resutl.cmdOHTC;
                    if (cmd_ohtc.CMD_TPYE == E_CMD_TYPE.Move)
                    {
                        return cmd_ohtc.DESTINATION_ADR;
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
        }
        private bool TryFindAvoidAddress(AVEHICLE commandingVehicle, AVEHICLE escapedVehicle, string needToAvoidAdr, out string escapeAddressID)
        {
            try
            {
                bool isSuccess = false;
                escapeAddressID = String.Empty;
                List<string> bypassSections = new List<string>();
                if (!string.IsNullOrEmpty(needToAvoidAdr))
                {
                    bypassSections = scApp.SectionBLL.cache.GetSectionsByToAddress(needToAvoidAdr).Select(s => s.SEC_ID).ToList();
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"vh:{escapedVehicle.VEHICLE_ID} 正在找尋停車位，需要特別避開的點位:{needToAvoidAdr},因此需要特別避開的section ids:{string.Join(",", bypassSections)}",
                           VehicleID: commandingVehicle.VEHICLE_ID);
                }

                var avoidAddresses = scApp.ParkingZoneBLL.LoadAvoidParkingzoneAddresses(escapedVehicle);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{escapedVehicle.VEHICLE_ID}-Type:{escapedVehicle.VEHICLE_TYPE} 正在找尋停車位，目前上有停車空間的停車區點位:{string.Join(",", avoidAddresses)}",
                       VehicleID: commandingVehicle.VEHICLE_ID);

                isSuccess = findBestEscapeAddress(avoidAddresses, commandingVehicle, escapedVehicle.CUR_ADR_ID, bypassSections, out escapeAddressID);
                if (!isSuccess)
                {
                    avoidAddresses = scApp.AddressBLL.cache.LoadCanAvoidAddresses()
                        .Where(adr => !adr.ADR_ID.Equals(escapedVehicle.CUR_ADR_ID))
                        .Select(adr => adr.ADR_ID.Trim()).ToList();
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"vh:{escapedVehicle.VEHICLE_ID}-Type:{escapedVehicle.VEHICLE_TYPE} 正在找尋停車位，停車區的找尋失敗改找可以避車的點位:{string.Join(",", avoidAddresses)}",
                           VehicleID: commandingVehicle.VEHICLE_ID);

                    isSuccess = findBestEscapeAddress(avoidAddresses, commandingVehicle, escapedVehicle.CUR_ADR_ID, bypassSections, out escapeAddressID);
                }
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{escapedVehicle.VEHICLE_ID}-Type:{escapedVehicle.VEHICLE_TYPE} 正在找尋停車位，停車點找尋結果:{isSuccess}，找尋到的停車點位:{escapeAddressID}",
                       VehicleID: commandingVehicle.VEHICLE_ID);

                return isSuccess;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
                escapeAddressID = String.Empty;
                return false;
            }
        }
        //private bool findBestEscapeAddress(List<string> avoidAddresses, AVEHICLE commandingVehicle, string escapedVehicleaddress, List<string> bypassSections, out string escapeAddressID)
        //{
        //    int minCost = int.MaxValue;
        //    bool isSuccess = false;
        //    escapeAddressID = String.Empty;
        //    foreach (var avoidPoint in avoidAddresses)
        //    {
        //        bool isOKPoint;
        //        //確認退讓車移位後的結果，是否不在命令車的路徑上了，若找不到車輛路徑則無條件通過
        //        if (commandingVehicle.WillPassSectionID == null || commandingVehicle.WillPassSectionID.Count == 0)
        //            isOKPoint = true;
        //        else
        //            isOKPoint = !commandingVehicle.WillPassAddressIDs.Contains(avoidPoint);
        //        if (isOKPoint)
        //        {
        //            //通過移位後路權檢驗，確認cost
        //            var roadCheckResult = scApp.GuideBLL.getGuideInfo(escapedVehicleaddress, avoidPoint, bypassSections);
        //            //logToVehicleServiceLogger($"check avoid candidate address {avoidPoint.ADR_ID}, " +
        //            //    $"PassVehicle:{commandingVehicle.VEHICLE_ID}, AvoidVehicle:{escapedVehicle.VEHICLE_ID}, cost:{roadCheckResult.totalCost}.", LogLevel.Info);
        //            if (roadCheckResult.totalCost < minCost && roadCheckResult.totalCost != 0)
        //            {
        //                minCost = roadCheckResult.totalCost;
        //                escapeAddressID = avoidPoint;
        //                isSuccess = true;
        //            }
        //        }
        //        else
        //        {
        //            //移位後路權檢驗不通過
        //            //logToVehicleServiceLogger($"Remove {avoidPoint.ADR_ID} from avoid candidate address list.", LogLevel.Info);
        //        }
        //    }
        //    return isSuccess;
        //}
        private bool findBestEscapeAddress(List<string> avoidAddresses, AVEHICLE commandingVehicle, string escapedVehicleaddress, List<string> bypassSections, out string escapeAddressID)
        {
            int minCost = int.MaxValue;
            bool isSuccess = false;
            escapeAddressID = String.Empty;
            foreach (var avoidPoint in avoidAddresses)
            {
                if (checkAvoidAdrIsCommandingVhWillPass(commandingVehicle, avoidPoint))
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"停車點:{avoidPoint} 為vh:{commandingVehicle.VEHICLE_ID} 即將通過的路段繼續找下一個...",
                           VehicleID: commandingVehicle.VEHICLE_ID);
                    continue;
                }
                var roadCheckResult = scApp.GuideBLL.IsRoadWalkable(escapedVehicleaddress, avoidPoint, bypassSections);
                if (roadCheckResult.isSuccess)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"停車點:{avoidPoint} 距離:{roadCheckResult.distance}",
                           VehicleID: commandingVehicle.VEHICLE_ID);
                    if (roadCheckResult.distance < minCost && roadCheckResult.distance > 0)
                    {
                        minCost = roadCheckResult.distance;
                        escapeAddressID = avoidPoint;
                        isSuccess = true;
                    }
                }
            }
            return isSuccess;
        }
        private bool checkAvoidAdrIsCommandingVhWillPass(AVEHICLE commandingVh, string avoidAdr)
        {
            if (commandingVh.WillPassSectionID == null || commandingVh.WillPassSectionID.Count == 0)
            {
                return false;
            }

            var avoid_sections = scApp.SectionBLL.cache.GetSectionsByToAddress(avoidAdr);
            if (avoid_sections == null || avoid_sections.Count == 0)
            {
                return false;
            }
            foreach (var avoid_sec in avoid_sections)
            {
                if (commandingVh.WillPassSectionID.Contains(avoid_sec.SEC_ID))
                {
                    return true;
                }
            }
            return false;

        }


        private void PositionReport_LoadingUnloading(BCFApplication bcfApp, AVEHICLE eqpt, ID_136_TRANS_EVENT_REP recive_str, int seq_num, EventType eventType)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Process report {eventType}",
               VehicleID: eqpt.VEHICLE_ID,
               CarrierID: eqpt.CST_ID);

            if (!SCUtility.isEmpty(eqpt.MCS_CMD))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"do report {eventType} to mcs.",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
                replyTranEventReport(bcfApp, recive_str.EventType, eqpt, seq_num);
            }
            else
            {
                replyTranEventReport(bcfApp, recive_str.EventType, eqpt, seq_num);
            }
            if (eventType == EventType.Vhloading)
            {
                scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_LOADING);

                scApp.VehicleBLL.doLoading(eqpt.VEHICLE_ID);
            }
            else if (eventType == EventType.Vhunloading)
            {
                scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_UNLOADING);

                scApp.VehicleBLL.doUnloading(eqpt.VEHICLE_ID);
            }
        }

        [ClassAOPAspect]
        //public void TranEventReport_100(BCFApplication bcfApp, AVEHICLE eqpt, ID_136_TRANS_EVENT_REP recive_str, int seq_num)
        public void TranEventReport(BCFApplication bcfApp, AVEHICLE vh, ID_136_TRANS_EVENT_REP recive_str, int seq_num)
        {
            if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                return;
            LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, recive_str, seq_num);
            EventType eventType = recive_str.EventType;
            string current_adr_id = recive_str.CurrentAdrID;
            string current_sec_id = recive_str.CurrentSecID;
            string carrier_id = recive_str.BOXID;
            string last_adr_id = vh.CUR_ADR_ID;
            string last_sec_id = vh.CUR_SEC_ID;
            string req_block_id = recive_str.RequestBlockID;
            string cmd_id = vh.MCS_CMD;
            BCRReadResult bCRReadResult = recive_str.BCRReadResult;
            string load_port_id = recive_str.LoadPortID;     //B0.01
            string unload_port_id = recive_str.UnloadPortID; //B0.01
            var reserveInfos = recive_str.ReserveInfos;
            string cst_type = recive_str.RequestHIDID;
            scApp.VehicleBLL.updateVehicleActionStatus(vh, eventType);

            switch (eventType)
            {
                case EventType.BlockReq:
                case EventType.Hidreq:
                case EventType.BlockHidreq:
                    //PositionReport_BlockReq_HIDReq(bcfApp, eqpt, seq_num, recive_str.RequestBlockID, recive_str.RequestHIDID);
                    ProcessBlockOrHIDReq(bcfApp, vh, eventType, seq_num, recive_str.RequestBlockID, recive_str.RequestHIDID);
                    break;

                case EventType.LoadArrivals:
                case EventType.LoadComplete:
                case EventType.UnloadArrivals:
                case EventType.UnloadComplete:
                case EventType.AdrOrMoveArrivals:
                    //B0.01 PositionReport_ArriveAndComplete(bcfApp, eqpt, seq_num, recive_str.EventType, recive_str.CurrentAdrID, recive_str.CurrentSecID, carrier_id);
                    PositionReport_ArriveAndComplete(bcfApp, vh, seq_num, recive_str.EventType, recive_str.CurrentAdrID, recive_str.CurrentSecID, carrier_id, //B0.01
                                                     load_port_id, unload_port_id);                                                                             //B0.01
                    break;
                case EventType.Vhloading:
                case EventType.Vhunloading:
                    PositionReport_LoadingUnloading(bcfApp, vh, recive_str, seq_num, eventType);
                    break;

                case EventType.BlockRelease:
                    PositionReport_BlockRelease(bcfApp, vh, recive_str, seq_num);
                    replyTranEventReport(bcfApp, recive_str.EventType, vh, seq_num);
                    break;

                case EventType.Hidrelease:
                    replyTranEventReport(bcfApp, recive_str.EventType, vh, seq_num);
                    break;

                case EventType.BlockHidrelease:
                    replyTranEventReport(bcfApp, recive_str.EventType, vh, seq_num);
                    break;

                case EventType.DoubleStorage:
                    PositionReport_DoubleStorage(bcfApp, vh, seq_num, recive_str.EventType, recive_str.CurrentAdrID, recive_str.CurrentSecID, carrier_id, cst_type);
                    break;

                case EventType.EmptyRetrieval:
                    PositionReport_EmptyRetrieval(bcfApp, vh, seq_num, recive_str.EventType, recive_str.CurrentAdrID, recive_str.CurrentSecID, carrier_id);
                    break;

                case EventType.CsttypeMismatch:
                    PositionReport_CSTTypeMismatch(bcfApp, vh, seq_num, recive_str.EventType, carrier_id, cst_type);
                    break;

                case EventType.Bcrread:
                    TransferReportBCRRead(bcfApp, vh, seq_num, eventType, carrier_id, bCRReadResult);
                    break;

                case EventType.ReserveReq:
                    replyTranEventReport(bcfApp, recive_str.EventType, vh, seq_num);
                    break;

                case EventType.Initial:
                    TransferReportInitial(bcfApp, vh, seq_num, eventType, carrier_id);
                    if (!DebugParameter.IsSyncWhenConnectionEvent)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: "Initial finish! Begin synchronize with vehicle...",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                        VehicleInfoSynchronize(vh.VEHICLE_ID);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: "Initial finish! End synchronize with vehicle.",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                    break;
            }
        }
        private void PositionReport_BlockRelease(BCFApplication bcfApp, AVEHICLE eqpt, ID_136_TRANS_EVENT_REP recive_str, int seq_num)
        {
            string release_adr = recive_str.ReleaseBlockAdrID;
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Process block release,release address id:{release_adr}",
               VehicleID: eqpt.VEHICLE_ID,
               CarrierID: eqpt.CST_ID);
            doBlockRelease(eqpt, release_adr);
        }

        private (bool hasRelease, ABLOCKZONEMASTER releaseBlockMaster) doBlockRelease(AVEHICLE eqpt, string release_adr)
        {
            ABLOCKZONEMASTER releaseBlockMaster = null;
            bool hasRelease = false;
            try
            {
                hasRelease = tryReleaseBlockZoneByReserveModule(eqpt.VEHICLE_ID, release_adr);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Process block release, release address id:{release_adr}, release result:{hasRelease}",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
                logger.Warn(ex, "Warn");
            }
            return (hasRelease, releaseBlockMaster);
        }
        public bool tryReleaseBlockZoneByReserveModule(string vh_id, string release_adr)
        {
            bool hasRelease = false;
            AVEHICLE vh_vo = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);

            var related_block_masters = scApp.BlockControlBLL.cache.loadBlockZoneMasterByReleaseAddress(release_adr);
            foreach (var block_master in related_block_masters)
            {
                var block_detail_sections = block_master.GetBlockZoneDetailSectionIDs();
                foreach (var detail_sec in block_detail_sections)
                {
                    scApp.ReserveBLL.RemoveManyReservedSectionsByVIDSID(vh_id, detail_sec);
                    hasRelease = true;
                }
                if (DebugParameter.IsOpenTrackResetByVhBlockRelease)
                    Task.Run(() => tryResetTrackBlock(vh_vo, block_master));
            }
            return hasRelease;
        }

        private void tryResetTrackBlock(AVEHICLE eqpt, ABLOCKZONEMASTER block_master)
        {
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"start try to reset track block:{block_master.ENTRY_SEC_ID}...",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
                var related_tracks = block_master.RelatedTracks;
                if (related_tracks == null || related_tracks.Count == 0)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"start try to reset track block:{block_master.ENTRY_SEC_ID}, but not related track.",
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.CST_ID);
                    return;
                }
                List<string> track_ids = related_tracks.Select(t => t.UNIT_ID).ToList();

                foreach (var track in related_tracks)
                {
                    track.ResetBlock(scApp.TrackInfoClient);
                }
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Trace, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"reset track:{string.Join(",", track_ids)}",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
        }

        private const string CST_ID_ERROR_RENAME_SYMBOL = "UNKF";
        private const string CST_ID_ERROR_SYMBOL = "ERR";
        public Logger TransferServiceLogger = NLog.LogManager.GetLogger("TransferServiceLogger");

        private void TransferReportInitial(BCFApplication bcfApp, AVEHICLE eqpt, int seq_num, EventType eventType, string cstID)
        {
            string final_cst_id = cstID;
            try
            {
                TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} current cst ID:{cstID}, 開始 initial 流程...");
                scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(eqpt.OHTC_CMD);

                bool is_cmd_excute = !SCUtility.isEmpty(eqpt.OHTC_CMD);
                bool has_cst_on_vh = !SCUtility.isEmpty(cstID);
                //bool is_bcr_read_fail = cstID != null && cstID.ToUpper().Contains(CST_ID_ERROR_SYMBOL);

                bool is_bcr_read_fail = false;
                if (eqpt.VEHICLE_TYPE == E_VH_TYPE.ReelCST)
                {
                    is_bcr_read_fail = true;
                }
                else
                {
                    is_bcr_read_fail = cstID != null && cstID.ToUpper().Contains(CST_ID_ERROR_SYMBOL);
                }

                if (is_cmd_excute)
                {
                    TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 有命令在執行中，ohtc cmd id:{eqpt.OHTC_CMD},mcs cmd id:{eqpt.MCS_CMD}");

                    ACMD_OHTC cmd_ohtc = scApp.CMDBLL.getCMD_OHTCByID(eqpt.OHTC_CMD);
                    if (cmd_ohtc.IsTransferCmdByMCS)
                    {
                        ACMD_MCS cmd_mcs = scApp.CMDBLL.getCMD_MCSByID(cmd_ohtc.CMD_ID_MCS);
                        CassetteData cmdOHT_CSTdata = scApp.CassetteDataBLL.loadCassetteDataByBoxID(cmd_mcs.BOX_ID);

                        if (has_cst_on_vh)
                        {
                            if (is_bcr_read_fail)
                            {
                                final_cst_id = cmdOHT_CSTdata.BOXID;
                                cmdOHT_CSTdata.Carrier_LOC = eqpt.VEHICLE_ID;
                                scApp.CassetteDataBLL.UpdateCSTLoc(cmdOHT_CSTdata.BOXID, cmdOHT_CSTdata.Carrier_LOC, 1);
                                scApp.CassetteDataBLL.UpdateCSTState(cmdOHT_CSTdata.BOXID, (int)E_CSTState.Installed);
                                scApp.TransferService.ForceFinishMCSCmd(cmd_mcs, cmdOHT_CSTdata, "TransferReportInitial");
                                TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} initial 流程，cst id 讀取失敗，使用原本的cst id回復OHT");
                            }
                            else
                            {
                                CassetteData nowOHT_CSTdata = new CassetteData();
                                //nowOHT_CSTdata.CSTID = "ERROR1";
                                //nowOHT_CSTdata.CSTID = "";
                                nowOHT_CSTdata.BOXID = cstID;
                                nowOHT_CSTdata.Carrier_LOC = eqpt.VEHICLE_ID;
                                final_cst_id = nowOHT_CSTdata.BOXID;
                                if (cmdOHT_CSTdata != null)
                                {
                                    if (!SCUtility.isMatche(cstID, cmdOHT_CSTdata.BOXID))
                                    {
                                        cmdOHT_CSTdata.Carrier_LOC = eqpt.VEHICLE_ID;
                                        scApp.TransferService.ForceDeleteCstAndCmd(cmd_mcs, cmdOHT_CSTdata, "TransferReportInitial", ACMD_MCS.ResultCode.IDmismatch);
                                        scApp.TransferService.OHBC_InsertCassette(nowOHT_CSTdata.BOXID, nowOHT_CSTdata.Carrier_LOC, "TransferReportInitial");
                                        TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} initial 流程，發生mismatch initial cst id:{cstID},命令cst id:{cmdOHT_CSTdata.BOXID}");
                                    }
                                    else
                                    {
                                        cmdOHT_CSTdata.Carrier_LOC = eqpt.VEHICLE_ID;
                                        scApp.CassetteDataBLL.UpdateCSTLoc(cmdOHT_CSTdata.BOXID, cmdOHT_CSTdata.Carrier_LOC, 1);
                                        scApp.CassetteDataBLL.UpdateCSTState(cmdOHT_CSTdata.BOXID, (int)E_CSTState.Installed);
                                        if (cmd_mcs.RelayStation == cmd_ohtc.SOURCE && string.IsNullOrWhiteSpace(cmd_ohtc.SOURCE) == false)
                                        {
                                            scApp.ReportBLL.ReportCarrierResumed(cmd_mcs.CMD_ID);
                                        }
                                        else
                                        {
                                            scApp.ReportBLL.ReportCarrierTransferring(cmd_mcs, cmdOHT_CSTdata, eqpt.VEHICLE_ID);
                                        }
                                        scApp.TransferService.ForceFinishMCSCmd(cmd_mcs, cmdOHT_CSTdata, "TransferReportInitial");
                                        TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} initial 流程，強制將 cst id:{cmdOHT_CSTdata.BOXID}過帳至車上");
                                    }
                                }
                                else
                                {
                                    scApp.TransferService.ForceFinishMCSCmd(cmd_mcs, null, "TransferReportInitial");
                                    scApp.TransferService.OHBC_InsertCassette(nowOHT_CSTdata.BOXID, nowOHT_CSTdata.Carrier_LOC, "TransferReportInitial");
                                    TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} initial 流程，cst id:{cstID} 並無帳料於系統中，進行強制建帳");
                                }
                            }
                        }
                        else
                        {
                            if (cmd_mcs.isLoading)
                            {
                                cmdOHT_CSTdata.Carrier_LOC = cmd_mcs.HOSTSOURCE;
                                scApp.CassetteDataBLL.UpdateCSTLoc(cmdOHT_CSTdata.BOXID, cmdOHT_CSTdata.Carrier_LOC, 1);
                                scApp.CassetteDataBLL.UpdateCSTState(cmdOHT_CSTdata.BOXID, (int)E_CSTState.WaitIn);
                                scApp.TransferService.ForceFinishMCSCmd(cmd_mcs, cmdOHT_CSTdata, "TransferReportInitial");
                                TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} initial 流程，命令在進行Loading中，但cst 不再車上 將帳料強制更新回source port");
                            }
                            else if (cmd_mcs.isUnloading)
                            {
                                cmdOHT_CSTdata.Carrier_LOC = cmd_mcs.HOSTDESTINATION;
                                scApp.CassetteDataBLL.UpdateCSTLoc(cmdOHT_CSTdata.BOXID, cmdOHT_CSTdata.Carrier_LOC, 1);
                                scApp.CassetteDataBLL.UpdateCSTState(cmdOHT_CSTdata.BOXID, (int)E_CSTState.Completed);
                                scApp.TransferService.ForceFinishMCSCmd
                                    (cmd_mcs, cmdOHT_CSTdata, "TransferReportInitial", ACMD_MCS.ResultCode.Successful);

                                scApp.TransferService.UnloadCompleteForInitialScript(cmd_mcs.HOSTDESTINATION, cmdOHT_CSTdata);
                                //scApp.ReportBLL.ReportCarrierRemovedFromPort(cmdOHT_CSTdata, SECSConst.HandoffType_Automated);
                                //scApp.CassetteDataBLL.DeleteCSTbyCstBoxID(cmdOHT_CSTdata.CSTID, cmdOHT_CSTdata.BOXID);

                                TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} initial 流程，命令在進行Unloading中，但cst 不再車上 將帳料強制更新回desc port");
                            }
                            else
                            {
                                scApp.TransferService.ForceFinishMCSCmd(cmd_mcs, cmdOHT_CSTdata, "TransferReportInitial");
                                TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} initial 流程，但cst 不再車上 將命令強制結束");
                            }
                        }
                    }
                    finishOHTCCmd(eqpt, eqpt.OHTC_CMD, eqpt.MCS_CMD, CompleteStatus.CmpStatusVehicleAbort);
                    replyTranEventReport(bcfApp, eventType, eqpt, seq_num,
                        renameCarrierID: final_cst_id);
                }
                else
                {
                    if (has_cst_on_vh)
                    {
                        var carrier_data = scApp.CassetteDataBLL.loadCassetteDataByLoc(eqpt.VEHICLE_ID);
                        if (is_bcr_read_fail)
                        {
                            if (carrier_data != null)
                            {
                                final_cst_id = carrier_data.BOXID;
                                TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 非在執行命令中，身上有CST但CST ID為unknwon:{cstID}，故使用DB中的帳將其rename為:{final_cst_id}");
                            }
                            else
                            {
                                //string new_carrier_id =
                                //  $"UNKF{eqpt.Real_ID.Trim()}{DateTime.Now.ToString(SCAppConstants.TimestampFormat_12)}";
                                string new_carrier_id =
                                  scApp.TransferService.CarrierReadFail(eqpt.VEHICLE_ID, eqpt.Real_ID);
                                final_cst_id = new_carrier_id;
                                scApp.TransferService.OHBC_InsertCassette(new_carrier_id, eqpt.VEHICLE_ID, "TransferReportInitial");
                                TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 非在執行命令中，身上有CST但CST ID為unknwon:{cstID}，故將其rename為unknown id:{final_cst_id}");
                            }
                        }
                        else
                        {
                            if (carrier_data != null)
                            {
                                if (SCUtility.isMatche(carrier_data.BOXID, cstID))
                                {
                                    //not thing...
                                    TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 非在執行命令中，身上有CST但CST ID為:{cstID}，與原本在身上的ID一致，故不進行調整");
                                }
                                else
                                {
                                    final_cst_id = cstID;
                                    scApp.TransferService.OHBC_InsertCassette(final_cst_id, eqpt.VEHICLE_ID, "TransferReportInitial");
                                    TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 非在執行命令中，身上有CST但CST ID為:{cstID}，與資料庫的不一樣db cst id:{carrier_data.BOXID},故將其進行對資料庫的rename");
                                }
                            }
                            else
                            {
                                final_cst_id = cstID;
                                scApp.TransferService.OHBC_InsertCassette(final_cst_id, eqpt.VEHICLE_ID, "TransferReportInitial");
                                TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 非在執行命令中，身上有CST但CST ID為:{cstID}，但db中無該vh的帳料，進行建帳");
                            }
                        }
                    }
                    else
                    {
                        var carrier_data = scApp.CassetteDataBLL.loadCassetteDataByLoc(eqpt.VEHICLE_ID);
                        if (carrier_data != null)
                        {
                            scApp.TransferService.ForceDeleteCst(carrier_data.BOXID, "TransferReportInitial");
                            TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 非在執行命令中，身上無CST，但db中有該vh的帳料:{carrier_data.BOXID}，進行刪除帳料");
                        }
                    }
                    replyTranEventReport(bcfApp, eventType, eqpt, seq_num,
                        renameCarrierID: final_cst_id);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                replyTranEventReport(bcfApp, eventType, eqpt, seq_num,
                    renameCarrierID: final_cst_id);
            }
        }

        private bool finishOHTCCmd(AVEHICLE eqpt, string cmd_id, string cmd_mcs_id, CompleteStatus completeStatus)
        {
            bool isSuccess = true;
            E_CMD_STATUS ohtc_cmd_status = scApp.VehicleBLL.CompleteStatusToCmdStatus(completeStatus);
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    isSuccess &= scApp.VehicleBLL.doTransferCommandFinish(eqpt.VEHICLE_ID, cmd_id, completeStatus);
                    //isSuccess &= scApp.CMDBLL.updateCommand_OHTC_StatusByCmdID(cmd_id, ohtc_cmd_status);
                    isSuccess &= scApp.CMDBLL.updateOHTCCommandToFinishByCmdID(cmd_id, ohtc_cmd_status, completeStatus);
                    isSuccess &= scApp.VIDBLL.initialVIDCommandInfo(eqpt.VEHICLE_ID);

                    if (completeStatus == CompleteStatus.CmpStatusVehicleAbort)
                    {
                        TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 發生vehicle abort 確認是否有命令已經先預下給該vh...");
                        var check_result = scApp.CMDBLL.hasCMD_OHTCInQueue(eqpt.VEHICLE_ID);
                        if (check_result.has)
                        {
                            TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 有預下cmd_ohtc_id:{SCUtility.Trim(check_result.cmd_ohtc.CMD_ID, true)}、cmd_mcs_id:{SCUtility.Trim(check_result.cmd_ohtc.CMD_ID_MCS, true)}，" +
                                                       $"開始進行命令結束...");
                            ACMD_OHTC queue_cmd = check_result.cmd_ohtc;
                            //scApp.CMDBLL.updateCommand_OHTC_StatusByCmdID(queue_cmd.CMD_ID, E_CMD_STATUS.AbnormalEndByOHTC);
                            scApp.CMDBLL.updateOHTCCommandToFinishByCmdID(queue_cmd.CMD_ID, E_CMD_STATUS.AbnormalEndByOHTC, CompleteStatus.CmpStatusCommandInitailFail);
                            if (!SCUtility.isEmpty(queue_cmd.CMD_ID_MCS))
                            {
                                ACMD_MCS pre_initial_cmd_mcs = scApp.CMDBLL.getCMD_MCSByID(queue_cmd.CMD_ID_MCS);
                                if (pre_initial_cmd_mcs != null &&
                                    pre_initial_cmd_mcs.TRANSFERSTATE == E_TRAN_STATUS.Transferring)
                                {
                                    //scApp.CMDBLL.updateCMD_MCS_TranStatus2Queue(pre_initial_cmd_mcs.CMD_ID);
                                    CassetteData cmdOHT_CSTdata = scApp.CassetteDataBLL.loadCassetteDataByBoxID(pre_initial_cmd_mcs.BOX_ID);
                                    scApp.TransferService.ForceFinishMCSCmd(pre_initial_cmd_mcs, cmdOHT_CSTdata, "finishOHTCCmd");
                                    TransferServiceLogger.Info($"vh id:{eqpt.VEHICLE_ID} 命令結束cmd_ohtc_id:{SCUtility.Trim(check_result.cmd_ohtc.CMD_ID, true)}、cmd_mcs_id:{SCUtility.Trim(check_result.cmd_ohtc.CMD_ID_MCS, true)}，" +
                                                               $"結束命令完成。");
                                }
                            }
                        }
                    }

                    if (isSuccess)
                    {
                        tx.Complete();
                    }
                    else
                    {
                        //return;
                    }
                }
            }
            try
            {
                if (!SCUtility.isEmpty(cmd_mcs_id))
                {
                    scApp.SysExcuteQualityBLL.updateSysExecQity_CmdFinish(cmd_mcs_id, ohtc_cmd_status, completeStatus, out var quality);
                    if (quality != null)
                    {
                        SCUtility.TrimAllParameter(quality);
                        LogManager.GetLogger("SysExcuteQuality").Info(quality.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }


            return isSuccess;
        }

        private object reserve_lock = new object();


        private enum CAN_NOT_AVOID_RESULT
        {
            Normal
        }

        private (bool is_can, CAN_NOT_AVOID_RESULT result) canCreatDriveOutCommand(AVEHICLE reservedVh)
        {
            bool is_can = reservedVh.isTcpIpConnect &&
                          !reservedVh.IsError &&
                          (reservedVh.MODE_STATUS == VHModeStatus.AutoRemote ||
                           reservedVh.MODE_STATUS == VHModeStatus.AutoLocal) &&
                           reservedVh.ACT_STATUS == VHActionStatus.NoCommand &&
                           !scApp.CMDBLL.isCMD_OHTCExcuteByVh(reservedVh.VEHICLE_ID);
            //如果可以進行趕車，最後需再確認該車子是否停在CV上，且是不是需要等待BOX出來
            //if (is_can && scApp.TransferService.isNeedWatingBoxComeIn(reservedVh.CUR_ADR_ID))
            //{
            //    is_can = false;
            //}
            return (is_can, CAN_NOT_AVOID_RESULT.Normal);
        }

        private (bool isSuccess, string reservedVhID, string reservedSecID) IsReserveSuccessNew(string vhID, RepeatedField<ReserveInfo> reserveInfos, bool isAsk = false)
        {
            try
            {
                if (DebugParameter.isForcedPassReserve)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: "test flag: Force pass reserve is open, will driect reply to vh pass",
                       VehicleID: vhID);
                    return (true, string.Empty, string.Empty);
                }

                //強制拒絕Reserve的要求
                if (DebugParameter.isForcedRejectReserve)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: "test flag: Force reject reserve is open, will driect reply to vh can't pass",
                       VehicleID: vhID);
                    return (false, string.Empty, string.Empty);
                }
                AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vhID);
                if (vh.IsPrepareAvoid)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vhID} is prepare excute avoid action, will reject reserve request.",
                       VehicleID: vhID);
                    return (false, string.Empty, string.Empty);
                }
                if (reserveInfos == null || reserveInfos.Count == 0) return (false, string.Empty, string.Empty);
                string reserve_section_id = reserveInfos[0].ReserveSectionID;

                Mirle.Hlts.Utils.HltDirection hltDirection = Mirle.Hlts.Utils.HltDirection.Forward;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vhID} Try add reserve section:{reserve_section_id} ,hlt dir:{hltDirection}...",
                   VehicleID: vhID);
                var result = scApp.ReserveBLL.TryAddReservedSection(vhID, reserve_section_id,
                                                                    sensorDir: hltDirection,
                                                                    isAsk: isAsk);

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"vh:{vhID} Try add reserve section:{reserve_section_id},result:{result.ToString()}",
                   VehicleID: vhID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"current reserve section:{scApp.ReserveBLL.GetCurrentReserveSection()}",
                   VehicleID: vhID);
                return (result.OK, result.VehicleID, reserve_section_id);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   Details: $"process function:{nameof(IsReserveSuccessNew)} Exception");
                return (false, string.Empty, string.Empty);
            }
        }

        public void ReserveTest(string vhID, string secID)
        {
            RepeatedField<ReserveInfo> reserveInfos = new RepeatedField<ReserveInfo>()
            {
                new ReserveInfo()
                {
                     DriveDirction =   DriveDirction.DriveDirForward,
                      ReserveSectionID = secID
                },
                new ReserveInfo()
                {
                     DriveDirction =   DriveDirction.DriveDirForward,
                      ReserveSectionID = "00126"
                }
            };
            IsMultiReserveSuccess(vhID, reserveInfos);
        }

        private (bool isSuccess, string reservedVhID, RepeatedField<ReserveInfo> reserveSuccessInfos) IsMultiReserveSuccess
            (string vhID, RepeatedField<ReserveInfo> reserveInfos, bool isAsk = false)
        {
            try
            {
                if (DebugParameter.isForcedPassReserve)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: "test flag: Force pass reserve is open, will driect reply to vh pass",
                       VehicleID: vhID);
                    return (true, string.Empty, reserveInfos);
                }

                //強制拒絕Reserve的要求
                if (DebugParameter.isForcedRejectReserve)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: "test flag: Force reject reserve is open, will driect reply to vh can't pass",
                       VehicleID: vhID);
                    return (false, string.Empty, null);
                }
                AVEHICLE vh = scApp.getEQObjCacheManager().getVehicletByVHID(vhID);
                if (vh.IsPrepareAvoid)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vhID} is prepare excute avoid action, will reject reserve request.",
                       VehicleID: vhID);
                    return (false, string.Empty, null);
                }
                if (reserveInfos == null || reserveInfos.Count == 0) return (false, string.Empty, null);

                var reserve_success_section = new RepeatedField<ReserveInfo>();
                bool has_success = false;
                string final_blocked_vh_id = string.Empty;
                Mirle.Hlts.Utils.HltResult result = default(Mirle.Hlts.Utils.HltResult);
                foreach (var reserve_info in reserveInfos)
                {
                    string reserve_section_id = reserve_info.ReserveSectionID;
                    //if (SCUtility.isMatche(reserve_section_id, vh.CUR_SEC_ID))
                    //{
                    //    result = new Mirle.Hlts.Utils.HltResult(true, "");
                    //}
                    //else
                    {
                        if (scApp.SectionBLL.cache.IsNeedReserveChcek(reserve_section_id))
                        {
                            Mirle.Hlts.Utils.HltDirection hltDirection = Mirle.Hlts.Utils.HltDirection.Forward;
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"vh:{vhID} Try add reserve section:{reserve_section_id} ,hlt dir:{hltDirection}...",
                               VehicleID: vhID);
                            result = scApp.ReserveBLL.TryAddReservedSection(vhID, reserve_section_id,
                                                                                sensorDir: hltDirection,
                                                                                isAsk: isAsk);

                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"vh:{vhID} Try add reserve section:{reserve_section_id},result:{result.ToString()}",
                               VehicleID: vhID);
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"current reserve section:{scApp.ReserveBLL.GetCurrentReserveSection()}",
                               VehicleID: vhID);
                            //如果預約不到的時候，確認目前要的Section是否為CurrentAdr的上一段Section
                            //式的話就代表已經走過了，就給他直接通過
                            if (!result.OK)
                            {
                                string current_adr = vh.CUR_ADR_ID;
                                var last_sections = scApp.SectionBLL.cache.GetSectionsByToAddress(current_adr);
                                if (last_sections.Count > 0)
                                {
                                    ASECTION last_sec = last_sections[0];
                                    if (SCUtility.isMatche(reserve_section_id, last_sec.SEC_ID))
                                    {
                                        result.OK = true;
                                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                                           Data: $"Froce pass reserve section:{reserve_section_id},becuse it is last section of adr:{current_adr}",
                                           VehicleID: vhID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Mirle.Hlts.Utils.HltDirection hltDirection = Mirle.Hlts.Utils.HltDirection.Forward;
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"vh:{vhID} Try add(Only ask) reserve section:{reserve_section_id} ,hlt dir:{hltDirection}...",
                               VehicleID: vhID);
                            result = scApp.ReserveBLL.TryAddReservedSection(vhID, reserve_section_id,
                                                                                sensorDir: hltDirection,
                                                                                isAsk: true);
                            //result = new Mirle.Hlts.Utils.HltResult(true, "");
                        }
                    }
                    if (result.OK)
                    {
                        reserve_success_section.Add(reserve_info);
                        has_success |= true;
                    }
                    else
                    {
                        has_success |= false;
                        final_blocked_vh_id = result.VehicleID;
                        break;
                    }
                }

                return (has_success, final_blocked_vh_id, reserve_success_section);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   Details: $"process function:{nameof(IsMultiReserveSuccess)} Exception");
                return (false, string.Empty, null);
            }
        }

        private void TransferReportBCRRead(BCFApplication bcfApp, AVEHICLE eqpt, int seqNum,
                                             EventType eventType, string read_carrier_id, BCRReadResult bCRReadResult)
        {
            AVIDINFO vid_info = scApp.VIDBLL.getVIDInfo(eqpt.VEHICLE_ID);
            string old_carrier_id = SCUtility.Trim(vid_info.CARRIER_ID, true);

            string rename_carrier_id = string.Empty;
            CMDCancelType cancel_type;
            switch (bCRReadResult)
            {
                case BCRReadResult.BcrMisMatch:
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"BCR miss match happend,start abort command id:{eqpt.OHTC_CMD?.Trim()} and rename cst id:{old_carrier_id}...",
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.BOX_ID);

                    var missmatch_process_result = scApp.TransferService.IDReadMismatchHappend(eqpt.VEHICLE_ID, read_carrier_id);
                    rename_carrier_id = missmatch_process_result.RemaneBox;
                    cancel_type = missmatch_process_result.isContinue ?
                        CMDCancelType.CmdNone : CMDCancelType.CmdCancelIdMismatch;
                    replyTranEventReport(bcfApp, eventType, eqpt, seqNum,
                                         renameCarrierID: rename_carrier_id,
                                         cancelType: cancel_type);

                    //-----Read fail 準備被取代的部分 Start-----
                    //if (!checkHasDuplicateHappend(bcfApp, eqpt, seqNum, eventType, read_carrier_id, old_carrier_id))
                    //{
                    //scApp.VehicleBLL.updataVehicleBOXID(eqpt.VEHICLE_ID, read_carrier_id);
                    //if (scApp.CMDBLL.getCMD_OHTCByID(eqpt.OHTC_CMD).CMD_TPYE == E_CMD_TYPE.Scan)
                    //{
                    //    replyTranEventReport(bcfApp, eventType, eqpt, seqNum,
                    //    renameCarrierID: read_carrier_id,
                    //    cancelType: CMDCancelType.CmdNone);
                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    //       Data: $"BCR miss match happend,but in Scan command id:{eqpt.OHTC_CMD?.Trim()} and rename cst id:{old_carrier_id} to {read_carrier_id}",
                    //       VehicleID: eqpt.VEHICLE_ID,
                    //       CarrierID: eqpt.BOX_ID);
                    //}
                    //else
                    //{

                    //    replyTranEventReport(bcfApp, eventType, eqpt, seqNum,
                    //    renameCarrierID: read_carrier_id,
                    //    cancelType: CMDCancelType.CmdCancelIdMismatch);

                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    //       Data: $"BCR miss match happend,start abort command id:{eqpt.OHTC_CMD?.Trim()} and rename cst id:{old_carrier_id} to {read_carrier_id}",
                    //       VehicleID: eqpt.VEHICLE_ID,
                    //       CarrierID: eqpt.BOX_ID);
                    //}
                    //}
                    // Task.Run(() => doAbortCommand(eqpt, eqpt.OHTC_CMD, CMDCancelType.CmdCancelIdMismatch));
                    //-----Read fail 準備被取代的部分 End-----
                    //20200130 Hsinyu Chang
                    scApp.CMDBLL.updateCMD_MCS_BCROnCrane(eqpt.MCS_CMD, read_carrier_id);
                    break;

                case BCRReadResult.BcrReadFail:
                    //todo ->想改成的模式
                    var readfail_process_result = scApp.TransferService.IDReadFailHappend(eqpt.VEHICLE_ID, read_carrier_id);
                    rename_carrier_id = readfail_process_result.RemaneBox;
                    cancel_type = readfail_process_result.isContinue ?
                        CMDCancelType.CmdNone : CMDCancelType.CmdCancelIdReadFailed;
                    replyTranEventReport(bcfApp, eventType, eqpt, seqNum,
                                         renameCarrierID: rename_carrier_id,
                                         cancelType: cancel_type);

                    //-----Read fail 準備被取代的部分 Start-----
                    //string new_carrier_id = "";
                    //CMDCancelType cancelType = CMDCancelType.CmdNone;
                    //if (SystemParameter.IsEnableIDReadFailScenario)
                    //{
                    //    ALINE line = scApp.getEQObjCacheManager().getLine();
                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    //       Data: $"BCR read fail happend,start abort command id:{eqpt.OHTC_CMD?.Trim()} and rename BOX id...",
                    //       VehicleID: eqpt.VEHICLE_ID,
                    //       CarrierID: eqpt.BOX_ID);
                    //    //string old_carrier_id = SCUtility.Trim(vid_info.CARRIER_ID, true);
                    //    bool is_unknow_old_name_cst = SCUtility.isEmpty(old_carrier_id);
                    //    //string new_carrier_id = string.Empty;
                    //    if (is_unknow_old_name_cst)
                    //    {
                    //        new_carrier_id = "ERROR1";
                    //        scApp.VIDBLL.upDateVIDCarrierID(eqpt.VEHICLE_ID, new_carrier_id);
                    //    }
                    //    else
                    //    {
                    //        // Rename the cmd boxID to the readfail ID for the MCS to rename the CST when it pass the OHCV.
                    //        new_carrier_id = eqpt.BOX_ID;
                    //    }
                    //    scApp.VehicleBLL.updataVehicleBOXID(eqpt.VEHICLE_ID, new_carrier_id);
                    //    if (scApp.CMDBLL.getCMD_OHTCByID(eqpt.OHTC_CMD).CMD_TPYE == E_CMD_TYPE.Scan)
                    //    {
                    //        cancelType = CMDCancelType.CmdNone;
                    //    }
                    //    else
                    //    {
                    //        cancelType = CMDCancelType.CmdNone;
                    //    }
                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    //       Data: $"BCR read fail happend,start abort command id:{eqpt.OHTC_CMD?.Trim()} and rename cst id:{old_carrier_id} to {new_carrier_id} ",
                    //       VehicleID: eqpt.VEHICLE_ID,
                    //       CarrierID: eqpt.BOX_ID);
                    //}
                    //else
                    //{
                    //    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    //       Data: $"BCR read fail happend,continue excute command.",
                    //       VehicleID: eqpt.VEHICLE_ID,
                    //       CarrierID: eqpt.BOX_ID);
                    //    cancelType = CMDCancelType.CmdCancelIdReadFailed; // None => CmdCancelIdReadFailed for readFail reply to OHT.
                    //    if (scApp.CMDBLL.getCMD_OHTCByID(eqpt.OHTC_CMD).CMD_TPYE == E_CMD_TYPE.Scan)
                    //    {
                    //        cancelType = CMDCancelType.CmdNone;
                    //    }
                    //    else
                    //    {
                    //        cancelType = CMDCancelType.CmdNone;
                    //    }
                    //    if (!SCUtility.isEmpty(eqpt.MCS_CMD))
                    //    {
                    //        ACMD_MCS mcs_cmd = scApp.CMDBLL.getCMD_MCSByID(eqpt.MCS_CMD);
                    //        if (mcs_cmd != null)
                    //        {
                    //            new_carrier_id = SCUtility.Trim(mcs_cmd.CARRIER_ID);
                    //        }
                    //        else
                    //        {
                    //            new_carrier_id = "";
                    //        }
                    //    }
                    //    else
                    //    {
                    //        new_carrier_id = "";
                    //    }
                    //}

                    //replyTranEventReport(bcfApp, eventType, eqpt, seqNum,
                    //    renameCarrierID: new_carrier_id,
                    //    cancelType: cancelType);
                    //-----Read fail 準備被取代的部分 End-----

                    scApp.TransferService.OHBC_AlarmSet(eqpt.VEHICLE_ID, ((int)AlarmLst.OHT_BCR_READ_FAIL).ToString());
                    scApp.TransferService.OHBC_AlarmCleared(eqpt.VEHICLE_ID, ((int)AlarmLst.OHT_BCR_READ_FAIL).ToString());
                    //
                    //20200130 Hsinyu Chang
                    scApp.CMDBLL.updateCMD_MCS_BCROnCrane(eqpt.MCS_CMD, rename_carrier_id);
                    break;

                case BCRReadResult.BcrNormal:
                    if (!checkHasDuplicateHappend(bcfApp, eqpt, seqNum, eventType, read_carrier_id, old_carrier_id))
                    {
                        scApp.VehicleBLL.updataVehicleBOXID(eqpt.VEHICLE_ID, read_carrier_id);
                        replyTranEventReport(bcfApp, eventType, eqpt, seqNum);
                        //20200130 Hsinyu Chang
                        scApp.CMDBLL.updateCMD_MCS_BCROnCrane(eqpt.MCS_CMD, read_carrier_id);
                    }
                    break;
            }

            scApp.TransferService.OHT_IDRead(eqpt.MCS_CMD, eqpt.VEHICLE_ID, read_carrier_id, bCRReadResult);
        }

        private bool checkHasDuplicateHappend(BCFApplication bcfApp, AVEHICLE eqpt, int seqNum, EventType eventType, string read_carrier_id, string oldCarrierID)
        {
            bool is_happend = false;
            //AVEHICLE vh = scApp.VehicleBLL.cache.getVhByCSTID(read_carrier_id);
            int has_carry_this_cst_of_vh = scApp.VehicleBLL.cache.getVhByHasCSTIDCount(read_carrier_id);
            if (DebugParameter.TestDuplicate || has_carry_this_cst_of_vh >= 2)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $" Carrier duplicate happend,start abort command id:{eqpt.OHTC_CMD?.Trim()},and check is need rename cst id:{oldCarrierID}...",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
                bool was_renamed = oldCarrierID.StartsWith("UNKNOWNDUP");

                string rename_duplicate_carrier_id = was_renamed ?
                    oldCarrierID :
                    $"UNKNOWNDUP-{read_carrier_id}-{DateTime.Now.ToString(SCAppConstants.TimestampFormat_12)}001";//固定加入001的Sequence
                scApp.VehicleBLL.updataVehicleCSTID(eqpt.VEHICLE_ID, rename_duplicate_carrier_id);
                replyTranEventReport(bcfApp, eventType, eqpt, seqNum,
                            renameCarrierID: rename_duplicate_carrier_id, cancelType: CMDCancelType.CmdCancelIdReadDuplicate);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $" Carrier duplicate happend,start abort command id:{eqpt.OHTC_CMD?.Trim()},and check is need rename cst id:{oldCarrierID} to {rename_duplicate_carrier_id}",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);

                is_happend = true;
            }
            return is_happend;
        }

        private void ProcessBlockOrHIDReq(BCFApplication bcfApp, AVEHICLE eqpt, EventType eventType, int seqNum, string req_block_id, string req_hid_secid)
        {
            if (eventType == EventType.BlockReq || eventType == EventType.BlockHidreq)
            {
                var workItem = new com.mirle.ibg3k0.bcf.Data.BackgroundWorkItem(bcfApp, eqpt, eventType, seqNum, req_block_id, req_hid_secid);//A0.05
                scApp.BackgroundWorkBlockQueue.triggerBackgroundWork("BlockQueue", workItem);//A0.05
                return;//A0.05
            }
        }

        public (bool isSuccess, TrackDir TrackDir) ProcessBlockReqByReserveModule(BCFApplication bcfApp, AVEHICLE eqpt, string req_block_id)
        {
            string vhID = eqpt.VEHICLE_ID;
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Process block request,request block id:{req_block_id}",
               VehicleID: eqpt.VEHICLE_ID,
               CarrierID: eqpt.CST_ID);
            if (DebugParameter.isForcedPassBlockControl)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: "test flag: Force pass block control is open, will driect reply to vh can pass block",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
                return (true, TrackDir.None);
            }
            else if (DebugParameter.isForcedRejectBlockControl)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: "test flag: Force reject block control is open, will driect reply to vh can't pass block",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
                return (false, TrackDir.None);
            }
            else
            {
                var block_master = scApp.BlockControlBLL.cache.getBlockZoneMaster(req_block_id);
                if (block_master == null)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Vh:{eqpt.VEHICLE_ID} ask block:{req_block_id},but this block id not exist!",
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.CST_ID);
                    return (false, TrackDir.None);
                }
                var block_detail_section = block_master.GetBlockZoneDetailSectionIDs();
                if (block_detail_section == null || block_detail_section.Count == 0)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Vh:{eqpt.VEHICLE_ID} ask block:{req_block_id},but this block id of detail is null!",
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.CST_ID);
                    return (false, TrackDir.None);
                }


                bool is_first_vh = isNextPassVh(eqpt, req_block_id);
                if (!is_first_vh)
                {
                    return (false, TrackDir.None);
                }
                foreach (var detail in block_detail_section)
                {
                    HltDirection hltDirection = HltDirection.None;
                    if (SCUtility.isMatche(detail, "30101"))
                    {
                        hltDirection = HltDirection.Forward;
                    }
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vhID} Try add reserve section:{detail} ,hlt dir:{hltDirection}...",
                       VehicleID: vhID);
                    var result = scApp.ReserveBLL.TryAddReservedSection(vhID, detail,
                                                                         sensorDir: hltDirection,
                                                                         isAsk: true);

                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vhID} Try add reserve section:{detail},result:{result}",
                       VehicleID: vhID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"current reserve section:{scApp.ReserveBLL.GetCurrentReserveSection()}",
                       VehicleID: vhID);
                    if (!result.OK)
                    {
                        if (!SCUtility.isEmpty(result.VehicleID))
                        {
                            //Task.Run(() => scApp.VehicleBLL.whenVhObstacle(result.VehicleID, vhID));
                            Task.Run(() => tryDriveOutTheVh(vhID, result.VehicleID));
                        }
                        return (false, TrackDir.None);
                    }
                }

                //Block Request的From adr來重新計算路徑
                //判斷是否需要進行命令改派，若算出來後不包含在原本行走路徑
                //代表路徑有變化了就暫時不要給該block的通行權然後去下達cancel
                //結束後再讓他把命令改回queue(尚未載到貨)、重新再派改該台車(已有載到貨)
                bool is_need_change_route = checkGuideSectionHasChange(eqpt, block_master.RealEntrySectionID);
                if (is_need_change_route)
                {
                    bool is_interrupt_success = StartProcessCommandInterruptByChangeGuideSection(eqpt);
                    if (is_interrupt_success)
                    {
                        return (false, TrackDir.None);
                    }
                }

                //bool is_block_ready = block_master.IsAllTrackBlockReady();
                var block_tracks_status_check_result = block_master.IsAllTrackBlockReady();
                if (block_tracks_status_check_result.BlockTracksStatus != ABLOCKZONEMASTER.BlockTracksStatus.Ready)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Vh:{eqpt.VEHICLE_ID} ask block:{req_block_id}," +
                             $"but related track block not ready,status:{block_tracks_status_check_result.BlockTracksStatus}.",
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.CST_ID);
                    if (block_tracks_status_check_result.BlockTracksStatus == ABLOCKZONEMASTER.BlockTracksStatus.Blocking)
                    {
                        var release_blocking_track = block_tracks_status_check_result.notReadyTrack;
                        if (release_blocking_track.canExcuteBlockRelease)
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"track:{release_blocking_track.UNIT_ID} is blocking,try to reset it",
                               VehicleID: eqpt.VEHICLE_ID,
                               CarrierID: eqpt.CST_ID);

                            release_blocking_track.ResetBlock(scApp.TrackInfoClient);
                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"track:{release_blocking_track.UNIT_ID} is blocking,but ask block reset interval less then {Track.MIN_ALLOW_BLOCK_RELEASE_INTERVAL_ms} ms",
                               VehicleID: eqpt.VEHICLE_ID,
                               CarrierID: eqpt.CST_ID);
                        }
                    }
                    return (false, TrackDir.None);
                }



                foreach (var detail in block_detail_section)
                {
                    HltDirection hltDirection = HltDirection.None;
                    if (SCUtility.isMatche(detail, "30101"))
                    {
                        hltDirection = HltDirection.Forward;
                    }

                    var result = scApp.ReserveBLL.TryAddReservedSection(vhID, detail,
                                                                        sensorDir: hltDirection,
                                                                        isAsk: false);
                }

                TrackDir track_dir = TrackDir.None;
                if (DebugParameter.IsForceNonStraightPass)
                {
                    track_dir = TrackDir.None;
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"test flag: Force non straight is open, will driect reply to vh TrackDir:{track_dir}",
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.CST_ID);
                }
                else if (DebugParameter.IsForceStraightPass)
                {
                    track_dir = TrackDir.Straight;
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"test flag: Force straight is open, will driect reply to vh TrackDir:{track_dir}",
                       VehicleID: eqpt.VEHICLE_ID,
                       CarrierID: eqpt.CST_ID);
                }
                else
                {
                    bool is_all_track_ready_straight = block_master.IsAllTrackReadyStraight();
                    track_dir = is_all_track_ready_straight ? TrackDir.Straight : TrackDir.None;
                }
                return (true, track_dir);
            }
        }

        private bool StartProcessCommandInterruptByChangeGuideSection(AVEHICLE eqpt)
        {
            string excute_ohtc_cmd_id = eqpt.OHTC_CMD;
            ACMD_OHTC cmd_ohtc = scApp.CMDBLL.getCMD_OHTCByID(excute_ohtc_cmd_id);
            if (cmd_ohtc == null)
                return false;
            var getResult = getCMDCancelType(eqpt, cmd_ohtc);
            if (getResult.isSuccess)
            {
                if (cmd_ohtc.IsTransferCmdByMCS)
                {
                    string cmd_mcs_pause_flag = scApp.CMDBLL.GetCmdMCSPauseFlag(cmd_ohtc.CMD_ID_MCS);
                    if (SCUtility.isMatche(cmd_mcs_pause_flag, SCAppConstants.YES_FLAG))
                    {
                        return true;
                    }
                    using (TransactionScope tx = SCUtility.getTransactionScope())
                    {
                        using (DBConnection_EF con = DBConnection_EF.GetUContext())
                        {
                            scApp.CMDBLL.updateCMD_MCS_PauseFlag(cmd_ohtc.CMD_ID_MCS, SCAppConstants.YES_FLAG);
                            bool is_success = doAbortCommand(eqpt, excute_ohtc_cmd_id, getResult.cancelType); //A0.01

                            if (is_success)
                            {
                                tx.Complete();
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    doAbortCommand(eqpt, excute_ohtc_cmd_id, getResult.cancelType); //A0.01
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        private (bool isSuccess, CMDCancelType cancelType) getCMDCancelType(AVEHICLE vh, ACMD_OHTC cmd_ohtc)
        {
            if (cmd_ohtc == null)
                return (false, default(CMDCancelType));
            switch (cmd_ohtc.CMD_TPYE)
            {
                case E_CMD_TYPE.Unload:
                    return (true, CMDCancelType.CmdAbort);
                case E_CMD_TYPE.LoadUnload:
                    //if (vh.HAS_BOX == 1)
                    if (vh.HAS_CST == 1)
                    {
                        return (true, CMDCancelType.CmdAbort);
                    }
                    else
                    {
                        return (true, CMDCancelType.CmdCancel);
                    }
                default:
                    return (true, CMDCancelType.CmdCancel);
            }
        }
        public void checkGuideSectionHasChangeTest(AVEHICLE vh, string requsetSecID)
        {
            checkGuideSectionHasChange(vh, requsetSecID);
        }

        private bool checkGuideSectionHasChange(AVEHICLE vh, string requsetSecID)
        {
            if (!DebugParameter.IsOpneChangeGuideSection)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"change guide section funcion is close.",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }


            ASECTION req_sec = scApp.SectionBLL.cache.GetSection(requsetSecID);
            if (req_sec == null) return false;
            List<string> original_pass_section_ids = vh.WillPassSectionID;
            if (original_pass_section_ids == null || original_pass_section_ids.Count == 0)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Want to check guide section has change,but wiil pass section is null.",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }
            string will_pass_final_sec_id = original_pass_section_ids.Last();
            ASECTION will_pass_final_sec = scApp.SectionBLL.cache.GetSection(will_pass_final_sec_id);
            if (will_pass_final_sec == null)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Want to check guide section has change,but final section:{will_pass_final_sec_id} not exist.",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }
            string req_sec_form_adr = req_sec.FROM_ADR_ID;
            string target_adr = will_pass_final_sec.TO_ADR_ID;
            var guide_info = scApp.GuideBLL.getGuideInfo(req_sec_form_adr, target_adr);
            if (guide_info.isSuccess)
            {
                List<string> new_guide_section_ids = guide_info.guideSectionIds;
                List<string> exceptResult = new_guide_section_ids.Except(original_pass_section_ids).ToList();
                if (exceptResult.Count == 0)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Want to check guide section has change,result:[No change].",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    return false;
                }
                else
                {
                    string s_new_guide_section = string.Join(",", new_guide_section_ids);
                    string s_original_guide_section = string.Join(",", original_pass_section_ids);
                    string s_except_guide_section = string.Join(",", original_pass_section_ids);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Want to check guide section has change,result:[Is diff]." +
                             $"new:{s_new_guide_section},original:{s_original_guide_section},except:{s_except_guide_section}",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    return true;
                }
            }
            else
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Want to check guide section has change,result:[No guide section to go].",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }
        }

        private bool isNextPassVh(AVEHICLE vh, string currentRequestBlockID)
        {
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"Start check is next pass vh,block id:{currentRequestBlockID}...",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                bool is_next_pass_vh = false;
                //ABLOCKZONEMASTER request_block_master = scApp.BlockControlBLL.cache.getBlockZoneMaster(currentRequestBlockID);
                //先判斷是不是最接近該Block的第一台車，不然會有後車先要到該Block的問題
                //  a.要先判斷在同一段Section是否有其他車輛且的他的距離在前面
                //  b.判斷是否自己已經是在該Block的前一段Section上，如果是則即為該Block的第一台Vh
                //  c.如果不是在前一段Section，則需要去找出從vh目前所在位置到該Block的Entry section中，
                //    是否有其他車輛在
                is_next_pass_vh = IsClosestBlockOfVh(vh, currentRequestBlockID);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"End check is closest block vh,result:{is_next_pass_vh}",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return is_next_pass_vh;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                return false;
            }
        }

        private bool IsClosestBlockOfVh(AVEHICLE vh, string blockSecID)
        {
            string vh_current_section_id = SCUtility.Trim(vh.CUR_SEC_ID, true);
            string block_entry_section_id = SCUtility.Trim(blockSecID, true);
            if (block_entry_section_id.Length > 5)
                block_entry_section_id = block_entry_section_id.Substring(0, 5);
            ASECTION block_entry_section = scApp.SectionBLL.cache.GetSection(block_entry_section_id);

            if (!vh.IsOnAdr)
            {
                ASECTION vh_current_sec = scApp.SectionBLL.cache.GetSection(vh_current_section_id);
                //確認是否有車子是在to Address的位置上
                var on_to_adr_vh = scApp.VehicleBLL.cache.getVhByAddressID(vh_current_sec.TO_ADR_ID);
                if (on_to_adr_vh != null &&
                    on_to_adr_vh != vh)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Has vh:{on_to_adr_vh.VEHICLE_ID} on section:{vh_current_section_id} of to adr:{vh_current_sec.TO_ADR_ID}," +
                             $"so request vh:{vh.VEHICLE_ID} it not closest block vh",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    return false;
                }
                //a.要先判斷在同一段Section是否有其他車輛且的他的距離在前面
                var on_same_section_of_vhs = scApp.VehicleBLL.cache.loadVhsBySectionID(vh_current_section_id);
                foreach (AVEHICLE same_section_vh in on_same_section_of_vhs)
                {
                    if (same_section_vh == vh) continue;
                    if (same_section_vh.ACC_SEC_DIST > vh.ACC_SEC_DIST)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"Has vh:{same_section_vh.VEHICLE_ID} in same section:{vh_current_section_id} and infront of the request vh:{vh.VEHICLE_ID}," +
                                 $"request vh distance:{vh.ACC_SEC_DIST} orther vh distance:{same_section_vh.ACC_SEC_DIST},so request vh:{vh.VEHICLE_ID} it not closest block vh",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                        return false;
                    }
                }

                //b-0.經過"a"的判斷後，如果自己已經是在該Block裡面，則代表該vh已經是最接近這個Block的車子了 //A0.06
                bool is_already_in_req_block = SCUtility.isMatche(vh_current_section_id, block_entry_section_id);
                if (is_already_in_req_block)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vh.VEHICLE_ID} is already in req block,it is closest block:{block_entry_section_id}",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    return true;
                }
                //b-1.經過"a"的判斷後，如果自己已經是在該Block的前一段Section上，則即為該Block的下一台將要通過的Vh
                List<string> entry_section_of_previous_section_id =
                scApp.SectionBLL.cache.GetSectionsByToAddress(block_entry_section.FROM_ADR_ID).
                Select(section => SCUtility.Trim(section.SEC_ID)).
                ToList();
                if (entry_section_of_previous_section_id.Contains(vh_current_section_id))
                {
                    return true;
                }
            }
            else
            {
                if (SCUtility.isMatche(block_entry_section.FROM_ADR_ID, vh.CUR_ADR_ID))
                {
                    //如果已經在block 的from address
                    //代表已經在第一台車
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vh.VEHICLE_ID} is already in req block of from adr:{vh.CUR_ADR_ID},it is closest block:{block_entry_section_id}",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    return true;
                }
            }

            //  c.如果不是在前一段Section，則需要去找出從vh目前所在位置到該Block的Entry section中，
            //    將經過的Vh，是否有其他車輛在
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"vh:{vh.VEHICLE_ID} start check it is closest block id:{block_entry_section_id} of vh...",
               VehicleID: vh.VEHICLE_ID,
               CarrierID: vh.CST_ID);
            bool is_Closest = checkIsFirstVh(vh, block_entry_section_id);
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"vh:{vh.VEHICLE_ID} start check it is closest block id:{block_entry_section_id} , check result:{is_Closest}",
               VehicleID: vh.VEHICLE_ID,
               CarrierID: vh.CST_ID);

            return is_Closest;
        }

        private bool checkIsFirstVh(AVEHICLE vh, string reqBlockId)
        {
            string vh_id = vh.VEHICLE_ID;
            string current_sec_id = SCUtility.Trim(vh.CUR_SEC_ID);
            string start_find_adr = "";
            if (vh.IsOnAdr)
            {
                start_find_adr = vh.CUR_ADR_ID;
            }
            else
            {
                ASECTION vh_current_sec = scApp.SectionBLL.cache.GetSection(current_sec_id);
                start_find_adr = vh_current_sec.TO_ADR_ID;
            }
            ASECTION req_block_sec = scApp.SectionBLL.cache.GetSection(reqBlockId);
            var guide_info =
            scApp.GuideBLL.getGuideInfo(start_find_adr, req_block_sec.FROM_ADR_ID);
            foreach (string sec in guide_info.guideSectionIds)
            {
                var result = scApp.ReserveBLL.TryAddReservedSection(vh_id, sec,
                                              sensorDir: HltDirection.Forward,
                                              isAsk: true);
                if (!result.OK)
                {
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"vh:{vh_id} Try ask reserve section:{sec},result:{result}",
                       VehicleID: vh_id);
                    return false;
                }
            }
            return true;
        }

        private void PositionReport_ArriveAndComplete(BCFApplication bcfApp, AVEHICLE eqpt, int seqNum
                                                    , EventType eventType, string current_adr_id, string current_sec_id, string carrier_id
                                                    , string load_port_id, string unload_port_id) //B0.01
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               Data: $"Process report {eventType}",
               VehicleID: eqpt.VEHICLE_ID,
               CarrierID: eqpt.CST_ID);

            switch (eventType)
            {
                case EventType.LoadArrivals:
                    if (!SCUtility.isEmpty(eqpt.MCS_CMD))
                    {
                        scApp.CMDBLL.updateCMD_MCS_CmdStatus2LoadArrivals(eqpt.MCS_CMD);
                    }
                    scApp.CMDBLL.setWillPassSectionInfo(eqpt.VEHICLE_ID, eqpt.PredictSectionsToDesination);
                    scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(eqpt.VEHICLE_ID);
                    //scApp.ReserveBLL.TryAddReservedSection(eqpt.VEHICLE_ID, eqpt.CUR_SEC_ID);
                    break;

                case EventType.UnloadArrivals:
                    if (eqpt.IsUnloadArriveByPassReply)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"vh:{eqpt.VEHICLE_ID} unlaod arrive reply by pass, is open.eqpt.IsUnloadArriveByPassReply:{eqpt.IsUnloadArriveByPassReply}",
                           VehicleID: eqpt.VEHICLE_ID);
                        return;
                    }

                    if (!SCUtility.isEmpty(eqpt.MCS_CMD))
                    {
                        scApp.CMDBLL.updateCMD_MCS_CmdStatus2UnloadArrive(eqpt.MCS_CMD);
                    }
                    scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(eqpt.VEHICLE_ID);
                    //scApp.ReserveBLL.TryAddReservedSection(eqpt.VEHICLE_ID, eqpt.CUR_SEC_ID);
                    break;

                case EventType.LoadComplete:
                    scApp.CMDBLL.setWillPassSectionInfo(eqpt.VEHICLE_ID, eqpt.PredictSectionsToDesination);
                    break;

                case EventType.UnloadComplete:
                    if (!SCUtility.isEmpty(eqpt.MCS_CMD))
                    {
                        scApp.CMDBLL.updateCMD_MCS_CmdStatus2UnloadComplete(eqpt.MCS_CMD);
                    }
                    break;
            }
            replyTranEventReport(bcfApp, eventType, eqpt, seqNum);

            switch (eventType)
            {
                case EventType.LoadArrivals:
                    scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                                    eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_LOAD_ARRIVE);
                    scApp.VehicleBLL.doLoadArrivals(eqpt.VEHICLE_ID, current_adr_id, current_sec_id);
                    break;

                case EventType.LoadComplete:
                    scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                                    eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE);
                    scApp.VehicleBLL.doLoadComplete(eqpt.VEHICLE_ID, current_adr_id, current_sec_id, carrier_id);
                    break;

                case EventType.UnloadArrivals:
                    scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                                    eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_UNLOAD_ARRIVE);
                    scApp.VehicleBLL.doUnloadArrivals(eqpt.VEHICLE_ID, current_adr_id, current_sec_id);
                    break;

                case EventType.UnloadComplete:
                    scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                                eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_UNLOAD_COMPLETE);
                    scApp.VehicleBLL.doUnloadComplete(eqpt.VEHICLE_ID);
                    break;
            }
        }

        public bool replyTranEventReport(BCFApplication bcfApp, EventType eventType, AVEHICLE vh, int seq_num,
                                          bool canBlockPass = false, bool canHIDPass = false, bool canReservePass = false,
                                          string renameCarrierID = "", CMDCancelType cancelType = CMDCancelType.CmdNone,
                                          RepeatedField<ReserveInfo> reserveInfos = null,
                                          TrackDir trackDir = TrackDir.None)
        {
            cancelType = checkCancelType(vh, eventType, cancelType);

            ID_36_TRANS_EVENT_RESPONSE send_str = new ID_36_TRANS_EVENT_RESPONSE
            {
                IsBlockPass = canBlockPass ? PassType.Pass : PassType.Block,
                IsHIDPass = canHIDPass ? PassType.Pass : PassType.Block,
                IsReserveSuccess = canReservePass ? ReserveResult.Success : ReserveResult.Unsuccess,
                ReplyCode = 0,
                RenameBOXID = renameCarrierID,
                ReplyActiveType = cancelType,
                EventType = eventType,
                TrackDir = trackDir
            };
            if (reserveInfos != null)
            {
                send_str.ReserveInfos.AddRange(reserveInfos);
            }
            WrapperMessage wrapper = new WrapperMessage
            {
                SeqNum = seq_num,
                ImpTransEventResp = send_str
            };

            Boolean resp_cmp = vh.sendMessage(wrapper, true);
            LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, send_str, seq_num);
            return resp_cmp;
        }
        /// <summary>
        /// 當車子在OHTC Command Table中已無命令，在回復36-Loading...事件時，皆要要求車子進行cacnel
        /// </summary>
        private CMDCancelType checkCancelType(AVEHICLE vh, EventType eventType, CMDCancelType cancelType)
        {
            try
            {
                if (cancelType != CMDCancelType.CmdNone)
                {
                    return cancelType;
                }
                switch (eventType)
                {
                    case EventType.LoadArrivals:
                    case EventType.Vhloading:
                    case EventType.LoadComplete:
                    case EventType.UnloadArrivals:
                    case EventType.Vhunloading:
                        //case EventType.UnloadComplete:
                        bool has_cmd_excute = scApp.CMDBLL.hasCmdOhtcExcute(vh.VEHICLE_ID);
                        if (has_cmd_excute)
                        {
                            return cancelType;
                        }
                        else
                        {
                            LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                               Data: $"vh:{vh.VEHICLE_ID} report event:{eventType}, but no command in db, return:{CMDCancelType.CmdCancel}",
                               VehicleID: vh.VEHICLE_ID,
                               CarrierID: vh.CST_ID);
                            return CMDCancelType.CmdCancel;
                        }
                    default:
                        return cancelType;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
                return cancelType;
            }
        }


        private void PositionReport_DoubleStorage(BCFApplication bcfApp, AVEHICLE eqpt, int seqNum
                                                  , EventType eventType, string current_adr_id, string current_sec_id, string carrier_id
                                                  , string cstType)
        {
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                Data: $"Process report {eventType}",
                VehicleID: eqpt.VEHICLE_ID,
                CarrierID: eqpt.CST_ID);
                scApp.CMDBLL.updateCMD_OHTC_CompleteStatus(eqpt.OHTC_CMD, CompleteStatus.CmpStatusIddoubleStorage);
                if (!SCUtility.isEmpty(eqpt.MCS_CMD))
                {
                    bool retryOrAbort = true;
                    retryOrAbort = scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                            eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_DOUBLE_STORAGE
                            , cstType);
                    Boolean resp_cmp;
                    resp_cmp = replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                }
                else
                {
                    replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
            }
        }

        private void PositionReport_EmptyRetrieval(BCFApplication bcfApp, AVEHICLE eqpt, int seqNum
                                                    , EventType eventType, string current_adr_id, string current_sec_id, string carrier_id)
        {
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                Data: $"Process report {eventType}",
                VehicleID: eqpt.VEHICLE_ID,
                CarrierID: eqpt.CST_ID);

                scApp.CMDBLL.updateCMD_OHTC_CompleteStatus(eqpt.OHTC_CMD, CompleteStatus.CmpStatusIdemptyRetrival);
                if (!SCUtility.isEmpty(eqpt.MCS_CMD))
                {
                    bool retryOrAbort = true;
                    retryOrAbort = scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                            eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_EMPTY_RETRIEVAL);
                    Boolean resp_cmp;
                    resp_cmp = replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                }
                else
                {
                    replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
            }
        }

        private void PositionReport_CSTTypeMismatch(BCFApplication bcfApp, AVEHICLE eqpt, int seqNum
                                                   , EventType eventType, string carrier_id, string cstType)
        {
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                Data: $"Process report {eventType}",
                VehicleID: eqpt.VEHICLE_ID,
                CarrierID: eqpt.CST_ID);

                if (!SCUtility.isEmpty(eqpt.MCS_CMD))
                {
                    bool retryOrAbort = true;
                    retryOrAbort = scApp.TransferService.OHT_TransferStatus(eqpt.OHTC_CMD,
                            eqpt.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_CST_TYPE_MISMATCH,
                            cstType);
                    Boolean resp_cmp;
                    resp_cmp = replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                }
                else
                {
                    replyTranEventReport(bcfApp, eventType, eqpt, seqNum, true, true, true, "", CMDCancelType.CmdCancel);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
            }
        }

        #endregion Position Report

        #region Status Report

        private const string VEHICLE_ERROR_REPORT_DESCRIPTION = "Vehicle:{0} ,error happend.";

        [ClassAOPAspect]
        public void StatusReport(BCFApplication bcfApp, AVEHICLE eqpt, ID_144_STATUS_CHANGE_REP recive_str, int seq_num)
        {
            if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                return;
            LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, eqpt, recive_str, seq_num);

            string current_adr = recive_str.CurrentAdrID;
            VHModeStatus modeStat = DecideVhModeStatus(eqpt.VEHICLE_ID, current_adr, recive_str.ModeStatus);
            VHActionStatus actionStat = recive_str.ActionStatus;
            VhPowerStatus powerStat = recive_str.PowerStatus;
            string cstID = recive_str.CSTID;
            VhStopSingle obstacleStat = recive_str.ObstacleStatus;
            VhStopSingle blockingStat = recive_str.BlockingStatus;
            VhStopSingle pauseStat = recive_str.PauseStatus;
            VhStopSingle hidStat = recive_str.HIDStatus;
            VhStopSingle errorStat = recive_str.ErrorStatus;
            VhLoadCarrierStatus loadCSTStatus = recive_str.HasCst;
            VhLoadCarrierStatus loadBOXStatus = recive_str.HasBox;
            if (loadBOXStatus == VhLoadCarrierStatus.Exist) //B0.05
            {
                eqpt.BOX_ID = recive_str.CarBoxID;
            }

            checkObstacleState(eqpt, obstacleStat);

            //VhGuideStatus leftGuideStat = recive_str.LeftGuideLockStatus;
            //VhGuideStatus rightGuideStat = recive_str.RightGuideLockStatus;
            // 0317 Jason 此部分之loadBOXStatus 原為loadCSTStatus ，現在之狀況為暫時解法
            bool hasdifferent =
                    !SCUtility.isMatche(eqpt.CST_ID, cstID) ||
                    eqpt.MODE_STATUS != modeStat ||
                    eqpt.ACT_STATUS != actionStat ||
                    eqpt.ObstacleStatus != obstacleStat ||
                    eqpt.BlockingStatus != blockingStat ||
                    eqpt.PauseStatus != pauseStat ||
                    eqpt.HIDStatus != hidStat ||
                    eqpt.ERROR != errorStat ||
                    eqpt.HAS_CST != (int)loadBOXStatus;

            if (eqpt.ERROR != errorStat)
            {
                //todo 在error flag 有變化時，上報S5F1 alarm set/celar
                //string alarm_desc = string.Format(VEHICLE_ERROR_REPORT_DESCRIPTION, eqpt.Real_ID);
                //string alarm_code = $"000{eqpt.Num}";
                //ErrorStatus error_status =
                //    errorStat == VhStopSingle.StopSingleOn ? ErrorStatus.ErrSet : ErrorStatus.ErrReset;
                //scApp.ReportBLL.ReportAlarmHappend(error_status, alarm_code, alarm_desc);
                eqpt.onErrorStatusChange(errorStat);

                if (!SCUtility.isEmpty(eqpt.MCS_CMD))
                {
                    scApp.ReportBLL.newReportTransferCommandPaused(eqpt.MCS_CMD, null);
                }
            }

            int obstacleDIST = recive_str.ObstDistance;
            string obstacleVhID = recive_str.ObstVehicleID;
            // 0317 Jason 此部分之loadBOXStatus 原為loadCSTStatus ，現在之狀況為暫時解法
            if (hasdifferent && !scApp.VehicleBLL.doUpdateVehicleStatus(eqpt, cstID,
                                   modeStat, actionStat,
                                   blockingStat, pauseStat, obstacleStat, hidStat, errorStat, loadBOXStatus))
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"update vhicle status fail!",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
                return;
            }

            //if (modeStat == VHModeStatus.AutoMtl)
            //{
            //    var check_is_in_maintain_device = scApp.EquipmentBLL.cache.IsInMaintainDevice(eqpt.CUR_ADR_ID);
            //    if (check_is_in_maintain_device.isIn)
            //    {
            //        var device = check_is_in_maintain_device.device;
            //        if (device is MaintainLift)
            //            scApp.MTLService.carInSafetyAndVehicleStatusCheck(device as MaintainLift);

            //    }
            //}

            //UpdateVehiclePositionFromStatusReport(eqpt, recive_str);

            List<AMCSREPORTQUEUE> reportqueues = null;
            //using (TransactionScope tx = SCUtility.getTransactionScope())
            //{
            //    using (DBConnection_EF con = DBConnection_EF.GetUContext())
            //    {
            //        bool isSuccess = true;
            //        switch (actionStat)
            //        {
            //            case VHActionStatus.Loading:
            //            case VHActionStatus.Unloading:
            //                if (preActionStat != actionStat)
            //                {
            //                    isSuccess = scApp.ReportBLL.ReportLoadingUnloading(eqpt.VEHICLE_ID, actionStat, out reportqueues);
            //                }
            //                break;
            //            default:
            //                isSuccess = true;
            //                break;
            //        }
            //        if (!isSuccess)
            //        {
            //            return;
            //        }
            //        if (reply_status_event_report(bcfApp, eqpt, seq))
            //        {
            //            tx.Complete();
            //        }
            //    }
            //}
            //reply_status_event_report(bcfApp, eqpt, seq_num);

            //if (actionStat == VHActionStatus.Stop)
            //{
            //if (obstacleStat == VhStopSingle.StopSingleOn)
            //{
            //    ASEGMENT seg = scApp.SegmentBLL.cache.GetSegment(eqpt.CUR_SEG_ID);
            //    AVEHICLE next_vh_on_seg = seg.GetNextVehicle(eqpt);
            //    //if (!SCUtility.isEmpty(obstacleVhID))
            //    if (next_vh_on_seg != null)
            //    {
            //        //scApp.VehicleBLL.whenVhObstacle(obstacleVhID);
            //        scApp.VehicleBLL.whenVhObstacle(next_vh_on_seg.VEHICLE_ID);
            //    }
            //}
            //}
        }

        private void checkObstacleState(AVEHICLE eqpt, VhStopSingle obstacleStat)
        {
            try
            {

                if (obstacleStat == VhStopSingle.StopSingleOn)
                {

                    if (!eqpt.CurrentObstaclingTime.IsRunning)
                    {
                        eqpt.CurrentObstaclingTime.Restart();
                    }
                }
                else
                {
                    eqpt.CurrentObstaclingTime.Reset();
                    eqpt.CurrentObstaclingTime.Stop();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        private VHModeStatus DecideVhModeStatus(string vh_id, string current_adr, VHModeStatus vh_current_mode_status)
        {
            AVEHICLE eqpt = scApp.VehicleBLL.getVehicleByID(vh_id);

            LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
              Data: $"current vh mode is:{eqpt.MODE_STATUS} and vh report mode:{vh_current_mode_status}",
              VehicleID: eqpt.VEHICLE_ID,
              CarrierID: eqpt.CST_ID);
            VHModeStatus modeStat = default(VHModeStatus);
            if (vh_current_mode_status == VHModeStatus.AutoRemote)
            {
                if (eqpt.MODE_STATUS == VHModeStatus.AutoLocal ||
                         eqpt.MODE_STATUS == VHModeStatus.AutoMtl ||
                         eqpt.MODE_STATUS == VHModeStatus.AutoMts)
                {
                    modeStat = eqpt.MODE_STATUS;
                }
                else if (scApp.EquipmentBLL.cache.IsInMatainLift(current_adr))
                {
                    modeStat = VHModeStatus.AutoMtl;
                }
                else
                {
                    modeStat = vh_current_mode_status;
                }
            }
            else
            {
                modeStat = vh_current_mode_status;
            }
            return modeStat;
        }

        //private void whenVhObstacle(string obstacleVhID)
        //{
        //    AVEHICLE obstacleVh = scApp.VehicleBLL.getVehicleByID(obstacleVhID);
        //    if (obstacleVh != null)
        //    {
        //        if (obstacleVh.IS_PARKING &&
        //            !SCUtility.isEmpty(obstacleVh.PARK_ADR_ID))
        //        {
        //            scApp.VehicleBLL.FindParkZoneOrCycleRunZoneForDriveAway(obstacleVh);
        //        }
        //        else if (SCUtility.isEmpty(obstacleVh.OHTC_CMD))
        //        {
        //            string[] nextSections = scApp.MapBLL.loadNextSectionIDBySectionID(obstacleVh.CUR_SEC_ID);
        //            if (nextSections != null && nextSections.Count() > 0)
        //            {
        //                ASECTION nextSection = scApp.MapBLL.getSectiontByID(nextSections[0]);
        //                bool isSuccess = scApp.CMDBLL.doCreatTransferCommand(obstacleVhID
        //                         , string.Empty
        //                         , string.Empty
        //                         , E_CMD_TYPE.Move
        //                         , obstacleVh.CUR_ADR_ID
        //                         , nextSection.TO_ADR_ID, 0, 0);

        //            }
        //        }
        //    }
        //}
        private bool reply_status_event_report(BCFApplication bcfApp, AVEHICLE eqpt, int seq_num)
        {
            ID_44_STATUS_CHANGE_RESPONSE send_str = new ID_44_STATUS_CHANGE_RESPONSE
            {
                ReplyCode = 0
            };
            WrapperMessage wrapper = new WrapperMessage
            {
                SeqNum = seq_num,
                StatusChangeResp = send_str
            };

            //Boolean resp_cmp = ITcpIpControl.sendGoogleMsg(bcfApp, eqpt.TcpIpAgentName, wrapper, true);
            Boolean resp_cmp = eqpt.sendMessage(wrapper, true);
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
              seq_num: seq_num, Data: send_str,
              VehicleID: eqpt.VEHICLE_ID,
              CarrierID: eqpt.CST_ID);
            SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seq_num, send_str, resp_cmp.ToString());
            return resp_cmp;
        }

        #endregion Status Report

        #region Command Complete Report

        [ClassAOPAspect]
        public void CommandCompleteReport(string tcpipAgentName, BCFApplication bcfApp, AVEHICLE vh, ID_132_TRANS_COMPLETE_REPORT recive_str, int seq_num)
        {
            try
            {
                vh.isCommandEnding = true;
                if (scApp.getEQObjCacheManager().getLine().ServerPreStop)
                    return;
                LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, recive_str, seq_num);
                string vh_id = vh.VEHICLE_ID;
                string finish_ohxc_cmd = vh.OHTC_CMD;
                string finish_mcs_cmd = vh.MCS_CMD;
                string cmd_id = recive_str.CmdID;
                int travel_dis = recive_str.CmdDistance;
                CompleteStatus completeStatus = recive_str.CmpStatus;
                string cur_sec_id = recive_str.CurrentSecID;
                string cur_adr_id = recive_str.CurrentAdrID;
                string cst_id = SCUtility.Trim(recive_str.CSTID, true);
                VhLoadCarrierStatus vhLoadCSTStatus = recive_str.HasCst;
                string car_cst_id = recive_str.BOXID;
                bool isSuccess = true;
                bool is_direct_finish = true;
                ACMD_MCS cmd_mcs = scApp.CMDBLL.getCMD_MCSByID(finish_mcs_cmd);
                if (cmd_mcs != null)
                {
                    if (cmd_mcs.IsScanCommand)
                    {
                        is_direct_finish = true;
                    }
                    else
                    {
                        if (cmd_mcs.isLoading || cmd_mcs.isUnloading)
                        {
                            is_direct_finish = false;
                        }
                    }
                }
                if (scApp.CMDBLL.isCMCD_OHTCFinish(cmd_id))
                {
                    replyCommandComplete(vh, seq_num, finish_ohxc_cmd, finish_mcs_cmd);

                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"commnad id:{cmd_id} has already process. well pass this report.",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    return;
                }

                if (recive_str.CmpStatus == CompleteStatus.CmpStatusInterlockError)
                {
                    scApp.TransferService.OHT_TransferStatus(finish_ohxc_cmd,
                        vh.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_InterlockError);
                    //B0.03
                    scApp.TransferService.OHBC_AlarmSet(vh.VEHICLE_ID, ((int)AlarmLst.OHT_INTERLOCK_ERROR).ToString());
                    scApp.TransferService.OHBC_AlarmCleared(vh.VEHICLE_ID, ((int)AlarmLst.OHT_INTERLOCK_ERROR).ToString());
                    //
                }
                else if (recive_str.CmpStatus == CompleteStatus.CmpStatusVehicleAbort)
                {
                    if (!is_direct_finish)
                    {
                        //not thing...
                    }
                    else
                    {
                        scApp.TransferService.OHT_TransferStatus(finish_ohxc_cmd,
                            vh.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_VEHICLE_ABORT);
                        //B0.03
                        scApp.TransferService.OHBC_AlarmSet(vh.VEHICLE_ID, ((int)AlarmLst.OHT_VEHICLE_ABORT).ToString());
                        scApp.TransferService.OHBC_AlarmCleared(vh.VEHICLE_ID, ((int)AlarmLst.OHT_VEHICLE_ABORT).ToString());
                    }
                    //
                }
                //todo id mismatch、id read fail
                else if (recive_str.CmpStatus == CompleteStatus.CmpStatusIdmisMatch)
                {
                    scApp.TransferService.CommandCompleteByIDMismatch(vh_id, finish_ohxc_cmd);

                }
                else if (recive_str.CmpStatus == CompleteStatus.CmpStatusIdreadFailed)
                {
                    scApp.TransferService.CommandCompleteByIDReadFail(vh_id, finish_ohxc_cmd);
                }
                else if (recive_str.CmpStatus == CompleteStatus.CmpStatusCancel)
                {
                    scApp.TransferService.CommandCompleteByCancel(vh_id, finish_ohxc_cmd);
                }
                else if (recive_str.CmpStatus == CompleteStatus.CmpStatusAbort)
                {
                    scApp.TransferService.CommandCompleteByAbort(vh_id, finish_ohxc_cmd);
                }
                else
                {
                    scApp.TransferService.OHT_TransferStatus(finish_ohxc_cmd,
                        vh.VEHICLE_ID, ACMD_MCS.COMMAND_STATUS_BIT_INDEX_COMMNAD_FINISH);
                }


                if (recive_str.CmpStatus == CompleteStatus.CmpStatusVehicleAbort)
                {
                    if (is_direct_finish)
                        isSuccess = finishOHTCCmd(vh, cmd_id, finish_mcs_cmd, completeStatus);
                }
                else
                {
                    isSuccess = finishOHTCCmd(vh, cmd_id, finish_mcs_cmd, completeStatus);

                    scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vh.VEHICLE_ID);
                }


                replyCommandComplete(vh, seq_num, finish_ohxc_cmd, finish_mcs_cmd);
                scApp.CMDBLL.removeAllWillPassSection(vh.VEHICLE_ID);
                //scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vh.VEHICLE_ID); 20221004 若是Vehicle abort就不直接將路權移除
                //scApp.ReserveBLL.TryAddReservedSection(vh.VEHICLE_ID, vh.CUR_SEC_ID);

                if (DebugParameter.IsDebugMode && DebugParameter.IsCycleRun)
                {
                    SpinWait.SpinUntil(() => false, 3000);
                    TestCycleRun(vh, cmd_id);
                }
                else
                {
                    checkIsMoveToMTxDevice(vh, completeStatus, cur_adr_id);
                }

                if (scApp.getEQObjCacheManager().getLine().SCStats == ALINE.TSCState.PAUSING)
                {
                    List<ACMD_MCS> cmd_mcs_lst = scApp.CMDBLL.loadACMD_MCSIsUnfinished();
                    if (cmd_mcs_lst.Count == 0)
                    {
                        scApp.LineService.TSCStateToPause("");
                    }
                }
                Task.Run(() =>
                {
                    scApp.TransferService.TransferRun();//B0.08.0 處發TransferRun，使MCS命令可以在多車情形下早於趕車CMD下達。
                });
                vh.onCommandComplete(completeStatus);
                //if (recive_str.CmpStatus == CompleteStatus.CmpStatusLoadunload)
                //{
                //    scApp.TransferService.findTransferCommandByVhViewer(vh);
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
            finally
            {
                vh.isCommandEnding = false;
            }
        }

        private void checkIsMoveToMTxDevice(AVEHICLE vh, CompleteStatus completeStatus, string curAdrID)
        {
            MaintainLift maintainLift = null;
            switch (completeStatus)
            {
                case CompleteStatus.CmpStatusSystemOut:
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Process vh:{vh.VEHICLE_ID} system out complete, current address:{curAdrID},current mode:{vh.MODE_STATUS}",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    if (vh.MODE_STATUS == VHModeStatus.AutoMtl)
                    {
                        //在收到OHT的ID:132-SystemOut完成後，創建一個Transfer command，讓Vh移至移動至MTL上
                        maintainLift = scApp.EquipmentBLL.cache.GetMaintainLiftBySystemOutAdr(curAdrID);
                        if (maintainLift != null && maintainLift.CarOutSafetyCheck)
                            doAskVhToMaintainsAddress(vh.VEHICLE_ID, maintainLift.MTL_ADDRESS);
                    }

                    scApp.ReportBLL.newReportVehicleRemoved(vh.VEHICLE_ID, null);
                    //將該VH標記 Remove
                    Remove(vh.VEHICLE_ID);
                    break;
                case CompleteStatus.CmpStatusMoveToMtl:
                    maintainLift = scApp.EquipmentBLL.cache.GetMaintainLiftByMTLAdr(curAdrID);
                    LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                       Data: $"Process vh:{vh.VEHICLE_ID} move to mtl complete, current address:{curAdrID},current mode:{vh.MODE_STATUS}",
                       VehicleID: vh.VEHICLE_ID,
                       CarrierID: vh.CST_ID);
                    if (maintainLift != null)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: $"Process vh:{vh.VEHICLE_ID} move to mtl complete, notify mtx:{maintainLift.DeviceID} is complete",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                        //通知MTL Car out完成
                        scApp.MTLService.carOutComplete(maintainLift);
                    }
                    //將該VH標記 Remove
                    if (vh.IS_INSTALLED)
                    {
                        Remove(vh.VEHICLE_ID);
                    }
                    break;
                case CompleteStatus.CmpStatusMtlhome:
                    maintainLift = scApp.EquipmentBLL.cache.GetMaintainLiftByMTLHomeAdr(curAdrID);
                    if (maintainLift != null)
                    {
                        scApp.MTLService.DisableCarInInterlock(maintainLift);
                        doAskVhToSystemInAddress(vh.VEHICLE_ID, maintainLift.MTL_SYSTEM_IN_ADDRESS);
                    }
                    break;
                case CompleteStatus.CmpStatusSystemIn:
                    var maintain_device = scApp.EquipmentBLL.cache.GetMaintainDeviceBySystemInAdr(curAdrID);
                    if (maintain_device != null)
                    {
                        scApp.MTLService.carInComplete(maintain_device, vh.VEHICLE_ID);
                        Install(vh.VEHICLE_ID);
                    }
                    break;
                default:
                    if (vh.MODE_STATUS == VHModeStatus.AutoMtl && vh.HAS_CST == 0)
                    {
                        maintainLift = scApp.EquipmentBLL.cache.GetExcuteCarOutMTL(vh.VEHICLE_ID);
                        if (maintainLift != null)
                        {
                            if (maintainLift.CarOutSafetyCheck)//如果SafetyCheck已經解除則不能進行出車
                            {
                                doAskVhToMaintainsAddress(vh.VEHICLE_ID, maintainLift.MTL_ADDRESS);
                            }
                        }
                    }
                    break;
            }
        }

        private bool replyCommandComplete(AVEHICLE vh, int seq_num, string finish_ohxc_cmd, string finish_mcs_cmd)
        {
            ID_32_TRANS_COMPLETE_RESPONSE send_str = new ID_32_TRANS_COMPLETE_RESPONSE
            {
                ReplyCode = 0
            };
            WrapperMessage wrapper = new WrapperMessage
            {
                SeqNum = seq_num,
                TranCmpResp = send_str
            };
            Boolean resp_cmp = vh.sendMessage(wrapper, true);
            LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, vh, send_str, seq_num);
            return resp_cmp;
        }

        private void TestCycleRun(AVEHICLE vh, string cmd_id)
        {
            ACMD_OHTC cmd = scApp.CMDBLL.getCMD_OHTCByID(cmd_id);
            if (cmd == null) return;
            if (!(cmd.CMD_TPYE == E_CMD_TYPE.LoadUnload || cmd.CMD_TPYE == E_CMD_TYPE.Move)) return;

            string result = string.Empty;
            string cst_id = cmd.CARRIER_ID?.Trim();
            string box_id = cmd.BOX_ID?.Trim();
            string lot_id = cmd.LOT_ID?.Trim();
            string from_port_id = cmd.DESTINATION.Trim();
            string to_port_id = cmd.SOURCE.Trim();
            string from_adr = "";
            string to_adr = "";
            switch (cmd.CMD_TPYE)
            {
                case E_CMD_TYPE.LoadUnload:
                    scApp.MapBLL.getAddressID(from_port_id, out from_adr);
                    scApp.MapBLL.getAddressID(to_port_id, out to_adr);
                    break;

                case E_CMD_TYPE.Move:
                    to_adr = vh.startAdr.Trim();
                    break;
            }
            scApp.CMDBLL.doCreatTransferCommand(cmd.VH_ID,
                                            cst_id: cst_id,
                                            box_id: box_id,
                                            lot_id: lot_id,
                                            cmd_type: cmd.CMD_TPYE,
                                            source: from_port_id,
                                            destination: to_port_id,
                                            source_address: from_adr,
                                            destination_address: to_adr,
                                            gen_cmd_type: SCAppConstants.GenOHxCCommandType.Auto);
        }

        #endregion Command Complete Report

        #region Range Teach

        public void RangeTeachingCompleteReport(string tcpipAgentName, BCFApplication bcfApp, AVEHICLE eqpt, ID_172_RANGE_TEACHING_COMPLETE_REPORT recive_str, int seq_num)
        {
            SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seq_num, recive_str);

            string from_adr = recive_str.FromAdr;
            string to_adr = recive_str.ToAdr;
            uint sec_distance = recive_str.SecDistance;
            int cmp_code = recive_str.CompleteCode;
            ID_72_RANGE_TEACHING_COMPLETE_RESPONSE response = null;
            if (cmp_code == 0)
            {
                if (scApp.MapBLL.updateSecDistance(from_adr, to_adr, sec_distance, out ASECTION section))
                {
                    scApp.updateCatchData_Section(section);
                    scApp.VehicleBLL.setAndPublishPositionReportInfo2Redis(eqpt.VEHICLE_ID, recive_str, section.SEC_ID);
                }
            }
            response = new ID_72_RANGE_TEACHING_COMPLETE_RESPONSE()
            {
                ReplyCode = 0
            };

            WrapperMessage wrapper = new WrapperMessage
            {
                SeqNum = seq_num,
                RangeTeachingCmpResp = response
            };
            Boolean resp_cmp = eqpt.sendMessage(wrapper, true);
            SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seq_num, response, resp_cmp.ToString());

            AutoTeaching(eqpt.VEHICLE_ID);
        }

        public void AutoTeaching(string vh_id)
        {
            if (!sc.App.SystemParameter.AutoTeching) return;
            //1.找出VH，並得到他目前所在的Address。
            scApp.VehicleBLL.getAndProcPositionReportFromRedis(vh_id);
            AVEHICLE vh = scApp.VehicleBLL.getVehicleByID(vh_id);
            string vh_current_adr = vh.CUR_ADR_ID;
            List<string> base_address = new List<string> { vh.CUR_ADR_ID };
            HashSet<string> choess_sation = new HashSet<string>();

            do
            {
                //接著透過這個Address查詢哪些Section是該Address的From Adr.且還沒有Teching過的(LAST_TECH_TIME = null)
                List<ASECTION> sections = scApp.MapBLL.loadSectionByFromAdrs(base_address);
                base_address.Clear();
                foreach (var section in sections)
                {
                    if (section.SEC_TYPE == SectionType.Mtl) continue;

                    if (section.LAST_TECH_TIME.HasValue)
                    {
                        if (section.DIRC_DRIV == 0)
                            base_address.Add(section.TO_ADR_ID.Trim());
                    }
                    else
                    {
                        TechingAction(vh_id, vh_current_adr, section);
                        base_address.Clear();
                        break;
                    }
                }
                if (!scApp.MapBLL.hasNotYetTeachingSection())
                {
                    sc.App.SystemParameter.AutoTeching = false;
                    bcf.App.BCFApplication.onInfoMsg("All section teching complete.");
                    return;
                }
            } while (base_address.Count != 0);
        }

        private void TechingAction(string vh_id, string vh_current_adr, ASECTION section)
        {
            if (SCUtility.isMatche(section.FROM_ADR_ID, vh_current_adr))
            {
                TeachingRequest(vh_id, section.FROM_ADR_ID, section.TO_ADR_ID);
            }
            else
            {
                string[] ReutrnFromAdr2ToAdr = scApp.RouteGuide.DownstreamSearchSection
                    (vh_current_adr, section.FROM_ADR_ID, 1, true);
                string route = ReutrnFromAdr2ToAdr[0].Split('=')[0];
                string[] routeSection = route.Split(',');
                ASECTION first_sec = scApp.MapBLL.getSectiontByID(routeSection[0]);
                TeachingRequest(vh_id, vh_current_adr, first_sec.TO_ADR_ID);
                //scApp.CMDBLL.doCreatTransferCommand(vh_id
                //                              , string.Empty
                //                              , string.Empty
                //                              , E_CMD_TYPE.Move_Teaching
                //                              , vh_current_adr
                //                              , section.FROM_ADR_ID, 0, 0);
            }
        }

        #endregion Range Teach

        #region Receive Message

        public void BasicInfoVersionReport(BCFApplication bcfApp, AVEHICLE eqpt, ID_102_BASIC_INFO_VERSION_REP recive_str, int seq_num)
        {
            ID_2_BASIC_INFO_VERSION_RESPONSE send_str = new ID_2_BASIC_INFO_VERSION_RESPONSE
            {
                ReplyCode = 0
            };
            WrapperMessage wrapper = new WrapperMessage
            {
                SeqNum = seq_num,
                BasicInfoVersionResp = send_str
            };
            Boolean resp_cmp = eqpt.sendMessage(wrapper, true);
            //SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seqNum, send_str, resp_cmp.ToString());
        }

        public void GuideDataUploadRequest(BCFApplication bcfApp, AVEHICLE eqpt, ID_162_GUIDE_DATA_UPLOAD_REP recive_str, int seq_num)
        {
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               seq_num: seq_num,
               Data: recive_str,
               VehicleID: eqpt.VEHICLE_ID,
               CarrierID: eqpt.CST_ID);

            ID_62_GUID_DATA_UPLOAD_RESPONSE send_str = new ID_62_GUID_DATA_UPLOAD_RESPONSE
            {
                ReplyCode = 0
            };
            WrapperMessage wrapper = new WrapperMessage
            {
                SeqNum = seq_num,
                GUIDEDataUploadResp = send_str
            };
            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
               seq_num: seq_num,
               Data: send_str,
               VehicleID: eqpt.VEHICLE_ID,
               CarrierID: eqpt.CST_ID);

            Boolean resp_cmp = eqpt.sendMessage(wrapper, true);
            //SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seqNum, send_str, resp_cmp.ToString());
        }

        public void AddressTeachReport(BCFApplication bcfApp, AVEHICLE eqpt, ID_174_ADDRESS_TEACH_REPORT recive_str, int seq_num)
        {
            try
            {
                string adr_id = recive_str.Addr;
                int resolution = recive_str.Position;

                scApp.DataSyncBLL.updateAddressData(eqpt.VEHICLE_ID, adr_id, resolution);

                ID_74_ADDRESS_TEACH_RESPONSE send_str = new ID_74_ADDRESS_TEACH_RESPONSE
                {
                    ReplyCode = 0
                };
                WrapperMessage wrapper = new WrapperMessage
                {
                    SeqNum = seq_num,
                    AddressTeachResp = send_str
                };
                Boolean resp_cmp = eqpt.sendMessage(wrapper, true);
                //SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seqNum, send_str, resp_cmp.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        [ClassAOPAspect]
        public void AlarmReport(BCFApplication bcfApp, AVEHICLE eqpt, ID_194_ALARM_REPORT recive_str, int seq_num)
        {
            LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, eqpt, recive_str, seq_num);

            LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
              seq_num: seq_num, Data: recive_str,
              VehicleID: eqpt.VEHICLE_ID,
              CarrierID: eqpt.CST_ID);
            try
            {
                SCUtility.RecodeReportInfo(eqpt.VEHICLE_ID, seq_num, recive_str);
                string node_id = eqpt.NODE_ID;
                string eq_id = eqpt.VEHICLE_ID;
                string err_code = recive_str.ErrCode;
                ErrorStatus status = recive_str.ErrStatus;
                if (status == ErrorStatus.ErrSet)
                {
                    scApp.TransferService.OHBC_AlarmSet(eqpt.VEHICLE_ID, err_code);
                }
                else
                {
                    if (err_code != "0")
                    {
                        scApp.TransferService.OHBC_AlarmCleared(eqpt.VEHICLE_ID, err_code);
                    }
                    else
                    {
                        scApp.TransferService.OHBC_AlarmAllCleared(eqpt.VEHICLE_ID);
                    }
                }

                ID_94_ALARM_RESPONSE send_str = new ID_94_ALARM_RESPONSE
                {
                    ReplyCode = 0
                };
                WrapperMessage wrapper = new WrapperMessage
                {
                    SeqNum = seq_num,
                    AlarmResp = send_str
                };
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                  seq_num: seq_num, Data: send_str,
                  VehicleID: eqpt.VEHICLE_ID,
                  CarrierID: eqpt.CST_ID);

                Boolean resp_cmp = eqpt.sendMessage(wrapper, true);

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"do reply alarm report ,{resp_cmp}",
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);

                LogHelper.RecordReportInfoByQueue(scApp, scApp.CMDBLL, eqpt, send_str, seq_num);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: eqpt.VEHICLE_ID,
                   CarrierID: eqpt.CST_ID);
            }
        }

        #endregion Receive Message

        #region MTL Handle

        public bool doReservationVhToMaintainsBufferAddress(string vhID)
        {
            bool isSuccess = true;
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    isSuccess = isSuccess && VehicleAutoModeCahnge(vhID, VHModeStatus.AutoMtl);
                    if (isSuccess)
                    {
                        tx.Complete();
                    }
                }
            }
            return isSuccess;
        }

        public bool doReservationVhToMaintainsSpace(string vhID)
        {
            bool isSuccess = true;
            using (TransactionScope tx = SCUtility.getTransactionScope())
            {
                using (DBConnection_EF con = DBConnection_EF.GetUContext())
                {
                    isSuccess = isSuccess && VehicleAutoModeCahnge(vhID, VHModeStatus.AutoMts);
                    if (isSuccess)
                    {
                        tx.Complete();
                    }
                }
            }
            return isSuccess;
        }

        public bool doAskVhToSystemOutAddress(string vhID, string carOutBufferAdr)
        {
            bool isSuccess = true;
            isSuccess = scApp.CMDBLL.doCreatTransferCommand(vh_id: vhID, cmd_type: E_CMD_TYPE.SystemOut, destination_address: carOutBufferAdr);
            return isSuccess;
        }

        public bool doAskVhToMaintainsAddress(string vhID, string mtlAdtID)
        {
            bool isSuccess = true;
            isSuccess = isSuccess && scApp.CMDBLL.doCreatTransferCommand(vh_id: vhID, cmd_type: E_CMD_TYPE.MoveToMTL, destination_address: mtlAdtID);
            return isSuccess;
        }

        public bool doAskVhToCarInBufferAddress(string vhID, string carInBufferAdr)
        {
            bool isSuccess = true;
            isSuccess = scApp.CMDBLL.doCreatTransferCommand(vh_id: vhID, cmd_type: E_CMD_TYPE.MTLHome, destination_address: carInBufferAdr);
            return isSuccess;
        }

        public bool doAskVhToSystemInAddress(string vhID, string systemInAdr)
        {
            bool isSuccess = true;
            isSuccess = scApp.CMDBLL.doCreatTransferCommand(vh_id: vhID, cmd_type: E_CMD_TYPE.SystemIn, destination_address: systemInAdr);
            return isSuccess;
        }

        public bool doRecoverModeStatusToAutoRemote(string vh_id)
        {
            return VehicleAutoModeCahnge(vh_id, VHModeStatus.AutoRemote);
        }

        #endregion MTL Handle

        #region Vehicle Change The Path

        public void VhicleChangeThePath(string vh_id, bool isNeedPauseFirst)
        {
            string ohxc_cmd_id = "";
            try
            {
                bool isSuccess = true;
                AVEHICLE need_change_path_vh = scApp.getEQObjCacheManager().getVehicletByVHID(vh_id);
                if (need_change_path_vh.VhRecentTranEvent == EventType.Vhloading ||
                    need_change_path_vh.VhRecentTranEvent == EventType.Vhunloading)
                    return;
                //1.先下暫停給該台VH
                if (isNeedPauseFirst)
                    isSuccess = PauseRequest(vh_id, PauseEvent.Pause, OHxCPauseType.Normal);
                //2.送出31執行命令的Override
                //  a.取得執行中的命令
                //  b.重新將該命令改成Ready to rewrite
                ACMD_OHTC cmd_ohtc = null;
                using (TransactionScope tx = SCUtility.getTransactionScope())
                {
                    using (DBConnection_EF con = DBConnection_EF.GetUContext())
                    {
                        isSuccess &= scApp.CMDBLL.updateCMD_OHxC_Status2ReadyToReWirte(need_change_path_vh.OHTC_CMD, out cmd_ohtc);
                        isSuccess &= scApp.CMDBLL.update_CMD_Detail_2AbnormalFinsh(need_change_path_vh.OHTC_CMD, need_change_path_vh.WillPassSectionID);
                        if (isSuccess)
                            tx.Complete();
                    }
                }
                ohxc_cmd_id = cmd_ohtc.CMD_ID.Trim();
                scApp.VehicleService.doSendOHxCOverrideCmdToVh(need_change_path_vh, cmd_ohtc, isNeedPauseFirst);
            }
            catch (BLL.VehicleBLL.BlockedByTheErrorVehicleException blockedExecption)
            {
                logger.Warn(blockedExecption, "BlockedByTheErrorVehicleException:");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }
        }

        #endregion Vehicle Change The Path

        public bool VehicleAutoModeCahnge(string vh_id, VHModeStatus mode_status)
        {
            AVEHICLE vh = scApp.VehicleBLL.getVehicleByID(vh_id);
            if (vh.MODE_STATUS != VHModeStatus.Manual)
            {
                scApp.VehicleBLL.updataVehicleMode(vh_id, mode_status);
                vh.NotifyVhStatusChange();
                return true;
            }
            return false;
        }

        #region Vh connection / disconnention

        [ClassAOPAspect]
        public void Connection(BCFApplication bcfApp, AVEHICLE vh)
        {
            try
            {
                vh.isSynchronizing = true;
                lock (vh.Connection_Sync)
                {
                    vh.VhRecentTranEvent = EventType.AdrPass;

                    vh.isTcpIpConnect = true;
                    vh.StatusRequestFailTimes = 0;

                    if (DebugParameter.IsSyncWhenConnectionEvent)
                    {
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: "Connection ! Begin synchronize with vehicle...",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                        VehicleInfoSynchronize(vh.VEHICLE_ID);
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                           Data: "Connection ! End synchronize with vehicle.",
                           VehicleID: vh.VEHICLE_ID,
                           CarrierID: vh.CST_ID);
                    }
                    else
                    {
                        bool ask_status_success = VehicleStatusRequest(vh.VEHICLE_ID, true);
                    }
                    SCUtility.RecodeConnectionInfo
                        (vh.VEHICLE_ID,
                        SCAppConstants.RecodeConnectionInfo_Type.Connection.ToString(),
                        vh.getDisconnectionIntervalTime(bcfApp));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception");
            }
            finally
            {
                vh.isSynchronizing = false;
            }
        }

        [ClassAOPAspect]
        public void Disconnection(BCFApplication bcfApp, AVEHICLE vh)
        {
            lock (vh.Connection_Sync)
            {
                vh.isTcpIpConnect = false;

                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: "Disconnection !",
                   VehicleID: vh.VEHICLE_ID,
                   CarrierID: vh.CST_ID);
                SCUtility.RecodeConnectionInfo
                    (vh.VEHICLE_ID,
                    SCAppConstants.RecodeConnectionInfo_Type.Disconnection.ToString(),
                    vh.getConnectionIntervalTime(bcfApp));

                //set the connection alarm code 99999
                //scApp.TransferService.OHBC_AlarmSet(vh.VEHICLE_ID, SCAppConstants.SystemAlarmCode.OHT_Issue.OHTNormalDisconnection);
                string dis_connection_alarm_code = GetDisconnectionAlarmCode(vh);
                scApp.TransferService.OHBC_AlarmSet(vh.VEHICLE_ID, dis_connection_alarm_code);
            }
            Task.Run(() => scApp.VehicleBLL.web.vehicleDisconnection(vh.Num));
        }

        private string GetDisconnectionAlarmCode(AVEHICLE vh)
        {
            if (vh.IsError)
            {
                return SCAppConstants.SystemAlarmCode.OHT_Issue.OHTErrorDisconnection;
            }
            else if (!vh.IS_INSTALLED && vh.MODE_STATUS == VHModeStatus.AutoLocal)
            {
                return SCAppConstants.SystemAlarmCode.OHT_Issue.OHTManualDisconnection;
            }
            else
            {
                return SCAppConstants.SystemAlarmCode.OHT_Issue.OHTNormalDisconnection;
            }
        }


        #endregion Vh connection / disconnention

        #region Vehicle Install/Remove

        public void Install(string vhID)
        {
            try
            {
                bool is_success = true;

                is_success = is_success && scApp.VehicleBLL.updataVehicleInstall(vhID);
                if (is_success)
                {
                    AVEHICLE vh_vo = scApp.VehicleBLL.cache.getVhByID(vhID);
                    vh_vo.VehicleInstall();
                }
                List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                is_success = is_success && scApp.ReportBLL.newReportVehicleInstalled(vhID, reportqueues);
                scApp.ReportBLL.newSendMCSMessage(reportqueues);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vhID);
            }
        }

        public void Remove(string vhID)
        {
            try
            {
                bool is_success = true;
                is_success = is_success && scApp.VehicleBLL.updataVehicleRemove(vhID);
                if (is_success)
                {
                    AVEHICLE vh_vo = scApp.VehicleBLL.cache.getVhByID(vhID);
                    if (!vh_vo.isTcpIpConnect)
                    {
                        vh_vo.CUR_SEC_ID = "";
                        vh_vo.CUR_ADR_ID = "";
                        //如果車子沒有連線的時候，進行Remove才進行路權等資料的釋放
                        scApp.ReserveBLL.RemoveVehicle(vhID);
                        scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vhID);
                    }
                    vh_vo.VechileRemove();
                    //scApp.ReserveBLL.RemoveAllReservedSectionsByVehicleID(vhID);
                    //scApp.ReserveBLL.RemoveVehicle(vhID);
                }
                List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
                is_success = is_success && scApp.ReportBLL.newReportVehicleRemoved(vhID, reportqueues);
                scApp.ReportBLL.newSendMCSMessage(reportqueues);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex,
                   VehicleID: vhID);
            }
        }

        #endregion Vehicle Install/Remove

        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new AspectWeaver(parameter, this);
        }

        #region Specially Control

        public void forceReleaseBlockControl(string vh_id = "")
        {
            List<BLOCKZONEQUEUE> queues = null;
            using (DBConnection_EF con = DBConnection_EF.GetUContext())
            {
                if (SCUtility.isEmpty(vh_id))
                {
                    queues = scApp.MapBLL.loadAllNonReleaseBlockQueue();
                }
                else
                {
                    queues = scApp.MapBLL.loadNonReleaseBlockQueueByCarID(vh_id);
                }

                foreach (var queue in queues)
                {
                    scApp.MapBLL.updateBlockZoneQueue_AbnormalEnd(queue, SCAppConstants.BlockQueueState.Abnormal_Release_ForceRelease);
                    scApp.MapBLL.DeleteBlockControlKeyWordToRedis(queue.CAR_ID.Trim(), queue.ENTRY_SEC_ID);
                }
            }
        }

        public void PauseAllVehicleByOHxCPause()
        {
            List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
            foreach (var vh in vhs)
            {
                PauseRequest(vh.VEHICLE_ID, PauseEvent.Pause, OHxCPauseType.Earthquake);
            }
        }

        public void ResumeAllVehicleByOhxCPause()
        {
            List<AVEHICLE> vhs = scApp.getEQObjCacheManager().getAllVehicle();
            foreach (var vh in vhs)
            {
                PauseRequest(vh.VEHICLE_ID, PauseEvent.Continue, OHxCPauseType.Earthquake);
            }
        }
        public void updateVhType(string vhID, E_VH_TYPE vhType)
        {
            try
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: $"update vh:{vhID} to type:{vhType}");
                scApp.VehicleBLL.updataVehicleType(vhID, vhType);
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex);
            }
        }

        #endregion Specially Control

        #region RoadService Mark

        //public ASEGMENT doEnableDisableSegment(string segment_id, E_PORT_STATUS port_status, string laneCutType)
        //{
        //    ASEGMENT segment = null;
        //    try
        //    {
        //        List<APORTSTATION> port_stations = scApp.MapBLL.loadAllPortBySegmentID(segment_id);

        //        using (TransactionScope tx = SCUtility.getTransactionScope())
        //        {
        //            using (DBConnection_EF con = DBConnection_EF.GetUContext())
        //            {
        //                switch (port_status)
        //                {
        //                    case E_PORT_STATUS.InService:
        //                        segment = scApp.RouteGuide.OpenSegment(segment_id);
        //                        break;
        //                    case E_PORT_STATUS.OutOfService:
        //                        segment = scApp.RouteGuide.CloseSegment(segment_id);
        //                        break;
        //                }
        //                foreach (APORTSTATION port_station in port_stations)
        //                {
        //                    scApp.MapBLL.updatePortStatus(port_station.PORT_ID, port_status);
        //                    scApp.getEQObjCacheManager().getPortStation(port_station.PORT_ID).PORT_STATUS = port_status;
        //                }
        //                tx.Complete();
        //            }
        //        }
        //        List<AMCSREPORTQUEUE> reportqueues = new List<AMCSREPORTQUEUE>();
        //        List<ASECTION> sections = scApp.MapBLL.loadSectionsBySegmentID(segment_id);
        //        string segment_start_adr = sections.First().FROM_ADR_ID;
        //        string segment_end_adr = sections.Last().TO_ADR_ID;
        //        switch (port_status)
        //        {
        //            case E_PORT_STATUS.InService:
        //                scApp.ReportBLL.newReportLaneInService(segment_start_adr, segment_end_adr, laneCutType, reportqueues);
        //                break;
        //            case E_PORT_STATUS.OutOfService:
        //                scApp.ReportBLL.newReportLaneOutOfService(segment_start_adr, segment_end_adr, laneCutType, reportqueues);
        //                break;
        //        }
        //        foreach (APORTSTATION port_station in port_stations)
        //        {
        //            switch (port_status)
        //            {
        //                case E_PORT_STATUS.InService:
        //                    scApp.ReportBLL.newReportPortInServeice(port_station.PORT_ID, reportqueues);
        //                    break;
        //                case E_PORT_STATUS.OutOfService:
        //                    scApp.ReportBLL.newReportPortOutOfService(port_station.PORT_ID, reportqueues);
        //                    break;
        //            }
        //        }
        //        scApp.ReportBLL.newSendMCSMessage(reportqueues);
        //    }
        //    catch (Exception ex)
        //    {
        //        segment = null;
        //        logger.Error(ex, "Exception:");
        //    }
        //    return segment;
        //}

        #endregion RoadService Mark

        //************************************************************
        //B0.07 輸入port ID 後 可以回傳在該位置上的 VehicleID 若找不到或者 exception 會回報"Error"
        public string GetVehicleIDByPortID(string portID)
        {
            try
            {
                bool isSuccess = false;
                string portAddressID = null;
                string targetVehicleID = "Error";
                isSuccess = scApp.PortDefBLL.getAddressID(portID, out portAddressID);
                List<AVEHICLE> allVehicleList = scApp.getEQObjCacheManager().getAllVehicle();
                foreach (AVEHICLE vehicle in allVehicleList)
                {
                    AVEHICLE vehicleCache = scApp.VehicleBLL.cache.getVhByID(vehicle.VEHICLE_ID);
                    if (vehicleCache.CUR_ADR_ID == portAddressID)
                    {
                        targetVehicleID = vehicleCache.VEHICLE_ID;
                        LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                        Data: $"Find vehicle {vehicleCache.VEHICLE_ID}, vehicle address Id = {vehicleCache.CUR_ADR_ID}, = port address ID {portAddressID}");
                        break;
                    }
                }
                return targetVehicleID.Trim();
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                   Data: ex);

                scApp.TransferService.TransferServiceLogger.Error(ex, "GetVehicleIDByPortID");
                return "Error";
            }
        }

        //************************************************************
        //B0.08 輸入vehicleID 後 可以回傳該vehicle ID 之車輛目前實時在cache中的資料。若出現異常，則會回傳一空的AVEHICLE 物件。
        public AVEHICLE GetVehicleDataByVehicleID(string vehicleID)
        {
            try
            {
                AVEHICLE vehicleData = new AVEHICLE();
                vehicleData = scApp.VehicleBLL.cache.getVhByID(vehicleID);
                return vehicleData;
            }
            catch (Exception ex)
            {
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx, Data: ex);
                scApp.TransferService.TransferServiceLogger.Error(ex, "GetVehicleDataByVehicleID");
                AVEHICLE exceptionVehicleData = new AVEHICLE();
                return exceptionVehicleData;
            }
        }

        public bool IsCMD_MCSCanProcess()
        {
            bool isCMD_MCSCanProcess = false;
            try
            {
                List<ACMD_MCS> ACMD_MCSData = scApp.CMDBLL.GetMCSCmdQueue();
                foreach (ACMD_MCS commandMCS in ACMD_MCSData)
                {
                    isCMD_MCSCanProcess = scApp.TransferService.AreSourceAndDestEnable(commandMCS.HOSTSOURCE, commandMCS.HOSTDESTINATION);
                    if (isCMD_MCSCanProcess == true)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                isCMD_MCSCanProcess = false;
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Warn, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx, Data: ex);
                scApp.TransferService.TransferServiceLogger.Error(ex, "GetVehicleDataByVehicleID");
            }
            return isCMD_MCSCanProcess;
        }

        #region TEST

        private void CarrierInterfaceSim_LoadComplete(AVEHICLE vh)
        {
            //vh.CatchPLCCSTInterfacelog();
            bool[] bools_01 = new bool[16];
            bool[] bools_02 = new bool[16];
            bool[] bools_03 = new bool[16];
            bool[] bools_04 = new bool[16];
            bool[] bools_05 = new bool[16];
            bool[] bools_06 = new bool[16];
            bool[] bools_07 = new bool[16];
            bool[] bools_08 = new bool[16];
            bool[] bools_09 = new bool[16];
            bool[] bools_10 = new bool[16];

            bools_01[3] = true;

            bools_02[03] = true; bools_02[08] = true; bools_02[12] = true; bools_02[14] = true;
            bools_02[15] = true;

            bools_03[3] = true; bools_03[8] = true; bools_03[10] = true; bools_03[12] = true;
            bools_03[14] = true; bools_03[15] = true;

            bools_04[3] = true; bools_04[4] = true; bools_04[8] = true; bools_04[10] = true;
            bools_04[12] = true; bools_04[14] = true; bools_04[15] = true;

            bools_05[3] = true; bools_05[4] = true; bools_05[8] = true; bools_05[10] = true;
            bools_05[11] = true; bools_05[12] = true; bools_05[14] = true; bools_05[15] = true;

            bools_06[3] = true; bools_06[4] = true; bools_06[5] = true; bools_06[8] = true;
            bools_06[10] = true; bools_06[11] = true; bools_06[12] = true; bools_06[14] = true;
            bools_06[15] = true;

            bools_07[3] = true; bools_07[4] = true; bools_07[5] = true; bools_07[10] = true;
            bools_07[11] = true; bools_07[12] = true; bools_07[14] = true; bools_07[15] = true;

            bools_08[3] = true; bools_08[6] = true; bools_08[10] = true; bools_08[11] = true;
            bools_08[12] = true; bools_08[14] = true; bools_08[15] = true;

            bools_09[3] = true; bools_09[6] = true; bools_09[10] = true; bools_09[12] = true;
            bools_09[14] = true; bools_09[15] = true;

            bools_10[3] = true;

            List<bool[]> lst_bools = new List<bool[]>()
            {
                bools_01,bools_02,bools_03,bools_04,bools_05,bools_06,bools_07,bools_08,bools_09,bools_10,
            };
            if (DebugParameter.isTestCarrierInterfaceError)
            {
                RandomSetCSTInterfaceBool(bools_03);
                RandomSetCSTInterfaceBool(bools_04);
                RandomSetCSTInterfaceBool(bools_05);
                RandomSetCSTInterfaceBool(bools_06);
                RandomSetCSTInterfaceBool(bools_07);
                RandomSetCSTInterfaceBool(bools_08);
                RandomSetCSTInterfaceBool(bools_09);
                //lst_bools[6][11] = false;
            }
            string port_id = "";
            scApp.MapBLL.getPortID(vh.CUR_ADR_ID, out port_id);

            //scApp.PortBLL.OperateCatch.updatePortStationCSTExistStatus(port_id, string.Empty);

            CarrierInterface_LogOut(vh.VEHICLE_ID, port_id, lst_bools);
        }

        private static void RandomSetCSTInterfaceBool(bool[] bools_03)
        {
            Random rnd_Index = new Random(Guid.NewGuid().GetHashCode());
            int rnd_value_1 = rnd_Index.Next(bools_03.Length - 1);
            int rnd_value_2 = rnd_Index.Next(bools_03.Length - 1);
            int rnd_value_3 = rnd_Index.Next(bools_03.Length - 1);
            int rnd_value_4 = rnd_Index.Next(bools_03.Length - 1);
            int rnd_value_5 = rnd_Index.Next(bools_03.Length - 1);
            int rnd_value_6 = rnd_Index.Next(bools_03.Length - 1);
            bools_03[rnd_value_1] = true;
            bools_03[rnd_value_2] = true;
            bools_03[rnd_value_3] = true;
            bools_03[rnd_value_4] = true;
            bools_03[rnd_value_5] = true;
            bools_03[rnd_value_6] = true;
        }

        private void CarrierInterfaceSim_UnloadComplete(AVEHICLE vh, string carrier_id)
        {
            //vh.CatchPLCCSTInterfacelog();
            VehicleCSTInterface vehicleCSTInterface = new VehicleCSTInterface();
            bool[] bools_01 = new bool[16];
            bool[] bools_02 = new bool[16];
            bool[] bools_03 = new bool[16];
            bool[] bools_04 = new bool[16];
            bool[] bools_05 = new bool[16];
            bool[] bools_06 = new bool[16];
            bool[] bools_07 = new bool[16];
            bool[] bools_08 = new bool[16];
            bool[] bools_09 = new bool[16];
            bool[] bools_10 = new bool[16];

            bools_01[3] = true;

            bools_02[03] = true; bools_02[9] = true; bools_02[12] = true; bools_02[14] = true;
            bools_02[15] = true;

            bools_03[3] = true; bools_03[9] = true; bools_03[10] = true; bools_03[12] = true;
            bools_03[14] = true; bools_03[15] = true;

            bools_04[3] = true; bools_04[4] = true; bools_04[9] = true; bools_04[10] = true;
            bools_04[12] = true; bools_04[14] = true; bools_04[15] = true;

            bools_05[3] = true; bools_05[4] = true; bools_05[9] = true; bools_05[10] = true;
            bools_05[11] = true; bools_05[12] = true; bools_05[14] = true; bools_05[15] = true;

            bools_06[3] = true; bools_06[4] = true; bools_06[5] = true; bools_06[9] = true;
            bools_06[10] = true; bools_06[11] = true; bools_06[12] = true; bools_06[14] = true;
            bools_06[15] = true;

            bools_07[3] = true; bools_07[4] = true; bools_07[5] = true; bools_07[10] = true;
            bools_07[11] = true; bools_07[12] = true; bools_07[14] = true; bools_07[15] = true;

            bools_08[3] = true; bools_08[6] = true; bools_08[10] = true; bools_08[11] = true;
            bools_08[12] = true; bools_08[14] = true; bools_08[15] = true;

            bools_09[3] = true; bools_09[6] = true; bools_09[10] = true; bools_09[12] = true;
            bools_09[14] = true; bools_09[15] = true;

            bools_10[3] = true;
            List<bool[]> lst_bools = new List<bool[]>()
            {
                bools_01,bools_02,bools_03,bools_04,bools_05,bools_06,bools_07,bools_08,bools_09,bools_10,
            };
            if (DebugParameter.isTestCarrierInterfaceError)
            {
                RandomSetCSTInterfaceBool(bools_03);
                RandomSetCSTInterfaceBool(bools_04);
                RandomSetCSTInterfaceBool(bools_05);
                RandomSetCSTInterfaceBool(bools_06);
                RandomSetCSTInterfaceBool(bools_07);
                RandomSetCSTInterfaceBool(bools_08);
                RandomSetCSTInterfaceBool(bools_09);
            }
            string port_id = "";
            scApp.MapBLL.getPortID(vh.CUR_ADR_ID, out port_id);
            //scApp.PortBLL.OperateCatch.updatePortStationCSTExistStatus(port_id, carrier_id);

            CarrierInterface_LogOut(vh.VEHICLE_ID, port_id, lst_bools);
        }

        private static void CarrierInterface_LogOut(string vh_id, string port_id, List<bool[]> lst_bools)
        {
            VehicleCSTInterface vehicleCSTInterface = new VehicleCSTInterface();
            foreach (var bools in lst_bools)
            {
                DateTime now_time = DateTime.Now;
                vehicleCSTInterface.Details.Add(new VehicleCSTInterface.CSTInterfaceDetail()
                {
                    EQ_ID = vh_id,
                    //PORT_ID = port_id,
                    LogIndex = $"Recode{nameof(VehicleCSTInterface)}",
                    CSTInterface = bools,
                    Year = (ushort)now_time.Year,
                    Month = (ushort)now_time.Month,
                    Day = (ushort)now_time.Day,
                    Hour = (ushort)now_time.Hour,
                    Minute = (ushort)now_time.Minute,
                    Second = (ushort)now_time.Second,
                    Millisecond = (ushort)now_time.Millisecond,
                });
                SpinWait.SpinUntil(() => false, 100);
            }
            foreach (var detail in vehicleCSTInterface.Details)
            {
                LogManager.GetLogger("RecodeVehicleCSTInterface").Info(detail.ToString());
            }
        }

        #endregion TEST

        #region 轉轍器alarm發生的對應事件處理
        private void trackAlarmHappend(object sender, Track.alarmCodeChangeArgs e)
        {
            //要再上報Alamr Rerport給MCS
            //scApp.TransferService.OHBC_AlarmSet(scApp.getEQObjCacheManager().getLine().LINE_ID, ((int)AlarmLst.OHT_CommandNotFinishedInTime).ToString());
            foreach (Track.TrackAlarm alarm in e.AddAlarmList)
            {
                string alarmCode = ((int)alarm).ToString();
                string alarmDesc = alarm.ToString();
                //逐一上報
                scApp.TransferService.OHBC_AlarmSet(e.railChanger_No, alarmCode, alarmDesc, "");
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    Data: $"Track({e.railChanger_No}) alarm is happend, alarm Code:{alarmCode}, alarm Desc: {alarmDesc} ");
                //Data: $"Find vehicle {vehicleCache.VEHICLE_ID}, vehicle address Id = {vehicleCache.CUR_ADR_ID}, = port address ID {portAddressID}");
            }
            //如果有已經移除的alarm要逐一清掉
            foreach (Track.TrackAlarm alarm in e.RemoveAlarmList)
            {
                string alarmCode = ((int)alarm).ToString();
                string alarmDesc = alarm.ToString();
                //逐一清除
                scApp.TransferService.OHBC_AlarmCleared(e.railChanger_No, alarmCode);
                LogHelper.Log(logger: logger, LogLevel: LogLevel.Info, Class: nameof(VehicleService), Device: DEVICE_NAME_OHx,
                    Data: $"Track({e.railChanger_No}) alarm is cleared, alarm Code:{alarmCode}, alarm Desc: {alarmDesc} ");
            }
        }
        #endregion
    }
}