using com.mirle.ibg3k0.sc.Common;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using System;

namespace com.mirle.ibg3k0.sc.Common
{
    /// <summary>
    /// CarrierTypeHelper 使用範例
    /// 展示如何使用 CarrierTypeHelper 以及如何擴充新的 Carrier 類型
    /// </summary>
    public class CarrierTypeUsageExample
    {
        /// <summary>
        /// 基本使用範例
        /// </summary>
        public static void BasicUsageExample()
        {
            Console.WriteLine("=== CarrierTypeHelper 基本使用範例 ===");
            
            // 測試不同的 Carrier ID
            string[] testCarrierIds = {
                "01BE001",  // FOUP 類型
                "02LC002",  // LITE_CASSETTE 類型  
                "03EC003",  // LITE_CASSETTE_EC 類型 (新增的擴充)
                "04XX004",  // 未知類型
                "ABC"       // 無效格式
            };

            foreach (string carrierId in testCarrierIds)
            {
                Console.WriteLine($"\n測試 Carrier ID: {carrierId}");
                
                // 1. 判斷 Carrier 類型
                var carrierType = CarrierTypeHelper.GetCarrierType(carrierId);
                Console.WriteLine($"  Carrier 類型: {carrierType}");
                
                // 2. 檢查是否為特定類型
                Console.WriteLine($"  是否為 FOUP: {CarrierTypeHelper.IsFoupCarrier(carrierId)}");
                Console.WriteLine($"  是否為 LITE_CASSETTE: {CarrierTypeHelper.IsLiteCassetteCarrier(carrierId)}");
                
                // 3. 取得對應的 PLC CstType
                var plcCstType = CarrierTypeHelper.GetPLCCstType(carrierId);
                Console.WriteLine($"  PLC CstType 枚舉: {plcCstType}");
                
                // 4. 取得對應的 PLC CstType 字串
                var plcCstTypeString = CarrierTypeHelper.GetPLCCstTypeString(carrierId);
                Console.WriteLine($"  PLC CstType 字串: '{plcCstTypeString}'");
                
                // 5. 取得對應的 CarrierType 字串
                var carrierTypeString = CarrierTypeHelper.GetCarrierTypeString(carrierId);
                Console.WriteLine($"  CarrierType 字串: '{carrierTypeString}'");
            }
            
            // 6. 列出所有支援的類型
            Console.WriteLine("\n=== 支援的類型資訊 ===");
            Console.WriteLine("支援的 Symbols:");
            foreach (var symbol in CarrierTypeHelper.GetSupportedSymbols())
            {
                Console.WriteLine($"  {symbol}");
            }
            
            Console.WriteLine("支援的 CarrierTypes:");
            foreach (var type in CarrierTypeHelper.GetSupportedCarrierTypes())
            {
                Console.WriteLine($"  {type}");
            }
        }

        /// <summary>
        /// 實際應用範例 - 替代現有的 CassetteData 方法
        /// </summary>
        public static void ReplaceExistingLogicExample()
        {
            Console.WriteLine("\n=== 替代現有邏輯範例 ===");
            
            string carrierId = "01BE001";
            
            // 舊的做法 (在 CassetteData 中)
            bool isFoupOld = IsFoupCST_OldWay(carrierId);
            bool isLightOld = IsLightCST_OldWay(carrierId);
            
            // 新的做法 (使用 CarrierTypeHelper)
            bool isFoupNew = CarrierTypeHelper.IsFoupCarrier(carrierId);
            bool isLightNew = CarrierTypeHelper.IsLiteCassetteCarrier(carrierId);
            
            Console.WriteLine($"Carrier ID: {carrierId}");
            Console.WriteLine($"舊方法 - 是否為 FOUP: {isFoupOld}, 是否為 Light: {isLightOld}");
            Console.WriteLine($"新方法 - 是否為 FOUP: {isFoupNew}, 是否為 Light: {isLightNew}");
            Console.WriteLine($"結果一致: {isFoupOld == isFoupNew && isLightOld == isLightNew}");
        }

