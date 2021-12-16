using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Viewer.Protots;

namespace VehicleControl_Viewer.Vo.ValueObjToShow
{
    public class VehicleShow : INotifyPropertyChanged
    {

        public VehicleShow(Vehicle _vh)
        {
            this.vh = _vh;
            this.VEHICLE_ID = _vh.VEHICLE_ID;
        }
        private string vEHICLE_ID;
        private VHActionStatus aCT_STATUS;
        private VHModeStatus mODE_STATUS;
        private string mCS_CMD;
        private string oHTC_CMD;
        private string cST_ID;
        private string lOAD_PORT_ID;
        private string uNLOAD_PORT_ID;
        private string cMD_CST_ID;
        private string pRIORITY;
        private bool iS_INSTALLED;
        private readonly Vehicle vh;

        public string VEHICLE_ID { get => vEHICLE_ID; set { if (vEHICLE_ID != value) { vEHICLE_ID = value; OnPropertyChanged(); } } }
        public VHActionStatus ACT_STATUS { get => aCT_STATUS; set { if (aCT_STATUS != value) { aCT_STATUS = value; OnPropertyChanged(); } } }
        public VHModeStatus MODE_STATUS { get => mODE_STATUS; set { if (mODE_STATUS != value) { mODE_STATUS = value; OnPropertyChanged(); } } }
        public string MCS_CMD { get => mCS_CMD; set { if (mCS_CMD != value) { mCS_CMD = value; OnPropertyChanged(); } } }
        public string OHTC_CMD { get => oHTC_CMD; set { if (oHTC_CMD != value) { oHTC_CMD = value; OnPropertyChanged(); } } }
        public string CST_ID { get => cST_ID; set { if (cST_ID != value) { cST_ID = value; OnPropertyChanged(); } } }
        public string LOAD_PORT_ID { get => lOAD_PORT_ID; set { if (lOAD_PORT_ID != value) { lOAD_PORT_ID = value; OnPropertyChanged(); } } }
        public string UNLOAD_PORT_ID { get => uNLOAD_PORT_ID; set { if (uNLOAD_PORT_ID != value) { uNLOAD_PORT_ID = value; OnPropertyChanged(); } } }
        public string CMD_CST_ID { get => cMD_CST_ID; set { if (cMD_CST_ID != value) { cMD_CST_ID = value; OnPropertyChanged(); } } }
        public string PRIORITY { get => pRIORITY; set { if (pRIORITY != value) { pRIORITY = value; OnPropertyChanged(); } } }
        public bool IS_INSTALLED { get => iS_INSTALLED; set { iS_INSTALLED = value; OnPropertyChanged(); } }
        public void resresh()
        {
            ACT_STATUS = vh.ACT_STATUS;
            MODE_STATUS = vh.MODE_STATUS;
            MCS_CMD = vh.MCS_CMD;
            OHTC_CMD = vh.OHTC_CMD;
            CST_ID = vh.CST_ID;
            IS_INSTALLED = vh.IS_INSTALLED;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
