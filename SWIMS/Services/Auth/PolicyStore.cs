using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SWIMS.Data;

namespace SWIMS.Services.Auth
{
    public interface IPolicyStore
    {
        Task<PolicySpec?> GetAsync(string name, CancellationToken ct = default);
        Task InvalidateAsync(string name);
        Task InvalidateAllAsync();
    }

    public sealed record PolicySpec(
        string Name,
        IReadOnlyList<string> RequiredRoleNames,
        IReadOnlyList<(string type, string? value)> RequiredClaims,
        bool IsEnabled);

    public class EfPolicyStore : IPolicyStore
    {
        private readonly SwimsIdentityDbContext _db;
        private readonly IMemoryCache _cache;
        private const string CachePrefix = "policy:";

        public EfPolicyStore(SwimsIdentityDbContext db, IMemoryCache cache)
        { _db = db; _cache = cache; }

        public async Task<PolicySpec?> GetAsync(string name, CancellationToken ct = default)
        {
            if (_cache.TryGetValue(CachePrefix + name, out PolicySpec spec))
                return spec;

            var entity = await _db.AuthorizationPolicies
                .AsNoTracking()
                .Include(p => p.Roles)
                .Include(p => p.Claims)
                .FirstOrDefaultAsync(p => p.Name == name && p.IsEnabled, ct);

            spec = entity is null
                ? null
                : new PolicySpec(
                    entity.Name,
                    entity.Roles.Select(r => r.RoleName).ToList(),
                    entity.Claims.Select(c => (c.Type, c.Value)).ToList(),
                    entity.IsEnabled);

            _cache.Set(CachePrefix + name, spec, TimeSpan.FromMinutes(1));
            return spec;
        }

        public Task InvalidateAsync(string name)
        { _cache.Remove(CachePrefix + name); return Task.CompletedTask; }

        public Task InvalidateAllAsync()
        {
            if (_cache is MemoryCache mc) mc.Compact(1.0); // Compact is on MemoryCache, not IMemoryCache
            return Task.CompletedTask;
        }
    }
}
