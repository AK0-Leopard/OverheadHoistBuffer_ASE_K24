using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using static com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ManualPortEvents;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface
{
    public interface IManualPortValueDefMapAction : IValueDefMapAction
    {
        event ManualPortEventHandler OnWaitIn;

        event ManualPortEventHandler OnWaitOut;

        event ManualPortEventHandler OnDirectionChanged;

        event ManualPortEventHandler OnInServiceChanged;

        event ManualPortEventHandler OnBcrReadDone;

        event ManualPortEventHandler OnCstRemoved;

        event ManualPortEventHandler OnLoadPresenceChanged;

        event ManualPortEventHandler OnAlarmHappen;

        event ManualPortEventHandler OnAlarmClear;
    }
}