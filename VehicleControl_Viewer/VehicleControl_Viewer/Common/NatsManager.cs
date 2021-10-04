using NATS.Client;
using NLog;
using STAN.Client;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VehicleControl_Vierwer.Common
{
    public class NatsManager
    {
        Logger logger = NLog.LogManager.GetCurrentClassLogger();
        StanConnectionFactory StanConnectionFactory = null;
        IStanConnection conn = null;
        ConcurrentDictionary<string, IStanSubscription> dicSubscription = new ConcurrentDictionary<string, IStanSubscription>();
        StanOptions cOpts = StanOptions.GetDefaultOptions();
        Options natsOptions = null;
        readonly string DefaultNatsURL = "nats://192.168.39.111:4222";
        string clusterID = null;
        string clientID = null;
        string producID = null;

        public NatsManager(string product_id, string cluster_id, string client_id)
        {
            clusterID = cluster_id;
            clientID = client_id;
            producID = product_id;

            string nats_server_ip = getHostTableIP("nats.ohxc.mirle.com.tw");
            if (nats_server_ip != null)
            {
                DefaultNatsURL = $"nats://{nats_server_ip}:4222";
            }


            natsOptions = ConnectionFactory.GetDefaultOptions();
            natsOptions.MaxReconnect = Options.ReconnectForever;
            natsOptions.ReconnectWait = 1000;
            natsOptions.NoRandomize = true;
            //natsOptions.Servers = srevers_url;
            natsOptions.Name = client_id;
            natsOptions.AllowReconnect = true;
            natsOptions.Timeout = 1000;
            natsOptions.PingInterval = 5000;
            natsOptions.Url = DefaultNatsURL;
            natsOptions.AsyncErrorEventHandler += (sender, args) =>
            {
                logger.Error($"Server:{args.Conn.ConnectedUrl}{Environment.NewLine},Message:{args.Error}{Environment.NewLine},Subject:{args.Subscription.Subject}");
                Console.WriteLine("Error: ");
                Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);
                Console.WriteLine("   Message: " + args.Error);
                Console.WriteLine("   Subject: " + args.Subscription.Subject);
            };

            natsOptions.ServerDiscoveredEventHandler += (sender, args) =>
            {
                logger.Info($"A new server has joined the cluster:{String.Join(", ", args.Conn.DiscoveredServers)}");
                Console.WriteLine("A new server has joined the cluster:");
                Console.WriteLine("    " + String.Join(", ", args.Conn.DiscoveredServers));
            };

            natsOptions.ClosedEventHandler += (sender, args) =>
            {
                logger.Info($"Connection Closed:{Environment.NewLine}Server:{args.Conn.ConnectedUrl}");
                Console.WriteLine("Connection Closed: ");
                Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);
            };

            natsOptions.DisconnectedEventHandler += (sender, args) =>
            {
                logger.Info($"Connection Disconnected:{Environment.NewLine}Server:{args.Conn.ConnectedUrl}");
                Console.WriteLine("Connection Disconnected: ");
                Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);
            };
            natsOptions.ReconnectedEventHandler += (sender, args) =>
            {
                logger.Info($"Connection Reconnected:{Environment.NewLine}Server:{args.Conn.ConnectedUrl}");
                Console.WriteLine("Connection Disconnected: ");
                Console.WriteLine("   Server: " + args.Conn.ConnectedUrl);
            };
            cOpts.NatsURL = DefaultNatsURL;
            StanConnectionFactory = new StanConnectionFactory();

            //IConnection natsConn = null;
            //natsConn = new ConnectionFactory().CreateConnection(natsOptions);
            //cOpts.NatsConn = natsConn;
            //conn = getConnection();
        }

        public Task StartConnection()
        {
            return Task.Run(() => retryGetConnection());
        }

        private void retryGetConnection()
        {
            do
            {
                try
                {
                    conn = getConnection();
                }
                catch (Exception e)
                {

                }
                SpinWait.SpinUntil(() => false, 1000);
            } while (conn == null);
        }

        private string getHostTableIP(string ip)
        {
            string return_ip = null;
            var remoteipAdr = System.Net.Dns.GetHostAddresses(ip);
            if (remoteipAdr != null && remoteipAdr.Count() > 0)
            {
                return_ip = remoteipAdr[0].ToString();
            }

            return return_ip;
        }

        IStanConnection getConnection()
        {
            IConnection natsConn = new ConnectionFactory().CreateConnection(natsOptions);
            cOpts.NatsConn = natsConn;
            return StanConnectionFactory.CreateConnection(clusterID, clientID, cOpts);
        }

        public string Publish(string subject, byte[] data, EventHandler<StanAckHandlerArgs> handler)
        {
            subject = $"{producID}_{subject}";
            return conn.Publish(subject, data, handler);
        }

        public void Publish(string subject, byte[] data)
        {
            subject = $"{producID}_{subject}";
            conn.Publish(subject, data);
        }

        public void PublishAsync(string subject, byte[] data)
        {
            subject = $"{producID}_{subject}";
            conn.PublishAsync(subject, data);
        }

        public void Subscriber(string subject, EventHandler<StanMsgHandlerArgs> handler, bool in_all = false, bool is_last = false, ulong since_seq_no = 0, DateTime? since_duration = null)
        {
            subject = $"{producID}_{subject}";
            StanSubscriptionOptions sOpts = StanSubscriptionOptions.GetDefaultOptions();
            if (in_all)
            {
                sOpts.DeliverAllAvailable();
            }
            else if (is_last)
            {
                sOpts.StartWithLastReceived();
            }
            else if (since_seq_no != 0)
            {
                sOpts.StartAt(since_seq_no);
            }
            else if (since_duration.HasValue)
            {
                sOpts.StartAt(since_duration.Value);
            }
            dicSubscription.GetOrAdd(subject, conn.Subscribe(subject, sOpts, handler));
        }

        public void close()
        {
            if (dicSubscription.Count > 0)
            {
                foreach (var keyPair in dicSubscription)
                {
                    //keyPair.Value.Unsubscribe();
                    keyPair.Value.Close();
                }
            }

            conn?.Close();
        }
    }
}
