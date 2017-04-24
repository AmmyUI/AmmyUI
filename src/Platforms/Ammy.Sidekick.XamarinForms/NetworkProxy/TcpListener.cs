using System.Reflection;
using AmmySidekick;

// ReSharper disable once CheckNamespace
namespace System.Net.Sockets
{
    public class TcpListener
    {
        private object Object { get; }

        private readonly MethodInfo _listenerStartMethod;
        private readonly MethodInfo _listenerBeginAcceptSocketMethod;
        private readonly MethodInfo _listenerEndAcceptSocketMethod;

        public TcpListener(IPEndPoint ipEndPoint)
        {
            var listenerType = KnownTypes.FindType("System.Net.Sockets.TcpListener");
            var epType = KnownTypes.FindType("System.Net.IPEndPoint");
            var listenerCtor = listenerType.FindConstructor(epType);

            _listenerStartMethod = listenerType.GetMethod("Start", true);
            _listenerBeginAcceptSocketMethod = listenerType.GetMethod("BeginAcceptSocket", true);
            _listenerEndAcceptSocketMethod = listenerType.GetMethod("EndAcceptSocket", true);

            Object = listenerCtor.Invoke(new [] {ipEndPoint.Object});
        }

        public void Start()
        {
            _listenerStartMethod.Invoke(Object, new object[0]);
        }

        public void BeginAcceptSocket(AsyncCallback acceptClient, object o)
        {
            _listenerBeginAcceptSocketMethod.Invoke(Object, new[] {acceptClient, o});
        }

        public Socket EndAcceptSocket(IAsyncResult ar)
        {
            var socketObject = _listenerEndAcceptSocketMethod.Invoke(Object, new object[] { ar });

            return Socket.FromObject(socketObject);
        }
    }
}