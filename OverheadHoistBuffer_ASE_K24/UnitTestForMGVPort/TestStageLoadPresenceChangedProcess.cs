using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events;
using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Interface;
using com.mirle.ibg3k0.sc.Service;
using com.mirle.ibg3k0.sc.Service.Interface;
using NSubstitute;
using NUnit.Framework;
using UnitTestForMGVPort.StubObjects;
using static com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ManualPortEvents;

namespace UnitTestForMGVPort
{
    public class TestStageLoadPresenceChangedProcess
    {
        #region Const

        private const string _portName = "M01";
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
            var transferService = Substitute.For<IManualPortTransferService>();

            return new NStubObject(stubManualPortValueDefMapAction, reportBll, portBll, shelfBll, cassetteDataBll, commandBll, alarmBLL, transferService);
        }

        private ManualPortPLCInfo GetInModePortHasNoCarrierInfo()
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsRun = true;
            info.IsInMode = true;
            info.IsOutMode = false;
            info.IsDoorOpen = false;
            info.IsAlarm = false;
            info.IsDown = false;
            info.LoadPosition1 = false;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.CarrierIdOfStage1 = "";

            return info;
        }

        private ManualPortPLCInfo GetInModePortHasCarrierInfo(string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsRun = true;
            info.IsInMode = true;
            info.IsOutMode = false;
            info.IsDoorOpen = false;
            info.IsAlarm = false;
            info.IsDown = false;
            info.LoadPosition1 = true;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.CarrierIdOfStage1 = carrierId;

            return info;
        }

        private ManualPortPLCInfo GetOutModePortHasNoCarrierInfo()
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsRun = true;
            info.IsInMode = false;
            info.IsOutMode = true;
            info.IsDoorOpen = false;
            info.IsAlarm = false;
            info.IsDown = false;
            info.LoadPosition1 = false;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.CarrierIdOfStage1 = "";

            return info;
        }

        private ManualPortPLCInfo GetOutModePortHasCarrierInfo(string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsRun = true;
            info.IsInMode = false;
            info.IsOutMode = true;
            info.IsDoorOpen = false;
            info.IsAlarm = false;
            info.IsDown = false;
            info.LoadPosition1 = true;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.CarrierIdOfStage1 = carrierId;

            return info;
        }

        private CassetteData GetCarrierOnThisManualPort(string carrierId)
        {
            var cassette = new CassetteData();
            cassette.Carrier_LOC = _portName;
            cassette.BOXID = carrierId;
            cassette.CSTState = E_CSTState.WaitOut;
            cassette.Stage = 1;
            return cassette;
        }

        #endregion GET

        #region Port 變成有物

        [Test]
        public void InMode時StagePresenceON___不上報MCS任何事件()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL, stub.TransferService);
            var carrierId = "A";
            var info = GetInModePortHasCarrierInfo(carrierId);

            stub.ManualPortValueDefMapAction.OnLoadPresenceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportCarrierWaitIn(new CassetteData());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportForcedRemoveCarrier(new CassetteData());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportPortDirectionChanged(_portName, false);
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportPortInServiceChanged(_portName, false);
        }

        [Test]
        public void OutMode時StagePresenceON___除了WaitOut其他不上報MCS()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL, stub.TransferService);
            var carrierId = "A";
            var info = GetOutModePortHasCarrierInfo(carrierId);

            stub.ManualPortValueDefMapAction.OnLoadPresenceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportCarrierWaitIn(new CassetteData());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportForcedRemoveCarrier(new CassetteData());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportPortDirectionChanged(_portName, false);
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportPortInServiceChanged(_portName, false);
        }

        [Test]
        public void OutMode時StagePresenceON___需要上報WaitOut()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL, stub.TransferService);
            var carrierId = "A";
            var carrierDataOnPort = GetCarrierOnThisManualPort(carrierId);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(c => { c[2] = carrierDataOnPort; return true; });
            var info = GetOutModePortHasCarrierInfo(carrierId);

            stub.ManualPortValueDefMapAction.OnLoadPresenceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportCarrierWaitOut(new CassetteData());
        }

        #endregion Port 變成有物

        #region Port 變成無物

        [Test]
        public void InMode時StagePresenceOFF___不上報MCS任何事件()
        {
            var stub = GetStubObject();

            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL, stub.TransferService);
            var info = GetInModePortHasNoCarrierInfo();

            stub.ManualPortValueDefMapAction.OnLoadPresenceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportCarrierWaitIn(new CassetteData());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportForcedRemoveCarrier(new CassetteData());
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportPortDirectionChanged(_portName, false);
            stub.ReportBLL.DidNotReceiveWithAnyArgs().ReportPortInServiceChanged(_portName, false);
        }

        [Test]
        public void OutMode時StagePresenceOFF___上報MCS_CarrierRemove()
        {
            var stub = GetStubObject();

            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL, stub.TransferService);
            var carrierId = "A";
            var carrierDataOnPort = GetCarrierOnThisManualPort(carrierId);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(c => { c[2] = carrierDataOnPort; return true; });
            var info = GetOutModePortHasNoCarrierInfo();

            stub.ManualPortValueDefMapAction.OnLoadPresenceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportCarrierRemoveFromManualPort(carrierDataOnPort);
        }

        #endregion Port 變成無物
    }
}