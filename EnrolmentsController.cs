using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

/// <summary>Entering events (Participant) and viewing enrolments (Participant: own, Organiser: own events).</summary>
[Route("api")]
[SessionAuthorize]
public class EnrolmentsController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;
    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    private IQueryable<Enrolment> Full() =>
        _db.Enrolments.Include(e => e.Category).ThenInclude(c => c.Event).Include(e => e.Participant).AsNoTracking();

    /// <summary>Enter an event by selecting one of its categories (Participant only).</summary>
    /// <response code="201">Enrolment recorded.</response>
    /// <response code="400">Category does not belong to this event.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not a Participant.</response>
    /// <response code="404">Event does not exist.</response>
    /// <response code="409">Already enrolled in this event.</response>
    [HttpPost("events/{eventId:int}/enrolments")]
    [SessionAuthorize(UserRole.Participant)]
    [ProducesResponseType(typeof(EnrolmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolRequest request)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
            return NotFound(new MessageResponse("Event not found."));

        if (!await _db.Categories.AnyAsync(c => c.CategoryId == request.CategoryId && c.EventId == eventId))
            return BadRequest(new MessageResponse("The selected category does not belong to this event."));

        // One entry per event, even though the table only enforces one per category.
        if (await _db.Enrolments.AnyAsync(e => e.ParticipantId == CurrentUserId && e.Category.EventId == eventId))
            return Conflict(new MessageResponse("You are already enrolled in this event."));

        var enrolment = new Enrolment { ParticipantId = CurrentUserId, CategoryId = request.CategoryId };
        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var dto = (await Full().FirstAsync(e => e.EnrolmentId == enrolment.EnrolmentId)).ToDto();
        return Created("/api/enrolments/my", dto);
    }

    /// <summary>List the logged-in Participant's own enrolments.</summary>
    /// <response code="200">The enrolments.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not a Participant.</response>
    [HttpGet("enrolments/my")]
    [SessionAuthorize(UserRole.Participant)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Mine()
    {
        var list = await Full().Where(e => e.ParticipantId == CurrentUserId)
            .OrderBy(e => e.Category.Event.EventDate).ToListAsync();
        return Ok(list.Select(e => e.ToDto()));
    }

    /// <summary>Withdraw from an event you have entered (Participant only).</summary>
    /// <response code="204">Enrolment removed.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not a Participant, or the enrolment belongs to someone else.</response>
    /// <response code="404">Enrolment does not exist.</response>
    [HttpDelete("enrolments/{id:int}")]
    [SessionAuthorize(UserRole.Participant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw(int id)
    {
        var enrolment = await _db.Enrolments.FindAsync(id);
        if (enrolment is null) return NotFound(new MessageResponse("Enrolment not found."));
        if (enrolment.ParticipantId != CurrentUserId) return Forbidden("You can only withdraw your own enrolments.");

        _db.Enrolments.Remove(enrolment);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>List all enrolments for one of your events (Organiser only).</summary>
    /// <response code="200">The enrolments.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("events/{eventId:int}/enrolments")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return NotFound(new MessageResponse("Event not found."));
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only view enrolments for your own events.");

        var list = await Full().Where(e => e.Category.EventId == eventId).OrderBy(e => e.EnrolmentDate).ToListAsync();
        return Ok(list.Select(e => e.ToDto()));
    }

    /// <summary>List enrolments across all of your events (Organiser only).</summary>
    /// <response code="200">The enrolments.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser.</response>
    [HttpGet("enrolments")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<EnrolmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AllForMyEvents()
    {
        var list = await Full().Where(e => e.Category.Event.OrganiserId == CurrentUserId)
            .OrderBy(e => e.Category.EventId).ThenBy(e => e.EnrolmentDate).ToListAsync();
        return Ok(list.Select(e => e.ToDto()));
    }
}
