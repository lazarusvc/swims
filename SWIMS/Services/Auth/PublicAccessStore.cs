using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SWIMS.Data;
using SWIMS.Models.Security;

namespace SWIMS.Services.Auth
{
    public interface IPublicAccessStore
    {
        // Runtime entry point (used by PublicOrAuthenticatedHandler)
        Task<bool> IsPublicAsync(HttpContext http, CancellationToken ct = default);

        // Offline / preview overload (used by RouteInspector)
        Task<bool> IsPublicAsync(
            string? area,
            string? controller,
            string? action,
            string? page,
            string? path,
            CancellationToken ct = default);

        Task InvalidateAsync();
    }

    public class EfPublicAccessStore : IPublicAccessStore
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "public:endpoints";

        public EfPublicAccessStore(SwimsIdentityDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        /// <summary>
        /// Entry point used at runtime by PublicOrAuthenticatedHandler.
        /// </summary>
        public async Task<bool> IsPublicAsync(HttpContext http, CancellationToken ct = default)
        {
            var (area, controller, action, page, path, fullPath) = GetRouteBits(http);
            return await IsPublicCoreAsync(area, controller, action, page, path, fullPath, ct);
        }

        /// <summary>
        /// Offline / preview entry point (e.g. RouteInspector) that only knows route bits.
        /// We treat the supplied path as both "path only" and "fullPath" for regex matching.
        /// </summary>
        public Task<bool> IsPublicAsync(
            string? area,
            string? controller,
            string? action,
            string? page,
            string? path,
            CancellationToken ct = default)
        {
            return IsPublicCoreAsync(area, controller, action, page, path, path, ct);
        }

        private async Task<bool> IsPublicCoreAsync(
            string? area,
            string? controller,
            string? action,
            string? page,
            string? path,
            string? fullPath,
            CancellationToken ct)
        {
            var items = await _cache.GetOrCreateAsync(CacheKey, async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return await _db.PublicEndpoints
                    .AsNoTracking()
                    .Where(x => x.IsEnabled)
                    .OrderBy(x => x.Priority)
                    .Select(x => new Slim(x))
                    .ToListAsync(ct);
            });

            if (items is null || items.Count == 0)
                return false;

            foreach (var x in items)
            {
                if (!x.IsEnabled)
                    continue;

                switch (x.MatchType)
                {
                    case MatchTypes.ControllerAction:
                        if (Eq(x.Area, area) &&
                            Eq(x.Controller, controller) &&
                            Eq(x.Action, action))
                        {
                            return true;
                        }
                        break;

                    case MatchTypes.Controller:
                        if (Eq(x.Area, area) &&
                            Eq(x.Controller, controller))
                        {
                            return true;
                        }
                        break;

                    case MatchTypes.RazorPage:
                        if (Eq(x.Area, area) &&
                            Eq(x.Page, page))
                        {
                            return true;
                        }
                        break;

                    case MatchTypes.Path:
                        // Path matching stays as "pure path".
                        if (Eq(x.Path, path))
                        {
                            return true;
                        }
                        break;

                    case MatchTypes.Regex:
                        // Regex sees the combined path + query string so we can
                        // match on things like "publicPreview=true" anywhere.
                        var target = fullPath ?? path ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(x.Regex) &&
                            Regex.IsMatch(target, x.Regex, RegexOptions.IgnoreCase))
                        {
                            return true;
                        }
                        break;
                }
            }

            return false;
        }

        public Task InvalidateAsync()
        {
            _cache.Remove(CacheKey);
            return Task.CompletedTask;
        }

        private record Slim(
            string MatchType,
            string? Area,
            string? Controller,
            string? Action,
            string? Page,
            string? Path,
            string? Regex,
            bool IsEnabled,
            int Priority)
        {
            public Slim(PublicEndpoint e)
                : this(
                    e.MatchType,
                    e.Area,
                    e.Controller,
                    e.Action,
                    e.Page,
                    e.Path,
                    e.Regex,
                    e.IsEnabled,
                    e.Priority)
            {
            }
        }

        private static bool Eq(string? a, string? b) =>
            string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        private static (string? area, string? controller, string? action, string? page, string? path, string? fullPath)
            GetRouteBits(HttpContext http)
        {
            var rd = http.GetRouteData()?.Values;

            var area = rd?["area"]?.ToString();
            var controller = rd?["controller"]?.ToString();
            var action = rd?["action"]?.ToString();
            var page = rd?["page"]?.ToString();

            var pathOnly = http.Request.Path.Value ?? string.Empty;
            var query = http.Request.QueryString.HasValue
                ? http.Request.QueryString.Value!
                : string.Empty;

            var fullPath = pathOnly + query;

            return (area, controller, action, page, pathOnly, fullPath);
        }
    }
}
