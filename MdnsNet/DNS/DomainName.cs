using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MdnsNet.DNS
{
    public class DomainName
    {
        public DomainName(BinaryReader reader)
        {
            List<string> nameParts = new List<string>();

            byte len = 1;
            while (true)
            {
                len = reader.ReadByte();
                if (len == 0) break;

                byte[] nextName = reader.ReadBytes(len);
                nameParts.Add(Encoding.ASCII.GetString(nextName));
            }

            this.NameParts = nameParts;
        }
        public DomainName(string name)
        {
            if (!name.Split('.').Last().ToLower().EndsWith("local"))
            {
                if (name.EndsWith(".")) name += "local";
                else name += ".local";
            }

            NameParts = name.Split('.').ToList();
        }

        public List<string> NameParts { get; private set; }
        public string Name
        {
            get
            {
                return string.Join(".", NameParts);
            }
        }

        public bool IsValidMulticastDnsName
        {
            get
            {
                return (NameParts.Count > 0 && NameParts.Last().ToLower() == "local");
            }
        }

        public byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms, Encoding.ASCII, true))
                {
                    foreach (string s in NameParts)
                    {
                        // Get the length of this part, convert it to a short,
                        // and swap the bytes for network order
                        byte length = (byte)s.Length;
                        writer.Write(length);

                        // Write the string itself (ASCII Encoding)
                        byte[] strBytes = Encoding.ASCII.GetBytes(s);
                        writer.Write(strBytes, 0, strBytes.Length);
                    }
                    writer.Flush();
                }
                return ms.ToArray();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
