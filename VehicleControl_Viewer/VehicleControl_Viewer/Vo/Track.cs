using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VehicleControl_Viewer.Vo
{
    public class Track
    {
        #region 列舉資料型態
        public enum TrackDir
        {
            TrackDir_None = 0,
            TrackDir_Straight = 1,
            TrackDir_Curve = 2
        }
        public enum TrackStatus
        {
            TrackStatus_NotDefine = 0,
            TrackStatus_Manaul = 1,
            TrackStatus_Auto = 2,
            TrackStatus_Alarm = 3
        }
        public enum TrackBlock
        {
            TrackBlock_None = 0,
            TrackBlock_Block = 1,
            TrackBlock_NonBlock = 2
        }
        public enum TrackAlarm
        {
            TrackAlarm_EMO_Error = 0,
            TrackAlarm_Servo_No_On = 1,
            TrackAlarm_Servo_NotGoHome = 2,
            TrackAlarm_CarOut_Timeout = 3,
            TrackAlarm_ServoOn_Timeout = 4,
            TrackAlarm_ServoOff_Timeout = 5,
            TrackAlarm_GoHome_TimeOut = 6,
            TrackAlarm_Pos1Move_TimeOut = 7,
            TrackAlarm_Pos2Move_TimeOut = 8,
            TrackAlarm_PosLimit_Error = 9,
            TrackAlarm_NegLimit_Error = 10,
            TrackAlarm_Drive_Error = 11,
            TrackAlarm_PosSensorAllOn = 12,
            TrackAlarm_CarInTrackCantAuto = 13,
            TrackAlarm_TrackIsManual = 14,
            TrackAlarm_BlockClosInManual = 15
            //TrackAlarm_IPCAlive_Error = 12,
        }
        #endregion

        #region 轉轍器資訊
        public string id { get; set; }
        public TrackDir dir { get; set;  }
        public int alarmCode { get; set;  }
        public TrackStatus status {  get; set; }
        public TrackBlock block { get; set;  }
        public TrackAlarm alarm { get; set; }
        public bool isAlive { get; set;  }
        public int Position_X { get; set;  }
        public int Position_Y { get; set; }
        #endregion
        public Track(string ID, string POSITION_X, string POSITION_Y)
        {
            Position_X = Convert.ToInt32(POSITION_X);
            Position_Y = Convert.ToInt32(POSITION_Y);
            id = ID;
        }


    }
}
