using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

/// <summary>Age or distance categories for an event (e.g. Under 20, Senior, 10km, 21km).</summary>
[Route("api")]
[SessionAuthorize]
public class CategoriesController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;
    public CategoriesController(RaceDayDbContext db) => _db = db;

    /// <summary>List the categories available for an event.</summary>
    /// <response code="200">The categories.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("events/{eventId:int}/categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForEvent(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
            return NotFound(new MessageResponse("Event not found."));

        var cats = await _db.Categories.AsNoTracking().Where(c => c.EventId == eventId).OrderBy(c => c.Name).ToListAsync();
        return Ok(cats.Select(c => c.ToDto()));
    }

    /// <summary>Add a category to one of your events (Organiser only).</summary>
    /// <response code="201">Category created.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Event does not exist.</response>
    /// <response code="409">A category with that name already exists for the event.</response>
    [HttpPost("events/{eventId:int}/categories")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(int eventId, [FromBody] CategoryRequest request)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new MessageResponse("Event not found."));
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only add categories to your own events.");

        var name = request.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.EventId == eventId && c.Name == name))
            return Conflict(new MessageResponse("This event already has a category with that name."));

        var cat = new Category { EventId = eventId, Name = name };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return Created($"/api/events/{eventId}/categories", cat.ToDto());
    }

    /// <summary>Update a category (Organiser only, own events).</summary>
    /// <response code="200">Category updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Category does not exist.</response>
    [HttpPut("categories/{id:int}")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
    {
        var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat is null) return NotFound(new MessageResponse("Category not found."));
        if (cat.Event.OrganiserId != CurrentUserId) return Forbidden("You can only modify categories of your own events.");

        cat.Name = request.Name.Trim();
        await _db.SaveChangesAsync();
        return Ok(cat.ToDto());
    }

    /// <summary>Delete a category that has no enrolments (Organiser only, own events).</summary>
    /// <response code="204">Category deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Category does not exist.</response>
    /// <response code="409">Participants are already enrolled in this category.</response>
    [HttpDelete("categories/{id:int}")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (cat is null) return NotFound(new MessageResponse("Category not found."));
        if (cat.Event.OrganiserId != CurrentUserId) return Forbidden("You can only delete categories of your own events.");
        if (await _db.Enrolments.AnyAsync(e => e.CategoryId == id))
            return Conflict(new MessageResponse("Participants are enrolled in this category and it cannot be deleted."));

        _db.Categories.Remove(cat);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
