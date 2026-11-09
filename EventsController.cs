using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

/// <summary>Browse events (both roles) and manage them (Organiser only).</summary>
[Route("api/events")]
[SessionAuthorize]
public class EventsController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;
    public EventsController(RaceDayDbContext db) => _db = db;

    /// <summary>List all events, optionally filtered by type (Run, Walk, Cycle).</summary>
    /// <response code="200">List of events ordered by date.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] EventType? type)
    {
        var query = _db.Events.Include(e => e.Organiser).AsNoTracking();
        if (type.HasValue) query = query.Where(e => e.EventType == type.Value);

        var events = await query.OrderBy(e => e.EventDate).ToListAsync();
        return Ok(events.Select(e => e.ToDto()));
    }

    /// <summary>Get a single event by id.</summary>
    /// <response code="200">The event.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events.Include(e => e.Organiser).AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
        return ev is null ? NotFound(new MessageResponse("Event not found.")) : Ok(ev.ToDto());
    }

    /// <summary>Create a new event (Organiser only).</summary>
    /// <response code="201">Event created.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser.</response>
    [HttpPost]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] EventRequest request)
    {
        var ev = new Event { OrganiserId = CurrentUserId };
        Apply(ev, request);
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        await _db.Entry(ev).Reference(e => e.Organiser).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, ev.ToDto());
    }

    /// <summary>Update one of your own events (Organiser only).</summary>
    /// <response code="200">Event updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpPut("{id:int}")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] EventRequest request)
    {
        var ev = await _db.Events.Include(e => e.Organiser).FirstOrDefaultAsync(e => e.EventId == id);
        if (ev is null) return NotFound(new MessageResponse("Event not found."));
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only modify your own events.");

        Apply(ev, request);
        await _db.SaveChangesAsync();
        return Ok(ev.ToDto());
    }

    /// <summary>Delete one of your own events, including its categories and enrolments (Organiser only).</summary>
    /// <response code="204">Event deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpDelete("{id:int}")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return NotFound(new MessageResponse("Event not found."));
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only delete your own events.");

        // Enrolments reference categories with Restrict, so remove them (and their results, via cascade)
        // and the categories explicitly before the event itself.
        var enrolments = await _db.Enrolments.Where(e => e.Category.EventId == id).ToListAsync();
        var categories = await _db.Categories.Where(c => c.EventId == id).ToListAsync();
        _db.Enrolments.RemoveRange(enrolments);
        _db.Categories.RemoveRange(categories);
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static void Apply(Event ev, EventRequest r)
    {
        ev.Name = r.Name.Trim();
        ev.Description = r.Description.Trim();
        ev.EventDate = r.EventDate!.Value;
        ev.Location = r.Location.Trim();
        ev.DistanceKm = r.DistanceKm;
        ev.EventType = r.EventType;
    }
}
