using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using System;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events
{
    public class ManualPortEventArgs : EventArgs
    {
        public ManualPortEventArgs(ManualPortPLCInfo manualPortPLCInfo)
        {
            ManualPortPLCInfo = manualPortPLCInfo;
        }

        public ManualPortPLCInfo ManualPortPLCInfo { get; }
    }
}