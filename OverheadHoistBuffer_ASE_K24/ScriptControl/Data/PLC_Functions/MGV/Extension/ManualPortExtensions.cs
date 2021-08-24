﻿using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;

namespace com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Extension
{
    public static class ManualPortExtensions
    {
        public static CstType ToCstType(this string cstType)
        {
            if (cstType.Trim() == "A")
                return CstType.LiteCassete;
            else if (cstType.Trim() == "B")
                return CstType.Foup;
            else
                return CstType.Undefined;
        }
    }
}