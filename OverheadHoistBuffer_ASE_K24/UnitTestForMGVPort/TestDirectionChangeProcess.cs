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
    public class TestDirectionChangeProcess
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

            return new NStubObject(stubManualPortValueDefMapAction, reportBll, portBll, shelfBll, cassetteDataBll, commandBll, alarmBLL);
        }

        private ManualPortPLCInfo GetInModePortInfo()
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

        private ManualPortPLCInfo GetOutModePortInfo()
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

        private CassetteData GetCarrierOnShelf(string carrierId)
        {
            var cassette = new CassetteData();
            cassette.Carrier_LOC = "100101";
            cassette.BOXID = carrierId;
            cassette.CSTState = E_CSTState.Installed;
            cassette.Stage = 1;
            return cassette;
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

        #region 轉成 InMode

        [Test]
        public void 無殘帳時轉成InMode___資料庫的PortDef資料表需轉成InMode()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInModePortInfo();
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.PortDefBLL.ChangeDirectionToInMode(_portName).Returns(true);

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.PortDefBLL.Received().ChangeDirectionToInMode(_portName);
            stub.PortDefBLL.DidNotReceive().ChangeDirectionToOutMode(_portName);
        }

        [Test]
        public void 無殘帳時轉成InMode___上報MCS轉成InMode()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInModePortInfo();
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportPortDirectionChanged(_portName, newDirectionIsInMode: true);
        }

        [Test]
        public void 有殘帳時轉成InMode__上報MCS轉成InMode()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportPortDirectionChanged(_portName, newDirectionIsInMode: true);
        }

        [Test]
        public void 有殘帳時轉成InMode__上報MCS刪除殘帳()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.ForcedRemoveCassetteData.BOXID == residueCarrierID);
        }

        [Test]
        public void 有殘帳時轉成InMode__資料庫刪除殘帳()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.ReceivedWithAnyArgs().Delete(_ignoreCarrierId);
        }

        [Test]
        public void 有殘帳且殘留命令時轉成InMode__資料庫刪除殘帳()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(residueCarrierID, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.ReceivedWithAnyArgs().Delete(_ignoreCarrierId);
        }

        [Test]
        public void 有殘帳且殘留命令時轉成InMode__資料庫刪除殘帳的命令()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(residueCarrierID, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CommandBLL.Received().Delete(residueCarrierID);
        }

        #endregion 轉成 InMode

        #region 轉成 OutMode

        [Test]
        public void 無殘帳時轉成OutMode___資料庫的PortDef資料表需轉成OutMode()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutModePortInfo();
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.PortDefBLL.ChangeDirectionToOutMode(_portName).Returns(true);

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.PortDefBLL.Received().ChangeDirectionToOutMode(_portName);
            stub.PortDefBLL.DidNotReceive().ChangeDirectionToInMode(_portName);
        }

        [Test]
        public void 無殘帳時轉成OutMode__上報MCS轉成OutMode()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutModePortInfo();
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportPortDirectionChanged(_portName, newDirectionIsInMode: false);
        }

        [Test]
        public void 有殘帳時轉成OutMode__上報MCS轉成OutMode()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportPortDirectionChanged(_portName, newDirectionIsInMode: false);
        }

        [Test]
        public void 有殘帳時轉成OutMode__上報MCS刪除殘帳()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.ForcedRemoveCassetteData.BOXID == residueCarrierID);
        }

        [Test]
        public void 有殘帳時轉成OutMode__資料庫刪除殘帳()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.ReceivedWithAnyArgs().Delete(_ignoreCarrierId);
        }

        [Test]
        public void 有殘帳且殘留命令時轉成OutMode__資料庫刪除殘帳()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(residueCarrierID, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.ReceivedWithAnyArgs().Delete(_ignoreCarrierId);
        }

        [Test]
        public void 有殘帳且殘留命令時轉成OutMode__資料庫刪除殘帳的命令()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutModePortInfo();
            var residueCarrierID = "B";
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(p => { p[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(residueCarrierID, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnDirectionChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CommandBLL.Received().Delete(residueCarrierID);
        }

        #endregion 轉成 OutMode
    }
}