using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;

namespace UnitTestForMGVPort.StubObjects
{
    public class NStubObject
    {
        public NStubObject(IManualPortValueDefMapAction manualPortValueDefMapAction,
            IReportBLL reportBLL,
            IPortDefBLL portDefBLL,
            IShelfDefBLL shelfDefBLL,
            ICassetteDataBLL cassetteDataBLL,
            ICMDBLL cMDBLL,
            IAlarmBLL alarmBLL)
        {
            ManualPortValueDefMapAction = manualPortValueDefMapAction;
            ReportBLL = reportBLL;
            PortDefBLL = portDefBLL;
            ShelfDefBLL = shelfDefBLL;
            CassetteDataBLL = cassetteDataBLL;
            CommandBLL = cMDBLL;
            AlarmBLL = alarmBLL;
        }

        public IManualPortValueDefMapAction ManualPortValueDefMapAction { get; }

        public IReportBLL ReportBLL { get; }

        public IPortDefBLL PortDefBLL { get; }

        public IShelfDefBLL ShelfDefBLL { get; }

        public ICassetteDataBLL CassetteDataBLL { get; }

        public ICMDBLL CommandBLL { get; }

        public IAlarmBLL AlarmBLL { get; }
    }
}