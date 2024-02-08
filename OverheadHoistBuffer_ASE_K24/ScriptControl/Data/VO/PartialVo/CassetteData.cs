using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Service;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class CassetteData
    {
        public const string SYMBLE_LITE_CASSETTE = "LC";
        public const string SYMBLE_FOUP = "BE";
        public static List<CassetteData> CassetteData_InfoList { get; set; } = new List<CassetteData>();
        public enum OHCV_STAGE
        {
            OHTtoPort = 0,  //入料進行中
            OP = 1,
            BP1,
            BP2,
            BP3,
            BP4,
            BP5,
            LP,
        }

        public CassetteData Clone()
        {
            return (CassetteData)this.MemberwiseClone();
        }

        public string CSTID { get { return BOXID; } }

        public bool hasCommandExcute(BLL.CMDBLL cmdBLL)
        {
            bool has_cmd_excute = cmdBLL.hasExcuteCMDByBoxID(BOXID);
            return has_cmd_excute;
        }

        public string CurrentBayID(sc.App.SCApplication scApp)
        {
            if (scApp.TransferService.isShelfPort(Carrier_LOC))
            {
                if (Carrier_LOC.Length < 6)
                {
                    return "";
                }
                //從倒數第二個字取出兩個
                //100101
                string bay_id = Carrier_LOC.Substring(Carrier_LOC.Length - 2, 2);
                return bay_id;
            }
            return "";
        }
        public string CurrentAdrID(sc.App.SCApplication scApp)
        {

            if (scApp.TransferService.isShelfPort(Carrier_LOC))
            {
                return scApp.TransferService.portINIData[Carrier_LOC].ADR_ID.ToString();
            }
            return "";
        }

    }

    public partial class CassetteData
    {
        //因為Reel CST 沒有明確的CST ID命令命令規則，因此透過CST TYPE來確認
        public bool IsReelCST
        {
            get { return CSTType == ((int)Data.PLC_Functions.MGV.Enums.CstType.ReelCST).ToString(); }
        }
        public bool IsFoupCST
        {
            get
            {
                if (BOXID.Length < 4)
                {
                    return false;
                }
                var sub_crrierID = BOXID.Substring(2, 2);
                if (sc.Common.SCUtility.isMatche(sub_crrierID, SYMBLE_FOUP))
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsLightCST
        {
            get
            {
                if (BOXID.Length < 4)
                {
                    return false;
                }
                var sub_crrierID = BOXID.Substring(2, 2);
                if (sc.Common.SCUtility.isMatche(sub_crrierID, SYMBLE_LITE_CASSETTE))
                {
                    return true;
                }
                return false;
            }
        }

        public CstType GetCstType()
        {
            if (IsReelCST)
            {
                return CstType.ReelCST;
            }
            else if (IsFoupCST)
            {
                return CstType.A;
            }
            else if (IsLightCST)
            {
                return CstType.B;
            }

            if (!int.TryParse(CSTType, out int i_cst_type))
            {
                return CstType.Undefined;
            }
            else
            {
                //檢查i_cst_type是否為合法的CST Type
                if (!Enum.IsDefined(typeof(CstType), i_cst_type))
                {
                    return CstType.Undefined;
                }
                return (CstType)i_cst_type;
            }

        }

        public (bool isUnknow, Data.PLC_Functions.MGV.Enums.CstType cstType) IsUnknowBox()
        {
            if (IsUnknow)
            {
                var cst_type = getUnknowCSTType();
                return (true, cst_type);
            }
            return (false, Data.PLC_Functions.MGV.Enums.CstType.Undefined);
        }

        private bool IsUnknow
        {
            get
            {
                return BOXID.StartsWith(Service.TransferService.SYMBOL_UNKNOW_CST_ID);
            }
        }
        private CstType getUnknowCSTType()
        {
            if (BOXID.Length < 6)
            {
                return Data.PLC_Functions.MGV.Enums.CstType.Undefined;
            }
            var sub_crrierID = BOXID.Substring(4, 2);
            if (sc.Common.SCUtility.isMatche(sub_crrierID, SYMBLE_FOUP))
            {
                return Data.PLC_Functions.MGV.Enums.CstType.A;
            }
            else if (sc.Common.SCUtility.isMatche(sub_crrierID, SYMBLE_LITE_CASSETTE))
            {
                return Data.PLC_Functions.MGV.Enums.CstType.B;
            }
            else
            {
                return Data.PLC_Functions.MGV.Enums.CstType.Undefined;
            }
        }
    }
}