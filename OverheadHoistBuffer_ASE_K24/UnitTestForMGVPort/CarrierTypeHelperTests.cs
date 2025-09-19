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
    }
}