// File: Areas/Admin/Controllers/ApiController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWIMS.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize] // tighten with roles as needed
public class ApiController : Controller
{
    [HttpGet]
    public IActionResult Dashboard() => View();
}
