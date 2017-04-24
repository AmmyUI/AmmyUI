using System.Linq;
using Nitra.Declarations;

namespace Ammy.VisualStudio.Service.Extensions
{
    public static class StackExtensions
    {
        public static bool IsInside<TAst>(this IAst[] stack) where TAst : IAst
        {
            return stack.Any(ast => ast is TAst);
        }
    }
}