using System.Collections.Generic;
using System.Reflection;
using AmmySidekick;

// ReSharper disable once CheckNamespace
namespace System.Net
{
    public class IPAddress
    {
        private static bool _initialized;
        private static ConstructorInfo _ipAddressCtor;

        public static object Any
        {
            get {
                if (!_initialized)
                    Initialize();

                return _ipAddressCtor.Invoke(new object[] { 0 });
            }
        }

        private static void Initialize()
        {
            var ipAddressType = KnownTypes.FindType("System.Net.IPAddress");
            _ipAddressCtor = ipAddressType.FindConstructor(typeof(long));

            if (_ipAddressCtor == null)
                throw new Exception("IPAddress constructor not found");
            //_ipAddressParseMethod = ipAddressType.GetMethod("Parse", false, new[] { typeof(string) });

            _initialized = true;
        }
    }
}