using System;
using System.Dynamic;
using System.Net;
using System.Net.Sockets;

// ReSharper disable once CheckNamespace
namespace AmmySidekick
{
    public class Listener
    {
        private static Listener _instance;

        public static Listener Instance
        {
            // ReSharper disable once ConvertPropertyToExpressionBody
            get { return _instance ?? (_instance = new Listener()); }
        }
        
        private readonly ListenerParser _parser = new ListenerParser();
        private readonly TcpListener _tcpListener;
        private readonly byte[] _buffer = new byte[1024 * 100];
        private readonly UdpClient _udpClient;
        private Socket _tcpSocket;

        private const int Port = 53029;

        private Listener()
        {
            _parser.MessageReceived += ParserOnMessageReceived;

            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, Port));
            _udpClient.BeginReceive(OnReceive, null);

            _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, Port));
            _tcpListener.Start();
            _tcpListener.BeginAcceptSocket(AcceptClient, null);
        }

        private void OnReceive(IAsyncResult ar)
        {
            var ep = new IPEndPoint(IPAddress.Any, 53029);
            var buffer = _udpClient.EndReceive(ar, ref ep);

            _udpClient.BeginReceive(OnReceive, null);

            for (int i = 0; i < buffer.Length; i++)
                _parser.Feed(buffer[i]);
        }

        private void AcceptClient(IAsyncResult ar)
        {
            _tcpSocket = _tcpListener.EndAcceptSocket(ar);
            StartReceiving();
        }

        private void StartReceiving()
        {
            try {
                _tcpSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, EndReceive, null);
            } catch {
                _tcpListener.BeginAcceptSocket(AcceptClient, null);
            }
        }

        private void EndReceive(IAsyncResult ar)
        {
            var bytesRead = _tcpSocket.EndReceive(ar);

            // bytesRead 0 means client has disconnected
            if (bytesRead == 0) {
                _tcpListener.BeginAcceptSocket(AcceptClient, null);
                return;
            }

            StartReceiving();
            
            for (int i = 0; i < bytesRead; i++)
                _parser.Feed(_buffer[i]);
        }

        private void ParserOnMessageReceived(object sender, ListenerParserEventArgs eventArgs)
        {
            var messages = eventArgs.Messages;
            MainThread.Run(() => RuntimeUpdateHandler.ReceiveMessages(messages));
        }
    }
}