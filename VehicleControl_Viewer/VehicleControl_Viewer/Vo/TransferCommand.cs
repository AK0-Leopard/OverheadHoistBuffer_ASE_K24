using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Viewer.Common;
using VehicleControl_Viewer.Protots;

namespace VehicleControl_Viewer.Vo
{
    public class TransferCommand
    {
        public TransferCommand()
        {
        }


        public string CMD_ID { get; set; }
        public string CARRIER_ID { get; set; }
        //public E_TRAN_STATUS TRANSFERSTATE { get; set; }
        public int COMMANDSTATE { get; set; }
        public string HOSTSOURCE { get; set; }
        public string HOSTDESTINATION { get; set; }
        public int PRIORITY { get; set; }
        public string CHECKCODE { get; set; }
        public string PAUSEFLAG { get; set; }
        public System.DateTime CMD_INSER_TIME { get; set; }
        public Nullable<System.DateTime> CMD_START_TIME { get; set; }
        public Nullable<System.DateTime> CMD_FINISH_TIME { get; set; }
        public int TIME_PRIORITY { get; set; }
        public int PORT_PRIORITY { get; set; }
        public int REPLACE { get; set; }
        public int PRIORITY_SUM { get; set; }

    }
}
