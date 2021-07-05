using com.mirle.ibg3k0.sc;
using com.mirle.ibg3k0.sc.BLL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestForMGVPort.StubObjects
{
    public class MockCassetteDataBLL : IManualPortCassetteDataBLL
    {
        public string InstalledCarrierId { get; private set; }
        public string InstalledCarrierLocation { get; private set; }

        public MockCassetteDataBLL()
        {
        }

        public void Delete(string carrierId)
        {
            throw new NotImplementedException();
        }

        public void Install(string carrierLocation, string carrierId)
        {
            InstalledCarrierLocation = carrierLocation;
            InstalledCarrierId = carrierId;
        }

        public bool GetCarrierByBoxId(string carrierId, out CassetteData cassetteData)
        {
            throw new NotImplementedException();
        }

        public bool GetCarrierByPortName(string portName, int stage, out CassetteData cassetteData)
        {
            throw new NotImplementedException();
        }
    }
}