using com.mirle.AK0.ProtocolFormat.VehicleControlPublishMessage;
using STAN.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VehicleControl_Viewer.App;
using VehicleControl_Viewer.Common;
using VehicleControl_Viewer.Protots;

namespace VehicleControl_Viewer.BLL
{
    public class LineBLL
    {
        private readonly WindownApplication app;
        public LineBLL(WindownApplication _app)
        {
            app = _app;
        }
        public void SubscriberLineInfoEvent()
        {
            app.NatsManager.Subscriber(WindownApplication.NATS_SUBJECT_LINE_STATUS_CHANGE, LineInfoChange, is_last: true);
        }
        private void LineInfoChange(object sender, StanMsgHandlerArgs e)
        {
            var bytes = e.Message.Data;
            LineInfo lineInfo = Utility.ToObject<LineInfo>(bytes);
            app.objCacheManager.refreshLineInfo(lineInfo);
        }
    }
}
