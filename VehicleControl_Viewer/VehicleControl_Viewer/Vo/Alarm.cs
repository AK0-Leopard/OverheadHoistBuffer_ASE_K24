using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleControl_Viewer.Vo
{
    public class Alarm
    {
        public string EQ_ID { get; set; }
        public string Unit_ID { get; set; }
        public string RPT_dateTime { get; set; }
        public string Code { get; set; }
        public string level { get; set; }
        public string alarmStatus { get; set; }
        public string alarmAffectCount { get; set; }
        public string Description { get; set; }
    }
}
