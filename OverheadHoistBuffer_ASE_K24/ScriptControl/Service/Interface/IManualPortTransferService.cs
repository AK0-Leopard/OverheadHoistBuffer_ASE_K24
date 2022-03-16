namespace com.mirle.ibg3k0.sc.Service.Interface
{
    public interface IManualPortTransferService
    {
        string ForceFinishMCSCmd(ACMD_MCS cmdMCS, CassetteData cassetteData, string cmdSource, string result = ACMD_MCS.ResultCode.WarnError);
        bool tryCancelMCSCmd(ACMD_MCS cmdMCS);
    }
}