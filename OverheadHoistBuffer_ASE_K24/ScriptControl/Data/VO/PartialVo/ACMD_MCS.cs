﻿//**********************************************************************************
// Date          Author         Request No.    Tag         Description
// ------------- -------------  -------------  ------      -----------------------------
// 2020/05/22    Jason Wu       N/A            A20.05.22   新增與shelfDef 相同的clone method.
// 2020/06/04    Jason Wu       N/A            A20.06.04   修改priority判定部分(由僅用priority sum大小比較 變為分組比較99 up or 99 down)
// 2020/06/09    Jason Wu       N/A            A20.06.09.0 修改判定部分(新增判定來源目的地是非shelf的優先)
//**********************************************************************************
using com.mirle.ibg3k0.sc.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class ACMD_MCS
    {
        public static ConcurrentDictionary<string, ACMD_MCS> MCS_CMD_InfoList { get; private set; } = new ConcurrentDictionary<string, ACMD_MCS>();
        public static void tryAddCMD_MCS_ToList(ACMD_MCS cmdMCS)
        {
            string cmd_id = sc.Common.SCUtility.Trim(cmdMCS.CMD_ID, true);
            MCS_CMD_InfoList.TryAdd(cmd_id, cmdMCS);
        }
        public static List<ACMD_MCS> tryGetMCSCommandList()
        {
            var cmd_mcs_Key_value_array = MCS_CMD_InfoList.ToArray();
            var cmd_mcs_list = cmd_mcs_Key_value_array.Select(kv => kv.Value).ToList();
            return cmd_mcs_list;
        }

        //**********************************************************************************
        //A20.05.22 給定一個私有變數去儲存2點間距離
        private int _distanceFromVehicleToHostSource;

        /// <summary>
        /// 1 2 4 8 16 32 64 128
        /// 1 1 1 1 1  1  1  1
        /// 1 0 0 0 ...
        /// 1 1 0 0 ....
        /// 1 1 1 0 ....
        /// </summary>
        public const int COMMAND_iIdle = 0;

        public const int COMMAND_STATUS_BIT_INDEX_ENROUTE = 1;
        public const int COMMAND_STATUS_BIT_INDEX_LOAD_ARRIVE = 2;
        public const int COMMAND_STATUS_BIT_INDEX_LOADING = 4;
        public const int COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE = 8;
        public const int COMMAND_STATUS_BIT_INDEX_UNLOAD_ARRIVE = 16;
        public const int COMMAND_STATUS_BIT_INDEX_UNLOADING = 32;
        public const int COMMAND_STATUS_BIT_INDEX_UNLOAD_COMPLETE = 64;
        public const int COMMAND_STATUS_BIT_INDEX_COMMNAD_FINISH = 128;

        public const int COMMAND_STATUS_BIT_INDEX_DOUBLE_STORAGE = 256;     //二重格，異常流程
        public const int COMMAND_STATUS_BIT_INDEX_EMPTY_RETRIEVAL = 512;    //空取、異常流程
        public const int COMMAND_STATUS_BIT_INDEX_InterlockError = 1024;    //交握異常
        public const int COMMAND_STATUS_BIT_INDEX_VEHICLE_ABORT = 2048;     //車子異常結束
        public const int COMMAND_STATUS_BIT_INDEX_CST_TYPE_MISMATCH = 4096;     //CST Type Mismatch異常流程
        public const int COMMAND_STATUS_BIT_INDEX_INTER_ERROR_WHEN_LOAD = 8192;     //CST Type Mismatch異常流程
        public const int COMMAND_STATUS_BIT_INDEX_INTER_ERROR_WHEN_UNLOAD = 16384;     //CST Type Mismatch異常流程

        public const string Successful = "1";

        //**********************************************************************************
        //A20.05.22 給定呼叫該變數之method
        public int DistanceFromVehicleToHostSource
        {
            get { return _distanceFromVehicleToHostSource; }
            set { _distanceFromVehicleToHostSource = value; }
        }

        public bool isLoading
        {
            get
            {
                COMMANDSTATE = COMMANDSTATE & 252;
                return COMMANDSTATE == COMMAND_STATUS_BIT_INDEX_LOADING;
            }
        }

        public bool isUnloading
        {
            get
            {
                COMMANDSTATE = COMMANDSTATE & 224;
                return COMMANDSTATE == COMMAND_STATUS_BIT_INDEX_UNLOADING;
            }
        }

        public bool IsQueue
        {
            get
            {
                return TRANSFERSTATE == E_TRAN_STATUS.Queue;
            }
        }

        public bool IsTransferring
        {
            get
            {
                return COMMANDSTATE >= COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE;
            }
        }
        public bool IsLoadArriveBefore
        {
            get
            {
                return COMMANDSTATE < COMMAND_STATUS_BIT_INDEX_LOAD_ARRIVE;
            }
        }
        public bool IsScanCommand
        {
            get
            {
                return CMD_ID.StartsWith(Service.TransferService.SYMBOL_SCAN);
            }
        }

        public string getCSTType()
        {
            try
            {

                if (sc.Common.SCUtility.isEmpty(BOX_ID)) return "";
                if (BOX_ID.Length < 4)
                {
                    return "";
                }
                var sub_crrierID = BOX_ID.Substring(2, 2);
                if (sc.Common.SCUtility.isMatche(sub_crrierID, CassetteData.SYMBLE_LITE_CASSETTE))
                {
                    return CassetteData.SYMBLE_LITE_CASSETTE;
                }
                else if (sc.Common.SCUtility.isMatche(sub_crrierID, CassetteData.SYMBLE_FOUP))
                {
                    return CassetteData.SYMBLE_FOUP;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception e)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(e, "Exception:");
                return "";
            }
        }

        public bool isCarrierLightCST
        {
            get
            {
                if (sc.Common.SCUtility.isEmpty(BOX_ID)) return false;
                if (BOX_ID.Length < 4)
                {
                    return false;
                }
                var sub_crrierID = BOX_ID.Substring(2, 2);
                if (sc.Common.SCUtility.isMatche(sub_crrierID, CassetteData.SYMBLE_LITE_CASSETTE))
                {
                    return true;
                }
                return false;
            }
        }
        public bool isCarrierFoupCST
        {
            get
            {
                if (sc.Common.SCUtility.isEmpty(BOX_ID)) return false;
                if (BOX_ID.Length < 4)
                {
                    return false;
                }
                var sub_crrierID = BOX_ID.Substring(2, 2);
                if (sc.Common.SCUtility.isMatche(sub_crrierID, CassetteData.SYMBLE_FOUP))
                {
                    return true;
                }
                return false;
            }
        }



        public string CARRIER_ID { get { return BOX_ID; } }

        public bool IsCanNotServiceReasonChanged;
        public string CanNotServiceReason;
        public string PreAssignVhID;

        public enum CmdType
        {
            MCS,
            Manual,
            SCAN,
            OHBC,       //OHBC 自動產生的命令
            AGVStation, //AGV 退補 BOX
            PortTypeChange,        //控制Port流向
            MoveBack,
        }

        public class ResultCode
        {
            public const string Successful = "0";
            public const string OtherErrors = "1";
            public const string ZoneIsfull = "2";
            public const string DuplicateID = "3";
            public const string IDmismatch = "4";
            public const string IDReadFailed = "5";
            public const string BoxID_ReadFailed = "6";
            public const string BoxID_Mismatch = "7";
            public const string CarrierTypeMismatch = "8";
            public const string WarnError = "32"; //上報MCS 但不會跳Move Error
            public const string InterlockError = "64";
        }

        public enum IDreadStatus
        {
            successful = 0,
            failed = 1,
            duplicate = 2,
            mismatch = 3,
            BoxReadFail_CstIsOK = 4,
            CSTReadFail_BoxIsOK = 5,
        }

        public static string COMMAND_STATUS_BIT_To_String(int commandStatus)
        {
            switch (commandStatus)
            {
                case COMMAND_STATUS_BIT_INDEX_ENROUTE:
                    return "Enroute";

                case COMMAND_STATUS_BIT_INDEX_LOAD_ARRIVE:
                    return "Load arrive";

                case COMMAND_STATUS_BIT_INDEX_LOADING:
                    return "Loading";

                case COMMAND_STATUS_BIT_INDEX_LOAD_COMPLETE:
                    return "Load complete";

                case COMMAND_STATUS_BIT_INDEX_UNLOAD_ARRIVE:
                    return "Unload arrive";

                case COMMAND_STATUS_BIT_INDEX_UNLOADING:
                    return "Unloading";

                case COMMAND_STATUS_BIT_INDEX_UNLOAD_COMPLETE:
                    return "Unload complete";

                case COMMAND_STATUS_BIT_INDEX_COMMNAD_FINISH:
                    return "Command finish";
            }
            return "";
        }

        //**********************************************************************************
        //A20.05.22 設定與shelfDef相同之clone
        public ACMD_MCS Clone()
        {
            return (ACMD_MCS)this.MemberwiseClone();
        }

        //*******************************************************************
        //A20.05.22 用於List sort 指令 呼叫使用
        public class MCSCmdCompare_LessThan2 : IComparer<ACMD_MCS>
        {
            public int Compare(ACMD_MCS MCSCmd1, ACMD_MCS MCSCmd2)
            {
                //A20.08.04
                // -1. 判斷目的 port 為AGV者優先
                bool isCmd1_SourceTypeAGV = MCSCmd1.IsCmdSourceTypeAGV(MCSCmd1.HOSTDESTINATION);
                bool isCmd2_SourceTypeAGV = MCSCmd1.IsCmdSourceTypeAGV(MCSCmd2.HOSTDESTINATION);

                if ((isCmd1_SourceTypeAGV == true) && (isCmd2_SourceTypeAGV == true) ||
                    (isCmd1_SourceTypeAGV == false) && (isCmd2_SourceTypeAGV == false))
                {
                    //代表兩者相等，不動，且接著判斷距離
                }
                if ((isCmd1_SourceTypeAGV == false) && (isCmd2_SourceTypeAGV == true))
                {
                    return 1;
                    //代表後者較優先，換位
                }
                if ((isCmd1_SourceTypeAGV == true) && (isCmd2_SourceTypeAGV == false))
                {
                    return -1;
                    //代表前者較優先，不動
                }

                //A20.06.09.0
                // 0.判斷命令來源是否為shelf，非shelf者優先進行。
                bool isCmd1_SourceTypeShelf = MCSCmd1.IsCmdSourceTypeShelf(MCSCmd1.HOSTSOURCE);
                bool isCmd2_SourceTypeShelf = MCSCmd1.IsCmdSourceTypeShelf(MCSCmd2.HOSTSOURCE);

                if ((isCmd1_SourceTypeShelf == true) && (isCmd2_SourceTypeShelf == true) ||
                    (isCmd1_SourceTypeShelf == false) && (isCmd2_SourceTypeShelf == false))
                {
                    //代表兩者相等，不動，且接著判斷距離
                }
                if ((isCmd1_SourceTypeShelf == true) && (isCmd2_SourceTypeShelf == false))
                {
                    return 1;
                    //代表後者較優先，換位
                }
                if ((isCmd1_SourceTypeShelf == false) && (isCmd2_SourceTypeShelf == true))
                {
                    return -1;
                    //代表前者較優先，不動
                }

                //A20.06.04
                // 1.先取priority 判斷
                if ((MCSCmd1.PRIORITY_SUM >= 99 && MCSCmd2.PRIORITY_SUM >= 99) ||
                    (MCSCmd1.PRIORITY_SUM < 99 && MCSCmd2.PRIORITY_SUM < 99))
                {
                    //代表兩者相等，不動，且接著判斷距離
                }
                if (MCSCmd1.PRIORITY_SUM < 99 && MCSCmd2.PRIORITY_SUM >= 99)
                {
                    return 1;
                    //代表後者較優先，換位
                }
                if (MCSCmd1.PRIORITY_SUM >= 99 && MCSCmd2.PRIORITY_SUM < 99)
                {
                    return -1;
                    //代表前者較優先，不動
                }

                // 2. 若priority 相同，則獲得各自 shelf 的 address 與起始 address的距離
                if (MCSCmd1.DistanceFromVehicleToHostSource == MCSCmd2.DistanceFromVehicleToHostSource)
                {
                    return 0;
                    //代表兩者相等，不動
                }
                if (MCSCmd1.DistanceFromVehicleToHostSource > MCSCmd2.DistanceFromVehicleToHostSource)
                {
                    return 1;
                    //代表後者較優先，換位
                }
                if (MCSCmd1.DistanceFromVehicleToHostSource < MCSCmd2.DistanceFromVehicleToHostSource)
                {
                    return -1;
                    //代表前者較優先，不動
                }
                return 0;
            }
        }

        public class MCSCmdCompare_MoreThan1 : IComparer<ACMD_MCS>
        {
            public int Compare(ACMD_MCS MCSCmd1, ACMD_MCS MCSCmd2)
            {
                //A20.06.09.0
                // 0.判斷命令來源是否為shelf，非shelf者優先進行。
                bool isCmd1_SourceTypeShelf = MCSCmd1.IsCmdSourceTypeShelf(MCSCmd1.HOSTSOURCE);
                bool isCmd2_SourceTypeShelf = MCSCmd1.IsCmdSourceTypeShelf(MCSCmd2.HOSTSOURCE);

                if ((isCmd1_SourceTypeShelf == true) && (isCmd2_SourceTypeShelf == true) ||
                    (isCmd1_SourceTypeShelf == false) && (isCmd2_SourceTypeShelf == false))
                {
                    //代表兩者相等，不動，且接著判斷距離
                }
                if ((isCmd1_SourceTypeShelf == true) && (isCmd2_SourceTypeShelf == false))
                {
                    return 1;
                    //代表後者較優先，換位
                }
                if ((isCmd1_SourceTypeShelf == false) && (isCmd2_SourceTypeShelf == true))
                {
                    return -1;
                    //代表前者較優先，不動
                }

                //A20.06.04
                // 1.先取priority 判斷
                if ((MCSCmd1.PRIORITY_SUM >= 99 && MCSCmd2.PRIORITY_SUM >= 99) ||
                    (MCSCmd1.PRIORITY_SUM < 99 && MCSCmd2.PRIORITY_SUM < 99))
                {
                    //代表兩者相等，不動，且接著判斷距離
                }
                if (MCSCmd1.PRIORITY_SUM < 99 && MCSCmd2.PRIORITY_SUM >= 99)
                {
                    return 1;
                    //代表後者較優先，換位
                }
                if (MCSCmd1.PRIORITY_SUM >= 99 && MCSCmd2.PRIORITY_SUM < 99)
                {
                    return -1;
                    //代表前者較優先，不動
                }

                // 2. 若priority 相同，則獲得各自 shelf 的 address 與起始 address的距離
                if (MCSCmd1.DistanceFromVehicleToHostSource == MCSCmd2.DistanceFromVehicleToHostSource)
                {
                    return 0;
                    //代表兩者相等，不動
                }
                if (MCSCmd1.DistanceFromVehicleToHostSource > MCSCmd2.DistanceFromVehicleToHostSource)
                {
                    return 1;
                    //代表後者較優先，換位
                }
                if (MCSCmd1.DistanceFromVehicleToHostSource < MCSCmd2.DistanceFromVehicleToHostSource)
                {
                    return -1;
                    //代表前者較優先，不動
                }
                return 0;
            }
        }

        public bool IsCmdSourceTypeShelf(string cmdSource)
        {
            bool isCmdSourceTypeShelf = false;
            if (cmdSource.StartsWith("10") || cmdSource.StartsWith("11") || cmdSource.StartsWith("20") || cmdSource.StartsWith("21"))
            {
                isCmdSourceTypeShelf = true;
            }
            return isCmdSourceTypeShelf;
        }

        public bool IsCmdSourceTypeAGV(string cmdSource)
        {
            bool isCmdSourceTypeAGV = false;
            if (cmdSource.Contains("A0") || cmdSource.Contains("ST0"))
            {
                isCmdSourceTypeAGV = true;
            }
            return isCmdSourceTypeAGV;
        }

        public HCMD_MCS ToHCMD_MCS()
        {
            return new HCMD_MCS()
            {
                CMD_ID = this.CMD_ID,
                CARRIER_ID = this.CARRIER_ID,
                TRANSFERSTATE = this.TRANSFERSTATE,
                COMMANDSTATE = this.COMMANDSTATE,
                HOSTSOURCE = this.HOSTSOURCE,
                HOSTDESTINATION = this.HOSTDESTINATION,
                PRIORITY = this.PRIORITY,
                CHECKCODE = this.CHECKCODE,
                PAUSEFLAG = this.PAUSEFLAG,
                CMD_INSER_TIME = this.CMD_INSER_TIME,
                CMD_START_TIME = this.CMD_START_TIME,
                CMD_FINISH_TIME = this.CMD_FINISH_TIME,
                TIME_PRIORITY = this.TIME_PRIORITY,
                PORT_PRIORITY = this.PORT_PRIORITY,
                PRIORITY_SUM = this.PRIORITY_SUM,
                REPLACE = this.REPLACE,
                BOX_ID = this.BOX_ID,
                CARRIER_LOC = this.CARRIER_LOC,
                LOT_ID = this.LOT_ID,
                CARRIER_ID_ON_CRANE = this.CARRIER_ID_ON_CRANE,
                CMDTYPE = this.CMDTYPE,
                CRANE = this.CRANE,
                RelayStation = this.RelayStation,
            };
        }

        public bool put(ACMD_MCS ortherObj)
        {
            bool has_change = false;
            if (!sc.Common.SCUtility.isMatche(CMD_ID, ortherObj.CMD_ID))
            {
                CMD_ID = ortherObj.CMD_ID;
                has_change = true;
            }
            if (!sc.Common.SCUtility.isMatche(CARRIER_ID, ortherObj.CARRIER_ID))
            {
                BOX_ID = ortherObj.CARRIER_ID;
                has_change = true;
            }
            if (TRANSFERSTATE != ortherObj.TRANSFERSTATE)
            {
                TRANSFERSTATE = ortherObj.TRANSFERSTATE;
                has_change = true;
            }
            if (COMMANDSTATE != ortherObj.COMMANDSTATE)
            {
                COMMANDSTATE = ortherObj.COMMANDSTATE;
                has_change = true;
            }
            if (!sc.Common.SCUtility.isMatche(HOSTSOURCE, ortherObj.HOSTSOURCE))
            {
                HOSTSOURCE = ortherObj.HOSTSOURCE;
                has_change = true;
            }
            if (!sc.Common.SCUtility.isMatche(HOSTDESTINATION, ortherObj.HOSTDESTINATION))
            {
                HOSTDESTINATION = ortherObj.HOSTDESTINATION;
                has_change = true;
            }
            if (PRIORITY != ortherObj.PRIORITY)
            {
                PRIORITY = ortherObj.PRIORITY;
                has_change = true;
            }
            if (!sc.Common.SCUtility.isMatche(HOSTDESTINATION, ortherObj.HOSTDESTINATION))
            {
                HOSTDESTINATION = ortherObj.HOSTDESTINATION;
                has_change = true;
            }
            if (!sc.Common.SCUtility.isMatche(HOSTDESTINATION, ortherObj.HOSTDESTINATION))
            {
                HOSTDESTINATION = ortherObj.HOSTDESTINATION;
                has_change = true;
            }
            if (!sc.Common.SCUtility.isMatche(HOSTDESTINATION, ortherObj.HOSTDESTINATION))
            {
                HOSTDESTINATION = ortherObj.HOSTDESTINATION;
                has_change = true;
            }
            if (!sc.Common.SCUtility.isMatche(CHECKCODE, ortherObj.CHECKCODE))
            {
                CHECKCODE = ortherObj.CHECKCODE;
                has_change = true;
            }
            if (!sc.Common.SCUtility.isMatche(PAUSEFLAG, ortherObj.PAUSEFLAG))
            {
                PAUSEFLAG = ortherObj.PAUSEFLAG;
                has_change = true;
            }
            if (CMD_INSER_TIME != ortherObj.CMD_INSER_TIME)
            {
                CMD_INSER_TIME = ortherObj.CMD_INSER_TIME;
                has_change = true;
            }
            if (CMD_START_TIME != ortherObj.CMD_START_TIME)
            {
                CMD_START_TIME = ortherObj.CMD_START_TIME;
                has_change = true;
            }
            if (CMD_FINISH_TIME != ortherObj.CMD_FINISH_TIME)
            {
                CMD_FINISH_TIME = ortherObj.CMD_FINISH_TIME;
                has_change = true;
            }
            if (TIME_PRIORITY != ortherObj.TIME_PRIORITY)
            {
                TIME_PRIORITY = ortherObj.TIME_PRIORITY;
                has_change = true;
            }
            if (PORT_PRIORITY != ortherObj.PORT_PRIORITY)
            {
                PORT_PRIORITY = ortherObj.PORT_PRIORITY;
                has_change = true;
            }
            if (PRIORITY_SUM != ortherObj.PRIORITY_SUM)
            {
                PRIORITY_SUM = ortherObj.PRIORITY_SUM;
                has_change = true;
            }
            if (REPLACE != ortherObj.REPLACE)
            {
                REPLACE = ortherObj.REPLACE;
                has_change = true;
            }
            if (LOT_ID != ortherObj.LOT_ID)
            {
                LOT_ID = ortherObj.LOT_ID;
                has_change = true;
            }
            if (CARRIER_ID_ON_CRANE != ortherObj.CARRIER_ID_ON_CRANE)
            {
                CARRIER_ID_ON_CRANE = ortherObj.CARRIER_ID_ON_CRANE;
                has_change = true;
            }
            if (!SCUtility.isMatche(CRANE, ortherObj.CRANE))
            {
                CRANE = SCUtility.Trim(ortherObj.CRANE, true);
                has_change = true;
            }
            if (IsCanNotServiceReasonChanged)
            {
                IsCanNotServiceReasonChanged = false;
                has_change = true;
            }
            return has_change;
        }

        public bool setCanNotServiceReason(string reason)
        {
            if (!sc.Common.SCUtility.isMatche(CanNotServiceReason, reason))
            {
                CanNotServiceReason = reason;
                IsCanNotServiceReasonChanged = true;
                string id = sc.Common.SCUtility.Trim(CMD_ID, true);
                string source = sc.Common.SCUtility.Trim(HOSTSOURCE, true);
                string dest = sc.Common.SCUtility.Trim(HOSTDESTINATION, true);
                string relay = sc.Common.SCUtility.Trim(RelayStation, true);
                NLog.LogManager.GetLogger("TransferServiceLogger").Info($"tran id:{id} source:{source} dest:{dest} relay:{relay},無法派送原因:{reason}");
                return true;
            }
            return false;
        }
        public void setPreAssignVh(string vhID)
        {
            PreAssignVhID = vhID;
        }


        public string getHostSourceSegment(BLL.PortStationBLL portStationBLL, BLL.SectionBLL sectionBLL)
        {
            var port_st = portStationBLL.OperateCatch.getPortStationByID(HOSTSOURCE);
            if (port_st == null) return "";
            var sections = sectionBLL.cache.GetSectionsByToAddress(port_st.ADR_ID);
            if (sections == null || sections.Count == 0) return "";
            return sections.FirstOrDefault().SEG_NUM;
        }
        public string getHostDestSegment(BLL.PortStationBLL portStationBLL, BLL.SectionBLL sectionBLL)
        {
            var port_st = portStationBLL.OperateCatch.getPortStationByID(HOSTDESTINATION);
            if (port_st == null) return "";
            var sections = sectionBLL.cache.GetSectionsByToAddress(port_st.ADR_ID);
            if (sections == null || sections.Count == 0) return "";
            return sections.FirstOrDefault().SEG_NUM;
        }

        public string getHostSourceAdr(BLL.PortStationBLL portStationBLL)
        {
            var port_st = portStationBLL.OperateCatch.getPortStationByID(HOSTSOURCE);
            if (port_st == null) return "";
            return port_st.ADR_ID;
        }
        public string getHostDestAdr(BLL.PortStationBLL portStationBLL)
        {
            var port_st = portStationBLL.OperateCatch.getPortStationByID(HOSTDESTINATION);
            if (port_st == null) return "";
            return port_st.ADR_ID;
        }
    }
    public partial class ACMD_MCS //ForReelNTB使用
    {
        public bool IsDestReelNTB(sc.Service.TransferService transferService)
        {
            return transferService.isNTBPort(HOSTDESTINATION);
        }
    }
    public partial class ACMD_MCS //EFEM機台使用
    {
        public bool IsHostEFEM(sc.Service.TransferService transferService)
        {
            return transferService.isEFEMPort(HOSTDESTINATION);
        }
    }
}