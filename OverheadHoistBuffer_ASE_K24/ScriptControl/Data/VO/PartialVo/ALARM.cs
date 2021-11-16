using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.bcf.Data.ValueDefMapAction;
using com.mirle.ibg3k0.bcf.Data.VO;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ObjectRelay;
using com.mirle.ibg3k0.stc.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc
{
    public partial class ALARM
    {
        public static ConcurrentDictionary<string, ALARM> Alarm_InfoList { get; private set; } = new ConcurrentDictionary<string, ALARM>();
        public string ComplexKey
        {
            get { return $"{sc.Common.SCUtility.Trim(EQPT_ID, true)}#{sc.Common.SCUtility.Trim(ALAM_CODE)}"; }
        }
        public void put(ALARM newAlarmObj)
        {
            this.EQPT_ID = newAlarmObj.EQPT_ID;
            this.UNIT_NUM = newAlarmObj.UNIT_NUM;
            this.RPT_DATE_TIME = newAlarmObj.RPT_DATE_TIME;
            this.ALAM_CODE = newAlarmObj.ALAM_CODE;
            this.ALAM_LVL = newAlarmObj.ALAM_LVL;
            this.ALAM_STAT = newAlarmObj.ALAM_STAT;
            this.ALAM_DESC = newAlarmObj.ALAM_DESC;
            this.ERROR_ID = newAlarmObj.ERROR_ID;
            this.UnitID = newAlarmObj.UnitID;
            this.UnitState = newAlarmObj.UnitState;
            this.RecoveryOption = newAlarmObj.RecoveryOption;
            this.CMD_ID = newAlarmObj.CMD_ID;
            this.END_TIME = newAlarmObj.END_TIME;
        }
    }

}
