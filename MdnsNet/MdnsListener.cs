using System;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;

namespace MdnsNet
{
    public class MdnsListener
    {
        public const int MDNS_PORT = 5353;

        private IPAddress _ip = IPAddress.Parse("224.0.0.251");
        private IPEndPoint _endpoint = new IPEndPoint(IPAddress.Any, MDNS_PORT);
        private UdpClient _client;
        private Thread _listenThread;
        private bool _hasStarted = false;
        private volatile bool _keepListening = true;


        public MdnsListener()
        {
            // Create UDP Client and instruct it to allow socket reuse
            _client = new UdpClient();
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        }

        public void Start()
        {
            if (_hasStarted) throw new InvalidOperationException("To start the MDNS Server, it must first be in the \"stopped\" state.");

            try
            {
                _client.Client.Bind(_endpoint);
                _client.JoinMulticastGroup(_ip);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while attempting to bind to the socket address. (See inner exception for more details)", ex);
            }

            // We have successfully started listening
            _hasStarted = true;

            // Start the handler thread
            _listenThread = new Thread(SocketListener);
            _listenThread.Start();

            // Yay! We are done!
        }
        public void Stop()
        {
            if (!_hasStarted) throw new InvalidOperationException("You cannot stop a listener that has not started.");

            // Try the polite way of exiting the thread
            _keepListening = false;

            // Force close it
            try
            {
                _listenThread.Abort();
            }
            catch { }

            // Reset it
            try
            {
                _listenThread = null;
            }
            catch { }

            _hasStarted = false;
        }



        /// <summary>
        /// This method handles the actual listening on the socket. Any messages are then
        /// sent off to another thread for dispatch.
        /// </summary>
        private void SocketListener()
        {
            while (_keepListening)
            {
                try
                {
                    var tEndpoint = new IPEndPoint(IPAddress.Any, MDNS_PORT);
                    byte[] msg = _client.Receive(ref tEndpoint);

                    
                    MdnsQuery query = null;
                    try
                    {
                        query = new MdnsQuery(tEndpoint, msg);
                    }
                    catch {}

                    // If an MDNS query is received, but no one is listening, does it make any sound?
                    if (query != null && QueryReceived != null)
                    {
                        QueryReceived(this, query);
                    }
                }
                catch { }
            }

            _keepListening = true;
        }

        public void SendResponse(IPEndPoint endpoint, MdnsRecord record)
        {
            byte[] payload = record.ToPayload();
            _client.Send(payload, payload.Length, new IPEndPoint(_ip, MDNS_PORT));
        }
            // Reset this back to true

        public delegate void QueryReceivedEventHandler(object sender, MdnsQuery query);
        public event QueryReceivedEventHandler QueryReceived;
    }
}
