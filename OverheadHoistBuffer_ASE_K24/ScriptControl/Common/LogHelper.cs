using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.stc.Data.SecsData;
using Google.Protobuf;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace com.mirle.ibg3k0.sc.Common
{
    public static class LogHelper
    {
        public const string CALL_CONTEXT_KEY_WORD_SERVICE_ID = "SERVICE_ID";
        static ObjectPool<LogObj> LogObjPool = new ObjectPool<LogObj>(() => new LogObj());
        static Logger logger = LogManager.GetCurrentClassLogger();


        public static void setCallContextKey_ServiceID(string service_id)
        {
            string xid = System.Runtime.Remoting.Messaging.CallContext.GetData(CALL_CONTEXT_KEY_WORD_SERVICE_ID) as string;
            if (SCUtility.isEmpty(xid))
            {
                System.Runtime.Remoting.Messaging.CallContext.SetData(LogHelper.CALL_CONTEXT_KEY_WORD_SERVICE_ID, service_id);
            }
        }

        public static void Log(Logger logger, NLog.LogLevel LogLevel,
            string Class, string Device, SXFY Data,
            string VehicleID = null, string CarrierID = null, string LogID = null, string Level = null, string ThreadID = null, string Lot = null, string XID = null, string Transaction = null,
            [CallerMemberName] string Method = "")
        {
            //如果被F'Y'，Y可以被2整除的話代表是收到的
            bool isReceive = Data.getF() % 2 == 0;
            LogConstants.Type type = isReceive ? LogConstants.Type.Receive : LogConstants.Type.Send;
            Log(logger, LogLevel, Class, Device,
                Data: $"[{Data.SystemByte}]{Data.StreamFunction}-{Data.StreamFunctionName}",
                VehicleID: VehicleID,
                CarrierID: CarrierID,
                Type: type,
                LogID: LogID,
                Level: Level,
                ThreadID: ThreadID,
                Lot: Lot,
                XID: XID,
                //Details: Data.toSECSString(),
                Details: "",
                Method: Method
                );
        }


        public static void Log(Logger logger, NLog.LogLevel LogLevel,
            string Class, string Device, int seq_num, IMessage Data,
            string VehicleID = null, string CarrierID = null, string LogID = null, string Level = null, string ThreadID = null, string Lot = null, string XID = null, string Transaction = null,
            [CallerMemberName] string Method = "")
        {
            string function_name = $"[{seq_num}]{Data.Descriptor.Name}";

            LogConstants.Type? type = null;
            if (function_name.Contains("_"))
            {
                int packet_id = 0;
                string[] function_name_splil = function_name.Split('_');
                if (int.TryParse(function_name_splil[1], out packet_id))
                {
                    type = packet_id > 100 ? LogConstants.Type.Receive : LogConstants.Type.Send;
                }
            }
            Log(logger, LogLevel, Class, Device,
            Data: function_name,
            VehicleID: VehicleID,
            CarrierID: CarrierID,
            Type: type,
            LogID: LogID,
            Level: Level,
            ThreadID: ThreadID,
            Lot: Lot,
            XID: XID,
            Details: Data.ToString(),
            Method: Method
            );
        }

        public static void Log(Logger logger, NLog.LogLevel LogLevel,
            string Class, string Device, Exception Data,
            string VehicleID = null, string CarrierID = null, string LogID = null, string Level = null, string ThreadID = null, string Lot = null, string XID = null, string Details = null,
            [CallerMemberName] string Method = "")
        {
            Log(logger, LogLevel, Class, Device,
                Data: Data.ToString(),
                VehicleID: VehicleID,
                CarrierID: CarrierID,
                LogID: LogID,
                Level: Level,
                ThreadID: ThreadID,
                Lot: Lot,
                XID: XID,
                Details: Details,
                Method: Method
                );
        }

        public static void Log(Logger logger, NLog.LogLevel LogLevel,
            string Class, string Device, string Data,
            string VehicleID = null, string CarrierID = null, LogConstants.Type? Type = null, string LogID = null, string Level = null, string ThreadID = null, string Lot = null, string XID = null, string Details = null,
            [CallerMemberName] string Method = "")
        {
            LogObj logObj = LogObjPool.GetObject();
            try
            {
                logObj.dateTime = DateTime.Now;
                logObj.Sequence = getSequence();
                logObj.LogLevel = LogLevel.Name;
                logObj.Class = Class;
                logObj.Method = Method;
                logObj.Device = Device;
                logObj.Data = Data;
                logObj.VH_ID = VehicleID;
                logObj.CarrierID = CarrierID;

                logObj.Type = Type;
                logObj.LogID = LogID;
                logObj.ThreadID = ThreadID != null ?
                    ThreadID : Thread.CurrentThread.ManagedThreadId.ToString();
                logObj.Lot = Lot;
                logObj.Level = Level;

                string xid = System.Runtime.Remoting.Messaging.CallContext.GetData(CALL_CONTEXT_KEY_WORD_SERVICE_ID) as string;
                logObj.XID = xid;

                Transaction Transaction = getCurrentTransaction();
                logObj.TransactionID = Transaction == null ?
                    string.Empty : Transaction.TransactionInformation.LocalIdentifier.ToString();
                logObj.Details = Details;
                logObj.Index = "SystemProcessLog";

                LogHelper.logger.Log(LogLevel, logObj.ToString());

                SYSTEMPROCESS_INFO systemProc = new SYSTEMPROCESS_INFO();
                systemProc.TIME = DateTime.Now.ToString(SCAppConstants.DateTimeFormat_23);
                systemProc.SEQ = logObj.Sequence;
                systemProc.LOGLEVEL = LogLevel.Name == null ? string.Empty : LogLevel.Name;
                systemProc.CLASS = Class == null ? string.Empty : Class;
                systemProc.METHOD = Method == null ? string.Empty : Method;
                systemProc.DEVICE = Device == null ? string.Empty : Device;
                systemProc.DATA = Data == null ? string.Empty : Data;
                systemProc.VHID = VehicleID == null ? string.Empty : VehicleID;
                systemProc.CRRID = CarrierID == null ? string.Empty : CarrierID;
                systemProc.TYPE = Type.ToString();
                systemProc.LOGID = LogID == null ? string.Empty : LogID;
                systemProc.THREADID = logObj.ThreadID;
                systemProc.LOT = Lot == null ? string.Empty : Lot;
                systemProc.LEVEL = Level == null ? string.Empty : Level;
                systemProc.XID = xid == null ? string.Empty : xid;
                systemProc.TRXID = logObj.TransactionID;
                systemProc.DETAILS = Details == null ? string.Empty : Details;
                System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(SCApplication.getInstance().LineService.PublishSystemMsgInfo), systemProc);
            }
            catch (Exception e)
            {
                LogHelper.logger.Error($"{e}, Exception");
            }
            finally
            {
                LogObjPool.PutObject(logObj);
            }
        }


        public static void LogBCRReadInfo(string VehicleID, string portID, string mcsCmdID, string ohtcCmdID, string carrierID, string readCarrierID, ProtocolFormat.OHTMessage.BCRReadResult bCRReadResult,
                                          bool IsEnableIDReadFailScenario, [CallerMemberName] string Method = "")
        {
            try
            {
                dynamic logEntry = new Newtonsoft.Json.Linq.JObject();
                logEntry.dateTime = DateTime.Now;
                logEntry.Method = Method;
                logEntry.VH_ID = VehicleID;
                logEntry.PortID = SCUtility.Trim(portID);
                logEntry.CarrierID = SCUtility.Trim(carrierID);
                logEntry.ReadCarrierID = SCUtility.Trim(readCarrierID);
                logEntry.MCS_CMD_ID = SCUtility.Trim(mcsCmdID);
                logEntry.OHTC_CMD_ID = SCUtility.Trim(ohtcCmdID);
                logEntry.BCRReadResult = bCRReadResult.ToString();
                logEntry.IsEnableIDReadFailScenario = IsEnableIDReadFailScenario;
                logEntry.Index = "BCRReadInfo";

                var json = logEntry.ToString(Newtonsoft.Json.Formatting.None);
                json = json.Replace("dateTime", "@timestamp");
                LogManager.GetLogger("BCRReadInfo").Info(json);
            }
            catch (Exception e)
            {
                LogHelper.logger.Error($"{e}, Exception");
            }
        }


        public static string PrintMessage(IMessage message, string cmdID, string cmsMCSID)
        {
            var descriptor = message.Descriptor;
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"Name:{descriptor.Name}");
            sb.AppendLine($"CmdID:{cmdID}");
            sb.AppendLine($"CmdMCSID:{cmsMCSID}");
            foreach (var field in descriptor.Fields.InDeclarationOrder())
            {
                sb.AppendLine($"{field.Name} : {field.Accessor.GetValue(message)}");
            }
            return sb.ToString();
        }
        public static string getIMessageName(IMessage message)
        {
            var descriptor = message.Descriptor;
            string name = descriptor.Name;
            if (name.Contains('_'))
            {
                string[] name_temp = name.Split('_');
                if (name_temp.Length >= 2)
                {
                    name = $"{name_temp[0]}_{name_temp[1]}";
                }
            }
            return name;
        }
        public static void RecordReportInfoByQueue(sc.App.SCApplication scApp, sc.BLL.CMDBLL cmdBLL, AVEHICLE vh, IMessage message, int seqNum, [CallerMemberName] string Method = "")
        {
            LogHelper.RecordReportInfoNew(cmdBLL, vh, message, seqNum, Method);

            //var workItem = new com.mirle.ibg3k0.bcf.Data.BackgroundWorkItem(cmdBLL, vh, message, seqNum, Method);//A0.05
            //scApp.BackgroundWorkProcRecordReportInfo.triggerBackgroundWork($"BlockQueue_{vh.VEHICLE_ID}", workItem);//A0.05
        }

        public static void RecordReportInfoNew(sc.BLL.CMDBLL cmdBLL, AVEHICLE vh, IMessage message, int seqNum, [CallerMemberName] string Method = "")
        {
            dynamic logEntry = new JObject();
            DateTime nowDt = DateTime.Now;
            string vhID = vh.VEHICLE_ID;
            string current_excute_cmd_id = SCUtility.Trim(vh.OHTC_CMD, true);
            string current_excute_cmd_mcs_id = SCUtility.Trim(vh.MCS_CMD, true);
            string detail = PrintMessage(message, current_excute_cmd_id, current_excute_cmd_mcs_id);
            string function = getIMessageName(message);
            logEntry.dateTime = nowDt.ToString(SCAppConstants.DateTimeFormat_23);

            logEntry.eq_id = vhID;
            logEntry.name = function;
            logEntry.seq_no = seqNum;
            logEntry.type = "";

            logEntry.detail = detail;

            var json = logEntry.ToString(Newtonsoft.Json.Formatting.None);
            LogManager.GetLogger("RecordReportInfo").Info(json);
        }


        public static void RecordReportInfo(sc.BLL.CMDBLL cmdBLL, AVEHICLE vh, IMessage message, int seqNum, [CallerMemberName] string Method = "")
        {
            Logger logger = LogManager.GetLogger("RecordReportInfo");
            string vhID = vh.VEHICLE_ID;
            string detail = PrintMessage(message, "", "");
            string function = getIMessageName(message);
            if (message is ID_31_TRANS_REQUEST)
            {
                var id_31 = message as ID_31_TRANS_REQUEST;
                var cmd_id = id_31.CmdID;
                string cst_id = id_31.CSTID;
                string lot_id = id_31.LOTID;
                var command_action = id_31.ActType;
                var load_adr = id_31.LoadAdr;
                var dest_adr = id_31.ToAdr;
                var load_port = id_31.LoadPortID;
                var unload_port = id_31.UnloadPortID;
                string display_load = sc.Common.SCUtility.isEmpty(load_port) ? load_adr : $"{load_port}({load_adr})";
                string display_dest = sc.Common.SCUtility.isEmpty(unload_port) ? dest_adr : $"{unload_port}({dest_adr})";

                var cmd = cmdBLL.cache.getExcuteCmd(cmd_id);
                string tran_id = "";
                if (cmd != null)
                {
                    tran_id = SCUtility.Trim(cmd.CMD_ID_MCS);
                }
                var attenion_vaule_1 = new { cmdID = cmd_id, tranID = tran_id, cstID = cst_id, lotID = lot_id, commandAction = command_action, load = display_load, dest = display_dest };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenion_vaule_1);
            }
            else if (message is ID_131_TRANS_RESPONSE)
            {
                var id_131 = message as ID_131_TRANS_RESPONSE;
                var cmd_id = id_131.CmdID;
                var command_action = id_131.ActType;
                var reply_code = id_131.ReplyCode;
                var ng_reason = id_131.NgReason;
                var vehicle_c_ngreason = id_131.NgReason;
                var cmd = cmdBLL.cache.getExcuteCmd(cmd_id);
                string tran_id = "";
                if (cmd != null)
                {
                    tran_id = SCUtility.Trim(cmd.CMD_ID_MCS);
                }

                var attenion_vaule_1 = new { cmdID = cmd_id, tranID = tran_id, commandAction = command_action, replyCode = reply_code, NgReason = ng_reason, vehicleControlNgReason = vehicle_c_ngreason };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenion_vaule_1);

            }
            else if (message is ID_132_TRANS_COMPLETE_REPORT)
            {
                var id_132 = message as ID_132_TRANS_COMPLETE_REPORT;
                var cmd_id = id_132.CmdID;
                string cst_id = id_132.CSTID;
                var complete_status = id_132.CmpStatus;
                var cmd = cmdBLL.cache.getExcuteCmd(cmd_id);
                string tran_id = "";
                if (cmd != null)
                {
                    tran_id = SCUtility.Trim(cmd.CMD_ID_MCS);
                }
                var attenion_vaule_1 = new { cmdID = cmd_id, tranID = tran_id, cstID = cst_id, completeStatus = complete_status };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenion_vaule_1);
            }
            else if (message is ID_32_TRANS_COMPLETE_RESPONSE)
            {
                var id_32 = message as ID_32_TRANS_COMPLETE_RESPONSE;
                var reply_code = id_32.ReplyCode;

                var attenion_vaule_1 = new { replyCode = reply_code };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenion_vaule_1);

            }
            else if (message is ID_134_TRANS_EVENT_REP)
            {
                var id_134 = message as ID_134_TRANS_EVENT_REP;
                var attenion_vaule = new { secID = id_134.CurrentSecID, adrID = id_134.CurrentSecID, distance = id_134.SecDistance };
                logger.WithProperty("msgDetail", detail).Info(vh, "{method} | {vhID} | {seqNum} {@attenion_vaule}"
                                                                , function
                                                                , vhID
                                                                , seqNum
                                                                , attenion_vaule);
            }
            else if (message is ID_136_TRANS_EVENT_REP)
            {
                var id_136 = message as ID_136_TRANS_EVENT_REP;
                var event_tpye = id_136.EventType;
                string cmd_id = SCUtility.Trim(vh.OHTC_CMD, true);
                string trna_id = "";
                var cmd = cmdBLL.cache.getExcuteCmd(cmd_id);
                string location = getLoction(id_136);
                if (cmd != null)
                {
                    trna_id = SCUtility.Trim(cmd.CMD_ID_MCS);
                }
                switch (event_tpye)
                {
                    case EventType.LoadArrivals:
                    case EventType.Vhloading:
                    case EventType.LoadComplete:
                    case EventType.UnloadArrivals:
                    case EventType.Vhunloading:
                    case EventType.UnloadComplete:
                        var attenion_vaule_1 = new { cmdID = cmd_id, tranID = trna_id, cstID = id_136.CSTID, location = location };
                        logger.WithProperty("msgDetail", detail).
                               Info(vh, "{method} | {eventType} | {vhID} | {seqNum} {@attenionVaule}"
                                        , function, event_tpye, vhID, seqNum, attenion_vaule_1);
                        break;
                    case EventType.ReserveReq:
                        break;
                    case EventType.BlockReq:
                        var attenion_vaule_2 = new { reqSec = id_136.RequestBlockID };
                        logger.WithProperty("msgDetail", detail).
                               Info(vh, "{method} | {eventType} | {vhID} | {seqNum} {@attenionVaule}"
                                        , function, event_tpye, vhID, seqNum, attenion_vaule_2);
                        break;
                    case EventType.Bcrread:
                        var attenion_vaule_3 = new { cmdID = cmd_id, BcrReadResult = id_136.BCRReadResult, cstID = id_136.CSTID };
                        logger.WithProperty("msgDetail", detail).
                               Info(vh, "{method} | {eventType} | {vhID} | {seqNum} {@attenionVaule}"
                                        , function, event_tpye, vhID, seqNum, attenion_vaule_3);
                        break;
                    default:
                        logger.WithProperty("msgDetail", detail).
                               Info(vh, "{method} | {eventType} | {vhID} | {seqNum}"
                                       , function, event_tpye, vhID, seqNum);
                        break;
                }
            }
            else if (message is ID_36_TRANS_EVENT_RESPONSE)
            {
                var id_36 = message as ID_36_TRANS_EVENT_RESPONSE;
                string cmd_id = SCUtility.Trim(vh.OHTC_CMD, true);
                string trna_id = "";
                var cmd = cmdBLL.cache.getExcuteCmd(cmd_id);
                var event_type = id_36.EventType;

                if (cmd != null)
                {
                    trna_id = SCUtility.Trim(cmd.CMD_ID_MCS);
                }

                switch (event_type)
                {
                    case EventType.LoadArrivals:
                    case EventType.Vhloading:
                    case EventType.LoadComplete:
                    case EventType.UnloadArrivals:
                    case EventType.Vhunloading:
                    case EventType.UnloadComplete:
                        var attenionVaule_1 = new { cmdID = cmd_id, tranID = trna_id, replyAction = id_36.ReplyActiveType };
                        logger.WithProperty("msgDetail", detail).
                               Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                        , function, event_type, vhID, seqNum, attenionVaule_1);
                        break;
                    case EventType.ReserveReq:

                        break;
                    case EventType.BlockReq:
                        var attenionVaule_2 = new { cmdID = cmd_id, isReserveOK = id_36.IsBlockPass };
                        logger.WithProperty("msgDetail", detail).
                               Info(vh, "{method} | {eventType} | {vhID} | {seqNum} {@attenionVaule}"
                                        , function, event_type, vhID, seqNum, attenionVaule_2);
                        break;
                    case EventType.Bcrread:
                        var attenionVaule_3 = new { cmdID = cmd_id, replyAction = id_36.ReplyActiveType, renameID = id_36.RenameBOXID };
                        logger.WithProperty("msgDetail", detail).
                               Info(vh, "{method} | {eventType} | {vhID} | {seqNum} {@attenionVaule}"
                                        , function, event_type, vhID, seqNum, attenionVaule_3);
                        break;
                    default:
                        logger.WithProperty("msgDetail", detail).
                               Info(vh, "{method} | {eventType} | {vhID} | {seqNum}"
                                        , function, event_type, vhID, seqNum);
                        break;
                }
            }
            else if (message is ID_37_TRANS_CANCEL_REQUEST)
            {
                var id_37 = message as ID_37_TRANS_CANCEL_REQUEST;
                var cmd_id = id_37.CmdID;
                var cancel_action = id_37.ActType;
                var attenion_vaule_1 = new { cmdID = cmd_id, cancelAction = cancel_action };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenion_vaule_1);
            }
            else if (message is ID_137_TRANS_CANCEL_RESPONSE)
            {
                var id_137 = message as ID_137_TRANS_CANCEL_RESPONSE;
                var cmd_id = id_137.CmdID;
                var cancel_action = id_137.ActType;
                var reply_code = id_137.ReplyCode;
                var attenion_vaule_1 = new { cmdID = cmd_id, cancelAction = cancel_action, replyCode = reply_code };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenion_vaule_1);
            }
            else if (message is ID_39_PAUSE_REQUEST)
            {
                var id_39 = message as ID_39_PAUSE_REQUEST;
                var event_tpye = id_39.EventType;
                var pause_tpye = id_39.PauseType;
                var attenion_vaule_1 = new { pauseEventType = event_tpye, pauseType = pause_tpye };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenion_vaule_1);
            }
            else if (message is ID_139_PAUSE_RESPONSE)
            {
                var id_139 = message as ID_139_PAUSE_RESPONSE;
                var event_tpye = id_139.EventType;
                var attenion_vaule_1 = new { pauseEventType = event_tpye, repleCode = id_139.ReplyCode };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenion_vaule_1);
            }
            else if (message is ID_43_STATUS_REQUEST)
            {
                var id_43 = message as ID_43_STATUS_REQUEST;
                var attenionVaule_1 = new
                {
                    time = id_43.SystemTime
                };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenionVaule_1);
            }
            else if (message is ID_143_STATUS_RESPONSE)
            {
                var id_143 = message as ID_143_STATUS_RESPONSE;
                var mode = id_143.ModeStatus;
                var action = id_143.ActionStatus;
                var error = id_143.ErrorStatus == VhStopSingle.StopSingleOn;
                var pause = id_143.PauseStatus == VhStopSingle.StopSingleOn;
                var block = id_143.BlockingStatus == VhStopSingle.StopSingleOn;
                var obs = id_143.ObstacleStatus == VhStopSingle.StopSingleOn;
                var hid = id_143.HIDStatus == VhStopSingle.StopSingleOn;
                var earquake = id_143.EarthquakePauseTatus == VhStopSingle.StopSingleOn;
                var safety = id_143.SafetyPauseStatus == VhStopSingle.StopSingleOn;
                var has_box = id_143.HasBox;
                var cmd_box_id = id_143.BOXID;
                var car_box_id = id_143.CarBoxID;

                var attenionVaule_1 = new
                {
                    Mode = mode,
                    Action = action,
                    Error = error,
                    Pause = pause,
                    Block = block,
                    Obs = obs,
                    HID = hid,
                    Earquake = earquake,
                    Safety = safety,
                    HasBox = has_box,
                    CmdBoxID = cmd_box_id,
                    CarBoxID = car_box_id
                };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenionVaule_1);
            }
            else if (message is ID_144_STATUS_CHANGE_REP)
            {
                var id_144 = message as ID_144_STATUS_CHANGE_REP;
                var mode = id_144.ModeStatus;
                var action = id_144.ActionStatus;
                var error = id_144.ErrorStatus == VhStopSingle.StopSingleOn;
                var pause = id_144.PauseStatus == VhStopSingle.StopSingleOn;
                var block = id_144.BlockingStatus == VhStopSingle.StopSingleOn;
                var obs = id_144.ObstacleStatus == VhStopSingle.StopSingleOn;
                var hid = id_144.HIDStatus == VhStopSingle.StopSingleOn;
                var earquake = id_144.EarthquakePauseTatus == VhStopSingle.StopSingleOn;
                var safety = id_144.SafetyPauseStatus == VhStopSingle.StopSingleOn;
                var has_box = id_144.HasBox;
                var cmd_box_id = id_144.BOXID;
                var car_box_id = id_144.CarBoxID;

                var attenionVaule_1 = new
                {
                    Mode = mode,
                    Action = action,
                    Error = error,
                    Pause = pause,
                    Block = block,
                    Obs = obs,
                    HID = hid,
                    Earquake = earquake,
                    Safety = safety,
                    HasBox = has_box,
                    CmdBoxID = cmd_box_id,
                    CarBoxID = car_box_id
                };
                logger.WithProperty("msgDetail", detail).
                       Info(vh, "{method} | {vhID} | {seqNum} {@attenionVaule}"
                                , function, vhID, seqNum, attenionVaule_1);
            }

        }

        public static void RecordHostReportInfoAsk(IMessage message, [CallerMemberName] string method = "", int seqNum = 0)
        {
            RecordHostReportInfo(message, $"{method}Ask", seqNum);
        }
        public static void RecordHostReportInfo(IMessage message, [CallerMemberName] string method = "", int seqNum = 0)
        {
            recodeLog(message, "NTB", method, "", "", method.Contains("Ask"));


        }
        static Logger NTBlogger = LogManager.GetLogger("NTBLogger");
        public const string TITLE_NAME_EQID = "EQ ID";
        public const string TITLE_NAME_TIME = "T";
        public const string TITLE_NAME_FUNNAME = "Name";
        public const string TITLE_NAME_ID = "ID";
        public const string TITLE_NAME_SEQ_NO = "Seq no";
        public const string TITLE_NAME_TYPE = "Type";
        public const string TITLE_NAME_KEYWORD = "Key Word";

        public const string CHAR_TRILE_STAR = "-";
        public const string CHAR_LEFT_BRACKETS = "[";
        public const string CHAR_RIGHT_BRACKETS = "]";
        public const string CHAR_COLON = ":";
        public const string CHAR_BREAK = " ";
        public const string CHAR_TAB_BREAK = "  ";
        public const string CHAR_EQUAL = "=";
        public static void recodeLog(IMessage msg, string eq_name, string fun_name, string id, string seq_no, bool isRece)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.
            Append(CHAR_TRILE_STAR).Append(TITLE_NAME_EQID).Append(CHAR_COLON).
            Append(CHAR_LEFT_BRACKETS).Append(eq_name).
            AppendLine(CHAR_RIGHT_BRACKETS).

            Append(CHAR_TRILE_STAR).Append(TITLE_NAME_FUNNAME).Append(CHAR_COLON).
            Append(CHAR_LEFT_BRACKETS).Append(fun_name).
            AppendLine(CHAR_RIGHT_BRACKETS).

            Append(CHAR_TRILE_STAR).Append(TITLE_NAME_TYPE).Append(CHAR_COLON).
            Append(CHAR_LEFT_BRACKETS).Append(isRece ? "Receive" : "Send").
            AppendLine(CHAR_RIGHT_BRACKETS);

            foreach (var field in msg.Descriptor.Fields.InDeclarationOrder())
            {
                object obj = field.Accessor.GetValue(msg);
                sb.Append(CHAR_TAB_BREAK);
                sb.Append(CHAR_TAB_BREAK);
                sb.Append(field.Name);
                sb.Append(CHAR_BREAK);
                sb.Append(CHAR_EQUAL);
                sb.Append(CHAR_BREAK);
                sb.AppendLine(obj.ToString());
            }
            NTBlogger.Info(sb.ToString());
        }

        private static string getLoction(ID_136_TRANS_EVENT_REP id_136)
        {
            switch (id_136.EventType)
            {
                case EventType.Vhloading:
                case EventType.LoadArrivals:
                case EventType.LoadComplete:
                    return id_136.LoadPortID;
                case EventType.Vhunloading:
                case EventType.UnloadArrivals:
                case EventType.UnloadComplete:
                    return id_136.UnloadPortID;
                default:
                    return "";
            }
        }
        private static Transaction getCurrentTransaction()
        {
            try
            {
                Transaction Transaction = Transaction.Current;
                return Transaction;
            }
            catch { return null; }
        }


        static object sequence_lock = new object();
        static UInt64 NextSequence = 1;
        static private UInt64 getSequence()
        {
            lock (sequence_lock)
            {
                UInt64 currentSeq = NextSequence;
                NextSequence++;
                return currentSeq;
            }

        }


    }

    public static class LogConstants
    {
        public enum Type
        {
            Send,
            Receive
        }
    }
}
