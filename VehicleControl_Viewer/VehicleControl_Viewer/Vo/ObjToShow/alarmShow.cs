using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleControl_Viewer.Vo.ObjToShow
{
    class alarmShow
    {
        public Alarm alarm;
        public alarmShow(Alarm _alarm)
        {
            alarm= _alarm;
        }
        public string EQ_ID { get { return alarm.EQ_ID; } }
        public string Unit_ID { get { return alarm.Unit_ID; } }
        public string RPT_dateTime { get{ return alarm.RPT_dateTime; } }
        public string Code { get { return alarm.Code; } }
        public string level { get { return alarm.level; } }
        public string Description { get { return alarm.Description; } }
    }
}
