using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.BLL.Interface;
using System.Collections.Generic;

namespace UnitTestForMGVPort.StubObjects
{
    public class StubAlarmBLL : IManualPortAlarmBLL
    {
        public List<ALARM> Alarms { get; set; }

        public StubAlarmBLL()
        {
            Alarms = new List<ALARM>();
        }

        public bool ClearAllAlarm(string portName, ACMD_MCS commandOfPort, out List<ALARM> alarmReports, out string reason)
        {
            reason = "";
            alarmReports = Alarms;
            return true;
        }

        public bool SetAlarm(string portName, string alarmCode, ACMD_MCS commandOfPort, out ALARM alarmReport, out string reasonOfAlarmSetFalied)
        {
            reasonOfAlarmSetFalied = "";
            var alarm = new ALARM();
            alarm.ALAM_CODE = alarmCode;
            Alarms.Add(alarm);
            alarmReport = alarm;
            return true;
        }
    }
}