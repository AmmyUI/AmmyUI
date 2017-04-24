using System;
using System.Collections.Generic;
using System.Linq;

namespace AmmySidekick
{
    public static class NumericTypes
    {
        private static readonly HashSet<Type> Types = new HashSet<Type> {
            typeof(int),
            typeof(double),
            typeof(decimal),
            typeof(long),
            typeof(short),
            typeof(sbyte),
            typeof(byte),
            typeof(ulong),
            typeof(ushort),
            typeof(uint),
            typeof(float)
        };

        private static string[] _typeNameCache;

        public static string[] GetTypeNames()
        {
            if (_typeNameCache == null)
                _typeNameCache = Types.Select(t => t.FullName)
                                      .ToArray();

            return _typeNameCache;
        }

        public static bool TypeIsNumeric(Type type)
        {
            return Types.Contains(Nullable.GetUnderlyingType(type) ?? type);
        }

        public static bool TypeIsNumeric(string fullname)
        {
            return Types.Any(t => t.FullName == fullname);
        }
    }
}