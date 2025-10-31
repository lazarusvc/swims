using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SWIMS.Services.Auth
{
    public class DbEndpointPolicyFilter : IAsyncAuthorizationFilter
    {
        private readonly IEndpointPolicyAssignmentStore _store;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly IAuthorizationService _authz;

        public DbEndpointPolicyFilter(
            IEndpointPolicyAssignmentStore store,
            IAuthorizationPolicyProvider policyProvider,
            IAuthorizationService authz)
        {
            _store = store;
            _policyProvider = policyProvider;
            _authz = authz;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;
            var policyNames = await _store.GetPolicyNamesForAsync(http);
            if (policyNames.Count == 0) return;

            var builder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
            foreach (var name in policyNames)
            {
                var policy = await _policyProvider.GetPolicyAsync(name);
                if (policy is not null) builder.Combine(policy);
            }

            var combined = builder.Build();
            var result = await _authz.AuthorizeAsync(http.User, resource: null, combined);
            if (!result.Succeeded)
            {
                // Challenge if unauthenticated, else forbid
                context.Result = (http.User?.Identity?.IsAuthenticated ?? false)
                    ? new ForbidResult()
                    : new ChallengeResult();
            }
        }
    }
}
