using Microsoft.AspNetCore.Authorization;

namespace SWIMS.Services.Auth
{
    public class PublicOrAuthenticatedHandler : AuthorizationHandler<PublicOrAuthenticatedRequirement>
    {
        private readonly IPublicAccessStore _store;

        public PublicOrAuthenticatedHandler(IPublicAccessStore store)
        { _store = store; }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PublicOrAuthenticatedRequirement requirement)
        {
            // Try to get HttpContext from resource
            var http = context.Resource as HttpContext
                ?? (context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext)?.HttpContext;

            if (http is null)
            {
                // If we can't inspect, require auth
                if (context.User?.Identity?.IsAuthenticated == true)
                    context.Succeed(requirement);
                return;
            }

            var isPublic = await _store.IsPublicAsync(http);
            if (isPublic || (context.User?.Identity?.IsAuthenticated ?? false))
                context.Succeed(requirement);
        }
    }
}
