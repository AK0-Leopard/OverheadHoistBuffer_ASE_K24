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

    class OHxCToMtl_MessageDownload_PH2 : PLC_FunBase
    {
        [PLCElement(ValueName = "OHXC_TO_MTL_DATA_MESSAGE_DOWNLOAD_MESSAGE_PH2")]
        public string Message;
        [PLCElement(ValueName = "OHXC_TO_MTL_DATA_MESSAGE_DOWNLOAD_INDEX_PH2")]
        public uint Index;
    }

}
