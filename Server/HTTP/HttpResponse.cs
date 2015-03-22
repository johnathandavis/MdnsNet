using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ArchClassLib.HTTP
{
    public class HttpResponse
    {
        public HttpResponse(string response)
        {
            StatusCode = HttpStatusCode._200OK;
            Content = Encoding.ASCII.GetBytes(response);
            ContentType = "text/html";
        }
        public HttpResponse(HttpStatusCode code)
        {
            StatusCode = code;
        }
        public HttpResponse(HttpStatusCode code, string response)
        {
            StatusCode = code;
            Content = Encoding.ASCII.GetBytes(response);
            ContentType = "text/html";
        }
        public HttpResponse(HttpStatusCode code, string contentType, byte[] data)
        {
            StatusCode = code;
            ContentType = contentType;
            Content = data;
        }
        public HttpResponse(string contentType, byte[] data)
        {
            StatusCode = HttpStatusCode._200OK;
            ContentType = contentType;
            Content = data;
        }

        public HttpStatusCode StatusCode { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }

        public byte[] ToPacketData()
        {
            string status = "HTTP/1.1 ";
            switch (StatusCode)
            {
                case HttpStatusCode._200OK:
                    status += "200 OK";
                    break;
            }

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.WriteLine(status);
                writer.WriteLine("Date: " + DateTime.Now.ToString("D, d M Y H:i:s T"));
                writer.WriteLine("Connection: close");
                writer.WriteLine("Server: Recon/HttpServer 1.0");
                writer.WriteLine("Content-Type: " + ContentType);
                writer.WriteLine("Content-Length: " + Content.Length);
                writer.WriteLine();
                writer.Flush();
                byte[] header = ms.ToArray();
                byte[] data = Content;
                byte[] concated = header.Concat(data).ToArray();
                return concated;
            }
        }
    }
}
