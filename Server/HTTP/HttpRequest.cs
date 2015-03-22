using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace ArchClassLib.HTTP
{
    public class HttpRequest
    {
        public HttpRequest(Stream stream)
        {
            

            this.Headers = new Dictionary<string, string[]>();

            System.Net.Security.SslStream sslstream = new System.Net.Security.SslStream(stream, true);
            var reader = new StreamReader(stream);

            // First Line is the HTTP header
            string header = reader.ReadLine();

            if (header == null) return;
            string[] headerParts = header.Split(' ');

            Method = (HttpMessageType)Enum.Parse(HttpMessageType.CONNECT.GetType(), headerParts[0]);
            RequestedResource = headerParts[1];
            string url = RequestedResource;
            GetMethodParameters = new Dictionary<string, string>();
            if (url.Contains('?'))
            {
                string getStr = url.Split('?')[1];
                RequestedResource = url.Split('?')[0];
                string[] getParts = getStr.Split('&');
                foreach (string s in getParts)
                {
                    string[] getVarParts = s.Split('=');
                    if (getVarParts.Length == 2)
                    {
                        GetMethodParameters.Add(getVarParts[0], getVarParts[1]);
                    }
                    else
                    {
                        GetMethodParameters.Add(getVarParts[0], "");
                    }
                }
            }

            decimal version = 1.1M;
            decimal.TryParse(headerParts[2], out version);
            HttpVersion = version;

            // The next lines are all headers, until we get a double new line
            while (header != "" && header != null && !reader.EndOfStream)
            {
                header = reader.ReadLine();
                if (header == "" || header == null) break;
                if (!header.Contains(':')) continue;
                string headerName = header.Split(':')[0];
                string headerVal = header.Split(':')[1].Trim();

                if (!this.Headers.ContainsKey(headerName))
                {
                    this.Headers.Add(headerName, new string[1] { headerVal });
                }
                else
                {
                    this.Headers[headerName] = this.Headers[headerName].Concat(new string[1] { headerVal }).ToArray();
                }
            }

            if (this.Headers.ContainsKey("Content-Length") && int.Parse(this.Headers["Content-Length"][0]) > 0)
            {
                char[] body = new char[int.Parse(this.Headers["Content-Length"][0])];
                reader.Read(body, 0, body.Length);
                Body = new string(body);
            }
        }

        public Dictionary<string, string> GetMethodParameters { get; private set; }
        public string RequestedResource { get; set; }
        public HttpMessageType Method { get; private set; }
        public decimal HttpVersion { get; private set; }
        public Dictionary<string, string[]> Headers { get; private set; }
        public string Host
        {
            get
            {
                if (Headers.ContainsKey("Host"))
                {
                    return Headers["Host"][0];
                }
                return "";
            }
        }

        public string Body { get; private set; }
    }
}
