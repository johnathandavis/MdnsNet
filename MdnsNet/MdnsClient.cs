using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using MdnsNet.MDNS;
using MdnsNet.Server;

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

        public List<MdnsRecord> QueryAll(string serviceName, int timeout)
        {
            byte[] payload = MdnsQuery.Create(serviceName);
            var _client = CreateClient();


            List<MdnsRecord> records = new List<MdnsRecord>();
            bool KeepSearching = true;
            var now = DateTime.Now;

            var queryThread = new System.Threading.Thread(() =>
            {
                _client.Send(payload, payload.Length, new IPEndPoint(IPAddress.Parse(MdnsListener.MDNS_IP), MdnsListener.MDNS_PORT));

                while (KeepSearching)
                {
                    try
                    {
                        var endpoint = new IPEndPoint(IPAddress.Any, MdnsListener.MDNS_PORT);
                        byte[] recData = _client.Receive(ref endpoint);
                        var record = new MdnsRecord(recData);
                        if (record.Domain.Replace(".local", "") == serviceName)
                        {
                            records.Add(record);
                        } 
                    }
                    catch { }
                }
            });

            queryThread.Start();

            while ((DateTime.Now - now).TotalSeconds < timeout);

            try
            {
                queryThread.Abort();
                queryThread = null;
            }
            catch {}
            return new List<MdnsRecord>(records);
        }

        public MdnsRecord Query(string serviceName)
        {
            byte[] data = MdnsQuery.Create(serviceName);

            var _client = CreateClient();
            MdnsRecord rec = null;
            bool worked = false;
            _client.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(MdnsListener.MDNS_IP), MdnsListener.MDNS_PORT));

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
            return rec;
        }

        public async Task<MdnsRecord> QueryAsync(string serviceName)
        {
            byte[] data = MdnsQuery.Create(serviceName);

            var _client = CreateClient();
            

            MdnsRecord rec = null;

            await Task.Factory.StartNew(() =>
                {
                    
                    bool worked = false;
                    _client.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(MdnsListener.MDNS_IP), MdnsListener.MDNS_PORT));
                    DateTime now = DateTime.Now;
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
