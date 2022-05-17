using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.BLL.Interface;
using com.mirle.ibg3k0.sc.Data.PLC_Functions.MGV.Enums;
using System.Collections.Generic;

namespace UnitTestForMGVPort.StubObjects
{
    public class MockCassetteDataBLL : IManualPortCassetteDataBLL
    {
        public Dictionary<string, string> _datasKeyLocationValueCarrierId { get; set; }

        public Dictionary<string, string> _datasKeyCarrierIdValueLocation { get; set; }

        public Dictionary<string, CstType> _datasKeyCarrierIdValueCstType { get; set; }

        public string CarrierIdByDelete { get; private set; }

        public string CarrierIdByInstall { get; private set; }

        public CstType CstTypeByInstall { get; private set; }

        public string CarrierLocationByInstall { get; private set; }

        public MockCassetteDataBLL()
        {
            _datasKeyLocationValueCarrierId = new Dictionary<string, string>();
            _datasKeyCarrierIdValueLocation = new Dictionary<string, string>();
            _datasKeyCarrierIdValueCstType = new Dictionary<string, CstType>();
        }

        public bool Delete(string carrierId)
        {
            CarrierIdByDelete = carrierId;

            if (_datasKeyCarrierIdValueLocation.ContainsKey(carrierId))
            {
                _datasKeyLocationValueCarrierId[_datasKeyCarrierIdValueLocation[carrierId]] = "";
                _datasKeyCarrierIdValueLocation.Remove(carrierId);
                _datasKeyCarrierIdValueCstType.Remove(carrierId);
            }
            return true;
        }

        public void Install(string carrierLocation, string carrierId, CstType type)
        {
            CarrierIdByInstall = carrierId;
            CarrierLocationByInstall = carrierLocation;
            CstTypeByInstall = type;

            if (_datasKeyLocationValueCarrierId.ContainsKey(carrierLocation))
            {
                _datasKeyLocationValueCarrierId[carrierLocation] = carrierId;
                _datasKeyCarrierIdValueLocation[carrierId] = carrierLocation;
                _datasKeyCarrierIdValueCstType[carrierId] = type;
            }
            else
            {
                _datasKeyLocationValueCarrierId.Add(carrierLocation, carrierId);
                _datasKeyCarrierIdValueLocation.Add(carrierId, carrierLocation);
                _datasKeyCarrierIdValueCstType.Add(carrierId, type);
            }
        }

        public bool GetCarrierByBoxId(string carrierId, out CassetteData cassetteData)
        {
            cassetteData = null;
            if (_datasKeyCarrierIdValueLocation.ContainsKey(carrierId) == false)
                return false;

            cassetteData = new CassetteData();
            cassetteData.BOXID = carrierId;
            cassetteData.Carrier_LOC = _datasKeyCarrierIdValueLocation[carrierId];
            cassetteData.Stage = 1;
            cassetteData.CSTType = _datasKeyCarrierIdValueCstType[carrierId].ToString();

            return true;
        }

        public bool GetCarrierByPortName(string portName, int stage, out CassetteData cassetteData)
        {
            cassetteData = null;

            if (stage > 1)
                return false;

            if (_datasKeyLocationValueCarrierId.ContainsKey(portName) == false)
                return false;

            cassetteData = new CassetteData();
            cassetteData.BOXID = _datasKeyLocationValueCarrierId[portName];
            cassetteData.Carrier_LOC = portName;
            cassetteData.Stage = 1;
            cassetteData.CSTType = _datasKeyCarrierIdValueCstType[cassetteData.BOXID].ToString();

            return true;
        }
    }
}