﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Grpc.Core;
using CommonMessage.ProtocolFormat.ManualPortFun;
using ScriptControl;
using com.mirle.ibg3k0.sc.Common;

namespace com.mirle.ibg3k0.sc.WebAPI.Grpc
{
    internal class ManualPort : manualPortGreeter.manualPortGreeterBase
    {
        com.mirle.ibg3k0.sc.App.SCApplication app;
        //List<string> manualPortList = new List<string>();
        public ManualPort(com.mirle.ibg3k0.sc.App.SCApplication _app)
        {
            app = _app;
            //eqObj = EQObjCacheManager.getInstance();
            //foreach (var portStation in eqObj.getALLPortStation())
            //{
            //    if (portStation is MANUAL_PORTSTATION)
            //    {
            //        manualPortList.Add(portStation.PORT_ID);
            //    }
            //}
        }

        public override Task<replyAllManualPortInfo> getAllManualPortInfo(Empty empty, ServerCallContext context)
        {
            replyAllManualPortInfo result = new replyAllManualPortInfo();
            var manual_ports = app.PortStationBLL.OperateCatch.loadPortStations();
            foreach (var port in manual_ports)
            {
                if (!(port is MANUAL_PORTSTATION))
                {
                    continue;
                }
                
                var manual_port = port as MANUAL_PORTSTATION;
                var temp = manual_port.getManualPortPLCInfo();
                if (temp is null) continue;
                manualPort data = new manualPort();
                //app.ManualPortControlService.GetPortPlcState(portID, out var temp);
                #region ManuPortPLCInfo to manualPort(Proto type)
                data.AlarmCode = temp.AlarmCode;
                data.CarrierIdOfStage1 = temp.CarrierIdOfStage1;
                data.CarrierIdReadResult = temp.CarrierIdReadResult;
                data.CstTypes = temp.CstTypes;
                data.ErrorIndex = temp.ErrorIndex;
                data.IsAlarm = temp.IsAlarm;
                data.IsBcrReadDone = temp.IsBcrReadDone;
                data.IsDirectionChangable = temp.IsDirectionChangable;
                data.IsDoorOpen = temp.IsDoorOpen;
                data.IsDown = temp.IsDown;
                data.IsHeartBeatOn = temp.IsHeartBeatOn;
                data.IsInMode = temp.IsInMode;
                data.IsLoadOK = temp.IsLoadOK;
                data.IsOutMode = temp.IsOutMode;
                data.IsRemoveCheck = temp.IsRemoveCheck;
                data.IsRun = temp.IsRun;
                data.IsTransferComplete = temp.IsTransferComplete;
                data.IsUnloadOK = temp.IsUnloadOK;
                data.IsWaitIn = temp.IsWaitIn;
                data.IsWaitOut = temp.IsWaitOut;
                data.LoadPosition1 = temp.LoadPosition1;
                data.LoadPosition2 = temp.LoadPosition2;
                data.LoadPosition3 = temp.LoadPosition3;
                data.LoadPosition4 = temp.LoadPosition4;
                data.LoadPosition5 = temp.LoadPosition5;
                data.ManualPortId = port.PORT_ID;
                data.RunEnable = temp.RunEnable;
                data.AddressID = port.ADR_ID;
                #endregion

                result.ManualPortInfo.Add(data);
            }
            return Task.FromResult(result);
        }
        public override Task<replyAllEQPortInfo> getAllEQPortInfo(Empty empty, ServerCallContext context)
        {
            replyAllEQPortInfo result = new replyAllEQPortInfo();
            foreach (var port in app.PortStationBLL.OperateCatch.loadPortStations())
            {
                if(port.IsEqPort(app.EquipmentBLL))
                {
                    EQPort eqPort = new EQPort();
                    eqPort.EQPortAddress = port.ADR_ID;
                    eqPort.EQPortID = port.PORT_ID;
                    result.EQPortInfo.Add(eqPort);
                }
            }
            return Task.FromResult(result);
        }
    }
}
