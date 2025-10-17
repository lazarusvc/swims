using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SWIMS.Services.Diagnostics
{
    public record ControllerActionInfo(string? Area, string Controller, string Action);
    public record RazorPageInfo(string? Area, string PageRoute);

    public interface IEndpointCatalog
    {
        IReadOnlyList<ControllerActionInfo> GetControllerActions();
        IReadOnlyList<(string? Area, string Controller)> GetControllers();
        IReadOnlyList<RazorPageInfo> GetRazorPages();
    }

    public class EndpointCatalog : IEndpointCatalog
    {
        private readonly IActionDescriptorCollectionProvider _provider;
        private IReadOnlyList<ControllerActionInfo>? _actions;
        private IReadOnlyList<(string? Area, string Controller)>? _controllers;
        private IReadOnlyList<RazorPageInfo>? _pages;

        public EndpointCatalog(IActionDescriptorCollectionProvider provider)
        {
            _provider = provider;
        }

        public IReadOnlyList<ControllerActionInfo> GetControllerActions()
        {
            if (_actions is not null) return _actions;
            _actions = _provider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .Select(ad =>
                {
                    ad.RouteValues.TryGetValue("area", out var area);
                    return new ControllerActionInfo(area, ad.ControllerName, ad.ActionName);
                })
                .OrderBy(x => x.Area).ThenBy(x => x.Controller).ThenBy(x => x.Action)
                .ToList();
            return _actions;
        }

        public IReadOnlyList<(string? Area, string Controller)> GetControllers()
        {
            if (_controllers is not null) return _controllers;
            _controllers = GetControllerActions()
                .Select(x => (x.Area, x.Controller))
                .Distinct()
                .OrderBy(x => x.Area).ThenBy(x => x.Controller)
                .ToList();
            return _controllers;
        }

        public IReadOnlyList<RazorPageInfo> GetRazorPages()
        {
            if (_pages is not null) return _pages;
            _pages = _provider.ActionDescriptors.Items
                .Where(a => a is PageActionDescriptor)
                .Cast<PageActionDescriptor>()
                .Select(p =>
                {
                    p.RouteValues.TryGetValue("area", out var area);
                    return new RazorPageInfo(area, p.ViewEnginePath); // like "/Privacy"
                })
                .OrderBy(x => x.Area).ThenBy(x => x.PageRoute)
                .ToList();
            return _pages;
        }
    }
}
