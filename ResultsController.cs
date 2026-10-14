using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

/// <summary>Capturing finish times / positions (Organiser) and viewing personal results (Participant).</summary>
[Route("api")]
[SessionAuthorize]
public class ResultsController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;
    public ResultsController(RaceDayDbContext db) => _db = db;

    private IQueryable<Result> Full() =>
        _db.Results.Include(r => r.Enrolment).ThenInclude(e => e.Category).ThenInclude(c => c.Event)
            .Include(r => r.Enrolment).ThenInclude(e => e.Participant);

    /// <summary>Capture the finish time and position for an enrolment (Organiser only).</summary>
    /// <response code="201">Result captured.</response>
    /// <response code="400">Validation failed (e.g. finish time not positive).</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Enrolment does not exist.</response>
    /// <response code="409">A result already exists for this enrolment, or the position is taken in this event.</response>
    [HttpPost("enrolments/{enrolmentId:int}/result")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Capture(int enrolmentId, [FromBody] ResultRequest request)
    {
        if (request.FinishTime!.Value <= TimeSpan.Zero)
            return BadRequest(new MessageResponse("Finish time must be greater than zero."));

        var enrolment = await _db.Enrolments.Include(e => e.Category).ThenInclude(c => c.Event).FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);
        if (enrolment is null) return NotFound(new MessageResponse("Enrolment not found."));
        if (enrolment.Category.Event.OrganiserId != CurrentUserId) return Forbidden("You can only capture results for your own events.");

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == enrolmentId))
            return Conflict(new MessageResponse("A result has already been captured for this enrolment."));
        if (await _db.Results.AnyAsync(r => r.Enrolment.Category.EventId == enrolment.Category.EventId && r.Position == request.Position))
            return Conflict(new MessageResponse("That finishing position is already taken in this event."));

        var result = new Result { EnrolmentId = enrolmentId, FinishTime = request.FinishTime.Value, Position = request.Position, Status = request.Status };
        enrolment.Status = EnrolmentStatus.Completed;
        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var dto = (await Full().AsNoTracking().FirstAsync(r => r.ResultId == result.ResultId)).ToDto();
        return Created($"/api/events/{enrolment.Category.EventId}/results", dto);
    }

    /// <summary>Correct an existing result (Organiser only).</summary>
    /// <response code="200">Result updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Result does not exist.</response>
    /// <response code="409">The position is taken by another participant in this event.</response>
    [HttpPut("results/{id:int}")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(ResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] ResultRequest request)
    {
        if (request.FinishTime!.Value <= TimeSpan.Zero)
            return BadRequest(new MessageResponse("Finish time must be greater than zero."));

        var result = await Full().FirstOrDefaultAsync(r => r.ResultId == id);
        if (result is null) return NotFound(new MessageResponse("Result not found."));
        if (result.Enrolment.Category.Event.OrganiserId != CurrentUserId) return Forbidden("You can only modify results for your own events.");

        var eventId = result.Enrolment.Category.EventId;
        if (await _db.Results.AnyAsync(r => r.ResultId != id && r.Enrolment.Category.EventId == eventId && r.Position == request.Position))
            return Conflict(new MessageResponse("That finishing position is already taken in this event."));

        result.FinishTime = request.FinishTime.Value;
        result.Position = request.Position;
        result.Status = request.Status;
        await _db.SaveChangesAsync();
        return Ok(result.ToDto());
    }

    /// <summary>List all results for one of your events, ordered by position (Organiser only).</summary>
    /// <response code="200">The results.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not an Organiser, or the event belongs to another Organiser.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("events/{eventId:int}/results")]
    [SessionAuthorize(UserRole.Organiser)]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await _db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == eventId);
        if (ev is null) return NotFound(new MessageResponse("Event not found."));
        if (ev.OrganiserId != CurrentUserId) return Forbidden("You can only view results for your own events.");

        var list = await Full().AsNoTracking().Where(r => r.Enrolment.Category.EventId == eventId).OrderBy(r => r.Position).ToListAsync();
        return Ok(list.Select(r => r.ToDto()));
    }

    /// <summary>List the logged-in Participant's own results.</summary>
    /// <response code="200">The results.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Caller is not a Participant.</response>
    [HttpGet("results/my")]
    [SessionAuthorize(UserRole.Participant)]
    [ProducesResponseType(typeof(IEnumerable<ResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Mine()
    {
        var list = await Full().AsNoTracking().Where(r => r.Enrolment.ParticipantId == CurrentUserId)
            .OrderBy(r => r.Enrolment.Category.Event.EventDate).ToListAsync();
        return Ok(list.Select(r => r.ToDto()));
    }
}
