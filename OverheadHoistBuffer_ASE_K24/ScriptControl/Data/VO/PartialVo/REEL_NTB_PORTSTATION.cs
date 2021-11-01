using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using Mirle.U332MA30.Grpc.OhbcNtbcConnect;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public class REEL_NTB_PORTSTATION : APORTSTATION
    {
        public REEL_NTB_PORTSTATION() : base()
        {
        }

        public IManualPortValueDefMapAction getExcuteMapAction()
        {
            IManualPortValueDefMapAction mapAction = this.getMapActionByIdentityKey(typeof(MGVDefaultValueDefMapAction).Name) as IManualPortValueDefMapAction;
            return mapAction;
        }

        public PortState state;
        public DirectionType direction;
        public RequestType requestType;
        public string CarrierReelId;
    }
}