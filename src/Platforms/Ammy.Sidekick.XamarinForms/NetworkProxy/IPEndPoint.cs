using AmmySidekick;

// ReSharper disable once CheckNamespace
namespace System.Net
{
    // ReSharper disable once InconsistentNaming
    public class IPEndPoint
    {
        public object Object { get; }

        public IPEndPoint(object ipAddress, int port)
        {
            var epType = KnownTypes.FindType("System.Net.IPEndPoint");
            var ipAddressType = KnownTypes.FindType("System.Net.IPAddress");
            var epCtor = epType.FindConstructor(ipAddressType, typeof (int));

            Object = epCtor.Invoke(new[] {ipAddress, port});
        }
    }
}