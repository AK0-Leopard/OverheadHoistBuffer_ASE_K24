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
    public class TestInServiceChangedProcess
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

        private ManualPortPLCInfo GetInServiceInModePortInfo()
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

        private ManualPortPLCInfo GetOutServiceInModePortInfo()
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsRun = false;
            info.IsInMode = true;
            info.IsOutMode = false;
            info.IsDoorOpen = false;
            info.IsAlarm = false;
            info.IsDown = true;
            info.LoadPosition1 = false;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.CarrierIdOfStage1 = "";

            return info;
        }

        private ManualPortPLCInfo GetInServiceOutModePortInfo()
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

        private ManualPortPLCInfo GetOutServiceOutModePortInfo()
        {
            var info = new ManualPortPLCInfo();
            info.EQ_ID = _portName;
            info.IsRun = false;
            info.IsInMode = false;
            info.IsOutMode = true;
            info.IsDoorOpen = false;
            info.IsAlarm = false;
            info.IsDown = true;
            info.LoadPosition1 = false;
            info.IsBcrReadDone = false;
            info.IsWaitIn = false;
            info.CarrierIdOfStage1 = "";

            return info;
        }

        #endregion GET

        #region Change to In Service

        [Test]
        public void InMode時變為InService___上報MCS轉成InService()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInServiceInModePortInfo();

            stub.ManualPortValueDefMapAction.OnInServiceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportPortInServiceChanged(_portName, newStateIsInService: true);
        }

        [Test]
        public void OutMode時變為InService___上報MCS轉成InService()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetInServiceOutModePortInfo();

            stub.ManualPortValueDefMapAction.OnInServiceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportPortInServiceChanged(_portName, newStateIsInService: true);
        }

        #endregion Change to In Service

        #region Change to Out Of Service

        [Test]
        public void InMode時變為OutService___上報MCS轉成InService()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutServiceInModePortInfo();

            stub.ManualPortValueDefMapAction.OnInServiceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportPortInServiceChanged(_portName, newStateIsInService: false);
        }

        [Test]
        public void OutMode時變為OutService___上報MCS轉成InService()
        {
            var stub = GetStubObject();
            IManualPortEventService manualPortService = new ManualPortEventService(stub.ManualPortValueDefMapActions, stub.ReportBLL, stub.PortDefBLL, stub.ShelfDefBLL, stub.CassetteDataBLL, stub.CommandBLL, stub.AlarmBLL);
            var info = GetOutServiceOutModePortInfo();

            stub.ManualPortValueDefMapAction.OnInServiceChanged += Raise.Event<ManualPortEventHandler>(this, new ManualPortEventArgs(info));

            stub.ReportBLL.Received().ReportPortInServiceChanged(_portName, newStateIsInService: false);
        }

        #endregion Change to Out Of Service
    }
}