        // 模擬舊的判斷方法 (參考 CassetteData.cs)
        private static bool IsFoupCST_OldWay(string boxId)
        {
            if (boxId.Length < 4) return false;
            var sub_crrierID = boxId.Substring(2, 2);
            return sub_crrierID == "BE"; // 簡化版的 SCUtility.isMatche
        }

        private static bool IsLightCST_OldWay(string boxId)
        {
            if (boxId.Length < 4) return false;
            var sub_crrierID = boxId.Substring(2, 2);
            return sub_crrierID == "LC"; // 簡化版的 SCUtility.isMatche
        }

        /// <summary>
        /// 展示如何擴充新的 Carrier 類型
        /// </summary>
        public static void ExtensionExample()
        {
            Console.WriteLine("\n=== 擴充新類型範例 ===");
            
            // 假設我們要新增一個新的類型 "EC" (LITE_CASSETTE_EC)
            // 它在邏輯上和 LITE_CASSETTE 相同，但有不同的識別碼
            
            Console.WriteLine("1. 直接使用已預定義的 EC 類型:");
            string ecCarrierId = "03EC003";
            
            var carrierType = CarrierTypeHelper.GetCarrierType(ecCarrierId);
            var plcType = CarrierTypeHelper.GetPLCCstType(ecCarrierId);
            var plcString = CarrierTypeHelper.GetPLCCstTypeString(ecCarrierId);
            
            Console.WriteLine($"   Carrier ID: {ecCarrierId}");
            Console.WriteLine($"   Carrier Type: {carrierType}");
            Console.WriteLine($"   PLC CstType: {plcType}");
            Console.WriteLine($"   PLC String: {plcString}");
            Console.WriteLine($"   是否為 LITE_CASSETTE: {CarrierTypeHelper.IsLiteCassetteCarrier(ecCarrierId)}");
            
            // 這裡展示如何檢查某個 Symbol 是否被支援
            Console.WriteLine($"\n2. Symbol 支援檢查:");
            string[] symbolsToCheck = { "BE", "LC", "EC", "XX" };
            foreach (var symbol in symbolsToCheck)
            {
                bool isSupported = CarrierTypeHelper.IsSymbolSupported(symbol);
                Console.WriteLine($"   Symbol '{symbol}' 是否被支援: {isSupported}");
            }
        }

        /// <summary>
        /// 展示動態新增類型的方法 (僅作為範例，實際使用時建議修改靜態定義)
        /// </summary>
        public static void DynamicExtensionExample()
        {
            Console.WriteLine("\n=== 動態擴充範例 (不建議在生產環境使用) ===");
            
            // 注意：這個範例僅展示概念，實際應用中建議直接修改 CarrierTypeHelper 類別的靜態定義
            Console.WriteLine("動態新增類型的方法已在 CarrierTypeHelper.AddCarrierTypeMapping 中提供");
            Console.WriteLine("但建議的做法是:");
            Console.WriteLine("1. 在 CarrierTypeHelper 中新增新的常數");
            Console.WriteLine("2. 更新靜態映射表");
            Console.WriteLine("3. 如果需要，擴充 CarrierType 枚舉");
            
            Console.WriteLine("\n擴充步驟範例:");
            Console.WriteLine("// 1. 新增常數");
            Console.WriteLine("public const string NEW_TYPE_SYMBOL = \"NT\";");
            Console.WriteLine("");
            Console.WriteLine("// 2. 更新映射表");
            Console.WriteLine("private static readonly Dictionary<string, CarrierType> _symbolToCarrierTypeMap = new Dictionary<string, CarrierType>");
            Console.WriteLine("{");
            Console.WriteLine("    { FOUP_SYMBOL, CarrierType.FOUP },");
            Console.WriteLine("    { LITE_CASSETTE_SYMBOL, CarrierType.LITE_CASSETTE },");
            Console.WriteLine("    { NEW_TYPE_SYMBOL, CarrierType.NEW_TYPE }, // 新增這行");
            Console.WriteLine("};");
        }

