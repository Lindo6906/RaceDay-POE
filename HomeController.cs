using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApiService _api;
    public HomeController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        // Logged-in users get their profile; logged-out users just see the public event list.
        UserVm? profile = null;
        try { profile = await _api.GetProfileAsync(); } catch { /* anonymous is fine */ }

        // Anonymous users get 401 from the API. Show an empty list instead.
        List<EventVm> events = new();
        if (profile != null)
        {
            try { events = await _api.GetEventsAsync(); } catch { }
        }

        ViewBag.Profile = profile;
        return View(events);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}