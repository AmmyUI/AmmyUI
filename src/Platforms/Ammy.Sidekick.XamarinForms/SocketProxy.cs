using System;
using System.Linq;
using System.Reflection;

namespace AmmySidekick
{
    /*
    public class SocketProxy
    {
        private readonly object _socket;
        private readonly ConstructorInfo _epCtor;
        private readonly ConstructorInfo _ipAddressCtor;
        private readonly MethodInfo _socketBindMethod;
        private readonly MethodInfo _ipAddressParseMethod;
        private readonly MethodInfo _setSocketOptionMethod;
        private readonly ConstructorInfo _multicastOptionCtor;
        private readonly MethodInfo _socketReceiveMethod;

        public SocketProxy(int addressFamily, int type, int protocolType)
        {
            var socketType = KnownTypes.FindType("System.Net.Sockets.Socket");
            var multicastOptionType = KnownTypes.FindType("System.Net.Sockets.MulticastOption");
            var ipEndPointType = KnownTypes.FindType("System.Net.IPEndPoint");
            var endPointType = KnownTypes.FindType("System.Net.EndPoint");
            var socketCtor = socketType.GetConstructors()
                                       .FirstOrDefault(c => c.GetParameters().Length == 3);

            _socketBindMethod = socketType.GetMethod("Bind", true, new[] { endPointType });
            _socketReceiveMethod = socketType.GetMethod("Receive", true, new[] { typeof(byte[]) });

            _epCtor = ipEndPointType.FindConstructor(ipAddressType, typeof(int));


            _multicastOptionCtor = multicastOptionType.FindConstructor(ipAddressType, ipAddressType);
            _setSocketOptionMethod = socketType.GetTypeInfo().GetDeclaredMethods("SetSocketOption")
                                               .FirstOrDefault(mi => {
                                                   var parameters = mi.GetParameters();
                                                   return parameters.Length == 3 && parameters[2].ParameterType == typeof(object);
                                               });

            _socket = CreateObject("Socket", socketCtor, addressFamily, type, protocolType);
        }

        public void Bind(object ipAddress, int port)
        {
            //EndPoint ipep = new IPEndPoint(IPAddress.Any, 4567);
            var ipep = CreateObject("IPEndPoint", _epCtor, ipAddress, port);

            _socketBindMethod.Invoke(_socket, new[] { ipep });
        }

        public object GetIpAddressObject(string ip)
        {
            //var ip = IPAddress.Parse("224.0.1.188");
            return _ipAddressParseMethod.Invoke(null, new object[] { ip });

        }

        public object GetIpAddressObjectAny()
        {
            return _ipAddressCtor.Invoke(new object[] { 0 });
        }

        public void SetSocketOption(int optionLevel, int optionName, object option)
        {
            _setSocketOptionMethod.Invoke(_socket, new[] { optionLevel, optionName, option });
        }

        public object GetMulticastOptionObject(object group, object mcint)
        {
            return _multicastOptionCtor.Invoke(new [] {group, mcint});
        }
        
        private object CreateObject(string typeName, ConstructorInfo ctor, params object[] parms)
        {
            if (ctor == null)
                throw new Exception("Unable to resolve constructor for " + typeName);

            return ctor.Invoke(parms);
        }

        public int Receive(byte[] buffer)
        {
            return (int)_socketReceiveMethod.Invoke(_socket, new object[] {buffer});
        }
    }*/
}