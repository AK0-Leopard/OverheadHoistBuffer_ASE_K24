using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using System.Threading.Tasks;
using static com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ReelNTB.ReelNTBEvents;

namespace com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface
{
    public interface IReelNTBValueDefMapAction : IValueDefMapAction
    {
        event ReelNTBTranCmdReqEventHandler TransferCommandRequest;
    }
}