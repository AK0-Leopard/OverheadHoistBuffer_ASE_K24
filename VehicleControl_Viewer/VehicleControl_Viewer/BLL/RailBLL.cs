using STAN.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Viewer.App;

namespace VehicleControl_Viewer.BLL
{
    public class RailBLL
    {
        private readonly WindownApplication app;

        public RailBLL(WindownApplication _app)
        {
            app = _app;
        }
        public void SubscriberRailStatusChangeEvent()
        {
            app.NatsManager.Subscriber(WindownApplication.NATS_SUBJECT_RAIL_STATUS_CHANGE, RailStatusChanged, is_last: true);
        }
        private void RailStatusChanged(object sender, StanMsgHandlerArgs e)
        {
            app.objCacheManager.refreshRailObjectInfo();

        }

    }
}
