using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Net;
using System.Collections.Generic;

using MdnsNet.DNS;

namespace MdnsNet.MDNS
{
    public class MdnsQuery
    {
        public MdnsQuery(IPEndPoint ep, byte[] udpPayload)
        {
            this.RemoteEndpoint = ep;

            Questions = new List<QueryQuestion>();

            using (var ms = new MemoryStream(udpPayload))
            using (var reader = new BinaryReader(ms))
            {
                // Two bytes for transaction ID
                byte[] tidBytes = reader.ReadBytes(2);
                TransactionID = BitConverter.ToInt16(new byte[] { tidBytes[1], tidBytes[0]}, 0);

                // Skip two bytes (We don't need the DNS Flags)
                reader.ReadBytes(2);

                // Two bytes for Question Count
                byte[] countBytes = reader.ReadBytes(2);
                QuestionCount = BitConverter.ToInt16(new byte[2] { countBytes[1], countBytes[0]}, 0);

                // Skip 6 bytes for Answer RRs, Authority RRs,
                // and Additional RRs.
                reader.ReadBytes(6);

                for (int x = 0; x < QuestionCount; x++)
                {
                    // Get the domain name
                    var name = new DomainName(reader);

                    // Get Query Type
                    byte[] typeBytes = reader.ReadBytes(2);
                    DnsRecordType type = (DnsRecordType)BitConverter.ToInt16(new byte[2] { typeBytes[1], typeBytes[0] }, 0);

                    // Get DNS Class
                    byte[] clsBytes = reader.ReadBytes(2);
                    DnsClass cls = (DnsClass)BitConverter.ToInt16(new byte[2] { clsBytes[1], clsBytes[0] }, 0);

                    // Is it a valid Multicast-DNS query?
                    if (name.IsValidMulticastDnsName) Questions.Add(new QueryQuestion(name, type, cls));
                }
            }
        }

        public short TransactionID { get; private set; }
        public short QuestionCount { get; private set; }
        public List<QueryQuestion> Questions { get; private set; }
        public IPEndPoint RemoteEndpoint { get; private set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Incoming MDNS Query from " + RemoteEndpoint.Address.ToString() + " (" + QuestionCount + " " + (QuestionCount == 1 ? "question" : "questions") + "):");
            foreach (var question in Questions)
            {
                builder.AppendLine(question.ToString());
            }
            return builder.ToString();
        }


        private static Random _rnd = new Random();
        public static byte[] Create(string serviceName)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Generate a new Transaction ID
                byte[] transactionID = new byte[2];
                _rnd.NextBytes(transactionID);
                writer.Write(transactionID, 0, 2);

                // Write the DNS flags (0x00, 0x00)
                writer.Write(new byte[2] { 0x00, 0x00 }, 0, 2);

                // We are asking one question
                writer.Write(new byte[2] { 0x00, 0x01 }, 0, 2);

                // Nothing else
                writer.Write(new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 6);

                // Build the domain name
                var domain = new DomainName(serviceName + ".local");
                var domBytes = domain.ToBytes();
                writer.Write(domBytes, 0, domBytes.Length);
                writer.Write(new byte[2] { 0x00, 0x00 }, 0, 2);

                // We are asking for the PTR
                byte[] ptrBytes = BitConverter.GetBytes((short)DnsRecordType.PTR);
                writer.Write(new byte[2] { ptrBytes[1], ptrBytes[0] }, 0, 2);

                // Internet Class
                writer.Write(new byte[2] { 0x00, 0x01 }, 0, 2);


                writer.Flush();
                return ms.ToArray();
            }
        }
    }
}
