using System;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace MdnsTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("You must either provide a service name to lookup, or provide the host flag.");
                Console.WriteLine("\te.g.\tmdnstest _chromecast._tcp");
                Console.WriteLine("\tor\tmdnstest -host");
            }
            else
            {
                if (args[0].ToLower().Trim('-') == "host")
                {
                    Host();
                }
                else
                {
                    Test(args[0]);
                }
            }
        }

        static void Host()
        {
            Console.WriteLine("Enter Service Name (e.g. _chromecast._tcp):");
            string serviceName = Console.ReadLine();
            Console.WriteLine("Enter Host Name (e.g. \"Mother's Room\" [without quotes]):");
            string name = Console.ReadLine();
            Console.WriteLine("Enter Service Port (e.g. 3389):");
            short port = -1;
            while (!short.TryParse(Console.ReadLine(), out port)) Console.WriteLine("You entered an invalid port. Please try again.");

            var myip = GetPrimaryIP();
            if (myip == null)
            {
                Console.WriteLine("ERROR: No valid IP interface found. Exiting...");
                return;
            }

            var record = new MdnsNet.MDNS.MdnsRecord(serviceName, name, port, myip);

            Console.WriteLine("Enter any TXT records you want (press enter for a new one, blank when done):");
            Console.WriteLine("For example: myname=John");
            string input = "a";
            while (input != "")
            {
                input = Console.ReadLine();
                if (input != "")
                {
                    if (!input.Contains("="))
                    {
                        Console.WriteLine("All TXT records must by a key-value pair, such as: myname=John");
                    }
                    else
                    {
                        string key = input.Split('=')[0];
                        string val = input.Split('=')[1];
                        if (record.TxtRecords.ContainsKey(key))
                        {
                            Console.WriteLine("The TXT record already contains an item by the name of \"" + key + "\".");
                        }
                        record.TxtRecords.Add(key, val);
                    }
                }
            }
            Console.WriteLine();


            // Start this thing up
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Starting MDNS Server on " + myip.ToString());

            var listener = new MdnsNet.Server.MdnsServer();
            listener.ResolverDatabase.Add(record.Domain + ".local", record);
            listener.Start();

            Console.WriteLine("Press any key to stop.");
            Console.ReadKey();

            listener.Stop();
        }

        static IPAddress GetPrimaryIP()
        {
            // Find Our Main IP
            foreach (var iface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (iface.OperationalStatus == OperationalStatus.Up)
                {
                    var ip = iface.GetIPProperties();
                    if (ip != null && ip.UnicastAddresses != null && ip.UnicastAddresses.Count > 0)
                    {
                        foreach (var tryIp in ip.UnicastAddresses)
                        {
                            if (tryIp.Address != null && tryIp.Address.AddressFamily == AddressFamily.InterNetwork) return tryIp.Address;
                        }
                    }
                }
            }
            return null;
        }


        static void Test(string service)
        {
            var client = new MdnsNet.MdnsClient();
            Console.WriteLine("Querying...");
            var response = client.QueryAsync(service).Result;
            Console.WriteLine(response.ToString());

        }
    }
}