        /// <summary>
        /// 展示反向查詢功能 - 從 PLC 類型反推 Carrier Symbol
        /// </summary>
        public static void ReverseLookupExample()
        {
            Console.WriteLine("\n=== 反向查詢範例 ===");
            
            // 從 PLC 字串反推 Carrier Symbol
            Console.WriteLine("1. 從 PLC 字串反推 Carrier Symbol:");
            string[] plcStrings = { "A", "B", "C", "", null };
            foreach (var plcString in plcStrings)
            {
                var symbol = CarrierTypeHelper.GetSymbolFromPLCString(plcString);
                Console.WriteLine($"   PLC '{plcString}' -> Symbol '{symbol}'");
            }
            

            
            // 實際應用場景範例
            Console.WriteLine("\n3. 實際應用場景:");
            Console.WriteLine("   假設從 PLC 收到類型碼 'A'，需要知道對應的 Carrier Symbol:");
            string receivedFromPLC = "A";
            string correspondingSymbol = CarrierTypeHelper.GetSymbolFromPLCString(receivedFromPLC);
            Console.WriteLine($"   收到 PLC 類型碼: '{receivedFromPLC}'");
            Console.WriteLine($"   對應的 Carrier Symbol: '{correspondingSymbol}'");
            Console.WriteLine($"   可以用來構建範例 Carrier ID: '01{correspondingSymbol}001'");
        }

        /// <summary>
        /// 完整的整合範例 - 展示如何在現有系統中整合使用
        /// </summary>
        public static void IntegrationExample()
        {
            Console.WriteLine("\n=== 系統整合範例 ===");
            
            // 模擬一個處理 Carrier 的業務邏輯
            string[] carrierIds = { "01BE001", "02LC002", "03EC003" };
            
            foreach (string carrierId in carrierIds)
            {
                Console.WriteLine($"\n處理 Carrier: {carrierId}");
                
                // 使用 CarrierTypeHelper 進行統一的類型判斷
                var carrierType = CarrierTypeHelper.GetCarrierType(carrierId);
                var plcCstType = CarrierTypeHelper.GetPLCCstType(carrierId);
                var plcString = CarrierTypeHelper.GetPLCCstTypeString(carrierId);
                
                Console.WriteLine($"  Carrier 類型: {carrierType}");
                Console.WriteLine($"  PLC CstType: {plcCstType}");
                Console.WriteLine($"  PLC 字串: {plcString}");
                
                // 根據類型進行不同的處理
                switch (carrierType)
                {
                    case CarrierType.FOUP:
                        Console.WriteLine("  -> 執行 FOUP 相關處理邏輯");
                        Console.WriteLine($"  -> 發送給 PLC 的類型碼: {plcString}");
                        break;
                        
                    case CarrierType.LITE_CASSETTE:
                        Console.WriteLine("  -> 執行 LITE_CASSETTE 相關處理邏輯");
                        Console.WriteLine($"  -> 發送給 PLC 的類型碼: {plcString}");
                        break;
                        
                    case CarrierType.Unknown:
                        Console.WriteLine("  -> 未知類型，執行錯誤處理");
                        break;
                }
            }
        }

        /// <summary>
        /// 執行所有範例
        /// </summary>
        public static void RunAllExamples()
        {
            try
            {
                BasicUsageExample();
                ReplaceExistingLogicExample();
                ExtensionExample();
                DynamicExtensionExample();
                ReverseLookupExample();
                IntegrationExample();
                
                Console.WriteLine("\n=== 所有範例執行完成 ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"執行範例時發生錯誤: {ex.Message}");
            }
        }
    }
}