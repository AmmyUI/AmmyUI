using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ammy.VisualStudio.Service.Extensions
{
    public static class Extensions
    {
        public static string GetIndent(this string line)
        {
            return string.Join("", line.TakeWhile(c => c == ' ' || c == '\t'));
        }

        public static T Measure<T>(this object _, string title, Func<T> action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var result = action();

            stopwatch.Stop();

            Debug.WriteLine(title + ": " + stopwatch.ElapsedMilliseconds + "ms");

            return result;
        }

        public static string GetDirectoryPath(this Assembly assembly)
        {
            return Path.GetDirectoryName(new Uri(assembly.CodeBase).LocalPath);
        }
    }
}