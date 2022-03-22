using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.Service;
using com.mirle.ibg3k0.sc.Service.Interface;
using NSubstitute;
using NUnit.Framework;
using System;
using UnitTestForMGVPort.StubObjects;
using static com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ManualPortEvents;

namespace UnitTestForMGVPort
{
    public class TestAlarmProcess
    {
        #region Const

        private const string _portName = "M01";
        private const string _alarmCode = "41";
        private const string _ignoreCarrierId = "";

        #endregion Const

        #region GET

        private NStubObject GetStubObject()
        {
            var stubManualPortValueDefMapAction = Substitute.For<IManualPortValueDefMapAction>();
            var reportBll = Substitute.For<IManualPortReportBLL>();
            var portBll = Substitute.For<IManualPortDefBLL>();
            var shelfBll = Substitute.For<IManualPortShelfDefBLL>();
            var cassetteDataBll = Substitute.For<IManualPortCassetteDataBLL>();
            var commandBll = Substitute.For<IManualPortCMDBLL>();
            var alarmBLL = Substitute.For<IManualPortAlarmBLL>();

            return new NStubObject(stubManualPortValueDefMapAction, reportBll, portBll, shelfBll, cassetteDataBll, commandBll, alarmBLL);
        }

        private ManualPortPLCInfo GetNormalInModePortInfo(string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsDoorOpen = false;
            info.IsRun = true;
            info.IsAlarm = false;
            info.IsDown = false;
            info.IsInMode = true;
            info.IsOutMode = false;
            info.LoadPosition1 = string.IsNullOrEmpty(carrierId) == false;
            info.CarrierIdOfStage1 = carrierId;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.AlarmCode = 0;
            info.ErrorIndex = 1;

            return info;
        }

        private ManualPortPLCInfo GetAlarmInModePortInfo(string alarmCode, string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsDoorOpen = false;
            info.IsRun = false;
            info.IsAlarm = true;
            info.IsDown = true;
            info.IsInMode = true;
            info.IsOutMode = false;
            info.LoadPosition1 = string.IsNullOrEmpty(carrierId) == false;
            info.CarrierIdOfStage1 = carrierId;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.AlarmCode = (ushort)int.Parse(alarmCode);
            info.ErrorIndex = 1;

            return info;
        }

        private ManualPortPLCInfo GetWarningInModePortInfo(string alarmCode, string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsDoorOpen = false;
            info.IsRun = true;
            info.IsAlarm = false;
            info.IsDown = false;
            info.IsInMode = true;
            info.IsOutMode = false;
            info.LoadPosition1 = string.IsNullOrEmpty(carrierId) == false;
            info.CarrierIdOfStage1 = carrierId;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.AlarmCode = (ushort)int.Parse(alarmCode);
            info.ErrorIndex = 1;

            return info;
        }

        private ManualPortPLCInfo GetNormalOutModePortInfo(string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsDoorOpen = false;
            info.IsRun = true;
            info.IsAlarm = false;
            info.IsDown = false;
            info.IsInMode = false;
            info.IsOutMode = true;
            info.LoadPosition1 = string.IsNullOrEmpty(carrierId) == false;
            info.CarrierIdOfStage1 = carrierId;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.AlarmCode = 0;
            info.ErrorIndex = 1;

            return info;
        }

        private ManualPortPLCInfo GetAlarmOutModePortInfo(string alarmCode, string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsDoorOpen = false;
            info.IsRun = false;
            info.IsAlarm = true;
            info.IsDown = true;
            info.IsInMode = false;
            info.IsOutMode = true;
            info.LoadPosition1 = string.IsNullOrEmpty(carrierId) == false;
            info.CarrierIdOfStage1 = carrierId;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.AlarmCode = (ushort)int.Parse(alarmCode);
            info.ErrorIndex = 1;

            return info;
        }

        private ManualPortPLCInfo GetWarningOutModePortInfo(string alarmCode, string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsDoorOpen = false;
            info.IsRun = true;
            info.IsAlarm = false;
            info.IsDown = false;
            info.IsInMode = false;
            info.IsOutMode = true;
            info.LoadPosition1 = string.IsNullOrEmpty(carrierId) == false;
            info.CarrierIdOfStage1 = carrierId;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.AlarmCode = (ushort)int.Parse(alarmCode);
            info.ErrorIndex = 1;

            return info;
        }

