using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ReelNTB;
using System.Collections.Generic;

namespace com.mirle.ibg3k0.sc.BLL.Interface
{
    public interface IReelNTBReportBLL
    {
        bool ReportCarrierTransferRequest(ReelNTBTranCmdReqEventArgs arg);

    }
}