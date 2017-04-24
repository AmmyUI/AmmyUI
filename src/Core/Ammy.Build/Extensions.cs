using System.IO;

namespace Ammy.Build
{
    public static class Extensions
    {
        public static string ChangeExtension(this string filename, string newExtension)
        {
            return Path.ChangeExtension(filename, newExtension);
        } 
    }
}