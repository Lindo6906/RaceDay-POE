using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels;

public class RegisterVm
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = "";
    [Required] public string Role { get; set; } = "Participant";
    [Phone, StringLength(20)] public string? PhoneNumber { get; set; }
    public string? OrganiserCode { get; set; }
}

public class LoginVm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

public class UpdateProfileVm
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Phone, StringLength(20)] public string? PhoneNumber { get; set; }
}

public class UserVm
{
    public int UserId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string Message { get; set; } = "";
}

public class EventVm
{
    public int EventId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = "";
    public int OrganiserId { get; set; }
    public string OrganiserName { get; set; } = "";
}

public class EventFormVm
{
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [Required, StringLength(2000)] public string Description { get; set; } = "";
    [Required] public DateTime? EventDate { get; set; } = DateTime.UtcNow.AddMonths(1);
    [Required, StringLength(200)] public string Location { get; set; } = "";
    [Range(0.1, 1000)] public decimal DistanceKm { get; set; } = 10;
    [Required] public string EventType { get; set; } = "Run";
}

public class CategoryVm
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = "";
}

public class EnrolmentVm
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime EnrolmentDate { get; set; }
}

public class ResultVm
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = "";
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string FinishTime { get; set; } = "";
    public int Position { get; set; }
    public string Status { get; set; } = "";
}

public class ResultFormVm
{
    [Required] public string FinishTime { get; set; } = "00:30:00";
    [Range(1, int.MaxValue)] public int Position { get; set; } = 1;
    public string Status { get; set; } = "Completed";
}