using System.Threading.Tasks;
using Xamarin.Forms;

namespace AmmySidekick
{
    // ReSharper disable once InconsistentNaming
    /*public class Listener_Socket
    {
        private static Listener_Socket _instance;

        public static Listener_Socket Instance
        {
            get {
                if (_instance == null) {
                    _instance = new Listener_Socket();
                    _instance.Initialize();
                }

                return _instance;
            }
        }
        
        private SocketProxy _socket;
        private readonly ListenerParser _parser = new ListenerParser();
        private const int Port = 53029;
        private readonly byte[] _buffer = new byte[1024 * 100];

        public void Initialize()
        {
            _socket = new SocketProxy(2, 2, 17);
            
            var ipAddressAny = _socket.GetIpAddressObjectAny();

            _socket.Bind(ipAddressAny, Port);

            var multicastIp = _socket.GetIpAddressObject("224.0.1.188");
            var multicastOption = _socket.GetMulticastOptionObject(multicastIp, ipAddressAny);

            _socket.SetSocketOption(0, 12, multicastOption);
            
            _parser.MessageReceived += ParserOnMessageReceived;

            Task.Factory.StartNew(ReceiveMessagesLoop, TaskCreationOptions.LongRunning);
        }

        private void ReceiveMessagesLoop()
        {
            while (true) {
                var bytesReceived = _socket.Receive(_buffer);

                for (var i = 0; i < bytesReceived; i++)
                    _parser.Feed(_buffer[i]);
            }
        }

        private void ParserOnMessageReceived(object sender, ListenerParserEventArgs eventArgs)
        {
            var messages = eventArgs.Messages;

            Device.BeginInvokeOnMainThread(() => {
                RuntimeUpdateHandler.ReceiveMessages(messages);
            });
        }
    }*/
}