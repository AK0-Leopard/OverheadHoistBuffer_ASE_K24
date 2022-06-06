using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Grpc.Core;
using CommonMessage.ProtocolFormat.AlarmFun;
using ScriptControl;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.BLL.Interface;

namespace com.mirle.ibg3k0.sc.WebAPI.Grpc
{
    internal class AlarmFun : alarmGreeter.alarmGreeterBase
    {
        IAlarmRemarkFun alarmRemarkFun;
        //List<string> manualPortList = new List<string>();
        public AlarmFun(com.mirle.ibg3k0.sc.App.SCApplication _app)
        {
            alarmRemarkFun = _app.AlarmBLL;
        }
        public override Task<ControlReply> alarmControl(ControlRequest request, ServerCallContext context)
        {
            string eq_id = request.EQPTID;
            DateTime userLastUpdateTime = DateTime.FromBinary(request.RPTDATETIME);//這邊是client告訴我們他最後更新的時間
            string error_code = request.ALARMCODE;
            string update_user = request.USERID;
            var update_classification = request.ALARMCLASSIFICATION;
            string remark = request.ALARMREMARK;

            bool is_success = alarmRemarkFun.setAlarmRemarkInfo(eq_id, userLastUpdateTime, error_code, update_user, update_classification, remark);

            return Task.FromResult(new ControlReply() { Result = is_success ? "OK" : "NG" });
        }
    }
}
