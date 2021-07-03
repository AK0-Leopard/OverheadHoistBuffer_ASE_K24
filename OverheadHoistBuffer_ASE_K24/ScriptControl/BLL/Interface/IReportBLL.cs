namespace com.mirle.ibg3k0.sc.BLL.Interface
{
    public interface IReportBLL
    {
        bool ReportCarrierWaitIn(CassetteData cassetteData);

        bool ReportForcedRemoveCarrier(CassetteData cassetteData);
    }
}