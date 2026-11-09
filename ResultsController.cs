using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class ResultsController : Controller
{
    private readonly ApiService _api;
    public ResultsController(ApiService api) => _api = api;

    [WebSessionAuthorize("Organiser")]
    public IActionResult Capture(int enrolmentId) =>
        View(new ResultFormVm());

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> Capture(int enrolmentId, ResultFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var r = await _api.CaptureResultAsync(enrolmentId, vm);
        TempData["Flash"] = "Result captured.";
        return RedirectToAction("Details", "Events", new { id = r.EventId });
    }

    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> Edit(int id, int eventId)
    {
        var results = await _api.GetEventResultsAsync(eventId);
        var r = results.FirstOrDefault(x => x.ResultId == id);
        if (r is null) return NotFound();
        ViewBag.EventId = eventId;
        return View(new ResultFormVm
        {
            FinishTime = r.FinishTime,
            Position = r.Position,
            Status = r.Status
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> Edit(int id, int eventId, ResultFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await _api.UpdateResultAsync(id, vm);
        TempData["Flash"] = "Result updated.";
        return RedirectToAction(nameof(ForEvent), new { eventId });
    }

    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var results = await _api.GetEventResultsAsync(eventId);
        ViewBag.EventId = eventId;
        return View(results);
    }

    [WebSessionAuthorize("Participant")]
    public async Task<IActionResult> Mine()
    {
        var list = await _api.GetMyResultsAsync();
        return View(list);
    }
}