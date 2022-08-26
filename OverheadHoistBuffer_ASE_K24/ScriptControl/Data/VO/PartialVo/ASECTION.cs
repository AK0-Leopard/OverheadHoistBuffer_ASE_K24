using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.Common;
using NLog;

namespace com.mirle.ibg3k0.sc
{
    public partial class ASECTION
    {
        public event EventHandler<string> VehicleLeave;
        public event EventHandler<string> VehicleEntry;
        public double getSectionDistanceByAdr(BLL.ReserveBLL reserveBLL)
        {
            var from_adr_obj = reserveBLL.GetHltMapAddress(FROM_ADR_ID);
            var to_adr_obj = reserveBLL.GetHltMapAddress(TO_ADR_ID);
            if (from_adr_obj.isExist && to_adr_obj.isExist)
            {
                return getDistance(from_adr_obj.x, from_adr_obj.y, to_adr_obj.x, to_adr_obj.y);
            }
            else
            {
                return double.MaxValue;
            }
        }
        private double getDistance(double x1, double y1, double x2, double y2)
        {
            double dx, dy;
            dx = x2 - x1;
            dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void onSectinoLeave(string vh_id)
        {
            VehicleLeave?.Invoke(this, vh_id);
        }
        private void onSectinoEntry(string vh_id)
        {
            VehicleEntry?.Invoke(this, vh_id);
        }

        public void Leave(string vh_id)
        {
            onSectinoLeave(vh_id);
        }
        public void Entry(string vh_id)
        {
            onSectinoEntry(vh_id);
        }

        public string[] getNodeAdrs()
        {
            return new string[] { FROM_ADR_ID, TO_ADR_ID };
        }


    }
}
