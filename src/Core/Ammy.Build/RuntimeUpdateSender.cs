using System;
using System.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Ammy.Build
{
    public static class RuntimeUpdateSender
    {
        private static TcpClient _activeClient;
        public const int SendPort = 53029;

        public static void Send(string platformName, IList<XamlFileMeta> files, XamlProjectMeta previousMeta, string componentPrefix)
        {
            if (files.Count == 0)
                return;

            if (platformName == "WPF") {
                SendGeneric(files, previousMeta, componentPrefix);
            } else if (platformName == "XamarinForms") {
                SendXamarinForms(files, previousMeta, componentPrefix);
            }
        }

        private static void SendGeneric(IList<XamlFileMeta> files, XamlProjectMeta previousMeta, string componentPrefix)
        {
            try {
                var buffer = GetBuffer(files, previousMeta, componentPrefix);
                var bufferSent = false;

                try {
                    bufferSent = TrySendTcp(buffer);
                } catch (Exception e) {
                    Debug.WriteLine("Could not send to TCP server on 127.0.0.1: " + e.Message);
                }

                if (!bufferSent) {
                    using (var udpClient = new UdpClient()) {
                        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
                        udpClient.Send(buffer, buffer.Length, new IPEndPoint(IPAddress.Broadcast, SendPort));
                    }
                }
            } catch (Exception e) {
                Debug.WriteLine("Sending runtime update failed: " + e);
                throw;
            }
        }

        private static bool TrySendTcp(byte[] buffer, bool isReconnecting = false)
        {
            if (isReconnecting || _activeClient == null || !_activeClient.Connected) {
                _activeClient = new TcpClient();

                if (!_activeClient.ConnectAsync("127.0.0.1", 53029).Wait(50))
                    return false;
            }
            
            try {
                _activeClient.Client.Send(buffer);
            } catch (SocketException) {
                // if first attempt fails, try reconnecing in case connection was zombied
                // don't go into infinite loop in case sending always fails 
                if (!isReconnecting)
                    TrySendTcp(buffer, true);
                else 
                    throw;
            }

            return true;
        }

        private static void SendXamarinForms(IList<XamlFileMeta> files, XamlProjectMeta previousMeta, string componentPrefix)
        {
            SendGeneric(files, previousMeta, componentPrefix);
        }

        private static byte[] GetBuffer(IList<XamlFileMeta> files, XamlProjectMeta previousMeta, string componentPrefix)
        {
            var header = new byte[] { 0xbe, 0xef };
            var footer = new byte[] { 0xff };
            var result = header.ToList();

            result.Add((byte)files.Count);

            foreach (var file in files) {
                byte[] buffer;

                var extension = Path.GetExtension(file.FilePath);
                if (extension != null && extension.Equals(".xaml", StringComparison.InvariantCultureIgnoreCase))
                    buffer = Encoding.Unicode.GetBytes(File.ReadAllText(file.FilePath));
                else
                    buffer = File.ReadAllBytes(file.FilePath);

                var filename = file.Filename;
                var id = componentPrefix + filename.Replace("\\", "/").TrimStart('/');

                result.AddRange(CreateMarkupBuffer(buffer, id));

                var propertyList = GetPropertiesToReset(file, previousMeta);
                var propertiesBuffer = CreatePropertiesBuffer(propertyList);

                result.AddRange(propertiesBuffer);
            }

            result.AddRange(footer);

            var toSend = result.ToArray();
            return toSend;
        }

        private static byte[] CreatePropertiesBuffer(List<string> properties)
        {
            var propertiesString = string.Join(",", properties);
            var buffer = Encoding.Unicode.GetBytes(propertiesString);
            var bufferLen = BitConverter.GetBytes((ushort)buffer.Length);

            return bufferLen.Concat(buffer).ToArray();
        }

        private static byte[] CreateMarkupBuffer(byte[] buffer, string targetId)
        {
            var targetIdBuffer = Encoding.Unicode.GetBytes(targetId);
            var targetIdLength = BitConverter.GetBytes(targetIdBuffer.Length);
            var length = BitConverter.GetBytes(buffer.Length);
            var checksum = BitConverter.GetBytes(Fletcher16(buffer));

            return targetIdLength.Concat(targetIdBuffer)
                                 .Concat(length)
                                 .Concat(buffer)
                                 .Concat(checksum)
                                 .ToArray();
        }

        private static ushort Fletcher16(byte[] data)
        {
            ushort sum1 = 0;
            ushort sum2 = 0;

            for (var index = 0; index < data.Length; ++index) {
                sum1 = (ushort)((sum1 + data[index]) % 255);
                sum2 = (ushort)((sum2 + sum1) % 255);
            }

            return (ushort)((sum2 << 8) | sum1);
        }

        private static List<string> GetPropertiesToReset(XamlFileMeta file, XamlProjectMeta previousMeta)
        {
            var previousFileMeta = previousMeta.Files.FirstOrDefault(f => f.Filename.Equals(file.Filename, StringComparison.InvariantCultureIgnoreCase));

            if (previousFileMeta == null)
                return new List<string>();

            return previousFileMeta.Properties
                                   .SelectMany(m => new[] { m.PropertyType.ToString() + "|" + m.FullName })
                                   .Distinct()
                                   .ToList();
        }

    }
}