using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace com.mirle.ibg3k0.sc.Common
{
    /// <summary>
    /// Carrier Type 統一管理類別
    /// 用於統整判斷 Carrier Type 的方式，以及與 PLC CstType 的對應關係
    /// </summary>
    public static class CarrierTypeHelper
    {
        #region 常數定義

        /// <summary>
        /// FOUP 類型的 Carrier ID 識別碼
        /// </summary>
        public const string FOUP_SYMBOL = "BE";

        /// <summary>
        /// LITE_CASSETTE 類型的 Carrier ID 識別碼
        /// </summary>
        public const string LITE_CASSETTE_SYMBOL = "LC";

        /// <summary>
        /// LITE_CASSETTE_EC 類型的 Carrier ID 識別碼 (擴充範例)
        /// </summary>
        public const string LITE_CASSETTE_EC_SYMBOL = "EC";

        /// <summary>
        /// PLC 對應的 FOUP 類型
        /// </summary>
        public const string CST_TYPE_FOR_PLC_FOUP = "A";

        /// <summary>
        /// PLC 對應的 LITE_CASSETTE 類型
        /// </summary>
        public const string CST_TYPE_FOR_PLC_LITE_CASSETTE = "B";

        #endregion

        #region 內部映射表

        /// <summary>
        /// Carrier Symbol 到 CarrierType 的映射表
        /// </summary>
        private static readonly Dictionary<string, CarrierType> _symbolToCarrierTypeMap = new Dictionary<string, CarrierType>
        {
            { FOUP_SYMBOL, CarrierType.FOUP },
            { LITE_CASSETTE_SYMBOL, CarrierType.LITE_CASSETTE },
            { LITE_CASSETTE_EC_SYMBOL, CarrierType.LITE_CASSETTE } // 新增的 EC 也對應到 LITE_CASSETTE
        };

        /// <summary>
        /// CarrierType 到 PLC CstType 的映射表
        /// </summary>
        private static readonly Dictionary<CarrierType, CstType> _carrierTypeToCstTypeMap = new Dictionary<CarrierType, CstType>
        {
            { CarrierType.FOUP, CstType.A },
            { CarrierType.LITE_CASSETTE, CstType.B }
        };

        /// <summary>
        /// CarrierType 到 PLC CstType 字串的映射表
        /// </summary>
        private static readonly Dictionary<CarrierType, string> _carrierTypeToPLCStringMap = new Dictionary<CarrierType, string>
        {
            { CarrierType.FOUP, CST_TYPE_FOR_PLC_FOUP },
            { CarrierType.LITE_CASSETTE, CST_TYPE_FOR_PLC_LITE_CASSETTE }
        };

        /// <summary>
        /// PLC CstType 字串到主要 Carrier Symbol 的反向映射表
        /// </summary>
        private static readonly Dictionary<string, string> _plcStringToSymbolMap = new Dictionary<string, string>
        {
            { CST_TYPE_FOR_PLC_FOUP, FOUP_SYMBOL },           // "A" -> "BE"
            { CST_TYPE_FOR_PLC_LITE_CASSETTE, LITE_CASSETTE_SYMBOL }  // "B" -> "LC"
        };

        #endregion

        #region 公開 API

        /// <summary>
        /// 根據 Carrier ID 判斷 Carrier 類型
        /// </summary>
        /// <param name="carrierId">Carrier ID</param>
        /// <returns>Carrier 類型</returns>
        public static CarrierType GetCarrierType(string carrierId)
        {
            if (string.IsNullOrWhiteSpace(carrierId) || carrierId.Length < 4)
            {
                return CarrierType.Unknown;
            }

            // 取得第3、4碼 (索引 2、3)
            string symbol = carrierId.Substring(2, 2);
            
            return _symbolToCarrierTypeMap.TryGetValue(symbol, out CarrierType carrierType) 
                ? carrierType 
                : CarrierType.Unknown;
        }

        /// <summary>
        /// 根據 Carrier ID 判斷是否為 FOUP 類型
        /// </summary>
        /// <param name="carrierId">Carrier ID</param>
        /// <returns>是否為 FOUP 類型</returns>
        public static bool IsFoupCarrier(string carrierId)
        {
            return GetCarrierType(carrierId) == CarrierType.FOUP;
        }

        /// <summary>
        /// 根據 Carrier ID 判斷是否為 LITE_CASSETTE 類型
        /// </summary>
        /// <param name="carrierId">Carrier ID</param>
        /// <returns>是否為 LITE_CASSETTE 類型</returns>
        public static bool IsLiteCassetteCarrier(string carrierId)
        {
            return GetCarrierType(carrierId) == CarrierType.LITE_CASSETTE;
        }

        /// <summary>
        /// 根據 Carrier 類型取得對應的 PLC CstType
        /// </summary>
        /// <param name="carrierType">Carrier 類型</param>
        /// <returns>PLC CstType</returns>
        public static CstType GetPLCCstType(CarrierType carrierType)
        {
            return _carrierTypeToCstTypeMap.TryGetValue(carrierType, out CstType cstType) 
                ? cstType 
                : CstType.Undefined;
        }

        /// <summary>
        /// 根據 Carrier ID 取得對應的 PLC CstType
        /// </summary>
        /// <param name="carrierId">Carrier ID</param>
        /// <returns>PLC CstType</returns>
        public static CstType GetPLCCstType(string carrierId)
        {
            CarrierType carrierType = GetCarrierType(carrierId);
            return GetPLCCstType(carrierType);
        }

        /// <summary>
        /// 根據 Carrier 類型取得對應的 PLC CstType 字串表示
        /// </summary>
        /// <param name="carrierType">Carrier 類型</param>
        /// <returns>PLC CstType 字串 (A/B)</returns>
        public static string GetPLCCstTypeString(CarrierType carrierType)
        {
            return _carrierTypeToPLCStringMap.TryGetValue(carrierType, out string plcString) 
                ? plcString 
                : string.Empty;
        }

        /// <summary>
        /// 根據 Carrier ID 取得對應的 PLC CstType 字串表示
        /// </summary>
        /// <param name="carrierId">Carrier ID</param>
        /// <returns>PLC CstType 字串 (A/B)</returns>
        public static string GetPLCCstTypeString(string carrierId)
        {
            CarrierType carrierType = GetCarrierType(carrierId);
            return GetPLCCstTypeString(carrierType);
        }

        /// <summary>
        /// 根據 Carrier 類型取得字串表示
        /// </summary>
        /// <param name="carrierType">Carrier 類型</param>
        /// <returns>CarrierType 字串表示</returns>
        public static string GetCarrierTypeString(CarrierType carrierType)
        {
            return carrierType.ToString();
        }

        /// <summary>
        /// 根據 Carrier ID 取得對應的 CarrierType 字串表示
        /// </summary>
        /// <param name="carrierId">Carrier ID</param>
        /// <returns>CarrierType 字串表示</returns>
        public static string GetCarrierTypeString(string carrierId)
        {
            CarrierType carrierType = GetCarrierType(carrierId);
            return GetCarrierTypeString(carrierType);
        }

        /// <summary>
        /// 根據 PLC CstType 字串取得對應的主要 Carrier Symbol
        /// </summary>
        /// <param name="plcCstTypeString">PLC CstType 字串 (A/B)</param>
        /// <returns>對應的 Carrier Symbol (BE/LC)，如果找不到則返回空字串</returns>
        public static string GetSymbolFromPLCString(string plcCstTypeString)
        {
            if (string.IsNullOrWhiteSpace(plcCstTypeString))
                return string.Empty;

            return _plcStringToSymbolMap.TryGetValue(plcCstTypeString, out string symbol) 
                ? symbol 
                : string.Empty;
        }


        /// <summary>
        /// 根據 Carrier Symbol 直接取得對應的 PLC CstType
        /// "BE" -> CstType.A
        /// "LC", "EC" -> CstType.B
        /// </summary>
        /// <param name="symbol">Carrier Symbol (例如："BE", "LC", "EC")</param>
        /// <returns>PLC CstType，未知 Symbol 回傳 CstType.Undefined</returns>
        public static CstType GetCstTypeFromSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return CstType.Undefined;

            // 直接從 symbol 映射表查詢 CarrierType，然後轉換為 CstType
            if (_symbolToCarrierTypeMap.TryGetValue(symbol.ToUpper(), out CarrierType carrierType))
            {
                return GetPLCCstType(carrierType);
            }

            return CstType.Undefined;
        }

        /// <summary>
        /// 根據 Carrier Symbol 直接取得對應的 PLC CstType 字串
        /// "BE" -> "A"
        /// "LC", "EC" -> "B"
        /// </summary>
        /// <param name="symbol">Carrier Symbol (例如："BE", "LC", "EC")</param>
        /// <returns>PLC CstType 字串 ("A" 或 "B")，未知 Symbol 回傳空字串</returns>
        public static string GetPLCStringFromSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return string.Empty;

            // 直接從 symbol 映射表查詢 CarrierType，而不是使用 GetCarrierType (它是用於完整 Carrier ID)
            if (_symbolToCarrierTypeMap.TryGetValue(symbol.ToUpper(), out CarrierType carrierType))
            {
                return GetPLCCstTypeString(carrierType);
            }

            return string.Empty;
        }

        /// <summary>
        /// 根據 PLC CstType 枚舉取得對應的字串表示 (內部輔助方法)
        /// </summary>
        /// <param name="cstType">PLC CstType 枚舉</param>
        /// <returns>PLC CstType 字串表示</returns>
        private static string GetPLCStringFromCstType(CstType cstType)
        {
            switch (cstType)
            {
                case CstType.A:
                    return CST_TYPE_FOR_PLC_FOUP;
                case CstType.B:
                    return CST_TYPE_FOR_PLC_LITE_CASSETTE;
                default:
                    return string.Empty;
            }
        }


        /// <summary>
        /// 取得所有支援的 Carrier Symbol
        /// </summary>
        /// <returns>支援的 Symbol 列表</returns>
        public static IEnumerable<string> GetSupportedSymbols()
        {
            return _symbolToCarrierTypeMap.Keys;
        }

        /// <summary>
        /// 取得所有支援的 CarrierType
        /// </summary>
        /// <returns>支援的 CarrierType 列表</returns>
        public static IEnumerable<CarrierType> GetSupportedCarrierTypes()
        {
            return _symbolToCarrierTypeMap.Values.Distinct();
        }

        /// <summary>
        /// 檢查指定的 Symbol 是否被支援
        /// </summary>
        /// <param name="symbol">要檢查的 Symbol</param>
        /// <returns>是否被支援</returns>
        public static bool IsSymbolSupported(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                return false;
                
            return _symbolToCarrierTypeMap.ContainsKey(symbol);
        }

        #endregion

        #region 擴充方法 (用於未來新增類型)

        /// <summary>
        /// 動態新增新的 Carrier Symbol 到 CarrierType 的映射
        /// (這是為了展示擴充能力，實際使用時建議修改靜態映射表)
        /// </summary>
        /// <param name="symbol">新的 Symbol</param>
        /// <param name="carrierType">對應的 CarrierType</param>
        /// <param name="cstType">對應的 PLC CstType</param>
        /// <param name="plcString">對應的 PLC 字串</param>
        /// <returns>是否新增成功</returns>
        public static bool AddCarrierTypeMapping(string symbol, CarrierType carrierType, CstType cstType, string plcString)
        {
            try
            {
                if (string.IsNullOrEmpty(symbol) || string.IsNullOrEmpty(plcString))
                    return false;

                // 檢查是否已存在
                if (_symbolToCarrierTypeMap.ContainsKey(symbol))
                    return false;

                // 新增映射 (注意：這只是範例，實際應用中建議修改靜態定義)
                _symbolToCarrierTypeMap[symbol] = carrierType;
                _carrierTypeToCstTypeMap[carrierType] = cstType;
                _carrierTypeToPLCStringMap[carrierType] = plcString;

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    /// <summary>
    /// Carrier 類型枚舉
    /// </summary>
    public enum CarrierType
    {
        /// <summary>
        /// 未知類型
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// FOUP 類型 (Front Opening Unified Pod)
        /// </summary>
        FOUP = 1,

        /// <summary>
        /// LITE_CASSETTE 類型
        /// </summary>
        LITE_CASSETTE = 2
    }
}