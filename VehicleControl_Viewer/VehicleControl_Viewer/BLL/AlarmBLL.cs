using com.mirle.AK0.ProtocolFormat.VehicleControlPublishMessage;
using STAN.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Viewer.App;
using VehicleControl_Viewer.Common;


using VehicleControl_Viewer.App;
namespace VehicleControl_Viewer.BLL
{
    public class AlarmBLL
    {
        private readonly WindownApplication app;
        public AlarmBLL(WindownApplication _app)
        {
            app = _app;
        }
        public void SubscriberAlarmListChangedEvent()
        {
            app.NatsManager.Subscriber(WindownApplication.NATS_SUBJECT_ALARM_LIST_CHANGE, AlarmListChanged, is_last: true);
        }

        private void AlarmListChanged(object sender, StanMsgHandlerArgs e)
        {
            var bytes = e.Message.Data;
            var alarmList = Utility.ToObject<alarmInfo>(bytes);
            app.objCacheManager.refreshAlarmList(alarmList.AlarmList.ToList());
        }

    }
}
