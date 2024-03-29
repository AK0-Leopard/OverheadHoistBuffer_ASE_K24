﻿using com.mirle.ibg3k0.sc.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.PLC_Functions
{
    class MtlToOHxC_LFTStatus_PH2 : PLC_FunBase
    {
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_STATUS_HAS_VEHICLE_PH2")]
        public bool HasVehicle;
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_STATUS_STOP_SINGLE_PH2")]
        public bool StopSingle;
        [PLCElement(ValueName = "MTS_TO_OHXC_MTS_STATUS_HAS_VEHICLE_PH2")]
        public bool MTSHasVehicle;
        [PLCElement(ValueName = "MTS_TO_OHXC_MTS_STATUS_STOP_SINGLE_PH2")]
        public bool MTSStopSingle;
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_MODE_PH2")]
        public UInt16 Mode;
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_LOCATION_PH2")]
        public UInt16 LFTLocation;
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_MOVING_STATUS_PH2")]
        public UInt16 LFTMovingStatus;
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_ENCODER_PH2")]
        public UInt32 LFTEncoder;
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_VEHICLE_IN_POSITION_PH2")]
        public UInt16 VhInPosition;
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_FRONT_DOOR_STATUS_PH2")]
        public UInt16 FrontDoorStatus;
        [PLCElement(ValueName = "MTL_TO_OHXC_LFT_BACK_DOOR_STATUS_PH2")]
        public UInt16 BackDoorStatus;
    }
}
