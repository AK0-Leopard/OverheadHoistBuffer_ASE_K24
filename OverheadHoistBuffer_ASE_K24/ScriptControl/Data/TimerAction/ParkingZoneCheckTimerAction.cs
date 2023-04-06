//*********************************************************************************
//      ZoneBlockCheck.cs
//*********************************************************************************
// File Name: ZoneBlockCheck.cs
// Description: 
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using com.mirle.ibg3k0.bcf.App;
using com.mirle.ibg3k0.bcf.Controller;
using com.mirle.ibg3k0.bcf.Data.TimerAction;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.Service;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.TimerAction
{
    /// <summary>
    /// Class ZoneBlockCheck.
    /// </summary>
    /// <seealso cref="com.mirle.ibg3k0.bcf.Data.TimerAction.ITimerAction" />
    class ParkingZoneCheckTimerAction : ITimerAction
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The sc application
        /// </summary>
        protected SCApplication scApp = null;
        private ZoneService zoneService = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmCheckTimerAction"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="intervalMilliSec">The interval milli sec.</param>
        public ParkingZoneCheckTimerAction(string name, long intervalMilliSec)
            : base(name, intervalMilliSec)
        {

        }

        /// <summary>
        /// Initializes the start.
        /// </summary>
        public override void initStart()
        {
            //do nothing
            scApp = SCApplication.getInstance();
            zoneService = scApp.ZoneService;
        }

        /// <summary>
        /// Timer Action的執行動作
        /// </summary>
        /// <param name="obj">The object.</param>
        
        private long checkSyncPoint = 0;
        public override void doProcess(object obj)
        {
            if (System.Threading.Interlocked.Exchange(ref checkSyncPoint, 1) == 0)
            {
                try
                {
                    if (!DebugParameter.IsOpenParkingZoneAutoPull)
                    {
                        return;
                    }
                    //zoneService.MoveAllidleVHintoParkingzone();

                    var pzstatus = zoneService.GetHightAndLowWaterLevelPZ();
                    //List<ParkingZone> High_waterLevel_parkingzones = pzstatus.Hightpzs;
                    //List<ParkingZone> Low_waterLevel_parkingzones = pzstatus.Lowpzs;
                    AVEHICLE movedvh;

                    if (pzstatus.Hightpzs.Count > 0) //need_balence_waterlevel
                    {
                        foreach (ParkingZone parkingZone in pzstatus.Lowpzs)
                        {
                            bool result = zoneService.TryChooseVehicleToDriveIn(parkingZone, out movedvh);
                            
                            //LogHelper.Log(logger: logger, LogLevel: LogLevel.Debug, Class: nameof(ParkingZoneCheckTimerAction), Device: string.Empty,
                            //    Data: $" move vh is success: {result} ");
                        }
                    }

                    zoneService.pushAllParkingZone();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exection:");
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref checkSyncPoint, 0);
                }
            }
        }
    }
}