using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

using System.Net.Security;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ArchClassLib.HTTP
{
    public class SimpleWebServer
    {
        private TcpListener _listener;
        private Thread _listenThread;
        private volatile bool _keepListening = false;
        private X509Certificate2 _cert = null;
        public delegate HttpResponse HttpRequestHandler(HttpRequest request);

        public SimpleWebServer(HttpRequestHandler handler, ushort port)
        {
            this.Request = handler;
            _listener = new TcpListener(IPAddress.Any, port);
            this.Port = port;
            this.IndexPage = "index.html";
        }
        public SimpleWebServer(HttpRequestHandler handler, ushort port, X509Certificate2 certificate)
        {
            this.Request = handler;
            this._listener = new TcpListener(IPAddress.Any, port);
            this.Port = port;
            this.IndexPage = "index.html";
            this._cert = certificate;
        }

        public void Start()
        {
            _listener.Start();

            _listenThread = new Thread(Listen);
            _listenThread.Start();

            _keepListening = true;
        }
        public void Stop()
        {
            _keepListening = false;
            try
            {
                _listener.Stop();
                _listenThread.Abort();
                _listenThread = null;
            }
            catch { }
        }

        public void Listen()
        {
            while (_keepListening)
            {
                TcpClient client = null;
                try
                {
                    client = _listener.AcceptTcpClient();
                }
                catch
                {
                    continue;
                }
                ThreadPool.QueueUserWorkItem(HandleRequest, client);
            }
        }

        private void HandleRequest(object sendingClient)
        {
            if (sendingClient == null) return;

            var client = (TcpClient)sendingClient;
            if (!client.Connected) return;

            var inStream = client.GetStream();

            if (IsSsl)
            {
                var sslStream = new SslStream(inStream, true);
                sslStream.AuthenticateAsServer(_cert);
                HttpRequest request = null;
                try
                {
                    request = new HttpRequest(sslStream);
                }
                catch { return; }
                if (request.RequestedResource == "/") request.RequestedResource += IndexPage;

                var response = Request(request);
                var responseBytes = response.ToPacketData();
                try
                {
                    sslStream.Write(responseBytes, 0, responseBytes.Length);
                    sslStream.Close();
                }
                catch { }
            }
            else
            {

                HttpRequest request = null;
                try
                {
                    request = new HttpRequest(inStream);
                }
                catch { return; }
                if (request.RequestedResource == "/") request.RequestedResource += IndexPage;

                var response = Request(request);
                var responseBytes = response.ToPacketData();
                var stream = client.GetStream();
                stream.Write(responseBytes, 0, responseBytes.Length);
                stream.Close();
            }
          

            
        }

        public string IndexPage { get; set; }
        public HttpRequestHandler Request { get; private set; }
        public ushort Port { get; private set; }
        public bool IsSsl
        {
            get
            {
                return _cert != null;
            }
        }

        public static string GetMimeType(string fileExtension)
        {
            fileExtension = fileExtension.Split('.').Last().ToLower();
            switch (fileExtension)
            {
                case "html":
                case "htm":
                    return "text/html";
                case "txt":
                    return "text/plain";
                case "jpg":
                case "jpeg":
                    return "image/jpeg";
                case "bmp":
                    return "image/bmp";
                case "png":
                    return "image/png";
                case "ico":
                    return "image/x-icon";
                case "gif":
                    return "image/gif";
                case "js":
                    return "application/javascript";
                case "json":
                    return "application/json";
                case "cs":
                    return "text/html";
                case "css":
                    return "text/css";
                default:
                    return "text/plain";
            }
        }
    }
}
