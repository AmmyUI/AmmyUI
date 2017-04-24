using System.Reflection;
using AmmySidekick;

// ReSharper disable once CheckNamespace
namespace System.Net.Sockets
{
    public class UdpClient
    {
        private MethodInfo _beginReceiveMethod;
        private MethodInfo _endReceiveMethod;

        public object Object { get; set; }

        public UdpClient(IPEndPoint ipEndPoint)
        {
            var type = KnownTypes.FindType("System.Net.Sockets.UdpClient");
            var epType = KnownTypes.FindType("System.Net.IPEndPoint");
            var ctor = type.FindConstructor(epType);

            _beginReceiveMethod = type.GetMethod("BeginReceive", true);
            _endReceiveMethod = type.GetMethod("EndReceive", true);

            Object = ctor.Invoke(new[] { ipEndPoint.Object });
        }

        public void BeginReceive(AsyncCallback acceptClient, object o)
        {
            _beginReceiveMethod.Invoke(Object, new[] { acceptClient, o });
        }

        public byte[] EndReceive(IAsyncResult ar, ref IPEndPoint remoteEp)
        {
            var epObj = remoteEp.Object;
            var buffer = _endReceiveMethod.Invoke(Object, new object[] { ar, epObj });

            return (byte[])buffer;
        }
    }
}