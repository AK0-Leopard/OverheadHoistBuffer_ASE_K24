using com.mirle.AK0.ProtocolFormat.VehicleControlPublishMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleControl_Viewer.Vo.ObjToShow
{
    public class TaskCommandShow
    {
        TaskCommand taskCommand;
        public TaskCommandShow(TaskCommand _taskCommand)
        {
            this.taskCommand = _taskCommand;
        }
        public string VHID { get { return taskCommand.VHID; } }
        public string CMDIDMCS { get { return taskCommand.CMDIDMCS; } }
        public string CMDID { get { return taskCommand.CMDID; } }
        public string BOXID { get { return taskCommand.BOXID; } }
        public string LOTID { get { return taskCommand.LOTID; } }
        public CmdType CMDTPYE { get { return taskCommand.CMDTPYE; } }
        //public TranStatus TRANSFERSTATE { get { return taskCommand.CMD } }
        public CmdStatus COMMANDSTATE { get { return taskCommand.CMDSTATUS; } }
        public string SOURCE { get { return taskCommand.SOURCE; } }
        public string DESTINATION { get { return taskCommand.DESTINATION; } }
        public int PRIORITY { get { return taskCommand.PRIORITY; } }
        //public string CHECKCODE { get { return transferCommand.CHECKCODE; } }
        //public string PAUSEFLAG { get { return taskCommand.PAUS } }
        public string CMDINSERTIME
        {
            get
            {
                if (taskCommand.CMDINSERTIME == 0) return "";
                var date_time = DateTimeOffset.FromUnixTimeSeconds(taskCommand.CMDINSERTIME).ToLocalTime();
                return date_time.ToString(App.WindownApplication.DateTimeFormat_23);
            }
        }
        public string CMDSTARTTIME
        {
            get
            {
                if (taskCommand.CMDSTARTTIME == 0) return "";
                var date_time = DateTimeOffset.FromUnixTimeSeconds(taskCommand.CMDSTARTTIME).ToLocalTime();
                return date_time.ToString(App.WindownApplication.DateTimeFormat_23);
            }
        }
        /*public string CMDFINISHTIME
        {
            get
            {
                if (taskCommand.F == 0) return "";
                var date_time = DateTimeOffset.FromUnixTimeSeconds(taskCommand.CMDFINISHTIME).ToLocalTime();
                return date_time.ToString(App.WindownApplication.DateTimeFormat_23);
            }
        }*/
        public int TIMEPRIORITY { get { return taskCommand.PRIORITY; } }
        //public int PORTPRIORITY { get { return transferCommand.PORTPRIORITY; } }
        //public int PRIORITYSUM { get { return transferCommand.PRIORITYSUM; } }
        //public int REPLACE { get { return transferCommand.REPLACE; } }
        //public string DESCRIPTION { get { return transferCommand.DESCRIPTION; } }
    }
}
