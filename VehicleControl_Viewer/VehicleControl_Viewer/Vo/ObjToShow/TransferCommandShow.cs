using com.mirle.AK0.ProtocolFormat.VehicleControlPublishMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleControl_Viewer.Vo.ObjToShow
{
    public class TransferCommandShow
    {
        TransferCommand transferCommand;
        public TransferCommandShow(TransferCommand transferCommand)
        {
            this.transferCommand = transferCommand;
        }
        public string CMDID { get { return transferCommand.CMDID; } }
        public string CARRIERID { get { return transferCommand.CARRIERID; } }
        public TranStatus TRANSFERSTATE { get { return transferCommand.TRANSFERSTATE; } }
        public int COMMANDSTATE { get { return transferCommand.COMMANDSTATE; } }
        public string HOSTSOURCE { get { return transferCommand.HOSTSOURCE; } }
        public string HOSTDESTINATION { get { return transferCommand.HOSTDESTINATION; } }
        public int PRIORITY { get { return transferCommand.PRIORITY; } }
        public string CHECKCODE { get { return transferCommand.CHECKCODE; } }
        public string PAUSEFLAG { get { return transferCommand.PAUSEFLAG; } }
        public string CMDINSERTIME
        {
            get
            {
                if (transferCommand.CMDINSERTIME == 0) return "";
                var date_time = DateTimeOffset.FromUnixTimeSeconds(transferCommand.CMDINSERTIME).ToLocalTime();
                return date_time.ToString(App.WindownApplication.DateTimeFormat_23);
            }
        }
        public string CMDSTARTTIME
        {
            get
            {
                if (transferCommand.CMDSTARTTIME == 0) return "";
                var date_time = DateTimeOffset.FromUnixTimeSeconds(transferCommand.CMDSTARTTIME).ToLocalTime();
                return date_time.ToString(App.WindownApplication.DateTimeFormat_23);
            }
        }
        public string CMDFINISHTIME
        {
            get
            {
                if (transferCommand.CMDFINISHTIME == 0) return "";
                var date_time = DateTimeOffset.FromUnixTimeSeconds(transferCommand.CMDFINISHTIME).ToLocalTime();
                return date_time.ToString(App.WindownApplication.DateTimeFormat_23);
            }
        }
        public int TIMEPRIORITY { get { return transferCommand.TIMEPRIORITY; } }
        public int PORTPRIORITY { get { return transferCommand.PORTPRIORITY; } }
        public int PRIORITYSUM { get { return transferCommand.PRIORITYSUM; } }
        public int REPLACE { get { return transferCommand.REPLACE; } }
        public string DESCRIPTION { get { return transferCommand.DESCRIPTION; } }
    }
}
