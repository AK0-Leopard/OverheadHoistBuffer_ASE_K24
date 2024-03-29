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

    class MtlToOHxC_RequestCarInDataCheck_PH2 : PLC_FunBase
    {
        public DateTime Timestamp;
        [PLCElement(ValueName = "MTL_TO_OHXC_REQUEST_CAR_IN_DATA_CHECK_CAR_ID_PH2")]
        public UInt16 CarID;
        [PLCElement(ValueName = "MTL_TO_OHXC_REQUEST_CAR_IN_DATA_CHECK_HS_PH2")]
        public UInt16 Handshake;
    }

    class OHxCToMtl_ReplyCarInDataCheck_PH2 : PLC_FunBase
    {
        public DateTime Timestamp;
        [PLCElement(ValueName = "OHXC_TO_MTL_REPLY_CAR_IN_DATA_CHECK_RETURN_CODE_PH2")]
        public UInt16 ReturnCode;
        [PLCElement(ValueName = "OHXC_TO_MTL_REPLY_CAR_IN_DATA_CHECK_HS_PH2", IsHandshakeProp = true)]
        public UInt16 Handshake;
    }

}
