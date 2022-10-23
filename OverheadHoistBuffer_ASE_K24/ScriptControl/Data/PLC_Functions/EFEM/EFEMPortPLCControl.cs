using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using System;

namespace com.mirle.ibg3k0.sc.Data.PLC_Functions.EFEM
{
    public class EFEMPortPLCControl : PLC_FunBase
    {
        [PLCElement(ValueName = "OHxC_TO_MGV_INMODE")]
        public bool IsChangeToInMode;
    }
}