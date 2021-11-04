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
        public bool IsReelCST
        {
            get { return CSTType == ((int)Data.PLC_Functions.MGV.Enums.CstType.ReelCST).ToString(); }
        }
    }
}