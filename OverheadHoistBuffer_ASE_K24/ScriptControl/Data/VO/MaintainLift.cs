﻿using com.mirle.ibg3k0.bcf.Common;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction;
using com.mirle.ibg3k0.sc.Data.VO.Interface;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class MaintainLift : AEQPT, IMaintainDevice
    {
        public enum MTSDoorStatus
        {
            Close = 0,
            Open = 1
        }
        public string MTL_SYSTEM_OUT_ADDRESS = "20292";
        public string MTL_CAR_IN_BUFFER_ADDRESS = "24294";
        public string MTL_SEGMENT = "013";
        public string MTL_ADDRESS = "20293";
        public string MTL_SYSTEM_IN_ADDRESS = "20198";

        public string MTS_SEGMENT { get; private set; } = "013";
        public string MTS_ADDRESS { get; private set; } = "20292";
        public string MTS_SYSTEM_IN_ADDRESS { get; private set; } = "20199";



        public bool HasDokingMTS = false;

        public string DeviceID { get { return EQPT_ID; } set { } }
        public string DeviceSegment { get { return MTL_SEGMENT; } set { } }
        public string DeviceAddress { get { return MTL_ADDRESS; } set { } }
        [JsonIgnore]
        //public IMaintainDevice DokingMaintainDevice = null;

        //public string CurrentCarID { get; set; }
        //public bool HasVehicle { get; set; }
        //public bool StopSingle { get; set; }
        //public MTxMode MTxMode { get; set; }
        //public MTLLocation MTLLocation;
        //public MTLMovingStatus MTLMovingStatus;
        //public UInt32 Encoder;
        //public VhInPosition VhInPosition;
        //public bool CarInSafetyCheck { get; set; }
        //public bool CarOutSafetyCheck { get; set; }
        public string PreCarOutVhID { get; set; }
        public bool CancelCarOutRequest { get; set; }
        public bool CarOurSuccess { get; set; }

        public ushort CurrentPreCarOurID { get; set; }
        public ushort CurrentPreCarOurActionMode { get; set; }
        public ushort CurrentPreCarOurCSTExist { get; set; }
        public ushort CurrentPreCarOurSectionID { get; set; }
        public uint CurrentPreCarOurAddressID { get; set; }
        private uint currentPreCarOurDistance;
        public uint CurrentPreCarOurDistance
        {
            get { return currentPreCarOurDistance; }
            set
            {
                if (currentPreCarOurDistance != value)
                {
                    currentPreCarOurDistance = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.CurrentPreCarOurDistance));
                }
            }
        }


        public MTSDoorStatus MTSFrontDoorStatus;
        public MTSDoorStatus MTSBackDoorStatus;

        public bool MTLHasVehicle { get; set; }
        public bool MTSHasVehicle { get; set; }
        public bool MTLStopSingle { get; set; }
        public bool MTSStopSingle { get; set; }

        public ushort CurrentPreCarOurSpeed { get; set; }

        private bool carOutInterlock;
        public bool CarOutInterlock
        {
            get { return carOutInterlock; }
            set
            {
                if (carOutInterlock != value)
                {
                    carOutInterlock = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.CarOutInterlock));
                }
            }
        }

        private bool carInMoving;
        public bool CarInMoving
        {
            get { return carInMoving; }
            set
            {
                if (carInMoving != value)
                {
                    carInMoving = value;
                    OnPropertyChanged(BCFUtility.getPropertyName(() => this.CarInMoving));
                }
            }
        }

        //public bool IsAlive { get { return base.Is_Eq_Alive; } set { } }
        double MAX_ALLOW_ALIVE_NOT_CHANGE_TIME = 10;
        public bool IsAlive
        {
            get
            {
                // 12:00:00 + 10 = 12:00:10 > 12:00:05
                // 12:00:00 + 10 = 12:00:10 > 12:00:11
                return base.Eq_Alive_Last_Change_time.AddSeconds(MAX_ALLOW_ALIVE_NOT_CHANGE_TIME) > DateTime.Now;
                //base.Is_Eq_Alive;
            }
            set { }
        }

        public (bool isSendSuccess, UInt16 returnCode) carOutRequest(UInt16 carNum)
        {

            //return getExcuteMapAction().OHxC_CarOutNotify(carNum,2);
            MTLValueDefMapActionNewPH2 mapAction = getExcuteMapActionNew();
            return mapAction.OHxC_CarOutNotify(carNum, 2);
        }

        public (bool isSendSuccess, UInt16 returnCode) MTSToMTLRequest(UInt16 carNum)
        {
            MTLValueDefMapActionNewPH2 mapAction = getExcuteMapActionNew();
            return mapAction.OHxC_CarOutNotify(carNum, 3);
        }

        public bool SetCarOutInterlock(bool onOff)
        {
            //return getExcuteMapAction().setOHxC2MTL_CarOutInterlock(onOff);

            MTLValueDefMapActionNewPH2 mapAction = getExcuteMapActionNew();
            return mapAction.setOHxC2MTL_CarOutInterlock(onOff);
        }
        public bool SetCarInMoving(bool onOff)
        {
            //return getExcuteMapAction().setOHxC2MTL_CarInMoving(onOff);


            MTLValueDefMapActionNewPH2 mapAction = getExcuteMapActionNew();
            return mapAction.setOHxC2MTL_CarInMoving(onOff);
        }



        public void setCarRealTimeInfo(UInt16 car_id, UInt16 action_mode, UInt16 cst_exist, UInt16 current_section_id, UInt32 current_address_id,
                                            UInt32 buffer_distance, UInt16 speed)
        {
            MTLValueDefMapActionNewPH2 mapAction = getExcuteMapActionNew();
            mapAction.CarRealtimeInfo(car_id, action_mode, cst_exist, current_section_id, current_address_id, buffer_distance, speed);

        }

        private MTLValueDefMapActionNewPH2 getExcuteMapActionNew()
        {
            MTLValueDefMapActionNewPH2 mapAction;
            mapAction = this.getMapActionByIdentityKey(typeof(MTLValueDefMapActionNewPH2).Name) as MTLValueDefMapActionNewPH2;

            return mapAction;
        }

        public void setMTLSegment(string adrID)
        {
            MTL_SEGMENT = adrID;
        }

        public void setMTLAddress(string adrID)
        {
            MTL_ADDRESS = adrID;
        }
        public void setMTLSystemInAddress(string adrID)
        {
            MTL_SYSTEM_IN_ADDRESS = adrID;
        }
        public void setMTLCarInBufferAddress(string adrID)
        {
            MTL_CAR_IN_BUFFER_ADDRESS = adrID;
        }
        public void setMTLSystemOutAddress(string adrID)
        {
            MTL_SYSTEM_OUT_ADDRESS = adrID;
        }

        public void SetOHxCToMTx_Alive()
        {
            MTLValueDefMapActionNewPH2 mapAction = getExcuteMapActionNew();
            mapAction.OHxCToMTx_Alive();
        }
    }
}
