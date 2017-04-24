using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ammy.Platforms
{
    public interface IAmmyPlatform
    {
        string[] DefaultNamespaces { get; }
        bool SupportsRuntimeUpdate { get; }
        PlatformTypeNames PlatformTypeNames { get; }
        KeyValuePair<string, string>[] TopNodeAttributes { get; }
        string[] StaticPropertyImportList { get; }
        string Name { get; }
        string OutputFileSuffix { get; }
        string XPrefix { get; }

        Type[] ProvideTypes();
    }
}
