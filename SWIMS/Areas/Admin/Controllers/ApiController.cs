// File: Areas/Admin/Controllers/ApiController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SWIMS.Areas.Admin.Controllers;

[Area("Admin")]
public class ApiController : Controller
{
    [HttpGet]
    public IActionResult Dashboard() => View();
}