        private ALARM GetAlarmSetInfo(E_ALARM_LVL level, string alarmCode, string commandId)
        {
            var alarm = new ALARM();
            alarm.EQPT_ID = _portName;
            alarm.UnitID = _portName;
            alarm.ALAM_CODE = alarmCode;
            alarm.ERROR_ID = alarmCode;
            alarm.CMD_ID = commandId;
            alarm.ALAM_LVL = level;
            alarm.ALAM_STAT = ErrorStatus.ErrSet;
            alarm.END_TIME = null;

            return alarm;
        }

        private ALARM GetAlarmClearInfo(E_ALARM_LVL level, string alarmCode, string commandId)
        {
            var alarm = new ALARM();
            alarm.EQPT_ID = _portName;
            alarm.UnitID = _portName;
            alarm.ALAM_CODE = alarmCode;
            alarm.ERROR_ID = alarmCode;
            alarm.CMD_ID = commandId;
            alarm.ALAM_LVL = level;
            alarm.ALAM_STAT = ErrorStatus.ErrReset;
            alarm.END_TIME = DateTime.Now;

            return alarm;
        }

        private CassetteData GetCarrierOnThisManualPort(string carrierId)
        {
            var cassette = new CassetteData();
            cassette.Carrier_LOC = _portName;
            cassette.BOXID = carrierId;
            cassette.CSTState = E_CSTState.Installed;
            cassette.Stage = 1;
            return cassette;
        }

        #endregion GET

        #region Alarm Set

        [Test]
        public void 發生Alarm_InMode無物無命令__上報MCS_AlarmSet()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var portInfo = GetAlarmInModePortInfo(_alarmCode, carrierId: "");
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            var alarmInfo = GetAlarmSetInfo(E_ALARM_LVL.Error, _alarmCode, commandId: "");
            stub.AlarmBLL.SetAlarm(_portName, _alarmCode, Arg.Any<ACMD_MCS>(), out Arg.Any<ALARM>(), out Arg.Any<string>()).Returns(a => { a[3] = alarmInfo; return true; });

            stub.ManualPortValueDefMapAction.OnAlarmHappen += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportAlarmSet(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportUnitAlarmSet(new ALARM());
        }

