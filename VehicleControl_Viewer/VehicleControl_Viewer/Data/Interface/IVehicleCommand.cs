using com.mirle.AK0.ProtocolFormat;
using System.Threading.Tasks;

namespace VehicleControl_Viewer.Data.Interface
{
    public interface IVehicleCommand
    {
        Task<ReplyTrnsfer> RequestTrnsferAsync(VehicleCommandInfo commandInfo);
    }
}