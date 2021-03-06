﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using MdnsNet.MDNS;

namespace MdnsNet.Server
{
    public class MdnsServer
    {
        public MdnsServer()
        {
            this.ResolverDatabase = new Dictionary<string, MdnsRecord>();
            this.Listener = new MdnsListener();
            this.Listener.QueryReceived += Listener_QueryReceived;
        }
        public MdnsServer(IPAddress ip, int port)
        {
            this.ResolverDatabase = new Dictionary<string, MdnsRecord>();
            this.Listener = new MdnsListener(ip, port);
            this.Listener.QueryReceived += Listener_QueryReceived;
        }

        void Listener_QueryReceived(object sender, MdnsQuery query)
        {
            foreach (MdnsNet.DNS.QueryQuestion question in query.Questions)
            {
                string domain = question.Name.Name;
                if (ResolverDatabase.ContainsKey(domain.Replace(".local", "")))
                {
                    this.Listener.SendResponse(query.RemoteEndpoint, ResolverDatabase[domain.Replace(".local", "")]);
                }
            }
        }

        public Dictionary<string, MdnsRecord> ResolverDatabase { get; private set; }
        public MdnsListener Listener { get; private set; }

        public void Start()
        {
            Listener.Start();
        }

        public void Stop()
        {
            Listener.Stop();
        }
    }
}
