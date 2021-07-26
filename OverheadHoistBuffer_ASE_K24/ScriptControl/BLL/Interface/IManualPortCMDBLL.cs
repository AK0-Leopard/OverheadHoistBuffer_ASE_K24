namespace com.mirle.ibg3k0.sc.BLL.Interface
{
    public interface IManualPortCMDBLL
    {
        bool GetCommandByBoxId(string carrierId, out ACMD_MCS command);

        void Delete(string carrierId);
    }
}