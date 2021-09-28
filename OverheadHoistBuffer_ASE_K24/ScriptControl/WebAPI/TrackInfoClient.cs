using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.WebAPI
{
    public class TrackInfoClient
    {
        sc.App.SCApplication scApp = null;
        Channel channel = null;
        public TrackInfoClient()
        {
            scApp = sc.App.SCApplication.getInstance();
            string s_grpc_client_ip = scApp.getString("gRPCClientIP", "127.0.0.1");
            string s_grpc_client_port = scApp.getString("gRPCClientPort", "7001");
            int.TryParse(s_grpc_client_port, out int i_grpc_client_port);
            channel = new Channel(s_grpc_client_ip, i_grpc_client_port, ChannelCredentials.Insecure);

        }
    }
}
