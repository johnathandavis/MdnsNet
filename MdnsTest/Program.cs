﻿using System;
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
            
            
            var client = new MdnsNet.MdnsClient();
            var response = client.Query("_googlecast._tcp").Result;
            Console.WriteLine(response.Domain);
            Console.WriteLine(response.Name);

            Console.WriteLine("Query Sent.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            listener.Stop();
        }

    }
}