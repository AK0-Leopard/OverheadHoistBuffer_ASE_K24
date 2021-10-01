//**********************************************************************************
// Date          Author         Request No.    Tag         Description
// ------------- -------------  -------------  ------      -----------------------------
// 2020/05/15    Jason Wu       N/A            A20.05.15   新增針對性的compare 給 Loop 跟 Line獲得最短長度之排序
//**********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class ShelfDef
    {
        //*******************************************************************
        //A20.05.15 臨時存距離使用
        public int distanceFromHostSource = 0;

        public class E_ShelfState
        {
            public const string EmptyShelf = "N";
            public const string Stored = "S";
            public const string StorageInReserved = "I";
            public const string RetrievalReserved = "O";
            public const string Alternate = "A";
        }
        public ShelfDef Clone()
        {
            return (ShelfDef)this.MemberwiseClone();
        }
        //*******************************************************************
        //A20.05.15
        public class ShelfDefCompareByAddressDistance : IComparer<ShelfDef>
        {
            public int Compare(ShelfDef shelfA, ShelfDef shelfB)
            {
                // 1. 先獲得各自 shelf 的 address 與起始 address的距離
                if (shelfA.distanceFromHostSource == shelfB.distanceFromHostSource)
                {
                    return 0;
                    //代表兩者相等，不動
                }
                if (shelfA.distanceFromHostSource > shelfB.distanceFromHostSource)
                {
                    return 1;
                    //代表後者較優先，換位
                }
                if (shelfA.distanceFromHostSource < shelfB.distanceFromHostSource)
                {
                    return -1;
                    //代表前者較優先，不動
                }
                return 0;
            }
        }

        public void put(ShelfDef ortherValue)
        {
            this.Enable = ortherValue.Enable;
        }


        public string BayID
        {
            get
            {
                if (Common.SCUtility.isEmpty(ShelfID))
                {
                    return "";
                }
                if (ShelfID.Length < 6)
                {
                    return "";
                }
                //從倒數第二個字取出兩個
                //100101
                string bay_id = ShelfID.Substring(ShelfID.Length - 2, 2);
                return bay_id;
            }
        }

        public string SeqNo
        {
            get
            {
                if (Common.SCUtility.isEmpty(ShelfID))
                {
                    return "";
                }
                if (ShelfID.Length < 6)
                {
                    return "";
                }
                //100101
                string seq_no = ShelfID.Substring(ShelfID.Length - 5, 3);
                return seq_no;
            }
        }
        public string CSTID
        {
            get
            {
                try
                {
                    if (CassetteData.CassetteData_InfoList == null ||
                        CassetteData.CassetteData_InfoList.Count == 0)
                    {
                        return "";
                    }
                    List<CassetteData> cassetteDatas = CassetteData.CassetteData_InfoList.ToList();
                    var cst = cassetteDatas.Where(c => Common.SCUtility.isMatche(c.Carrier_LOC, ShelfID)).FirstOrDefault();
                    if (cst == null) return "";
                    return cst.BOXID;
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn(ex, "Exception:");
                    return "";
                }
            }
        }
    }
}
