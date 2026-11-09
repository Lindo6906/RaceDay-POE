using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly ApiService _api;
    public EventsController(ApiService api) => _api = api;

    [WebSessionAuthorize]
    public async Task<IActionResult> Index(string? type)
    {
        var events = await _api.GetEventsAsync(type);
        ViewBag.Type = type;
        return View(events);
    }

    [WebSessionAuthorize]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev is null) return NotFound();

        ViewBag.Categories = await _api.GetCategoriesAsync(id);
        ViewBag.IsOwner = HttpContext.Session.GetInt32("WebUserId") == ev.OrganiserId;
        ViewBag.Role = HttpContext.Session.GetString("WebRole");

        // Organiser's own view also lists enrolments so they can capture results.
        if (ViewBag.IsOwner == true)
            ViewBag.Enrolments = await _api.GetEventEnrolmentsAsync(id);

        return View(ev);
    }

    [WebSessionAuthorize("Organiser")]
    public IActionResult Create() => View(new EventFormVm());

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> Create(EventFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var created = await _api.CreateEventAsync(vm);
        TempData["Flash"] = $"Event '{created.Name}' created.";
        return RedirectToAction(nameof(Details), new { id = created.EventId });
    }

    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> Edit(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev is null) return NotFound();
        return View(new EventFormVm
        {
            Name = ev.Name,
            Description = ev.Description,
            EventDate = ev.EventDate,
            Location = ev.Location,
            DistanceKm = ev.DistanceKm,
            EventType = ev.EventType
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> Edit(int id, EventFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        await _api.UpdateEventAsync(id, vm);
        TempData["Flash"] = "Event updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        await _api.DeleteEventAsync(id);
        TempData["Flash"] = "Event deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ----- Categories on the details page -----

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> AddCategory(int id, string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            await _api.CreateCategoryAsync(id, name.Trim());
            TempData["Flash"] = "Category added.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [WebSessionAuthorize("Organiser")]
    public async Task<IActionResult> DeleteCategory(int id, int categoryId)
    {
        await _api.DeleteCategoryAsync(categoryId);
        TempData["Flash"] = "Category deleted.";
        return RedirectToAction(nameof(Details), new { id });
    }
}