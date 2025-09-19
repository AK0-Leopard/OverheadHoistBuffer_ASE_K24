using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using NUnit.Framework;
using System.Linq;

namespace UnitTestForMGVPort
{
    /// <summary>
    /// 擴充 Carrier Type 功能測試
    /// 展示如何測試新增的 Carrier 類型
    /// </summary>
    [TestFixture]
    public class ExtendedCarrierTypeTests
    {
        [Test]
        [Category("ExtendedCarrierType")]
        public void EC_Symbol_應該對應到_LITE_CASSETTE_類型()
        {
            // Arrange
            string carrierId = "03EC003";

            // Act
            var carrierType = CarrierTypeHelper.GetCarrierType(carrierId);

            // Assert
            Assert.AreEqual(CarrierType.LITE_CASSETTE, carrierType);
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void EC_Symbol_應該對應到_PLC_CstType_B()
        {
            // Arrange
            string carrierId = "03EC003";

            // Act
            var plcCstType = CarrierTypeHelper.GetPLCCstType(carrierId);
            var plcString = CarrierTypeHelper.GetPLCCstTypeString(carrierId);

            // Assert
            Assert.AreEqual(CstType.B, plcCstType);
            Assert.AreEqual("B", plcString);
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void EC_Symbol_IsLiteCassetteCarrier_應該返回_True()
        {
            // Arrange
            string carrierId = "03EC003";

            // Act
            var isLiteCassette = CarrierTypeHelper.IsLiteCassetteCarrier(carrierId);

            // Assert
            Assert.IsTrue(isLiteCassette);
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void EC_Symbol_IsFoupCarrier_應該返回_False()
        {
            // Arrange
            string carrierId = "03EC003";

            // Act
            var isFoup = CarrierTypeHelper.IsFoupCarrier(carrierId);

            // Assert
            Assert.IsFalse(isFoup);
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void EC_Symbol_應該被列在支援的_Symbol_清單中()
        {
            // Act
            var supportedSymbols = CarrierTypeHelper.GetSupportedSymbols().ToList();

            // Assert
            CollectionAssert.Contains(supportedSymbols, "EC");
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void EC_Symbol_IsSymbolSupported_應該返回_True()
        {
            // Act
            var isSupported = CarrierTypeHelper.IsSymbolSupported("EC");

            // Assert
            Assert.IsTrue(isSupported);
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void 測試多個_EC_類型的_Carrier_ID()
        {
            // Arrange
            var ecCarrierIds = new[]
            {
                "01EC001",
                "02EC002", 
                "03EC003",
                "99EC999"
            };

            foreach (var carrierId in ecCarrierIds)
            {
                // Act
                var carrierType = CarrierTypeHelper.GetCarrierType(carrierId);
                var plcCstType = CarrierTypeHelper.GetPLCCstType(carrierId);
                var plcString = CarrierTypeHelper.GetPLCCstTypeString(carrierId);
                var isLiteCassette = CarrierTypeHelper.IsLiteCassetteCarrier(carrierId);

                // Assert
                Assert.AreEqual(CarrierType.LITE_CASSETTE, carrierType, 
                    $"Carrier ID: {carrierId} - CarrierType 不正確");
                Assert.AreEqual(CstType.B, plcCstType, 
                    $"Carrier ID: {carrierId} - PLC CstType 不正確");
                Assert.AreEqual("B", plcString, 
                    $"Carrier ID: {carrierId} - PLC String 不正確");
                Assert.IsTrue(isLiteCassette, 
                    $"Carrier ID: {carrierId} - IsLiteCassetteCarrier 應該返回 True");
            }
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void 驗證_LC_和_EC_Symbol_都對應到相同的_LITE_CASSETTE_類型()
        {
            // Arrange
            string lcCarrierId = "02LC002";
            string ecCarrierId = "03EC003";

            // Act
            var lcCarrierType = CarrierTypeHelper.GetCarrierType(lcCarrierId);
            var ecCarrierType = CarrierTypeHelper.GetCarrierType(ecCarrierId);
            
            var lcPlcType = CarrierTypeHelper.GetPLCCstType(lcCarrierId);
            var ecPlcType = CarrierTypeHelper.GetPLCCstType(ecCarrierId);
            
            var lcPlcString = CarrierTypeHelper.GetPLCCstTypeString(lcCarrierId);
            var ecPlcString = CarrierTypeHelper.GetPLCCstTypeString(ecCarrierId);

            // Assert
            Assert.AreEqual(lcCarrierType, ecCarrierType, "LC 和 EC 應該對應到相同的 CarrierType");
            Assert.AreEqual(lcPlcType, ecPlcType, "LC 和 EC 應該對應到相同的 PLC CstType");
            Assert.AreEqual(lcPlcString, ecPlcString, "LC 和 EC 應該對應到相同的 PLC String");
            
            Assert.AreEqual(CarrierType.LITE_CASSETTE, lcCarrierType);
            Assert.AreEqual(CarrierType.LITE_CASSETTE, ecCarrierType);
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void 新的_Symbol_不應該影響現有的_BE_和_LC_邏輯()
        {
            // Arrange
            var testCases = new[]
            {
                new { CarrierId = "01BE001", ExpectedType = CarrierType.FOUP, ExpectedPLC = "A" },
                new { CarrierId = "02LC002", ExpectedType = CarrierType.LITE_CASSETTE, ExpectedPLC = "B" },
                new { CarrierId = "03EC003", ExpectedType = CarrierType.LITE_CASSETTE, ExpectedPLC = "B" }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var actualType = CarrierTypeHelper.GetCarrierType(testCase.CarrierId);
                var actualPLC = CarrierTypeHelper.GetPLCCstTypeString(testCase.CarrierId);

                // Assert
                Assert.AreEqual(testCase.ExpectedType, actualType,
                    $"Carrier ID: {testCase.CarrierId} - Type 不符合預期");
                Assert.AreEqual(testCase.ExpectedPLC, actualPLC,
                    $"Carrier ID: {testCase.CarrierId} - PLC String 不符合預期");
            }
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void 驗證_EC_常數值()
        {
            // Assert
            Assert.AreEqual("EC", CarrierTypeHelper.LITE_CASSETTE_EC_SYMBOL);
        }

        [Test]
        [Category("ExtendedCarrierType")]
        public void 支援的_CarrierType_清單應該包含_FOUP_和_LITE_CASSETTE()
        {
            // Act
            var supportedTypes = CarrierTypeHelper.GetSupportedCarrierTypes().ToList();

            // Assert
            Assert.IsTrue(supportedTypes.Count >= 2, "應該至少支援 2 種 CarrierType");
            CollectionAssert.Contains(supportedTypes, CarrierType.FOUP);
            CollectionAssert.Contains(supportedTypes, CarrierType.LITE_CASSETTE);
            
            // EC 應該對應到 LITE_CASSETTE，所以不會是新的類型
            Assert.IsFalse(supportedTypes.Contains(CarrierType.Unknown), 
                "支援的類型清單不應該包含 Unknown");
        }
    }
}