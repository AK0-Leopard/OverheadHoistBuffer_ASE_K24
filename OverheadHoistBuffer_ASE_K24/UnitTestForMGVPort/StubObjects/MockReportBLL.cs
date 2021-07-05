using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.BLL.Interface;

namespace UnitTestForMGVPort.StubObjects
{
    public class MockReportBLL : IManualPortReportBLL
    {
        public MockReportBLL()
        {
        }

        public CassetteData WaitInCassetteData { get; private set; }

        public bool IsWaitInIdReadDuplicate { get; private set; }

        public CassetteData ForcedRemoveCassetteData { get; private set; }

        public bool ReportCarrierWaitIn(CassetteData cst, bool isDuplicate)
        {
            WaitInCassetteData = cst;
            IsWaitInIdReadDuplicate = isDuplicate;
            return true;
        }

        public bool ReportForcedRemoveCarrier(CassetteData cassetteData)
        {
            ForcedRemoveCassetteData = cassetteData;
            return true;
        }

        public bool ReportPortDirectionChanged(string portName, bool newDirectionIsInMode)
        {
            throw new System.NotImplementedException();
        }

        public bool ReportPortInServiceChanged(string portName, bool newStateIsInService)
        {
            throw new System.NotImplementedException();
        }

        public bool ReportCarrierRemoveFromManualPort(string carrierId)
        {
            throw new System.NotImplementedException();
        }

        public bool ReportAlarmSet(ALARM alarm)
        {
            throw new System.NotImplementedException();
        }

        public bool ReportAlarmClear(ALARM alarm)
        {
            throw new System.NotImplementedException();
        }

        public bool ReportUnitAlarmSet(ALARM alarm)
        {
            throw new System.NotImplementedException();
        }

        public bool ReportUnitAlarmClear(ALARM alarm)
        {
            throw new System.NotImplementedException();
        }
    }
}