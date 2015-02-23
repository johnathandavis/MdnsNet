using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace MdnsNet
{
    public class MdnsRecord
    {
        public MdnsRecord(string domain, string name, short port, IPAddress ip)
        {
            TxtRecords = new Dictionary<string, string>();
            TTL = 120;
            Weight = 0;
            Priority = 0;

            this.Domain = domain;
            this.Name = name + "." + domain;
            this.Port = port;
            this.IP = ip;
        }
        public MdnsRecord(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                // We don't care about the transaction ID and flags
                reader.ReadBytes(6);

                // How many responses?
                byte[] resBytes = reader.ReadBytes(2);
                short responseCount = BitConverter.ToInt16(new byte[2] { resBytes[1], resBytes[0] }, 0);

                // NO Authority RRs
                reader.ReadBytes(2);

                // How many extra responses?
                byte[] extraBytes = reader.ReadBytes(2);
                short extraCount = BitConverter.ToInt16(new byte[2] { extraBytes[1], extraBytes[0] }, 0);

                if (responseCount != 1 && extraCount != 3) throw new Exception("Invalid MdnsRecord.");

                var domain = new DomainName(reader);
                this.Domain = domain.Name;

                // This should be a PTR
                byte[] answer1typeBytes = reader.ReadBytes(2);
                DnsRecordType answer1type = (DnsRecordType)BitConverter.ToInt16(new byte[2] { answer1typeBytes[1], answer1typeBytes[0] }, 0);

                // Read the Class
                reader.ReadBytes(2);

                // Read the TTL
                reader.ReadBytes(4);

                // Data length
                byte[] answer1lengthBytes = reader.ReadBytes(2);
                short answer1length = BitConverter.ToInt16(new byte[2] { answer1lengthBytes[1], answer1lengthBytes[0] }, 0);

                List<string> domainNameParts = new List<string>();

                short nameReadLength = 0;
                // Read the domain
                byte[] domainNameBytes = reader.ReadBytes(answer1length);
                using (var domainMS = new MemoryStream(domainNameBytes))
                using (var dom = new BinaryReader(domainMS))
                {
                    byte len = 1;
                    while (len != 0)
                    {
                        len = dom.ReadByte();
                        nameReadLength++;

                        if (len == 0) break;
                        domainNameParts.Add(Encoding.ASCII.GetString(dom.ReadBytes(len)));

                        nameReadLength+=len;
                    }
                }

                this.Name = domainNameParts[0];



                // Read the TXT record
                byte tlen = 1;
                while (true)
                {
                    tlen = reader.ReadByte();
                    if (tlen == 0) break;
                    reader.ReadBytes(tlen);
                }

                // Read TXT
                reader.ReadBytes(8);


                byte[] txtLengthBytes = reader.ReadBytes(2);
                short txtLen = BitConverter.ToInt16(new byte[2] { txtLengthBytes[1], txtLengthBytes[0] }, 0);

                byte[] txtBytes = reader.ReadBytes(txtLen);

                TxtRecords = new Dictionary<string, string>();

                using (var txtMs = new MemoryStream(txtBytes))
                using (var txtReader = new BinaryReader(txtMs))
                {
                    short read = 0;
                    while (read < txtLen)
                    {
                        byte myLen = txtReader.ReadByte();
                        read += (short)(myLen + 1);
                        string txt = Encoding.ASCII.GetString(txtReader.ReadBytes(myLen));
                        TxtRecords.Add(txt.Split('=')[0], txt.Split('=')[1]);
                    }
                }

                // Read the SRV part
                reader.ReadBytes(nameReadLength);


                // Skip SRV, TTL, Class
                reader.ReadBytes(8);

                byte[] srvLenBytes = reader.ReadBytes(2);
                short srvLength = BitConverter.ToInt16(new byte[2] { srvLenBytes[1], srvLenBytes[0] }, 0);

                byte[] priorityBytes = reader.ReadBytes(2);
                this.Priority = BitConverter.ToInt16(new byte[2] { priorityBytes[1], priorityBytes[0] }, 0);

                byte[] weightBytes = reader.ReadBytes(2);
                this.Weight = BitConverter.ToInt16(new byte[2] { weightBytes[1], weightBytes[0] }, 0);

                byte[] portBytes = reader.ReadBytes(2);
                this.Port = BitConverter.ToInt16(new byte[2] { portBytes[1], portBytes[0] }, 0);

                reader.ReadBytes(srvLength - 6);

                // Read A record
                reader.ReadBytes(srvLength - 6);
                reader.ReadBytes(10);

                this.IP = new IPAddress(reader.ReadBytes(4));
            }
        }

        public string Domain { get; set; }
        public string Name { get; set; }
        public short Port { get; set; }
        public short Priority { get; set; }
        public short Weight { get; set; }
        public IPAddress IP { get; set; }
        public int TTL { get; set; }
        public Dictionary<string, string> TxtRecords { get; set; }

        public byte[] ToPayload()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Transaction ID: 0000
                writer.Write((short)0);

                // Flags: 8400 (Authoritative Response, No Error)
                writer.Write(new byte[2] { 0x84, 0x00 }, 0, 2);

                // No Questions
                writer.Write(new byte[2] { 0, 0 }, 0, 2);
                
                // 1 Answer
                writer.Write(new byte[2] { 0, 1 }, 0, 2);

                // 0 Authority RRs
                writer.Write(new byte[2] { 0, 0 }, 0, 2);

                // Additional RRs
                short additional = TxtRecords.Count > 0 ? (short)3 : (short)2;
                byte[] addBytes = BitConverter.GetBytes(additional);
                writer.Write(new byte[2] { addBytes[1], addBytes[0] }, 0, 2);

                // Write the primary PTR answer:
                writer.Write(ToPtrPayload());

                // Write the TXT answer:
                writer.Write(ToTxtPayload());

                // Write the SRV answer:
                writer.Write(ToSrvPayload());

                // Write the A (host) answer:
                writer.Write(ToAPayload());

                // We done
                writer.Flush();
                return ms.ToArray();
            }
        }
        private byte[] ToPtrPayload()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                var domName = new DomainName(Domain);
                var domNameBytes = domName.ToBytes();
                writer.Write(domNameBytes, 0, domNameBytes.Length);
                writer.Write((byte)0);

                // PTR Record in the Internet Class
                writer.Write(new byte[4] { 0x00, 0x0c, 0x00, 0x01 }, 0, 4);

                // We need to write the TTL
                byte[] ttlBytes = BitConverter.GetBytes(TTL);
                byte[] newTTL = new byte[4] { ttlBytes[3], ttlBytes[2], ttlBytes[1], ttlBytes[0] };
                writer.Write(newTTL, 0, 4);

                // Name
                var myName = new DomainName(this.Name);
                byte[] myNameBytes = myName.ToBytes();
                short len = (short)(myNameBytes.Length+1);
                byte[] lenBytes = BitConverter.GetBytes(len);
                writer.Write(new byte[2] { lenBytes[1], lenBytes[0]}, 0, 2);

                // Write our name
                writer.Write(myNameBytes, 0, myNameBytes.Length);
                writer.Write((byte)0);

                writer.Flush();
                return ms.ToArray();
            }
        }
        private byte[] ToTxtPayload()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Write my name
                var myName = new DomainName(Name);
                var myNameBytes = myName.ToBytes();
                writer.Write(myNameBytes, 0, myNameBytes.Length);
                writer.Write((byte)0);

                // Write the record type (TXT=16) and Internet class
                writer.Write(new byte[4] { 0x00, 0x10, 0x80, 0x01 }, 0, 4);

                // We need to write the TTL
                byte[] ttlBytes = BitConverter.GetBytes(TTL);
                byte[] newTTL = new byte[4] { ttlBytes[3], ttlBytes[2], ttlBytes[1], ttlBytes[0] };
                writer.Write(newTTL, 0, 4);

                // Calculate the data length
                short txtLength = 0;
                foreach (var pair in TxtRecords) txtLength += (short)(pair.Key.Length + pair.Value.Length + 1);
                txtLength += (short)TxtRecords.Count;

                byte[] lenBytes = BitConverter.GetBytes(txtLength);
                writer.Write(new byte[2] { lenBytes[1], lenBytes[0] }, 0, 2);

                // Write the data
                foreach (var pair in TxtRecords)
                {
                    byte len = (byte)(pair.Key.Length + pair.Value.Length + 1);
                    writer.Write(len);
                    string finalVal = pair.Key + "=" + pair.Value;
                    byte[] finalValBytes = Encoding.ASCII.GetBytes(finalVal);
                    writer.Write(finalValBytes, 0, finalValBytes.Length);
                }

                writer.Flush();

                return ms.ToArray();
            }
        }
        private byte[] ToSrvPayload()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Write my name
                var myName = new DomainName(Name);
                var myNameBytes = myName.ToBytes();
                writer.Write(myNameBytes, 0, myNameBytes.Length);
                writer.Write((byte)0);

                // Write the record type (SRV=33) and Internet class
                writer.Write(new byte[4] { 0x00, 0x21, 0x80, 0x01 }, 0, 4);

                // We need to write the TTL
                byte[] ttlBytes = BitConverter.GetBytes(TTL);
                byte[] newTTL = new byte[4] { ttlBytes[3], ttlBytes[2], ttlBytes[1], ttlBytes[0] };
                writer.Write(newTTL, 0, 4);


                var modName = new DomainName(Name.Split('.')[0] + ".local");
                var modNameBytes = modName.ToBytes();

                // Write the data length
                short len = (short)(7 + modNameBytes.Length);
                byte[] lenBytes = BitConverter.GetBytes(len);
                writer.Write(new byte[2] { lenBytes[1], lenBytes[0] }, 0, 2);

                // Write the priority
                byte[] prioBytes = BitConverter.GetBytes(Priority);
                writer.Write(new byte[2] { prioBytes[1], prioBytes[0] }, 0, 2);

                // Write the Weight
                byte[] weightBytes = BitConverter.GetBytes(Weight);
                writer.Write(new byte[2] { weightBytes[1], weightBytes[0] }, 0, 2);

                // Write the Port
                byte[] portBytes = BitConverter.GetBytes(Port);
                writer.Write(new byte[2] { portBytes[1], portBytes[0] }, 0, 2);

                // Write the target
                writer.Write(modNameBytes, 0, modNameBytes.Length);
                writer.Write((byte)0);

                writer.Flush();
                return ms.ToArray();
            }
        }
        private byte[] ToAPayload()
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                // Write the service name
                var modName = new DomainName(Name.Split('.')[0] + ".local");
                var modNameBytes = modName.ToBytes();
                writer.Write(modNameBytes, 0, modNameBytes.Length);
                writer.Write((byte)0);

                // A Record in Internet Class
                writer.Write(new byte[4] { 0x00, 0x01, 0x80, 0x01 }, 0, 4);

                // We need to write the TTL
                byte[] ttlBytes = BitConverter.GetBytes(TTL);
                byte[] newTTL = new byte[4] { ttlBytes[3], ttlBytes[2], ttlBytes[1], ttlBytes[0] };
                writer.Write(newTTL, 0, 4);

                // Write the data length
                writer.Write(new byte[2] { 0x00, 0x04 }, 0, 2);

                // Write the IP
                writer.Write(IP.GetAddressBytes(), 0, 4);

                writer.Flush();
                return ms.ToArray();
            }
        }
    }
}
