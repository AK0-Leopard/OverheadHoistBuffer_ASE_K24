using com.mirle.ibg3k0.sc.Data.PLC_Functions.EFEM;
using System;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.EFEM
{
    public class EFEMEventArgs : EventArgs
    {
        public EFEMPortPLCInfo efemPortInfo { get; private set; }
        public EFEMEventArgs(EFEMPortPLCInfo _efemPortInfo)
        {
            efemPortInfo = _efemPortInfo;
        }
    }
}