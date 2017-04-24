using System.Collections.Generic;
using System.Linq;
using Nitra.Internal.Recovery;
using Ammy.Language;

namespace Ammy.Compiler
{
    public static class UwpArgs
    {
        public static List<string> Args { get; set; }
        static UwpArgs()
        {
            Args = new List<string> {
@"/outputPath:""obj\x86\Debug\\"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.ApplicationInsights\1.0.0\lib\portable-win81+wpa81\Microsoft.ApplicationInsights.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.ApplicationInsights.WindowsApps\1.0.0\lib\win81\Microsoft.ApplicationInsights.Extensibility.Windows.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.ApplicationInsights.PersistenceChannel\1.0.0\lib\portable-win81+wpa81\Microsoft.ApplicationInsights.PersistenceChannel.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.CSharp\4.0.0\ref\netcore50\Microsoft.CSharp.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.VisualBasic\10.0.0\ref\netcore50\Microsoft.VisualBasic.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.Win32.Primitives\4.0.0\ref\dotnet\Microsoft.Win32.Primitives.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\mscorlib.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.AppContext\4.0.0\ref\dotnet\System.AppContext.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Collections.Concurrent\4.0.10\ref\dotnet\System.Collections.Concurrent.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Collections\4.0.10\ref\dotnet\System.Collections.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Collections.Immutable\1.1.37\lib\dotnet\System.Collections.Immutable.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Collections.NonGeneric\4.0.0\ref\dotnet\System.Collections.NonGeneric.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Collections.Specialized\4.0.0\ref\dotnet\System.Collections.Specialized.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ComponentModel.Annotations\4.0.10\ref\dotnet\System.ComponentModel.Annotations.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.ComponentModel.DataAnnotations.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ComponentModel\4.0.0\ref\netcore50\System.ComponentModel.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ComponentModel.EventBasedAsync\4.0.10\ref\dotnet\System.ComponentModel.EventBasedAsync.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.Core.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Data.Common\4.0.0\ref\dotnet\System.Data.Common.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Diagnostics.Contracts\4.0.0\ref\netcore50\System.Diagnostics.Contracts.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Diagnostics.Debug\4.0.10\ref\dotnet\System.Diagnostics.Debug.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Diagnostics.StackTrace\4.0.0\ref\dotnet\System.Diagnostics.StackTrace.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Diagnostics.Tools\4.0.0\ref\netcore50\System.Diagnostics.Tools.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Diagnostics.Tracing\4.0.20\ref\dotnet\System.Diagnostics.Tracing.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Dynamic.Runtime\4.0.10\ref\dotnet\System.Dynamic.Runtime.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Globalization.Calendars\4.0.0\ref\dotnet\System.Globalization.Calendars.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Globalization\4.0.10\ref\dotnet\System.Globalization.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Globalization.Extensions\4.0.0\ref\dotnet\System.Globalization.Extensions.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.IO.Compression\4.0.0\ref\netcore50\System.IO.Compression.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.IO.Compression.ZipFile\4.0.0\ref\dotnet\System.IO.Compression.ZipFile.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.IO\4.0.10\ref\dotnet\System.IO.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.IO.FileSystem\4.0.0\ref\dotnet\System.IO.FileSystem.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.IO.FileSystem.Primitives\4.0.0\ref\dotnet\System.IO.FileSystem.Primitives.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.IO.IsolatedStorage\4.0.0\ref\dotnet\System.IO.IsolatedStorage.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.IO.UnmanagedMemoryStream\4.0.0\ref\dotnet\System.IO.UnmanagedMemoryStream.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Linq\4.0.0\ref\netcore50\System.Linq.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Linq.Expressions\4.0.10\ref\dotnet\System.Linq.Expressions.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Linq.Parallel\4.0.0\ref\netcore50\System.Linq.Parallel.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Linq.Queryable\4.0.0\ref\netcore50\System.Linq.Queryable.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.Net.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Net.Http\4.0.0\ref\netcore50\System.Net.Http.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Net.Http.Rtc\4.0.0\ref\netcore50\System.Net.Http.Rtc.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Net.NetworkInformation\4.0.0\ref\netcore50\System.Net.NetworkInformation.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Net.Primitives\4.0.10\ref\dotnet\System.Net.Primitives.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Net.Requests\4.0.10\ref\dotnet\System.Net.Requests.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Net.Sockets\4.0.0\ref\dotnet\System.Net.Sockets.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Net.WebHeaderCollection\4.0.0\ref\dotnet\System.Net.WebHeaderCollection.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.Numerics.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Numerics.Vectors\4.1.0\ref\dotnet\System.Numerics.Vectors.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Numerics.Vectors.WindowsRuntime\4.0.0\lib\dotnet\System.Numerics.Vectors.WindowsRuntime.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ObjectModel\4.0.10\ref\dotnet\System.ObjectModel.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Reflection.Context\4.0.0\ref\netcore50\System.Reflection.Context.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Reflection.DispatchProxy\4.0.0\ref\dotnet\System.Reflection.DispatchProxy.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Reflection\4.0.10\ref\dotnet\System.Reflection.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Reflection.Extensions\4.0.0\ref\netcore50\System.Reflection.Extensions.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Reflection.Metadata\1.0.22\lib\dotnet\System.Reflection.Metadata.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Reflection.Primitives\4.0.0\ref\netcore50\System.Reflection.Primitives.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Reflection.TypeExtensions\4.0.0\ref\dotnet\System.Reflection.TypeExtensions.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Resources.ResourceManager\4.0.0\ref\netcore50\System.Resources.ResourceManager.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime\4.0.20\ref\dotnet\System.Runtime.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.Extensions\4.0.10\ref\dotnet\System.Runtime.Extensions.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.Handles\4.0.0\ref\dotnet\System.Runtime.Handles.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.InteropServices\4.0.20\ref\dotnet\System.Runtime.InteropServices.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.InteropServices.WindowsRuntime\4.0.0\ref\netcore50\System.Runtime.InteropServices.WindowsRuntime.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.Numerics\4.0.0\ref\netcore50\System.Runtime.Numerics.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.Runtime.Serialization.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.Serialization.Json\4.0.0\ref\netcore50\System.Runtime.Serialization.Json.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.Serialization.Primitives\4.0.10\ref\dotnet\System.Runtime.Serialization.Primitives.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.Serialization.Xml\4.0.10\ref\dotnet\System.Runtime.Serialization.Xml.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.WindowsRuntime\4.0.10\ref\netcore50\System.Runtime.WindowsRuntime.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Runtime.WindowsRuntime.UI.Xaml\4.0.0\ref\netcore50\System.Runtime.WindowsRuntime.UI.Xaml.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Security.Claims\4.0.0\ref\dotnet\System.Security.Claims.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Security.Principal\4.0.0\ref\netcore50\System.Security.Principal.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.ServiceModel.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ServiceModel.Duplex\4.0.0\ref\netcore50\System.ServiceModel.Duplex.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ServiceModel.Http\4.0.10\ref\dotnet\System.ServiceModel.Http.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ServiceModel.NetTcp\4.0.0\ref\netcore50\System.ServiceModel.NetTcp.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ServiceModel.Primitives\4.0.0\ref\netcore50\System.ServiceModel.Primitives.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.ServiceModel.Security\4.0.0\ref\netcore50\System.ServiceModel.Security.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.ServiceModel.Web.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Text.Encoding.CodePages\4.0.0\ref\dotnet\System.Text.Encoding.CodePages.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Text.Encoding\4.0.10\ref\dotnet\System.Text.Encoding.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Text.Encoding.Extensions\4.0.10\ref\dotnet\System.Text.Encoding.Extensions.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Text.RegularExpressions\4.0.10\ref\dotnet\System.Text.RegularExpressions.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Threading\4.0.10\ref\dotnet\System.Threading.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Threading.Overlapped\4.0.0\ref\dotnet\System.Threading.Overlapped.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Threading.Tasks.Dataflow\4.5.25\lib\dotnet\System.Threading.Tasks.Dataflow.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Threading.Tasks\4.0.10\ref\dotnet\System.Threading.Tasks.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Threading.Tasks.Parallel\4.0.0\ref\netcore50\System.Threading.Tasks.Parallel.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Threading.Timer\4.0.0\ref\netcore50\System.Threading.Timer.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.Windows.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.Xml.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.Xml.Linq.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Xml.ReaderWriter\4.0.10\ref\dotnet\System.Xml.ReaderWriter.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\Microsoft.NETCore.Portable.Compatibility\1.0.0\ref\netcore50\System.Xml.Serialization.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Xml.XDocument\4.0.10\ref\dotnet\System.Xml.XDocument.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Xml.XmlDocument\4.0.0\ref\dotnet\System.Xml.XmlDocument.dll"" ",

@"/reference:""C:\Users\Mihhail\.nuget\packages\System.Xml.XmlSerializer\4.0.10\ref\dotnet\System.Xml.XmlSerializer.dll"" ",

@"/reference:""C:\Program Files (x86)\Windows Kits\10\References\Windows.ApplicationModel.Calls.CallsVoipContract\1.0.0.0\Windows.ApplicationModel.Calls.CallsVoipContract.winmd"" ",

@"/reference:""C:\Program Files (x86)\Windows Kits\10\References\Windows.Devices.Printers.PrintersContract\1.0.0.0\Windows.Devices.Printers.PrintersContract.winmd"" ",

@"/reference:""C:\Program Files (x86)\Windows Kits\10\References\Windows.Foundation.FoundationContract\2.0.0.0\Windows.Foundation.FoundationContract.winmd"" ",

@"/reference:""C:\Program Files (x86)\Windows Kits\10\References\Windows.Foundation.UniversalApiContract\2.0.0.0\Windows.Foundation.UniversalApiContract.winmd"" ",

@"/reference:""C:\Program Files (x86)\Windows Kits\10\References\Windows.Graphics.Printing3D.Printing3DContract\2.0.0.0\Windows.Graphics.Printing3D.Printing3DContract.winmd"" ",

@"/reference:""C:\Program Files (x86)\Windows Kits\10\References\Windows.Networking.Connectivity.WwanContract\1.0.0.0\Windows.Networking.Connectivity.WwanContract.winmd"" ",

@"/reference:""C:\Program Files (x86)\Windows Kits\10\\UnionMetadata\facade\Windows.winmd""",
@"""..\Common\Reddit.show""",
            };

            Args = Args.Select(s => s.Trim('\"'))
                       .Select(s => s.Replace("\"", ""))
                       .ToList();
        }
    }
}