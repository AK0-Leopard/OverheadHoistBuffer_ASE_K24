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
using static com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ManualPortEvents;

namespace UnitTestForMGVPort
{
    public class TestAlarmProcess
    {
        #region Const

        private const string _portName = "M01";
        private const string _alarmCode = "A001";
        private const string _ignoreCarrierId = "";

        #endregion Const

        #region GET
        #endregion GET

        [Test]
        public void 發生Alarm_InMode無物無命令__上報MCS_AlarmSet()
        {
        }

    }
}