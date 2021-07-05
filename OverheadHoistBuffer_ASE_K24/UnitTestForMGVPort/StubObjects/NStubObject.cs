using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using System.Collections.Generic;

namespace UnitTestForMGVPort.StubObjects
{
    public class NStubObject
    {
        public NStubObject(IManualPortValueDefMapAction manualPortValueDefMapAction,
            IManualPortReportBLL reportBLL,
            IManualPortDefBLL portDefBLL,
            IManualPortShelfDefBLL shelfDefBLL,
            IManualPortCassetteDataBLL cassetteDataBLL,
            IManualPortCMDBLL cMDBLL,
            IManualPortAlarmBLL alarmBLL)
        {
            ManualPortValueDefMapAction = manualPortValueDefMapAction;
            ReportBLL = reportBLL;
            PortDefBLL = portDefBLL;
            ShelfDefBLL = shelfDefBLL;
            CassetteDataBLL = cassetteDataBLL;
            CommandBLL = cMDBLL;
            AlarmBLL = alarmBLL;

            ManualPortValueDefMapActions = new List<IManualPortValueDefMapAction>();
            ManualPortValueDefMapActions.Add(manualPortValueDefMapAction);
        }

        public List<IManualPortValueDefMapAction> ManualPortValueDefMapActions { get; private set; }

        public IManualPortValueDefMapAction ManualPortValueDefMapAction { get; }

        public IManualPortReportBLL ReportBLL { get; }

        public IManualPortDefBLL PortDefBLL { get; }

        public IManualPortShelfDefBLL ShelfDefBLL { get; }

        public IManualPortCassetteDataBLL CassetteDataBLL { get; }

        public IManualPortCMDBLL CommandBLL { get; }

        public IManualPortAlarmBLL AlarmBLL { get; }
    }
}