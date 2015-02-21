using System;
using System.Text;
using System.Linq;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace MdnsNet
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
    }
}
