using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.BLL.Interface;

namespace UnitTestForMGVPort.StubObjects
{
    public class MockReportBLL : IReportBLL
    {
        public CassetteData WaitInCassetteData { get; private set; }

        public CassetteData ForcedRemoveCassetteData { get; private set; }

        public MockReportBLL()
        {
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
    }
}