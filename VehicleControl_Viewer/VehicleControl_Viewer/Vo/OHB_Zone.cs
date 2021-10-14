using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VehicleControl_Viewer.Vo
{
    class OHB_Zone
    {
        TextBlock tb;
        string id;
        int bufferCount;
        int alreadyUse;
        OHB_Zone(TextBlock t)
        {
            tb = t;
            tb.Text = "6666";
        }
    }
}
