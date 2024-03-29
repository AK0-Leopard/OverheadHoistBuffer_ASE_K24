﻿using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;

namespace com.mirle.ibg3k0.sc.BLL.Interface
{
    public interface IManualPortCassetteDataBLL
    {
        bool GetCarrierByBoxId(string carrierId, out CassetteData cassetteData);

        bool GetCarrierByPortName(string portName, int stage, out CassetteData cassetteData);

        bool Delete(string carrierId);

        void Install(string carrierLocation, string carrierId, CstType type);
    }
}