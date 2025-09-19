using com.mirle.ibg3k0.sc.Common;
using System;

namespace TestCarrierTypeHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("測試 IsSymbolSupported 方法：");
            
            // 測試支援的符號
            string[] supportedSymbols = { "BE", "LC", "EC" };
            foreach (var symbol in supportedSymbols)
            {
                var result = CarrierTypeHelper.IsSymbolSupported(symbol);
                Console.WriteLine($"Symbol '{symbol}': {result} (應該是 True)");
            }
            
            // 測試不支援的符號
            string[] unsupportedSymbols = { "XX", "YY", "ZZ", "12", "AB", null, "" };
            foreach (var symbol in unsupportedSymbols)
            {
                var result = CarrierTypeHelper.IsSymbolSupported(symbol);
                Console.WriteLine($"Symbol '{symbol}': {result} (應該是 False)");
            }
            
            Console.WriteLine("測試完成！");
        }
    }
}