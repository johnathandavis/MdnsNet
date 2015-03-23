using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Server.HTTP;
using MdnsNet;
using MdnsNet.DNS;
using MdnsNet.MDNS;
using MdnsNet.Server;

namespace Server
{
    partial class MulticastServer : ServiceBase
    {
        private ManagementServer manager;
        private MdnsServer mdns;
        private RecordSystem recordMan;
        private List<GlobalRecord> allRecords;
        public static Guid MY_SETTINGS = Guid.Parse("4A3157F7-E61D-4CF8-8323-B1164086DC0F");

        public MulticastServer()
        {
            InitializeComponent();

            mdns = new MdnsServer();
            manager = new ManagementServer(mdns);
            manager.RecordAdded += manager_RecordAdded;
            recordMan = new RecordSystem();
        }

        void manager_RecordAdded(MdnsRecord record)
        {
            var glob = new GlobalRecord(record);
            allRecords.Add(glob);
            mdns.ResolverDatabase.Add(record.Domain, record);
            recordMan.SaveRecord(glob);
        }

        protected override void OnStart(string[] args)
        {
            allRecords = recordMan.LoadRecords();
            allRecords.Add(CreateMyRecord());
            mdns.ResolverDatabase.Clear();
            foreach (var rec in allRecords)
            {
                mdns.ResolverDatabase.Add(rec.Record.Domain, rec.Record);
            }

            mdns.Start();
            manager.Start();
        }

        private GlobalRecord CreateMyRecord()
        {
            string myname = "_mdns._http";
            string serv = Environment.MachineName;
            short port = (short)manager.Port;
            System.Net.IPAddress ip = null;

            foreach (var iface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                {
                    try
                    {
                        var prop = iface.GetIPProperties();
                        foreach (var addr in prop.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                ip = addr.Address;
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }
            if (ip == null) ip = System.Net.IPAddress.Loopback;
            var record = new MdnsRecord(myname, serv, port, ip);
            return new GlobalRecord(record, MY_SETTINGS);
        }

        protected override void OnStop()
        {
            mdns.Stop();
            manager.Stop();
        }
    }
}
