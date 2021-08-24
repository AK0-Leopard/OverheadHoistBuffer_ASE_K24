using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using com.mirle.ibg3k0.sc.Service;
using NUnit.Framework;

namespace UnitTestForMGVPort
{
    public class TestWaitInCstType
    {
        [Test]
        public void 卡匣名稱第三四碼為LC代表為LITE_CASSETE___PLC讀到的是矮的____沒有Mismatch()
        {
            var manualPortService = new ManualPortEventService();
            var log = "";
            var plcInfo = new ManualPortPLCInfo();
            var carrierID = "12LC0001";
            plcInfo.CstTypes = (int)CstType.LiteCassete;
            plcInfo.CarrierIdOfStage1 = carrierID;
            plcInfo.CarrierIdReadResult = carrierID;

            var result = manualPortService.HasCstTypeMismatch(log, plcInfo);

            Assert.IsFalse(result);
        }

        [Test]
        public void 卡匣名稱第三四碼為BE代表為FOUP___PLC讀到的是高的____沒有Mismatch()
        {
            var manualPortService = new ManualPortEventService();
            var log = "";
            var plcInfo = new ManualPortPLCInfo();
            plcInfo.CstTypes = (int)CstType.Foup;
            var carrierID = "12BE0001";
            plcInfo.CarrierIdOfStage1 = carrierID;
            plcInfo.CarrierIdReadResult = carrierID;

            var result = manualPortService.HasCstTypeMismatch(log, plcInfo);

            Assert.IsFalse(result);
        }

        [Test]
        public void 卡匣名稱第三四碼為BE代表為FOUP___PLC讀到的是矮的____Mismatch()
        {
            var manualPortService = new ManualPortEventService();
            var log = "";
            var plcInfo = new ManualPortPLCInfo();
            plcInfo.CstTypes = (int)CstType.LiteCassete;
            var carrierID = "12BE0001";
            plcInfo.CarrierIdOfStage1 = carrierID;
            plcInfo.CarrierIdReadResult = carrierID;

            var result = manualPortService.HasCstTypeMismatch(log, plcInfo);

            Assert.True(result);
        }

        [Test]
        public void 卡匣名稱第三四碼為LC代表為LITE_CASSETE___PLC讀到的是高的____Mismatch()
        {
            var manualPortService = new ManualPortEventService();
            var log = "";
            var plcInfo = new ManualPortPLCInfo();
            plcInfo.CstTypes = (int)CstType.Foup;
            var carrierID = "12LC0001";
            plcInfo.CarrierIdOfStage1 = carrierID;
            plcInfo.CarrierIdReadResult = carrierID;

            var result = manualPortService.HasCstTypeMismatch(log, plcInfo);

            Assert.True(result);
        }
    }
}