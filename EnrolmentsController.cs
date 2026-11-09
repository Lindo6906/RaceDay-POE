using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class EnrolmentsController : Controller
{
    private readonly ApiService _api;
    public EnrolmentsController(ApiService api) => _api = api;

    [WebSessionAuthorize("Participant")]
    public async Task<IActionResult> Index()
    {
        var list = await _api.GetMyEnrolmentsAsync();
        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Participant")]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        await _api.EnrolAsync(eventId, categoryId);
        TempData["Flash"] = "You are enrolled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Participant")]
    public async Task<IActionResult> Withdraw(int id)
    {
        await _api.WithdrawAsync(id);
        TempData["Flash"] = "Enrolment withdrawn.";
        return RedirectToAction(nameof(Index));
    }

    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> AllMine()
    {
        var list = await _api.GetAllMyEventEnrolmentsAsync();
        return View(list);
    }
}