using System.Linq;
using System.Runtime.CompilerServices;

namespace EFSecondLevelCache.Core
{
    public static class FakeCacheableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IQueryable<T> Cacheable<T>(this IQueryable<T> q) => q;
    }
}
