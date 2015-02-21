using System;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;


namespace MdnsTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting MDNS Server...");

            var listener = new MdnsNet.MdnsServer();
            listener.Start();

            var record = new MdnsNet.MdnsRecord("_googlecast._tcp", "Mothers Room", 8009, IPAddress.Parse("8.8.8.8"));
            record.TxtRecords.Add("user", "john.davis");
            record.TxtRecords.Add("pass", "Testing12345");
            record.TxtRecords.Add("dom", "hockaday.org");


            listener.ResolverDatabase.Add("_googlecast._tcp.local", record);
            

            Console.WriteLine("MDNS Server is listening...");

            System.Threading.Thread.Sleep(500);
            
            
            Tmds.MDns.ServiceBrowser browser = new Tmds.MDns.ServiceBrowser();
            browser.StartBrowse("_googlecast._tcp");
            Console.WriteLine("Query Sent.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            listener.Stop();
        }

    }
}
