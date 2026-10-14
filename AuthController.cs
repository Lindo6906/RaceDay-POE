using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Security;

namespace RaceDay.Api.Controllers;

/// <summary>Registration, login and logout.</summary>
[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(RaceDayDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    /// <summary>Register a new account as an Organiser or a Participant.</summary>
    /// <remarks>
    /// Registering as an Organiser requires the secret <c>organiserCode</c> (configured in
    /// appsettings.json under Registration:OrganiserCode) so that anyone cannot simply make themselves an Organiser.
    /// </remarks>
    /// <response code="201">Account created.</response>
    /// <response code="400">Validation failed (missing fields, weak password, invalid role).</response>
    /// <response code="403">Organiser code missing or incorrect.</response>
    /// <response code="409">Email already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request.Role == UserRole.Organiser)
        {
            var required = _config["Registration:OrganiserCode"];
            if (!string.IsNullOrEmpty(required) && request.OrganiserCode != required)
                return Forbidden("A valid organiser code is required to register as an Organiser.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new MessageResponse("An account with this email already exists."));

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            RoleId = (int)request.Role,
            PhoneNumber = request.PhoneNumber
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        await _db.Entry(user).Reference(u => u.Role).LoadAsync();

        return Created("/api/profile", user.ToDto());
    }

    /// <summary>Log in with email and password. Starts a session that stores the user's id and role.</summary>
    /// <response code="200">Logged in; session cookie issued.</response>
    /// <response code="401">Invalid email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new MessageResponse("Invalid email or password."));

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role.RoleName);

        return Ok(new LoginResponse(user.UserId, user.FullName, user.Role.RoleName, "Login successful."));
    }

    /// <summary>Log out and end the current session.</summary>
    /// <response code="200">Session cleared.</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new MessageResponse("Logged out."));
    }
}
