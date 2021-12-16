using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VehicleControl_Viewer.Common;
using VehicleControl_Viewer.Protots;

namespace VehicleControl_Viewer.Vo
{
    public class Vehicle
    {
        public Vehicle(int num, string vhID)
        {
            VEHICLE_ID = vhID;
            Num = num;
        }

        public event EventHandler PositionChanged;
        public event EventHandler StatusChanged;
        public event EventHandler CommandSatusChanged;


        public int Num { get; set; }
        public string VEHICLE_ID { get; set; }
        public E_VH_TYPE VEHICLE_TYPE { get; set; }
        public string CUR_ADR_ID { get; set; }
        public string CUR_SEC_ID { get; set; }
        public double X_Axis { get; set; }
        public double Y_Axis { get; set; }

        public Nullable<System.DateTime> SEC_ENTRY_TIME { get; set; }
        public double ACC_SEC_DIST { get; set; }
        public VHModeStatus MODE_STATUS { get; set; }
        public VHActionStatus ACT_STATUS { get; set; }
        public string MCS_CMD { get; set; }
        public string OHTC_CMD { get; set; }
        public VhStopSingle BLOCK_PAUSE { get; set; }
        public VhStopSingle CMD_PAUSE { get; set; }
        public VhStopSingle OBS_PAUSE { get; set; }
        public VhStopSingle HID_PAUSE { get; set; }
        public VhStopSingle ERROR { get; set; }
        public VhStopSingle EARTHQUAKE_PAUSE { get; set; }
        public VhStopSingle SAFETY_DOOR_PAUSE { get; set; }
        public VhStopSingle OHXC_OBS_PAUSE { get; set; }
        public VhStopSingle OHXC_BLOCK_PAUSE { get; set; }
        public int OBS_DIST { get; set; }
        public int HAS_CST { get; set; }
        public string CST_ID { get; set; }
        public Nullable<System.DateTime> UPD_TIME { get; set; }
        public int VEHICLE_ACC_DIST { get; set; }
        public int MANT_ACC_DIST { get; set; }
        public Nullable<System.DateTime> MANT_DATE { get; set; }
        public int GRIP_COUNT { get; set; }
        public int GRIP_MANT_COUNT { get; set; }
        public Nullable<System.DateTime> GRIP_MANT_DATE { get; set; }
        public string NODE_ADR { get; set; }
        public bool IS_PARKING { get; set; }
        public Nullable<System.DateTime> PARK_TIME { get; set; }
        public string PARK_ADR_ID { get; set; }
        public bool IS_CYCLING { get; set; }
        public Nullable<System.DateTime> CYCLERUN_TIME { get; set; }
        public string CYCLERUN_ID { get; set; }
        public bool IS_INSTALLED { get; set; }
        public Nullable<System.DateTime> INSTALLED_TIME { get; set; }
        public Nullable<System.DateTime> REMOVED_TIME { get; set; }
        public bool IS_CONNECTION { get; set; }
        public CommandType CmdType { get; set; }

        public List<string> WillPassSectionID { get; set; }
        public EventType VhRecentTranEvent { get; set; }

        public enum E_VH_TYPE : int
        {
            Clean = 1,
            Dirty = 2,
            None = 0
        }

        public void setObject(VEHICLE_INFO aVEHICLE)
        {
            setPosition(aVEHICLE);
            setStatus(aVEHICLE);
        }

        private void setStatus(VEHICLE_INFO aVEHICLE)
        {
            bool has_change = false;
            if (IS_CONNECTION != aVEHICLE.IsTcpIpConnect)
            {
                IS_CONNECTION = aVEHICLE.IsTcpIpConnect;
                has_change = true;
            }
            if (!Utility.isMatche(OHTC_CMD, aVEHICLE.OHTCCMD))
            {
                OHTC_CMD = aVEHICLE.OHTCCMD;
                has_change = true;
            }
            if (VhRecentTranEvent != aVEHICLE.VhRecentTranEvent)
            {
                VhRecentTranEvent = aVEHICLE.VhRecentTranEvent;
                has_change = true;
            }
            if (OBS_PAUSE != aVEHICLE.OBSPAUSE)
            {
                OBS_PAUSE = aVEHICLE.OBSPAUSE;
                has_change = true;
            }
            if (BLOCK_PAUSE != aVEHICLE.BLOCKPAUSE)
            {
                BLOCK_PAUSE = aVEHICLE.BLOCKPAUSE;
                has_change = true;
            }
            if (CMD_PAUSE != aVEHICLE.CMDPAUSE)
            {
                CMD_PAUSE = aVEHICLE.CMDPAUSE;
                has_change = true;
            }
            if (HID_PAUSE != aVEHICLE.HIDPAUSE)
            {
                HID_PAUSE = aVEHICLE.HIDPAUSE;
                has_change = true;
            }
            if (ERROR != aVEHICLE.ERROR)
            {
                ERROR = aVEHICLE.ERROR;
                has_change = true;
            }
            //earthquake_pause = aVEHICLE.;
            //safty_door_pause = aVEHICLE;
            //ohxc_obs_pause = aVEHICLE;
            if (MODE_STATUS != aVEHICLE.MODESTATUS)
            {
                this.MODE_STATUS = aVEHICLE.MODESTATUS;
                has_change = true;
            }
            if(IS_INSTALLED != aVEHICLE.ISINSTALLED)
            {
                this.IS_INSTALLED = aVEHICLE.ISINSTALLED;
                has_change = true;
            }
            if (ACT_STATUS != aVEHICLE.ACTSTATUS)
            {
                this.ACT_STATUS = aVEHICLE.ACTSTATUS;
                has_change = true;
            }

            if (IS_PARKING != aVEHICLE.ISPARKING)
            {
                this.IS_PARKING = aVEHICLE.ISPARKING;
                has_change = true;
            }
            if (X_Axis != aVEHICLE.XAxis)
            {
                this.X_Axis = aVEHICLE.XAxis;
                has_change = true;
            }
            if (Y_Axis != aVEHICLE.YAxis)
            {
                this.Y_Axis = aVEHICLE.YAxis;
                has_change = true;
            }
            if (HAS_CST != aVEHICLE.HASCST)
            {
                this.HAS_CST = aVEHICLE.HASCST;
                has_change = true;
            }
            if (CmdType != aVEHICLE.CmdType)
            {
                this.CmdType = aVEHICLE.CmdType;
                has_change = true;
            }

            WillPassSectionID = aVEHICLE.WillPassSectionID.ToList();
            if (has_change)
                StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        private void setPosition(VEHICLE_INFO aVEHICLE)
        {
            bool has_change = false;
            if (!Utility.isMatche(CUR_SEC_ID, aVEHICLE.CURSECID))
            {
                CUR_SEC_ID = aVEHICLE.CURSECID.Trim();
                has_change = true;
            }
            if (!Utility.isMatche(CUR_ADR_ID, aVEHICLE.CURADRID))
            {
                CUR_ADR_ID = aVEHICLE.CURADRID.Trim();
                has_change = true;
            }
            if (ACC_SEC_DIST != aVEHICLE.ACCSECDIST)
            {
                ACC_SEC_DIST = aVEHICLE.ACCSECDIST;
                has_change = true;
            }
            if (has_change)
                PositionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
