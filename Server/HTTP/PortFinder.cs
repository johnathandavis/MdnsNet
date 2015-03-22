using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using System.Net;

namespace Server.HTTP
{
    public class PortFinder
    {
        public static ushort FindOpenPort()
        {
            var usedPorts = GetUsedPorts();

            // Prefer ports at least in the 1000s
            for (ushort i = 1000; i < ushort.MaxValue; i++)
            {
                if (!usedPorts.Contains(i))
                {
                    return i;
                }
            }

            // Fine, try the smaller ones
            for (ushort i = 1; i < 1000; i++)
            {
                if (!usedPorts.Contains(i))
                {
                    return i;
                }
            }

            // This is awkward. There are NO OPEN PORTS ON THIS MACHINE???

            throw new ApplicationException("There aren't any open ports on this computer. This is pretty weird...");
        }

        public static List<ushort> GetUsedPorts()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "netstat",
                Arguments = "-an",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            var proc = new Process();
            proc.StartInfo = startInfo;
            proc.Start();

            List<ushort> usedPorts = new List<ushort>();

            var stream = proc.StandardOutput;
            while (!stream.EndOfStream)
            {
                string line = stream.ReadLine();
                line = line.Trim();
                string[] parts = line.Split('\t');
                parts = (from a in parts where a.Trim() == "" select a.Trim()).ToArray();
                if (parts.Length >= 2 && parts[0].ToLower() == "tcp")
                {
                    string[] ipParts = parts[1].Split(':');
                    if (ipParts.Length == 2)
                    {
                        string ip = ipParts[0];
                        string port = ipParts[1];

                        var ipAddr = IPAddress.Parse(ip);
                        ushort ipPort = 0;
                        if (ushort.TryParse(port, out ipPort))
                        {
                            usedPorts.Add(ipPort);
                        }
                    }
                }
            }

            return usedPorts;
        }
    }
}
