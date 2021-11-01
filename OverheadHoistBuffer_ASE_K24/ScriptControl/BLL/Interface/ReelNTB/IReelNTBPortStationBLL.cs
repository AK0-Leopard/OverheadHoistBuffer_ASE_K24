using com.mirle.ibg3k0.sc.Data.ValueDefMapAction.Events.ReelNTB;
using com.mirle.ibg3k0.sc.Data.VO;
using System.Collections.Generic;

namespace com.mirle.ibg3k0.sc.BLL.Interface
{
    public interface IReelNTBPortStationBLL
    {
        List<REEL_NTB_PORTSTATION> loadReelNTBPortStations(string eqID);

    }
}