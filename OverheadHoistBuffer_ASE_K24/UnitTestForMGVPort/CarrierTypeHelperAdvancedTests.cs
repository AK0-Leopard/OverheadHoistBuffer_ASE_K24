using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTestForMGVPort
{
    /// <summary>
    /// CarrierTypeHelper 進階功能測試
    /// 測試擴充機制和邊界條件
    /// </summary>
    [TestFixture]
    public class CarrierTypeHelperAdvancedTests
    {
        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_與現有CassetteData邏輯一致性測試()
        {
            // 測試與現有 CassetteData 中的邏輯是否一致
            var testCases = new[]
            {
                new { CarrierId = "01BE001", ExpectedIsFoup = true, ExpectedIsLight = false },
                new { CarrierId = "02LC002", ExpectedIsFoup = false, ExpectedIsLight = true },
                new { CarrierId = "03EC003", ExpectedIsFoup = false, ExpectedIsLight = true }, // EC 算 LITE_CASSETTE
                new { CarrierId = "04XX004", ExpectedIsFoup = false, ExpectedIsLight = false },
                new { CarrierId = "ABC", ExpectedIsFoup = false, ExpectedIsLight = false },
            };

            foreach (var testCase in testCases)
            {
                // Act
                bool actualIsFoup = CarrierTypeHelper.IsFoupCarrier(testCase.CarrierId);
                bool actualIsLight = CarrierTypeHelper.IsLiteCassetteCarrier(testCase.CarrierId);

                // Assert
                Assert.AreEqual(testCase.ExpectedIsFoup, actualIsFoup, 
                    $"Carrier ID: {testCase.CarrierId} - FOUP 判斷不一致");
                Assert.AreEqual(testCase.ExpectedIsLight, actualIsLight, 
                    $"Carrier ID: {testCase.CarrierId} - LITE_CASSETTE 判斷不一致");
            }
        }

        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_PLC對應關係測試()
        {
            var testCases = new[]
            {
                new { CarrierId = "01BE001", ExpectedPLCType = CstType.A, ExpectedPLCString = "A" },
                new { CarrierId = "02LC002", ExpectedPLCType = CstType.B, ExpectedPLCString = "B" },
                new { CarrierId = "03EC003", ExpectedPLCType = CstType.B, ExpectedPLCString = "B" },
                new { CarrierId = "04XX004", ExpectedPLCType = CstType.Undefined, ExpectedPLCString = "" },
            };

            foreach (var testCase in testCases)
            {
                // Act
                var actualPLCType = CarrierTypeHelper.GetPLCCstType(testCase.CarrierId);
                var actualPLCString = CarrierTypeHelper.GetPLCCstTypeString(testCase.CarrierId);

                // Assert
                Assert.AreEqual(testCase.ExpectedPLCType, actualPLCType,
                    $"Carrier ID: {testCase.CarrierId} - PLC CstType 不一致");
                Assert.AreEqual(testCase.ExpectedPLCString, actualPLCString,
                    $"Carrier ID: {testCase.CarrierId} - PLC String 不一致");
            }
        }

        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_邊界條件測試()
        {
            var edgeCases = new[]
            {
                null,
                "",
                "A",
                "AB",
                "ABC",
                "12BE",    // 正好4碼，BE在正確位置
                "12LC3",   // 正好4碼，LC在正確位置  
                "12345678", // 長ID，但第3、4碼不是已知類型
                "  BE  ",   // 含空白
                "12be001",  // 小寫
            };

            foreach (var edgeCase in edgeCases)
            {
                // Act - 應該不會拋出例外
                var carrierType = CarrierTypeHelper.GetCarrierType(edgeCase);
                var isFoup = CarrierTypeHelper.IsFoupCarrier(edgeCase);
                var isLight = CarrierTypeHelper.IsLiteCassetteCarrier(edgeCase);
                var plcType = CarrierTypeHelper.GetPLCCstType(edgeCase);
                var plcString = CarrierTypeHelper.GetPLCCstTypeString(edgeCase);

                // Assert - 對於邊界情況，應該返回合理的預設值
                if (string.IsNullOrWhiteSpace(edgeCase) || edgeCase.Length < 4)
                {
                    Assert.AreEqual(CarrierType.Unknown, carrierType, $"邊界情況: '{edgeCase}'");
                    Assert.IsFalse(isFoup, $"邊界情況: '{edgeCase}'");
                    Assert.IsFalse(isLight, $"邊界情況: '{edgeCase}'");
                    Assert.AreEqual(CstType.Undefined, plcType, $"邊界情況: '{edgeCase}'");
                    Assert.AreEqual(string.Empty, plcString, $"邊界情況: '{edgeCase}'");
                }
            }
        }

        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_大小寫敏感性測試()
        {
            // 測試大小寫敏感性 - 目前設計為大小寫敏感
            var testCases = new[]
            {
                new { CarrierId = "01be001", ShouldMatch = false }, // 小寫 be
                new { CarrierId = "01BE001", ShouldMatch = true },  // 大寫 BE
                new { CarrierId = "02lc002", ShouldMatch = false }, // 小寫 lc
                new { CarrierId = "02LC002", ShouldMatch = true },  // 大寫 LC
            };

            foreach (var testCase in testCases)
            {
                // Act
                var carrierType = CarrierTypeHelper.GetCarrierType(testCase.CarrierId);

                // Assert
                if (testCase.ShouldMatch)
                {
                    Assert.AreNotEqual(CarrierType.Unknown, carrierType, 
                        $"應該匹配但沒有匹配: {testCase.CarrierId}");
                }
                else
                {
                    Assert.AreEqual(CarrierType.Unknown, carrierType, 
                        $"不應該匹配但匹配了: {testCase.CarrierId}");
                }
            }
        }

        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_符號支援檢查測試()
        {
            // 測試所有已知符號
            var supportedSymbols = new[] { "BE", "LC", "EC" };
            var unsupportedSymbols = new[] { "XX", "YY", "ZZ", "12", "AB", null, "" };

            foreach (var symbol in supportedSymbols)
            {
                Assert.IsTrue(CarrierTypeHelper.IsSymbolSupported(symbol),
                    $"符號 '{symbol}' 應該被支援");
            }

            foreach (var symbol in unsupportedSymbols)
            {
                Assert.IsFalse(CarrierTypeHelper.IsSymbolSupported(symbol),
                    $"符號 '{symbol}' 不應該被支援");
            }
        }

        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_取得支援類型清單測試()
        {
            // Act
            var supportedSymbols = CarrierTypeHelper.GetSupportedSymbols().ToList();
            var supportedCarrierTypes = CarrierTypeHelper.GetSupportedCarrierTypes().ToList();

            // Assert
            Assert.IsTrue(supportedSymbols.Count >= 3, "應該支援至少3個符號");
            Assert.IsTrue(supportedCarrierTypes.Count >= 2, "應該支援至少2個 CarrierType");

            // 檢查必要的符號和類型是否存在
            CollectionAssert.Contains(supportedSymbols, "BE");
            CollectionAssert.Contains(supportedSymbols, "LC");
            CollectionAssert.Contains(supportedSymbols, "EC");

            CollectionAssert.Contains(supportedCarrierTypes, CarrierType.FOUP);
            CollectionAssert.Contains(supportedCarrierTypes, CarrierType.LITE_CASSETTE);
        }

        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_常數值正確性測試()
        {
            // 確保常數值與現有系統一致
            Assert.AreEqual("BE", CarrierTypeHelper.FOUP_SYMBOL);
            Assert.AreEqual("LC", CarrierTypeHelper.LITE_CASSETTE_SYMBOL);
            Assert.AreEqual("EC", CarrierTypeHelper.LITE_CASSETTE_EC_SYMBOL);
            Assert.AreEqual("A", CarrierTypeHelper.CST_TYPE_FOR_PLC_FOUP);
            Assert.AreEqual("B", CarrierTypeHelper.CST_TYPE_FOR_PLC_LITE_CASSETTE);

            // 確保常數值與現有 CassetteData 中的常數一致
            Assert.AreEqual(CassetteData.SYMBLE_FOUP, CarrierTypeHelper.FOUP_SYMBOL);
            Assert.AreEqual(CassetteData.SYMBLE_LITE_CASSETTE, CarrierTypeHelper.LITE_CASSETTE_SYMBOL);
        }

        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_性能測試()
        {
            // 簡單的性能測試 - 確保大量調用不會有問題
            var carrierId = "01BE001";
            var iterations = 10000;

            var startTime = DateTime.Now;

            for (int i = 0; i < iterations; i++)
            {
                var carrierType = CarrierTypeHelper.GetCarrierType(carrierId);
                var isFoup = CarrierTypeHelper.IsFoupCarrier(carrierId);
                var plcType = CarrierTypeHelper.GetPLCCstType(carrierId);
                var plcString = CarrierTypeHelper.GetPLCCstTypeString(carrierId);
            }

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            // Assert - 應該在合理時間內完成
            Assert.IsTrue(duration.TotalMilliseconds < 1000, 
                $"性能測試失敗：{iterations} 次調用花費了 {duration.TotalMilliseconds} 毫秒");
        }

        [Test]
        [Category("CarrierTypeHelper.Advanced")]
        public void CarrierTypeHelper_多線程安全性測試()
        {
            // 簡單的多線程安全性測試
            var carrierId = "01BE001";
            var tasks = new Task[10];
            var results = new CarrierType[10];

            for (int i = 0; i < tasks.Length; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() =>
                {
                    results[index] = CarrierTypeHelper.GetCarrierType(carrierId);
                });
            }

            Task.WaitAll(tasks);

            // Assert - 所有結果應該一致
            for (int i = 0; i < results.Length; i++)
            {
                Assert.AreEqual(CarrierType.FOUP, results[i], 
                    $"多線程測試失敗：第 {i} 個結果不一致");
            }
        }
    }
}