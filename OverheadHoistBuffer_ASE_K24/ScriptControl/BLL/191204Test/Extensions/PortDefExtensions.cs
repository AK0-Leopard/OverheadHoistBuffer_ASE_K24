﻿using com.mirle.ibg3k0.sc.Data.Enum;

namespace com.mirle.ibg3k0.sc.BLL._191204Test.Extensions
{
    public static class PortDefExtensions
    {
        public static UnitType ToUnitType(this PortDef portDef)
        {
            if (portDef.UnitType == "OHCV")
                return UnitType.AGV;
            else if (portDef.UnitType == "AGV")
                return UnitType.AGVZONE;
            else if (portDef.UnitType == "CRANE")
                return UnitType.CRANE;
            else if (portDef.UnitType == "EQ")
                return UnitType.EQ;
            else if (portDef.UnitType == "LINE")
                return UnitType.LINE;
            else if (portDef.UnitType == "NTB")
                return UnitType.NTB;
            else if (portDef.UnitType == "OHCV")
                return UnitType.OHCV;
            else if (portDef.UnitType == "SHELF")
                return UnitType.SHELF;
            else if (portDef.UnitType == "STK")
                return UnitType.STK;
            else if (portDef.UnitType == "MANUALPORT")
                return UnitType.MANUALPORT;
            else if (portDef.UnitType == "EFEM")
                return UnitType.EFEM;
            else
                return UnitType.ZONE;
        }

        public static bool IsShlef(this UnitType type)
        {
            return type == UnitType.SHELF;
        }
        public static bool IsEQPort(this UnitType type)
        {
            return type == UnitType.EQ;
        }
        public static bool IsEFEMPort(this UnitType type)
        {
            return type == UnitType.EFEM;
        }

    }
}