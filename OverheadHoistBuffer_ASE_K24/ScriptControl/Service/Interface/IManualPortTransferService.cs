namespace com.mirle.ibg3k0.sc.Service.Interface
{
    internal interface IManualPortTransferService
    {
        (bool isContinue, string RemaneBox) ForceFinishMCSCmd(ACMD_MCS cmdMCS, CassetteData cassetteData, string cmdSource, string result = ACMD_MCS.ResultCode.WarnError);
    }
}