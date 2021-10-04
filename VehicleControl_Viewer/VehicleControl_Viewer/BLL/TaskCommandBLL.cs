using com.mirle.AK0.ProtocolFormat.VehicleControlPublishMessage;
using STAN.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Viewer.App;
using VehicleControl_Viewer.Common;

namespace VehicleControl_Viewer.BLL
{
    public class TaskCommandBLL
    {
        private readonly WindownApplication app;

        public TaskCommandBLL(WindownApplication _app)
        {
            app = _app;
        }
        public void SubscriberTaskCommandInfoChangeEvent()
        {
            app.NatsManager.Subscriber(WindownApplication.NATS_SUBJECT_TASK_COMMAND_CHANGE, TaskCommandInfoChanged, is_last: true);
        }
        private void TaskCommandInfoChanged(object sender, StanMsgHandlerArgs e)
        {
            var bytes = e.Message.Data;
            TaskCommandInfo task_info = Utility.ToObject<TaskCommandInfo>(bytes);
            app.objCacheManager.refreshTaskCommandInfos(task_info.Infos.ToList());
        }
    }
}
