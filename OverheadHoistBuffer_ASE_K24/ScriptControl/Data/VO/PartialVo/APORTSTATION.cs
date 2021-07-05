using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;

namespace com.mirle.ibg3k0.sc
{
    public partial class APORTSTATION : BaseEQObject
    {
        public APORTSTATION()
        {
            eqptObjectCate = SCAppConstants.EQPT_OBJECT_PORT_STATION;
        }
        public string CST_ID { get; set; }
        public string EQPT_ID { get; set; }
        public override void doShareMemoryInit(BCFAppConstants.RUN_LEVEL runLevel)
        {
            foreach (IValueDefMapAction action in valueDefMapActionDic.Values)
            {
                action.doShareMemoryInit(runLevel);
            }
        }
        public override string ToString()
        {
            return $"{PORT_ID} ({ADR_ID})";
        }

    }

}
