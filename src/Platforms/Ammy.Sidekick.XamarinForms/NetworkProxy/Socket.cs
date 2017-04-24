using System.Reflection;
using AmmySidekick;

// ReSharper disable once CheckNamespace
namespace System.Net.Sockets
{
    public class Socket
    {
        public object Object { get; }

        private readonly MethodInfo _beginReceiveMethod;
        private readonly MethodInfo _endReceiveMethod;

        private Socket(object socketObject)
        {
            Object = socketObject;
            
            var socketType = KnownTypes.FindType("System.Net.Sockets.Socket");
            _beginReceiveMethod = socketType.GetMethod("BeginReceive", true);
            _endReceiveMethod = socketType.GetMethod("EndReceive", true);
        }

        public static Socket FromObject(object socketObject)
        {
            return new Socket(socketObject);
        }

        public void BeginReceive(byte[] buffer, int offset, int length, SocketFlags flags, AsyncCallback endReceive, object parameter)
        {
            _beginReceiveMethod.Invoke(Object, new[] {buffer, offset, length, (int)flags, endReceive, parameter});
        }

        public int EndReceive(IAsyncResult ar)
        {
            return (int)_endReceiveMethod.Invoke(Object, new object[] {ar});
        }
    }
}