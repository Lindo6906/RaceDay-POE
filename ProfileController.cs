using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class ProfileController : Controller
{
    private readonly ApiService _api;
    public ProfileController(ApiService api) => _api = api;

    [WebSessionAuthorize]
    public async Task<IActionResult> Index()
    {
        var profile = await _api.GetProfileAsync();
        if (profile is null) return RedirectToAction("Login", "Account");
        return View(new UpdateProfileVm { FullName = profile.FullName, PhoneNumber = profile.PhoneNumber });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize]
    public async Task<IActionResult> Index(UpdateProfileVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var updated = await _api.UpdateProfileAsync(vm);
        HttpContext.Session.SetString("WebName", updated.FullName);
        TempData["Flash"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }
}