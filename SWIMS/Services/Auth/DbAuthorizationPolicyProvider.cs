using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SWIMS.Services.Auth
{
    public class DbAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private readonly DefaultAuthorizationPolicyProvider _fallback;
        private readonly IServiceScopeFactory _scopeFactory;

        public DbAuthorizationPolicyProvider(
            IOptions<AuthorizationOptions> options,
            IServiceScopeFactory scopeFactory)
        {
            _fallback = new DefaultAuthorizationPolicyProvider(options);
            _scopeFactory = scopeFactory;
        }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
        public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

        public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<IPolicyStore>();

            var spec = await store.GetAsync(policyName);
            if (spec is not null && spec.IsEnabled)
            {
                var b = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();

                if (spec.RequiredRoleNames.Count > 0)
                    b.RequireRole(spec.RequiredRoleNames);

                foreach (var (type, value) in spec.RequiredClaims)
                {
                    if (value is null) b.RequireClaim(type);
                    else b.RequireClaim(type, value);
                }

                return b.Build();
            }

            return await _fallback.GetPolicyAsync(policyName);
        }
    }
}
