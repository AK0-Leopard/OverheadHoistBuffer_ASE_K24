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

        public bool ReportCarrierIDRead(CassetteData cassetteData, bool isDuplicate)
        {
            IsWaitInIdReadDuplicate = isDuplicate;
            return true;
        }

        public bool ReportCarrierWaitIn(CassetteData cst)
        {
            WaitInCassetteData = cst;
            return true;
        }

        public bool ReportForcedRemoveCarrier(CassetteData cassetteData)
        {
            ForcedRemoveCassetteData = cassetteData;
            return true;
        }

        public bool RecivedReportPortDirectionChanged { get; private set; } = false;

        public bool ReportPortDirectionChanged(string portName, bool newDirectionIsInMode)
        {
            RecivedReportPortDirectionChanged = true;
            return true;
        }

        public bool RecivedReportPortInServiceChanged { get; private set; } = false;

        public bool ReportPortInServiceChanged(string portName, bool newStateIsInService)
        {
            RecivedReportPortInServiceChanged = true;
            return true;
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

        public bool ReportCarrierWaitOut(CassetteData cassetteData)
        {
            throw new System.NotImplementedException();
        }

        public bool ReportTransferCompleted(ACMD_MCS cmd, CassetteData cassette, string resultCode)
        {
            throw new System.NotImplementedException();
        }
    }
}