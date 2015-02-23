using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MdnsNet
{
    public class MdnsClient
    {
        private Random _rnd;
        private ConcurrentDictionary<string, MdnsRecord> _records;

        private IPAddress _ip = IPAddress.Parse(MdnsListener.MDNS_IP);
        private IPEndPoint _endpoint = new IPEndPoint(IPAddress.Any, MdnsListener.MDNS_PORT);

        public MdnsClient()
        {
            _rnd = new Random();
            _records = new ConcurrentDictionary<string, MdnsRecord>();
            

        }

        

        private UdpClient CreateClient()
        {
            var _client = new UdpClient();


            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _client.JoinMulticastGroup(_ip);
            _client.Client.Bind(_endpoint);

            return _client;
        }

        public async Task<MdnsRecord> Query(string serviceName)
        {
            byte[] data = MdnsQuery.Create(serviceName);

            var _client = CreateClient();
            _client.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(MdnsListener.MDNS_IP), MdnsListener.MDNS_PORT));

            MdnsRecord rec = null;

            await Task.Factory.StartNew(() =>
                {
                    bool worked = false;
                    while (!worked)
                    {
                        try
                        {
                            var endpoint = new IPEndPoint(IPAddress.Any, MdnsListener.MDNS_PORT);
                            byte[] qData = _client.Receive(ref endpoint);
                            var record = new MdnsRecord(qData);
                            if (record.Domain.Replace(".local", "") == serviceName)
                            {
                                rec = record;
                                worked = true;
                            }
                        }
                        catch { }
                    }
                });


            if (rec == null) throw new Exception("The timeout period was exceeded while waiting for a response for \"" + serviceName + "\".");

            return rec;
        }
    }
}
