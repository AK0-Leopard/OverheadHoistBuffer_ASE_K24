using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.BLL.Interface
{
    public interface ICassetteDataBLL
    {
        bool GetCarrierByBoxId(string carrierId, out CassetteData cassetteData);

        bool GetCarrierByPortName(string portName, int stage, out CassetteData cassetteData);

        void Delete(string carrierId);

        void Install(string carrierLocation, string carrierId);
    }
}