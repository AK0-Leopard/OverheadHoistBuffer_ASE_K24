using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.AK0.ProtocolFormat.Track;
using Grpc.Core;

namespace VehicleControl_Viewer.Protots
{
    public class TrackGreeter : com.mirle.AK0.ProtocolFormat.Track.Greeter.GreeterClient
    {
        public TrackGreeter()
        {

        }
    }
}
