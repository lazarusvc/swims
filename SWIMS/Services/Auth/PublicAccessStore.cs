using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SWIMS.Data;
using SWIMS.Models.Security;

namespace SWIMS.Services.Auth
{
    public interface IPublicAccessStore
    {
        Task<bool> IsPublicAsync(HttpContext http, CancellationToken ct = default);
        Task InvalidateAsync();
    }

    public class EfPublicAccessStore : IPublicAccessStore
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "public:endpoints";

        public EfPublicAccessStore(SwimsIdentityDbContext db, IMemoryCache cache)
        { _db = db; _cache = cache; }

        public async Task<bool> IsPublicAsync(HttpContext http, CancellationToken ct = default)
        {
            var items = await _cache.GetOrCreateAsync(CacheKey, async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return await _db.PublicEndpoints.AsNoTracking()
                    .Where(x => x.IsEnabled).OrderBy(x => x.Priority).ToListAsync(ct);
            });

            var (area, controller, action, page, path) = GetRouteBits(http);

            foreach (var x in items!)
            {
                switch (x.MatchType)
                {
                    case MatchTypes.ControllerAction:
                        if (Eq(x.Area, area) && Eq(x.Controller, controller) && Eq(x.Action, action)) return true;
                        break;
                    case MatchTypes.Controller:
                        if (Eq(x.Area, area) && Eq(x.Controller, controller)) return true;
                        break;
                    case MatchTypes.RazorPage:
                        if (Eq(x.Area, area) && Eq(x.Page, page)) return true;
                        break;
                    case MatchTypes.Path:
                        if (Eq(x.Path, path)) return true;
                        break;
                    case MatchTypes.Regex:
                        if (!string.IsNullOrWhiteSpace(x.Regex) && Regex.IsMatch(path ?? "", x.Regex, RegexOptions.IgnoreCase)) return true;
                        break;
                }
            }
            return false;

            static bool Eq(string? a, string? b)
                => string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);
        }

        public Task InvalidateAsync()
        {
            _cache.Remove(CacheKey);
            return Task.CompletedTask;
        }

        private static (string? area, string? controller, string? action, string? page, string? path)
            GetRouteBits(HttpContext http)
        {
            var rd = http.GetRouteData()?.Values;
            var area = rd?["area"]?.ToString();
            var controller = rd?["controller"]?.ToString();
            var action = rd?["action"]?.ToString();
            var page = rd?["page"]?.ToString(); // Razor Pages sets this
            var path = http.Request.Path.Value;
            return (area, controller, action, page, path);
        }
    }
}
