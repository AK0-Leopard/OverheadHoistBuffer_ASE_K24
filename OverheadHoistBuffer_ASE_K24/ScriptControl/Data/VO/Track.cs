using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class Track : AUNIT
    {
        public event EventHandler<alarmCodeChangeArgs> alarmCodeChange;
        public class alarmCodeChangeArgs : EventArgs
        {
            public string railChanger_No;
            public List<TrackAlarm> alarmList_old;
            public List<TrackAlarm> alarmList_new;
            public alarmCodeChangeArgs(int alarmCode_old, int alarmCode_new)
            {
                char[] alarmString = Convert.ToString(alarmCode_old, 2).PadLeft(16, '0').ToCharArray();
                #region alarmCode to alarmList (old)
                if (alarmString[15] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_EMO_Error);
                if (alarmString[14] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_Servo_No_On);
                if (alarmString[13] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_Servo_NotGoHome);
                if (alarmString[12] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_CarOut_Timeout);
                if (alarmString[11] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_ServoOn_Timeout);
                if (alarmString[10] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_ServoOff_Timeout);
                if (alarmString[9] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_GoHome_TimeOut);
                if (alarmString[8] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_Pos1Move_TimeOut);
                if (alarmString[7] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_Pos2Move_TimeOut);
                if (alarmString[6] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_PosLimit_Error);
                if (alarmString[5] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_NegLimit_Error);
                if (alarmString[4] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_Drive_Error);
                if (alarmString[3] == '1')
                    alarmList_old.Add(TrackAlarm.TrackAlarm_IPCAlive_Error);
                #endregion
                alarmString = Convert.ToString(alarmCode_new, 2).PadLeft(16, '0').ToCharArray();
                #region alarmCode to alarmList (new)
                if (alarmString[15] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_EMO_Error);
                if (alarmString[14] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_Servo_No_On);
                if (alarmString[13] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_Servo_NotGoHome);
                if (alarmString[12] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_CarOut_Timeout);
                if (alarmString[11] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_ServoOn_Timeout);
                if (alarmString[10] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_ServoOff_Timeout);
                if (alarmString[9] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_GoHome_TimeOut);
                if (alarmString[8] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_Pos1Move_TimeOut);
                if (alarmString[7] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_Pos2Move_TimeOut);
                if (alarmString[6] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_PosLimit_Error);
                if (alarmString[5] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_NegLimit_Error);
                if (alarmString[4] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_Drive_Error);
                if (alarmString[3] == '1')
                    alarmList_new.Add(TrackAlarm.TrackAlarm_IPCAlive_Error);
                #endregion
            }
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
            TrackAlarm_IPCAlive_Error = 12,
        }

        NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public RailChangerProtocol.TrackBlock TrackBlock { get; private set; }
        public bool IsBlocking { get { return TrackBlock == RailChangerProtocol.TrackBlock.Block; } }
        public bool IsAlive { get; private set; }
        public TrackDir TrackDir { get; private set; }
        public RailChangerProtocol.TrackStatus TrackStatus { get; private set; }
        public int AlarmCode { get; private set; }
        public UInt32 ResetCount { get; private set; }

        public List<string> RelatedSection { get; private set; } = new List<string>();
        public string sRelatedSection { get; private set; } = "";
        public Stopwatch stopwatch { get; private set; } = new Stopwatch();
        public string LastUpdateTime
        {
            get
            {
                return stopwatch.ElapsedMilliseconds.ToString();
            }
        }
        public void setRelatedSection(sc.App.SCApplication app)
        {
            var block_track_infos = app.BlockTrackMapDao.loadBlockTrackInfoByTackID(app, this.UNIT_ID);

            foreach (var info in block_track_infos)
            {
                var block_master = app.BlockControlBLL.cache.getBlockZoneMaster(info.ENTRY_SEC_ID);
                if (block_master == null)
                {
                    logger.Warn($"Want get block master:{info.ENTRY_SEC_ID},but no exist.Fun:setRelatedSection");
                    continue;
                }
                RelatedSection.AddRange(block_master.GetBlockZoneDetailSectionIDs());
            }
            RelatedSection = RelatedSection.Distinct().ToList();
            sRelatedSection = string.Join(",", RelatedSection);
        }
        public void setTrackInfo(RailChangerProtocol.TrackInfo trackInfo)
        {
            ProtocolFormat.OHTMessage.TrackDir trackDir = convert(trackInfo.Dir);

            if (hasDifferent(trackInfo))
            {
                logger.Debug(trackInfo.ToString());
            }

            TrackDir = trackDir;
            //如果現在狀態與先前不同而且現在狀態為alarm代表alarm第一次發生
            if (TrackStatus != trackInfo.Status && trackInfo.Status == RailChangerProtocol.TrackStatus.Alarm)
                this.onAlarmCodeChange(AlarmCode, trackInfo.AlarmCode, UNIT_ID);
            TrackStatus = trackInfo.Status;
            AlarmCode = trackInfo.AlarmCode;
            TrackBlock = trackInfo.IsBlock;
            IsAlive = trackInfo.Alive;

            stopwatch.Restart();
        }

        private bool hasDifferent(RailChangerProtocol.TrackInfo trackInfo)
        {
            ProtocolFormat.OHTMessage.TrackDir trackDir = convert(trackInfo.Dir);

            if (TrackDir != trackDir) return true;
            else if (TrackStatus != trackInfo.Status) return true;
            else if (AlarmCode != trackInfo.AlarmCode) return true;
            else if (TrackBlock != trackInfo.IsBlock) return true;
            else if (IsAlive != trackInfo.Alive) return true;
            else return false;
        }


        private ProtocolFormat.OHTMessage.TrackDir convert(RailChangerProtocol.TrackDir dir)
        {
            switch (dir)
            {
                case RailChangerProtocol.TrackDir.None:
                    return ProtocolFormat.OHTMessage.TrackDir.None;
                case RailChangerProtocol.TrackDir.Straight:
                    return ProtocolFormat.OHTMessage.TrackDir.Straight;
                case RailChangerProtocol.TrackDir.Curve:
                    return ProtocolFormat.OHTMessage.TrackDir.Curve;
                default:
                    return ProtocolFormat.OHTMessage.TrackDir.None;
            }
        }

        public void ResetBlock(WebAPI.TrackInfoClient trackInfoClient)
        {
            if (trackInfoClient == null) return;
            bool is_success = trackInfoClient.ResetBlockAsync(UNIT_ID).Result;
            if (is_success)
            {
                ResetCount++;
            }
        }
        
        public Track()
        {
            
        }
        
        public void onAlarmCodeChange(int alarmCode_old, int alarmCode_new, string no)
        {
            try
            {
                alarmCodeChangeArgs args = new alarmCodeChangeArgs(alarmCode_old, alarmCode_new);
                args.railChanger_No = no;

                alarmCodeChange?.Invoke(this, args);
            }
            catch(Exception e)
            {
                logger.Error(e, "Exception");
            }
        }

    }
}
