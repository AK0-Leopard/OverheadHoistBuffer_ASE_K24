using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;

namespace com.mirle.ibg3k0.sc
{
    public partial class MGV_PORTSTATION : APORTSTATION
    {
        public MGV_PORTSTATION() : base()
        {

        }
        public IManualPortValueDefMapAction getExcuteMapAction()
        {
            IManualPortValueDefMapAction mapAction = this.getMapActionByIdentityKey(typeof(MGVDefaultValueDefMapAction).Name) as IManualPortValueDefMapAction;
            return mapAction;
        }

    }

}
