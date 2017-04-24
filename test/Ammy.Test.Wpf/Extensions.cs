using System;

namespace Ammy.WpfTest
{
    public static class Extensions
    {
        public static void Assert(this object instance, bool result, string message = "Error")
        {
            if (!result)
                throw new Exception(message);
        }
    }
}