using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using System;

namespace TestCarrierTypeHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== 測試 CarrierTypeHelper 一致性檢查 ===\n");
            
            // 測試 IsSymbolSupported 方法
            TestIsSymbolSupported();
            
            // 測試與 CassetteData 的一致性
            TestConsistencyWithCassetteData();
            
            // 測試反向查詢功能
            TestReverseLookup();
            
            // 測試邊界條件
            TestEdgeCases();
            
            Console.WriteLine("\n=== 所有測試完成！===");
        }
        
        static void TestIsSymbolSupported()
        {
            Console.WriteLine("1. 測試 IsSymbolSupported 方法：");
            
            // 測試支援的符號
            string[] supportedSymbols = { "BE", "LC", "EC" };
            foreach (var symbol in supportedSymbols)
            {
                var result = CarrierTypeHelper.IsSymbolSupported(symbol);
                Console.WriteLine($"   Symbol '{symbol}': {result} (應該是 True)");
                if (!result) Console.WriteLine("   ❌ 錯誤：應該支援這個符號");
            }
            
            // 測試不支援的符號
            string[] unsupportedSymbols = { "XX", "YY", "ZZ", "12", "AB", null, "" };
            foreach (var symbol in unsupportedSymbols)
            {
                var result = CarrierTypeHelper.IsSymbolSupported(symbol);
                Console.WriteLine($"   Symbol '{symbol}': {result} (應該是 False)");
                if (result) Console.WriteLine("   ❌ 錯誤：不應該支援這個符號");
            }
        }
        
        static void TestConsistencyWithCassetteData()
        {
            Console.WriteLine("\n2. 測試與 CassetteData 的一致性：");
            
            var testCases = new[]
            {
                "01BE001",  // FOUP
                "02LC002",  // LITE_CASSETTE
                "03EC003",  // LITE_CASSETTE_EC (新增)
                "04XX004",  // Unknown
                "ABC",      // Invalid length
                null,       // null
                ""          // empty
            };
            
            foreach (var carrierId in testCases)
            {
                Console.WriteLine($"\n   測試 Carrier ID: '{carrierId}'");
                
                // 測試 CarrierTypeHelper 的結果
                bool helperIsFoup = CarrierTypeHelper.IsFoupCarrier(carrierId);
                bool helperIsLight = CarrierTypeHelper.IsLiteCassetteCarrier(carrierId);
                var helperCarrierType = CarrierTypeHelper.GetCarrierType(carrierId);
                var helperPLCType = CarrierTypeHelper.GetPLCCstType(carrierId);
                var helperPLCString = CarrierTypeHelper.GetPLCCstTypeString(carrierId);
                
                Console.WriteLine($"     CarrierTypeHelper - FOUP: {helperIsFoup}, Light: {helperIsLight}");
                Console.WriteLine($"     CarrierType: {helperCarrierType}, PLC: {helperPLCType} ({helperPLCString})");
                
                // 如果 carrierId 不為 null，測試 CassetteData 的結果
                if (!string.IsNullOrEmpty(carrierId))
                {
                    try
                    {
                        var cassetteData = new CassetteData { BOXID = carrierId };
                        bool cassetteIsFoup = cassetteData.IsFoupCST;
                        bool cassetteIsLight = cassetteData.IsLightCST;
                        var cassetteType = cassetteData.GetCstType();
                        
                        Console.WriteLine($"     CassetteData     - FOUP: {cassetteIsFoup}, Light: {cassetteIsLight}");
                        Console.WriteLine($"     CstType: {cassetteType}");
                        
                        // 檢查一致性
                        if (helperIsFoup != cassetteIsFoup || helperIsLight != cassetteIsLight)
                        {
                            Console.WriteLine("     ❌ 不一致！");
                        }
                        else
                        {
                            Console.WriteLine("     ✅ 一致");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"     ⚠️  CassetteData 測試失敗: {ex.Message}");
                    }
                }
            }
        }
        
        static void TestReverseLookup()
        {
            Console.WriteLine("\n3. 測試反向查詢功能：");
            
            var testCases = new[]
            {
                new { PLC = "A", ExpectedSymbol = "BE" },
                new { PLC = "B", ExpectedSymbol = "LC" },
                new { PLC = "C", ExpectedSymbol = "" },
                new { PLC = "", ExpectedSymbol = "" },
                new { PLC = (string)null, ExpectedSymbol = "" }
            };
            
            foreach (var test in testCases)
            {
                var result = CarrierTypeHelper.GetSymbolFromPLCString(test.PLC);
                Console.WriteLine($"   PLC '{test.PLC}' -> Symbol '{result}' (期望: '{test.ExpectedSymbol}')");
                
                if (result != test.ExpectedSymbol)
                {
                    Console.WriteLine("   ❌ 反向查詢結果不正確");
                }
                else
                {
                    Console.WriteLine("   ✅ 正確");
                }
            }
        }
        
        static void TestEdgeCases()
        {
            Console.WriteLine("\n4. 測試邊界條件：");
            
            var edgeCases = new[]
            {
                "BE",       // 只有2個字元
                "12BE",     // 正好4個字元，BE在正確位置
                "12LC3",    // 正好4個字元，LC在正確位置
                "12EC4",    // 正好4個字元，EC在正確位置
                "12be001",  // 小寫
                "  BE  ",   // 包含空白
                "XBEYZZ",   // BE不在正確位置
            };
            
            foreach (var testCase in edgeCases)
            {
                try
                {
                    var carrierType = CarrierTypeHelper.GetCarrierType(testCase);
                    var isFoup = CarrierTypeHelper.IsFoupCarrier(testCase);
                    var isLight = CarrierTypeHelper.IsLiteCassetteCarrier(testCase);
                    
                    Console.WriteLine($"   '{testCase}' -> Type: {carrierType}, FOUP: {isFoup}, Light: {isLight}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   '{testCase}' -> ❌ 例外: {ex.Message}");
                }
            }
        }
    }
}