using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.BLL.Interface;
using System;
using System.Collections.Generic;

namespace UnitTestForMGVPort.StubObjects
{
    public class MockCassetteDataBLL : IManualPortCassetteDataBLL
    {
        public Dictionary<string, string> _datasByLocation { get; set; }

        public Dictionary<string, string> _datasByCarrierId { get; set; }

        public string CarrierIdByDelete { get; private set; }

        public string CarrierIdByInstall { get; private set; }

        public string CarrierLocationByInstall { get; private set; }

        public MockCassetteDataBLL()
        {
            _datasByLocation = new Dictionary<string, string>();
            _datasByCarrierId = new Dictionary<string, string>();
        }

        public void Delete(string carrierId)
        {
            CarrierIdByDelete = carrierId;

            if (_datasByCarrierId.ContainsKey(carrierId))
            {
                _datasByLocation[_datasByCarrierId[carrierId]] = "";
                _datasByCarrierId.Remove(carrierId);
            }
        }

        public void Install(string carrierLocation, string carrierId)
        {
            CarrierIdByInstall = carrierId;
            CarrierLocationByInstall = carrierLocation;

            if (_datasByLocation.ContainsKey(carrierLocation))
            {
                _datasByLocation[carrierLocation] = carrierId;
                _datasByCarrierId[carrierId] = carrierLocation;
            }
            else
            {
                _datasByLocation.Add(carrierLocation, carrierId);
                _datasByCarrierId.Add(carrierId, carrierLocation);
            }
        }

        public bool GetCarrierByBoxId(string carrierId, out CassetteData cassetteData)
        {
            cassetteData = null;
            if (_datasByCarrierId.ContainsKey(carrierId) == false)
                return false;

            cassetteData = new CassetteData();
            cassetteData.BOXID = carrierId;
            cassetteData.Carrier_LOC = _datasByCarrierId[carrierId];
            cassetteData.Stage = 1;

            return true;
        }

        public bool GetCarrierByPortName(string portName, int stage, out CassetteData cassetteData)
        {
            cassetteData = null;

            if (stage > 1)
                return false;

            if (_datasByLocation.ContainsKey(portName) == false)
                return false;

            cassetteData = new CassetteData();
            cassetteData.BOXID = _datasByLocation[portName];
            cassetteData.Carrier_LOC = portName;
            cassetteData.Stage = 1;

            return true;
        }
    }
}