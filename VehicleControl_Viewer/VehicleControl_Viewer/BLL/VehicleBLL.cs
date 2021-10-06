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
    public class VehicleBLL
    {
        public WindownApplication app = null;
        public VehicleBLL(WindownApplication _app)
        {
            app = _app;
        }

        public void SubscriberVehicleInfos()
        {
            foreach (var vh in app.objCacheManager.LoadAllVehicles())
            {
                string subject_id = string.Format(WindownApplication.NATS_SUBJECT_VH_INFO_0, vh.VEHICLE_ID);
                app.NatsManager.Subscriber(subject_id, VehiclePositionChangeHandler, is_last: true);
                System.Threading.Thread.Sleep(50);
            }

        }
        private void VehiclePositionChangeHandler(object sender, StanMsgHandlerArgs e)
        {
            var bytes = e.Message.Data;
            VEHICLE_INFO vh_info = Utility.ToObject<VEHICLE_INFO>(bytes);
            app.objCacheManager.GetVehicle(vh_info.VEHICLEID).setObject(vh_info);
        }


    }
}
