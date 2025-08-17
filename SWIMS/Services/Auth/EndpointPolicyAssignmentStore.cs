using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SWIMS.Data;
using SWIMS.Models.Security;

namespace SWIMS.Services.Auth
{
    public interface IEndpointPolicyAssignmentStore
    {
        // Existing: check via HttpContext
        Task<IReadOnlyList<string>> GetPolicyNamesForAsync(HttpContext http, CancellationToken ct = default);

        // NEW: offline/preview overload (no HttpContext required)
        Task<IReadOnlyList<string>> GetPolicyNamesForAsync(string? area, string? controller, string? action, string? page, string? path, CancellationToken ct = default);

        Task InvalidateAsync();
    }

    public class EfEndpointPolicyAssignmentStore : IEndpointPolicyAssignmentStore
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "endpoint:assignments";

        public EfEndpointPolicyAssignmentStore(SwimsIdentityDbContext db, IMemoryCache cache)
        { _db = db; _cache = cache; }

        // Existing entry point now delegates to the core overload
        public async Task<IReadOnlyList<string>> GetPolicyNamesForAsync(HttpContext http, CancellationToken ct = default)
        {
            var (area, controller, action, page, path) = GetRouteBits(http);
            return await GetPolicyNamesCoreAsync(area, controller, action, page, path, ct);
        }

        // NEW: offline/preview overload
        public Task<IReadOnlyList<string>> GetPolicyNamesForAsync(string? area, string? controller, string? action, string? page, string? path, CancellationToken ct = default)
            => GetPolicyNamesCoreAsync(area, controller, action, page, path, ct);

        private async Task<IReadOnlyList<string>> GetPolicyNamesCoreAsync(string? area, string? controller, string? action, string? page, string? path, CancellationToken ct)
        {
            var items = await _cache.GetOrCreateAsync(CacheKey, async e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return await _db.EndpointPolicyAssignments.AsNoTracking()
                    .Where(x => x.IsEnabled)
                    .OrderBy(x => x.Priority)
                    .Select(x => new Slim(x))
                    .ToListAsync(ct);
            });

            var result = new List<string>();
            foreach (var x in items!)
            {
                if (!x.IsEnabled) continue;
                switch (x.MatchType)
                {
                    case MatchTypes.ControllerAction:
                        if (Eq(x.Area, area) && Eq(x.Controller, controller) && Eq(x.Action, action)) result.Add(x.PolicyName);
                        break;
                    case MatchTypes.Controller:
                        if (Eq(x.Area, area) && Eq(x.Controller, controller)) result.Add(x.PolicyName);
                        break;
                    case MatchTypes.RazorPage:
                        if (Eq(x.Area, area) && Eq(x.Page, page)) result.Add(x.PolicyName);
                        break;
                    case MatchTypes.Path:
                        if (Eq(x.Path, path)) result.Add(x.PolicyName);
                        break;
                    case MatchTypes.Regex:
                        if (!string.IsNullOrWhiteSpace(x.Regex) &&
                            Regex.IsMatch(path ?? "", x.Regex, RegexOptions.IgnoreCase))
                            result.Add(x.PolicyName);
                        break;
                }
            }
            return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            static bool Eq(string? a, string? b) => string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);
        }

        public Task InvalidateAsync()
        {
            _cache.Remove(CacheKey);
            return Task.CompletedTask;
        }

        private record Slim(string MatchType, string? Area, string? Controller, string? Action, string? Page, string? Path, string? Regex, string PolicyName, bool IsEnabled, int Priority)
        {
            public Slim(EndpointPolicyAssignment e) : this(e.MatchType, e.Area, e.Controller, e.Action, e.Page, e.Path, e.Regex, e.PolicyName, e.IsEnabled, e.Priority) { }
        }

        private static (string? area, string? controller, string? action, string? page, string? path)
            GetRouteBits(HttpContext http)
        {
            var rd = http.GetRouteData()?.Values;
            var area = rd?["area"]?.ToString();
            var controller = rd?["controller"]?.ToString();
            var action = rd?["action"]?.ToString();
            var page = rd?["page"]?.ToString();
            var path = http.Request.Path.Value;
            return (area, controller, action, page, path);
        }
    }
}
