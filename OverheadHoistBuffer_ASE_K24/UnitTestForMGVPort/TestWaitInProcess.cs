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
    public class TestWaitInProcess
    {
        #region Const

        private const string _duplicateCarrierId = "UNKD";
        private const string _portName = "M01";
        private const string _otherPortName = "M02";
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

        private ManualPortPLCInfo GetWaitInInfo(string carrierId)
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsDoorOpen = false;
            info.IsRun = true;
            info.IsAlarm = false;
            info.IsDown = false;
            info.IsInMode = true;
            info.IsOutMode = false;
            info.LoadPosition1 = true;
            info.CarrierIdOfStage1 = carrierId;
            info.IsBcrReadDone = true;
            info.IsWaitIn = true;

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

        private CassetteData GetCarrierOnPort(string carrierId, int stage)
        {
            var cassette = new CassetteData();
            cassette.Carrier_LOC = _otherPortName;
            cassette.BOXID = carrierId;
            cassette.CSTState = E_CSTState.Installed;
            cassette.Stage = stage;
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

        #region 正常 Wait In

        [Test]
        public void 沒有發生Duplicate__上報MCS()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out var _).Returns(false);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.ReceivedWithAnyArgs().ReportCarrierWaitIn(new CassetteData(), isDuplicate: false);
        }

        [Test]
        public void 沒有發生Duplicate__資料庫建帳於Port()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out var _).Returns(false);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.Received().Install(info.EQ_ID, carrierId);
        }

        [Test]
        public void 沒有發生Duplicate__上報MCS與PLC相同的CarrierId()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out var _).Returns(false);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.WaitInCassetteData.BOXID == carrierId);
            Assert.IsFalse(mockReportBLL.IsWaitInIdReadDuplicate);
        }

        #endregion 正常 Wait In

        #region 發生Duplicate_Duplicate的卡匣在儲位

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在儲位目前沒命令__上報MCS與PLC相同的CarrierId()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnShelf = GetCarrierOnShelf(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnShelf; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.WaitInCassetteData.BOXID == carrierId);
            Assert.IsTrue(mockReportBLL.IsWaitInIdReadDuplicate);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在儲位目前沒命令__上報MCS刪除Duplicate的卡匣()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnShelf = GetCarrierOnShelf(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnShelf; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.ForcedRemoveCassetteData.BOXID == carrierId);
            Assert.IsTrue(mockReportBLL.IsWaitInIdReadDuplicate);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在儲位目前沒命令__資料庫刪除Duplicate的卡匣()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnShelf = GetCarrierOnShelf(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnShelf; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.ReceivedWithAnyArgs().Delete(_ignoreCarrierId);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在儲位目前沒命令__資料庫建帳於Port()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnShelf = GetCarrierOnShelf(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnShelf; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.Received().Install(info.EQ_ID, carrierId);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在儲位目前有命令__上報MCS代表重複的UNKD開頭的CarrierId()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnShelf = GetCarrierOnShelf(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnShelf; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.WaitInCassetteData.BOXID.StartsWith(_duplicateCarrierId));
            Assert.IsTrue(mockReportBLL.IsWaitInIdReadDuplicate);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在儲位目前有命令__資料庫建UNKD帳於Port()
        {
            var stub = GetStubObject();
            var mockCassetteDataBLL = new MockCassetteDataBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnShelf = GetCarrierOnShelf(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnShelf; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockCassetteDataBLL.InstalledCarrierLocation == _portName);
            Assert.IsTrue(mockCassetteDataBLL.InstalledCarrierId.StartsWith(_duplicateCarrierId));
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在儲位目前有命令__資料庫不會刪除Duplicate的卡匣()
        {
            var stub = GetStubObject();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnShelf = GetCarrierOnShelf(carrierId);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnShelf; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.DidNotReceiveWithAnyArgs().Delete(_ignoreCarrierId);
        }

        #endregion 發生Duplicate_Duplicate的卡匣在儲位

        #region 發生Duplicate_Duplicate的卡匣在Port

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在Port目前沒命令__上報MCS與PLC相同的CarrierId()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnPort = GetCarrierOnPort(carrierId, stage: 1);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnPort; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.WaitInCassetteData.BOXID == carrierId);
            Assert.IsTrue(mockReportBLL.IsWaitInIdReadDuplicate);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在Port目前沒命令__資料庫刪除Duplicate的卡匣()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnPort = GetCarrierOnPort(carrierId, stage: 1);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnPort; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.ReceivedWithAnyArgs().Delete(_ignoreCarrierId);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在Port目前沒命令__資料庫建帳於WaitIn的Port()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnPort = GetCarrierOnPort(carrierId, stage: 1);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnPort; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.Received().Install(info.EQ_ID, carrierId);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在Port目前有命令__上報MCS代表重複的UNKD開頭的CarrierId()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnPort = GetCarrierOnPort(carrierId, stage: 1);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnPort; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.WaitInCassetteData.BOXID.StartsWith(_duplicateCarrierId));
            Assert.IsTrue(mockReportBLL.IsWaitInIdReadDuplicate);
        }

        [Test]
        public void 發生Duplicate_Duplicate的卡匣在Port目前有命令__資料庫不會刪除Duplicate的卡匣()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var info = GetWaitInInfo(carrierId);
            var carrierOnPort = GetCarrierOnPort(carrierId, stage: 1);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out Arg.Any<CassetteData>()).Returns(c => { c[1] = carrierOnPort; return true; });
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out var _).Returns(false);
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(true);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.DidNotReceiveWithAnyArgs().Delete(_ignoreCarrierId);
        }

        #endregion 發生Duplicate_Duplicate的卡匣在Port

        #region ManualPort 上有殘帳

        [Test]
        public void ManualPort上有殘帳且沒命令__資料庫刪除殘留帳()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var residueCarrierID = "B";
            var info = GetWaitInInfo(carrierId);
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out var _).Returns(false);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(c => { c[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.CassetteDataBLL.ReceivedWithAnyArgs().Delete(_ignoreCarrierId);
        }

        [Test]
        public void ManualPort上有殘帳且沒命令__上報MCS刪除Duplicate的卡匣()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var residueCarrierID = "B";
            var info = GetWaitInInfo(carrierId);
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out var _).Returns(false);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(c => { c[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.ForcedRemoveCassetteData.BOXID == residueCarrierID);
            Assert.IsFalse(mockReportBLL.IsWaitInIdReadDuplicate);
        }

        [Test]
        public void ManualPort上有殘帳且沒命令__上報MCS與PLC相同的CarrierId()
        {
            var stub = GetStubObject();
            var mockReportBLL = new MockReportBLL();
            //IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            IManualPortEventService manualPortService = new ManualPortEventService();
            manualPortService.Start(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var carrierId = "A";
            var residueCarrierID = "B";
            var info = GetWaitInInfo(carrierId);
            var carrierOnPort = GetCarrierOnThisManualPort(residueCarrierID);
            stub.CassetteDataBLL.GetCarrierByBoxId(carrierId, out var _).Returns(false);
            stub.CassetteDataBLL.GetCarrierByPortName(_portName, stage: 1, out Arg.Any<CassetteData>()).Returns(c => { c[2] = carrierOnPort; return true; });
            stub.CommandBLL.GetCommandByBoxId(carrierId, out var _).Returns(false);

            stub.ManualPortValueDefMapAction.OnWaitIn += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            Assert.IsTrue(mockReportBLL.WaitInCassetteData.BOXID == carrierId);
            Assert.IsFalse(mockReportBLL.IsWaitInIdReadDuplicate);
        }

        #endregion ManualPort 上有殘帳
    }
}