        [Test]
        public void 發生Alarm_InMode有物無命令__上報MCS_AlarmSet()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var portInfo = GetAlarmInModePortInfo(_alarmCode, carrierId);
            var carrierOnPort = GetCarrierOnThisManualPort(carrierId);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(c => { c[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);
            var alarmInfo = GetAlarmSetInfo(E_ALARM_LVL.Error, _alarmCode, commandId: "");
            stub.AlarmBLL.SetAlarm(_portName, _alarmCode, Arg.Any<ACMD_MCS>(), out Arg.Any<ALARM>(), out Arg.Any<string>()).Returns(a => { a[3] = alarmInfo; return true; });

            stub.ManualPortValueDefMapAction.OnAlarmHappen += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportAlarmSet(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportUnitAlarmSet(new ALARM());
        }

        [Test]
        public void 發生Alarm_OutMode無物無命令__上報MCS_AlarmSet()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var portInfo = GetAlarmOutModePortInfo(_alarmCode, carrierId: "");
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            var alarmInfo = GetAlarmSetInfo(E_ALARM_LVL.Error, _alarmCode, commandId: "");
            stub.AlarmBLL.SetAlarm(_portName, _alarmCode, Arg.Any<ACMD_MCS>(), out Arg.Any<ALARM>(), out Arg.Any<string>()).Returns(a => { a[3] = alarmInfo; return true; });

            stub.ManualPortValueDefMapAction.OnAlarmHappen += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportAlarmSet(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportUnitAlarmSet(new ALARM());
        }

        [Test]
        public void 發生Alarm_OutMode有物無命令__上報MCS_AlarmSet()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var portInfo = GetAlarmOutModePortInfo(_alarmCode, carrierId);
            var carrierOnPort = GetCarrierOnThisManualPort(carrierId);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(c => { c[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);
            var alarmInfo = GetAlarmSetInfo(E_ALARM_LVL.Error, _alarmCode, commandId: "");
            stub.AlarmBLL.SetAlarm(_portName, _alarmCode, Arg.Any<ACMD_MCS>(), out Arg.Any<ALARM>(), out Arg.Any<string>()).Returns(a => { a[3] = alarmInfo; return true; });

            stub.ManualPortValueDefMapAction.OnAlarmHappen += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportAlarmSet(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportUnitAlarmSet(new ALARM());
        }

        #endregion Alarm Set

        #region Alarm Clear

        [Test]
        public void 清除Alarm_InMode無物無命令__上報MCS_AlarmClear()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            var alarmBLL = new StubAlarmBLL();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, alarmBLL);
            var portInfo = GetNormalInModePortInfo(carrierId: "");
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            var alarmInfo = GetAlarmClearInfo(E_ALARM_LVL.Error, _alarmCode, commandId: "");
            alarmBLL.Alarms.Add(alarmInfo);

            stub.ManualPortValueDefMapAction.OnAlarmClear += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportAlarmClear(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportUnitAlarmClear(new ALARM());
        }

        [Test]
        public void 清除Alarm_OutMode無物無命令__上報MCS_AlarmClear()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            var alarmBLL = new StubAlarmBLL();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, alarmBLL);
            var portInfo = GetNormalOutModePortInfo(carrierId: "");
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            var alarmInfo = GetAlarmClearInfo(E_ALARM_LVL.Error, _alarmCode, commandId: "");
            alarmBLL.Alarms.Add(alarmInfo);

            stub.ManualPortValueDefMapAction.OnAlarmClear += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportAlarmClear(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportUnitAlarmClear(new ALARM());
        }

        #endregion Alarm Clear

        #region Unit Alarm Set

        [Test]
        public void 發生Warning_InMode無物無命令__上報MCS_UnitAlarmSet()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var portInfo = GetWarningInModePortInfo(_alarmCode, carrierId: "");
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            var alarmInfo = GetAlarmSetInfo(E_ALARM_LVL.Warn, _alarmCode, commandId: "");
            stub.AlarmBLL.SetAlarm(_portName, _alarmCode, Arg.Any<ACMD_MCS>(), out Arg.Any<ALARM>(), out Arg.Any<string>()).Returns(a => { a[3] = alarmInfo; return true; });

            stub.ManualPortValueDefMapAction.OnAlarmHappen += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportUnitAlarmSet(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportAlarmSet(new ALARM());
        }

        [Test]
        public void 發生Warning_OutMode無物無命令__上報MCS_UnitAlarmSet()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var portInfo = GetWarningOutModePortInfo(_alarmCode, carrierId: "");
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            var alarmInfo = GetAlarmSetInfo(E_ALARM_LVL.Warn, _alarmCode, commandId: "");
            stub.AlarmBLL.SetAlarm(_portName, _alarmCode, Arg.Any<ACMD_MCS>(), out Arg.Any<ALARM>(), out Arg.Any<string>()).Returns(a => { a[3] = alarmInfo; return true; });

            stub.ManualPortValueDefMapAction.OnAlarmHappen += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportUnitAlarmSet(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportAlarmSet(new ALARM());
        }

        #endregion Unit Alarm Set

        #region Unit Alarm Clear

        [Test]
        public void 清除Warning_InMode無物無命令__上報MCS_UnitAlarmClear()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            var alarmBLL = new StubAlarmBLL();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, alarmBLL);
            var portInfo = GetNormalInModePortInfo(carrierId: "");
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            var alarmInfo = GetAlarmClearInfo(E_ALARM_LVL.Warn, _alarmCode, commandId: "");
            alarmBLL.Alarms.Add(alarmInfo);

            stub.ManualPortValueDefMapAction.OnAlarmClear += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportUnitAlarmClear(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportAlarmClear(new ALARM());
        }

        [Test]
        public void 清除Warning_OutMode無物無命令__上報MCS_UnitAlarmClear()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            var alarmBLL = new StubAlarmBLL();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, alarmBLL);
            var portInfo = GetNormalOutModePortInfo(carrierId: "");
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            var alarmInfo = GetAlarmClearInfo(E_ALARM_LVL.Warn, _alarmCode, commandId: "");
            alarmBLL.Alarms.Add(alarmInfo);

            stub.ManualPortValueDefMapAction.OnAlarmClear += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(portInfo));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportUnitAlarmClear(new ALARM());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportAlarmClear(new ALARM());
        }

        #endregion Unit Alarm Clear
    }
}