using static com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.EFEM.EFEMEvents;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface
{
    public interface IEFEMValueDefMapAction : ICommonPortInfoValueDefMapAction
    {
        string PortName { get; }


        event EFEMEventHandler OnAlarmHappen;

        event EFEMEventHandler OnAlarmClear;

    }
}