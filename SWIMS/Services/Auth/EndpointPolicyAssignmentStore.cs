using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SWIMS.Data;
using SWIMS.Models.Security;

namespace SWIMS.Services.Auth
{
    public interface IEndpointPolicyAssignmentStore
    {
        // Check via HttpContext (runtime)
        Task<IReadOnlyList<string>> GetPolicyNamesForAsync(HttpContext http, CancellationToken ct = default);

        // Offline / preview overload (no HttpContext required, e.g. RouteInspector)
        Task<IReadOnlyList<string>> GetPolicyNamesForAsync(
            string? area,
            string? controller,
            string? action,
            string? page,
            string? path,
            CancellationToken ct = default);

        Task InvalidateAsync();
    }

    public class EfEndpointPolicyAssignmentStore : IEndpointPolicyAssignmentStore
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "endpoint:assignments";

        public EfEndpointPolicyAssignmentStore(SwimsIdentityDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        /// <summary>
        /// Entry point used at runtime by DbEndpointPolicyFilter.
        /// </summary>
        public async Task<IReadOnlyList<string>> GetPolicyNamesForAsync(
            HttpContext http,
            CancellationToken ct = default)
        {
            var (area, controller, action, page, path, fullPath) = GetRouteBits(http);
            return await GetPolicyNamesCoreAsync(area, controller, action, page, path, fullPath, ct);
        }

        /// <summary>
        /// Offline / preview entry point (e.g. RouteInspector) that only knows route bits.
        /// We treat the supplied path as both "path only" and "fullPath" for regex matching.
        /// </summary>
        public Task<IReadOnlyList<string>> GetPolicyNamesForAsync(
            string? area,
            string? controller,
            string? action,
            string? page,
            string? path,
            CancellationToken ct = default)
        {
            return GetPolicyNamesCoreAsync(area, controller, action, page, path, path, ct);
        }

        private async Task<IReadOnlyList<string>> GetPolicyNamesCoreAsync(
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
                return await _db.EndpointPolicyAssignments
                    .AsNoTracking()
                    .Where(x => x.IsEnabled)
                    .OrderBy(x => x.Priority)
                    .Select(x => new Slim(x))
                    .ToListAsync(ct);
            });

            var results = new List<string>();

            if (items is null || items.Count == 0)
                return results;

            foreach (var x in items)
            {
                if (!x.IsEnabled)
                    continue;

                switch (x.MatchType)
                {
                    case MatchTypes.ControllerAction:
                        if (AreaMatches(x.Area, area) &&
                            Eq(x.Controller, controller) &&
                            Eq(x.Action, action))
                        {
                            results.Add(x.PolicyName);
                        }
                        break;

                    case MatchTypes.Controller:
                        if (AreaMatches(x.Area, area) &&
                            Eq(x.Controller, controller))
                        {
                            results.Add(x.PolicyName);
                        }
                        break;

                    case MatchTypes.RazorPage:
                        if (AreaMatches(x.Area, area) &&
                            Eq(x.Page, page))
                        {
                            results.Add(x.PolicyName);
                        }
                        break;

                    case MatchTypes.Path:
                        // Path matching stays as "pure path" (no query string).
                        if (Eq(x.Path, path))
                        {
                            results.Add(x.PolicyName);
                        }
                        break;

                    case MatchTypes.Regex:
                        // Regex sees the combined path + query string so we can
                        // match on things like "count=1" anywhere in the URL.
                        var target = fullPath ?? path ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(x.Regex) &&
                            Regex.IsMatch(target, x.Regex, RegexOptions.IgnoreCase))
                        {
                            results.Add(x.PolicyName);
                        }
                        break;
                }
            }

            return results
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Area matching helper:
        /// - If the rule's Area is null/empty, treat it as a wildcard (matches any area).
        /// - Otherwise, require an exact (case-insensitive) match.
        /// </summary>
        private static bool AreaMatches(string? ruleArea, string? routeArea)
        {
            var rule = (ruleArea ?? string.Empty).Trim();
            var route = (routeArea ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(rule))
                return true; // wildcard

            return string.Equals(rule, route, StringComparison.OrdinalIgnoreCase);
        }

        private static bool Eq(string? a, string? b) =>
            string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.OrdinalIgnoreCase);

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
            string PolicyName,
            bool IsEnabled,
            int Priority)
        {
            public Slim(EndpointPolicyAssignment e)
                : this(
                    e.MatchType,
                    e.Area,
                    e.Controller,
                    e.Action,
                    e.Page,
                    e.Path,
                    e.Regex,
                    e.PolicyName,
                    e.IsEnabled,
                    e.Priority)
            {
            }
        }

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
