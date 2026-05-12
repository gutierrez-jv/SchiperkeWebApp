using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchiperkeWebApp.Models;

namespace SchiperkeWebApp.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Status(int id)
    {
        var statusCode = id;
        var statusCodeFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        var originalPath = statusCodeFeature is null
            ? string.Empty
            : $"{statusCodeFeature.OriginalPath}{statusCodeFeature.OriginalQueryString}";

        ViewData["StatusCode"] = statusCode;
        ViewData["OriginalPath"] = originalPath;
        Response.StatusCode = statusCode;

        return View("StatusCode");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
