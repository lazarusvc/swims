using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWIMS.Data.Reports;
using SWIMS.Services.Reporting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWIMS.Controllers
{
    [Authorize(Policy = "ReportsView")]
    public class ReportsController : Controller
    {
        private readonly SwimsReportsDbContext _db;
        private readonly ISsrsUrlBuilder _url;

        public ReportsController(SwimsReportsDbContext db, ISsrsUrlBuilder url)
        { _db = db; _url = url; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roleIds = User.Claims.Where(c => c.Type == ClaimTypes.Role)
                                     .Select(c => c.Value).ToList();

            var reports = await _db.SwReports
                .Where(r => roleIds.Contains(r.RoleId))
                .OrderBy(r => r.Desc ?? r.Name)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> Run(int id)
        {
            var rpt = await _db.SwReports.Include(r => r.Params).FirstOrDefaultAsync(r => r.Id == id);
            if (rpt == null) return NotFound();

            var parms = rpt.ParamCheck
                ? rpt.Params.Select(p => new KeyValuePair<string, string>(p.ParamKey.Trim(), p.ParamValue.Trim()))
                : Enumerable.Empty<KeyValuePair<string, string>>();

            var url = _url.BuildUrl(rpt.Name, parms, rpt.PathOverride);
            return View("Viewer", model: url);
        }
    }
}
