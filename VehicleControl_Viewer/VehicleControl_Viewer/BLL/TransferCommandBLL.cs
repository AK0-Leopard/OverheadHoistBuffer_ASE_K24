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
    public class TransferCommandBLL
    {
        private readonly WindownApplication app;

        public TransferCommandBLL(WindownApplication _app)
        {
            app = _app;
        }
        public void SubscriberTransferCommandInfoChangeEvent()
        {
            app.NatsManager.Subscriber(WindownApplication.NATS_SUBJECT_TRANSFER_COMMAND_CHANGE, TransferCommandInfoChanged, is_last: true);
        }
        private void TransferCommandInfoChanged(object sender, StanMsgHandlerArgs e)
        {
            var bytes = e.Message.Data;
            TransferCommandInfo tran_info = Utility.ToObject<TransferCommandInfo>(bytes);
            app.objCacheManager.refreshTransferCommandInfos(tran_info.Infos.ToList());
        }
    }
}
