﻿using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using System.Collections.Generic;

namespace com.mirle.ibg3k0.sc.Service.Interface
{
    public interface IManualPortControlService
    {
        void Start(IEnumerable<IManualPortValueDefMapAction> ports);

        void ReflashState();

        bool GetPortPlcState(string portName, out ManualPortPLCInfo info);

        bool ChangeToInMode(string portName);

        bool ChangeToOutMode(string portName);

        bool MoveBack(string portName);

        bool SetMoveBackReason(string portName, MoveBackReasons reason);

        bool ResetAlarm(string portName);

        bool StopBuzzer(string portName);

        bool SetRun(string portName);

        bool SetStop(string portName);

        bool SetCommanding(string portName, bool setOn);

        bool SetControllerErrorIndex(string portName, int newIndex);
    }
}