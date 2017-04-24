using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Ammy.Platforms;

namespace Ammy.Test.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = Path.Combine(Environment.CurrentDirectory, "test.ammy");
            var files = new Source[] {new FileSource(filename)};
            var a = new DependencyPropertyConverter();

            var host = new Ammy.Host(new WpfPlatform());
            var result = host.Compile(new CompilationRequest(files));
        }
    }
}
