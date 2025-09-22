using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using NUnit.Framework;
using System.Linq;

namespace UnitTestForMGVPort
{
    /// <summary>
    /// CarrierTypeHelper 單元測試
    /// </summary>
    [TestFixture]
    public class CarrierTypeHelperTests
    {
        [Test]
        [Category("CarrierTypeHelper")]
        public void GetCarrierType_FoupCarrier_ReturnsFoup()
        {
            // Arrange
            string carrierId = "01BE001";

            // Act
            var result = CarrierTypeHelper.GetCarrierType(carrierId);

            // Assert
            Assert.AreEqual(CarrierType.FOUP, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetCarrierType_LiteCassetteCarrier_ReturnsLiteCassette()
        {
            // Arrange
            string carrierId = "02LC002";

            // Act
            var result = CarrierTypeHelper.GetCarrierType(carrierId);

            // Assert
            Assert.AreEqual(CarrierType.LITE_CASSETTE, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetCarrierType_ECCarrier_ReturnsLiteCassette()
        {
            // Arrange
            string carrierId = "03EC003";

            // Act
            var result = CarrierTypeHelper.GetCarrierType(carrierId);

            // Assert
            Assert.AreEqual(CarrierType.LITE_CASSETTE, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetCarrierType_UnknownCarrier_ReturnsUnknown()
        {
            // Arrange
            string carrierId = "04XX004";

            // Act
            var result = CarrierTypeHelper.GetCarrierType(carrierId);

            // Assert
            Assert.AreEqual(CarrierType.Unknown, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetCarrierType_InvalidCarrierId_ReturnsUnknown()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(CarrierType.Unknown, CarrierTypeHelper.GetCarrierType(null));
            Assert.AreEqual(CarrierType.Unknown, CarrierTypeHelper.GetCarrierType(""));
            Assert.AreEqual(CarrierType.Unknown, CarrierTypeHelper.GetCarrierType("ABC"));
            Assert.AreEqual(CarrierType.Unknown, CarrierTypeHelper.GetCarrierType("12"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void IsFoupCarrier_FoupId_ReturnsTrue()
        {
            // Arrange
            string carrierId = "01BE001";

            // Act
            var result = CarrierTypeHelper.IsFoupCarrier(carrierId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void IsFoupCarrier_NonFoupId_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.IsFalse(CarrierTypeHelper.IsFoupCarrier("02LC002"));
            Assert.IsFalse(CarrierTypeHelper.IsFoupCarrier("03EC003"));
            Assert.IsFalse(CarrierTypeHelper.IsFoupCarrier("04XX004"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void IsLiteCassetteCarrier_LiteCassetteId_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(CarrierTypeHelper.IsLiteCassetteCarrier("02LC002"));
            Assert.IsTrue(CarrierTypeHelper.IsLiteCassetteCarrier("03EC003")); // EC 也算 LITE_CASSETTE
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void IsLiteCassetteCarrier_NonLiteCassetteId_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.IsFalse(CarrierTypeHelper.IsLiteCassetteCarrier("01BE001"));
            Assert.IsFalse(CarrierTypeHelper.IsLiteCassetteCarrier("04XX004"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetPLCCstType_FoupCarrier_ReturnsA()
        {
            // Arrange
            string carrierId = "01BE001";

            // Act
            var result = CarrierTypeHelper.GetPLCCstType(carrierId);

            // Assert
            Assert.AreEqual(CstType.A, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetPLCCstType_LiteCassetteCarrier_ReturnsB()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(CstType.B, CarrierTypeHelper.GetPLCCstType("02LC002"));
            Assert.AreEqual(CstType.B, CarrierTypeHelper.GetPLCCstType("03EC003"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetPLCCstType_UnknownCarrier_ReturnsUndefined()
        {
            // Arrange
            string carrierId = "04XX004";

            // Act
            var result = CarrierTypeHelper.GetPLCCstType(carrierId);

            // Assert
            Assert.AreEqual(CstType.Undefined, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetPLCCstTypeString_FoupCarrier_ReturnsA()
        {
            // Arrange
            string carrierId = "01BE001";

            // Act
            var result = CarrierTypeHelper.GetPLCCstTypeString(carrierId);

            // Assert
            Assert.AreEqual("A", result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetPLCCstTypeString_LiteCassetteCarrier_ReturnsB()
        {
            // Arrange & Act & Assert
            Assert.AreEqual("B", CarrierTypeHelper.GetPLCCstTypeString("02LC002"));
            Assert.AreEqual("B", CarrierTypeHelper.GetPLCCstTypeString("03EC003"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetPLCCstTypeString_UnknownCarrier_ReturnsEmpty()
        {
            // Arrange
            string carrierId = "04XX004";

            // Act
            var result = CarrierTypeHelper.GetPLCCstTypeString(carrierId);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetPLCCstType_ByCarrierType_ReturnsCorrectValues()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(CstType.A, CarrierTypeHelper.GetPLCCstType(CarrierType.FOUP));
            Assert.AreEqual(CstType.B, CarrierTypeHelper.GetPLCCstType(CarrierType.LITE_CASSETTE));
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetPLCCstType(CarrierType.Unknown));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetPLCCstTypeString_ByCarrierType_ReturnsCorrectValues()
        {
            // Arrange & Act & Assert
            Assert.AreEqual("A", CarrierTypeHelper.GetPLCCstTypeString(CarrierType.FOUP));
            Assert.AreEqual("B", CarrierTypeHelper.GetPLCCstTypeString(CarrierType.LITE_CASSETTE));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCCstTypeString(CarrierType.Unknown));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void IsSymbolSupported_SupportedSymbols_ReturnsTrue()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(CarrierTypeHelper.IsSymbolSupported("BE"));
            Assert.IsTrue(CarrierTypeHelper.IsSymbolSupported("LC"));
            Assert.IsTrue(CarrierTypeHelper.IsSymbolSupported("EC"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void IsSymbolSupported_UnsupportedSymbol_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.IsFalse(CarrierTypeHelper.IsSymbolSupported("XX"));
            Assert.IsFalse(CarrierTypeHelper.IsSymbolSupported("YY"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetSupportedSymbols_ReturnsExpectedSymbols()
        {
            // Act
            var symbols = CarrierTypeHelper.GetSupportedSymbols();

            // Assert
            Assert.IsNotNull(symbols);
            CollectionAssert.Contains(symbols.ToList(), "BE");
            CollectionAssert.Contains(symbols.ToList(), "LC");
            CollectionAssert.Contains(symbols.ToList(), "EC");
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetSupportedCarrierTypes_ReturnsExpectedTypes()
        {
            // Act
            var types = CarrierTypeHelper.GetSupportedCarrierTypes();

            // Assert
            Assert.IsNotNull(types);
            CollectionAssert.Contains(types.ToList(), CarrierType.FOUP);
            CollectionAssert.Contains(types.ToList(), CarrierType.LITE_CASSETTE);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void Constants_HaveCorrectValues()
        {
            // Assert
            Assert.AreEqual("BE", CarrierTypeHelper.FOUP_SYMBOL);
            Assert.AreEqual("LC", CarrierTypeHelper.LITE_CASSETTE_SYMBOL);
            Assert.AreEqual("EC", CarrierTypeHelper.LITE_CASSETTE_EC_SYMBOL);
            Assert.AreEqual("A", CarrierTypeHelper.CST_TYPE_FOR_PLC_FOUP);
            Assert.AreEqual("B", CarrierTypeHelper.CST_TYPE_FOR_PLC_LITE_CASSETTE);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetCarrierTypeString_ByCarrierType_ReturnsCorrectValues()
        {
            // Arrange & Act & Assert
            Assert.AreEqual("FOUP", CarrierTypeHelper.GetCarrierTypeString(CarrierType.FOUP));
            Assert.AreEqual("LITE_CASSETTE", CarrierTypeHelper.GetCarrierTypeString(CarrierType.LITE_CASSETTE));
            Assert.AreEqual("Unknown", CarrierTypeHelper.GetCarrierTypeString(CarrierType.Unknown));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetCarrierTypeString_ByCarrierId_ReturnsCorrectValues()
        {
            // Arrange & Act & Assert
            Assert.AreEqual("FOUP", CarrierTypeHelper.GetCarrierTypeString("01BE001"));
            Assert.AreEqual("LITE_CASSETTE", CarrierTypeHelper.GetCarrierTypeString("02LC002"));
            Assert.AreEqual("LITE_CASSETTE", CarrierTypeHelper.GetCarrierTypeString("03EC003"));
            Assert.AreEqual("Unknown", CarrierTypeHelper.GetCarrierTypeString("04XX004"));
            Assert.AreEqual("Unknown", CarrierTypeHelper.GetCarrierTypeString("ABC"));
            Assert.AreEqual("Unknown", CarrierTypeHelper.GetCarrierTypeString(null));
            Assert.AreEqual("Unknown", CarrierTypeHelper.GetCarrierTypeString(""));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetSymbolFromPLCString_ValidPLCStrings_ReturnsCorrectSymbols()
        {
            // Arrange & Act & Assert
            Assert.AreEqual("BE", CarrierTypeHelper.GetSymbolFromPLCString("A"));
            Assert.AreEqual("LC", CarrierTypeHelper.GetSymbolFromPLCString("B"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        public void GetSymbolFromPLCString_InvalidPLCStrings_ReturnsEmpty()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetSymbolFromPLCString("C"));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetSymbolFromPLCString("X"));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetSymbolFromPLCString(""));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetSymbolFromPLCString(null));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetSymbolFromPLCString("  "));
        }

        #region Symbol-based Methods Tests

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetCstTypeFromSymbol 方法：BE symbol 轉換為 CstType.A")]
        public void GetCstTypeFromSymbol_BeSymbol_ReturnsA()
        {
            // Arrange
            string symbol = "BE";

            // Act
            var result = CarrierTypeHelper.GetCstTypeFromSymbol(symbol);

            // Assert
            Assert.AreEqual(CstType.A, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetCstTypeFromSymbol 方法：LC symbol 轉換為 CstType.B")]
        public void GetCstTypeFromSymbol_LcSymbol_ReturnsB()
        {
            // Arrange
            string symbol = "LC";

            // Act
            var result = CarrierTypeHelper.GetCstTypeFromSymbol(symbol);

            // Assert
            Assert.AreEqual(CstType.B, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetCstTypeFromSymbol 方法：EC symbol 轉換為 CstType.B")]
        public void GetCstTypeFromSymbol_EcSymbol_ReturnsB()
        {
            // Arrange
            string symbol = "EC";

            // Act
            var result = CarrierTypeHelper.GetCstTypeFromSymbol(symbol);

            // Assert
            Assert.AreEqual(CstType.B, result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetCstTypeFromSymbol 方法：小寫輸入應該正常轉換")]
        public void GetCstTypeFromSymbol_LowerCaseSymbols_ReturnsCorrectTypes()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(CstType.A, CarrierTypeHelper.GetCstTypeFromSymbol("be"));
            Assert.AreEqual(CstType.B, CarrierTypeHelper.GetCstTypeFromSymbol("lc"));
            Assert.AreEqual(CstType.B, CarrierTypeHelper.GetCstTypeFromSymbol("ec"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetCstTypeFromSymbol 方法：未知 symbol 回傳 CstType.Undefined")]
        public void GetCstTypeFromSymbol_UnknownSymbol_ReturnsUndefined()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetCstTypeFromSymbol("XX"));
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetCstTypeFromSymbol("YZ"));
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetCstTypeFromSymbol("UNKNOWN"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetCstTypeFromSymbol 方法：無效輸入回傳 CstType.Undefined")]
        public void GetCstTypeFromSymbol_InvalidInput_ReturnsUndefined()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetCstTypeFromSymbol(null));
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetCstTypeFromSymbol(""));
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetCstTypeFromSymbol("  "));
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetCstTypeFromSymbol("\t"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetPLCStringFromSymbol 方法：BE symbol 轉換為 'A'")]
        public void GetPLCStringFromSymbol_BeSymbol_ReturnsA()
        {
            // Arrange
            string symbol = "BE";

            // Act
            var result = CarrierTypeHelper.GetPLCStringFromSymbol(symbol);

            // Assert
            Assert.AreEqual("A", result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetPLCStringFromSymbol 方法：LC symbol 轉換為 'B'")]
        public void GetPLCStringFromSymbol_LcSymbol_ReturnsB()
        {
            // Arrange
            string symbol = "LC";

            // Act
            var result = CarrierTypeHelper.GetPLCStringFromSymbol(symbol);

            // Assert
            Assert.AreEqual("B", result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetPLCStringFromSymbol 方法：EC symbol 轉換為 'B'")]
        public void GetPLCStringFromSymbol_EcSymbol_ReturnsB()
        {
            // Arrange
            string symbol = "EC";

            // Act
            var result = CarrierTypeHelper.GetPLCStringFromSymbol(symbol);

            // Assert
            Assert.AreEqual("B", result);
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetPLCStringFromSymbol 方法：小寫輸入應該正常轉換")]
        public void GetPLCStringFromSymbol_LowerCaseSymbols_ReturnsCorrectStrings()
        {
            // Arrange & Act & Assert
            Assert.AreEqual("A", CarrierTypeHelper.GetPLCStringFromSymbol("be"));
            Assert.AreEqual("B", CarrierTypeHelper.GetPLCStringFromSymbol("lc"));
            Assert.AreEqual("B", CarrierTypeHelper.GetPLCStringFromSymbol("ec"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetPLCStringFromSymbol 方法：未知 symbol 回傳空字串")]
        public void GetPLCStringFromSymbol_UnknownSymbol_ReturnsEmpty()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCStringFromSymbol("XX"));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCStringFromSymbol("YZ"));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCStringFromSymbol("UNKNOWN"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 GetPLCStringFromSymbol 方法：無效輸入回傳空字串")]
        public void GetPLCStringFromSymbol_InvalidInput_ReturnsEmpty()
        {
            // Arrange & Act & Assert
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCStringFromSymbol(null));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCStringFromSymbol(""));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCStringFromSymbol("  "));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCStringFromSymbol("\t"));
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 Symbol 方法之間的一致性：GetCstTypeFromSymbol 和 GetPLCStringFromSymbol 應該回傳一致的結果")]
        public void SymbolMethods_Consistency_ShouldReturnConsistentResults()
        {
            // Arrange
            string[] testSymbols = { "BE", "LC", "EC", "be", "lc", "ec" };

            foreach (string symbol in testSymbols)
            {
                // Act
                var cstType = CarrierTypeHelper.GetCstTypeFromSymbol(symbol);
                var plcString = CarrierTypeHelper.GetPLCStringFromSymbol(symbol);

                // Assert: 檢查一致性
                if (cstType == CstType.A)
                {
                    Assert.AreEqual("A", plcString, $"Symbol '{symbol}' should return 'A' for both methods");
                }
                else if (cstType == CstType.B)
                {
                    Assert.AreEqual("B", plcString, $"Symbol '{symbol}' should return 'B' for both methods");
                }
                else if (cstType == CstType.Undefined)
                {
                    Assert.AreEqual(string.Empty, plcString, $"Symbol '{symbol}' should return empty string when CstType is Undefined");
                }
            }
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試 Symbol 方法與 CarrierId 方法的一致性")]
        public void SymbolMethods_ConsistencyWithCarrierId_ShouldReturnSameResults()
        {
            // Arrange: 建立測試資料 (CarrierId, 對應的 Symbol)
            var testData = new[]
            {
                new { CarrierId = "01BE001", Symbol = "BE" },
                new { CarrierId = "02LC002", Symbol = "LC" },
                new { CarrierId = "03EC003", Symbol = "EC" }
            };

            foreach (var data in testData)
            {
                // Act: 分別使用 CarrierId 和 Symbol 取得結果
                var cstTypeFromCarrierId = CarrierTypeHelper.GetPLCCstType(data.CarrierId);
                var cstTypeFromSymbol = CarrierTypeHelper.GetCstTypeFromSymbol(data.Symbol);
                
                var plcStringFromCarrierId = CarrierTypeHelper.GetPLCCstTypeString(data.CarrierId);
                var plcStringFromSymbol = CarrierTypeHelper.GetPLCStringFromSymbol(data.Symbol);

                // Assert: 兩種方法應該回傳相同結果
                Assert.AreEqual(cstTypeFromCarrierId, cstTypeFromSymbol, 
                    $"CstType should be consistent for CarrierId '{data.CarrierId}' and Symbol '{data.Symbol}'");
                Assert.AreEqual(plcStringFromCarrierId, plcStringFromSymbol,
                    $"PLC String should be consistent for CarrierId '{data.CarrierId}' and Symbol '{data.Symbol}'");
            }
        }

        [Test]
        [Category("CarrierTypeHelper")]
        [Description("測試邊界條件：混合大小寫和空白字元")]
        public void SymbolMethods_EdgeCases_ShouldHandleCorrectly()
        {
            // 測試混合大小寫
            Assert.AreEqual(CstType.A, CarrierTypeHelper.GetCstTypeFromSymbol("Be"));
            Assert.AreEqual(CstType.A, CarrierTypeHelper.GetCstTypeFromSymbol("bE"));
            Assert.AreEqual("A", CarrierTypeHelper.GetPLCStringFromSymbol("Be"));
            Assert.AreEqual("A", CarrierTypeHelper.GetPLCStringFromSymbol("bE"));

            // 測試前後空白（應該回傳 Undefined/Empty，因為 "  BE  " != "BE"）
            Assert.AreEqual(CstType.Undefined, CarrierTypeHelper.GetCstTypeFromSymbol("  BE  "));
            Assert.AreEqual(string.Empty, CarrierTypeHelper.GetPLCStringFromSymbol("  BE  "));
        }

        #endregion

    }
}