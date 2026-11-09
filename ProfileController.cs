using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

/// <summary>View and update the logged-in user's own profile (both roles).</summary>
[Route("api/profile")]
[SessionAuthorize]
public class ProfileController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;
    public ProfileController(RaceDayDbContext db) => _db = db;

    /// <summary>Get the logged-in user's profile.</summary>
    /// <response code="200">The profile.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get()
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        return user is null ? NotFound(new MessageResponse("User not found.")) : Ok(user.ToDto());
    }

    /// <summary>Update the logged-in user's profile details.</summary>
    /// <response code="200">Profile updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="401">Not logged in.</response>
    [HttpPut]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
    {
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        if (user is null) return NotFound(new MessageResponse("User not found."));

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber;
        await _db.SaveChangesAsync();

        return Ok(user.ToDto());
    }
}
