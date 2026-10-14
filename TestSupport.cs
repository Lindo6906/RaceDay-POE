using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

/// <summary>Boots the real API in memory, swapping SQL Server for the EF Core in-memory provider.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string OrganiserCode = "TEST-ORG-CODE";
    public const string Password = "Passw0rd!123";
    private readonly string _dbName = $"RaceDayTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Registration:OrganiserCode", OrganiserCode);

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>));
            if (descriptor is not null) services.Remove(descriptor);
            services.AddDbContext<RaceDayDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}

/// <summary>A logged-in HTTP client (it keeps its own session cookie) plus the user's id.</summary>
public record TestUser(HttpClient Client, int UserId, string Email);

public abstract class ApiTestBase : IClassFixture<ApiFactory>
{
    protected readonly ApiFactory Factory;
    protected ApiTestBase(ApiFactory factory) => Factory = factory;

    protected static string UniqueEmail() => $"user-{Guid.NewGuid():N}@raceday.test";

    protected object RegisterBody(string role, string? email = null) => new
    {
        fullName = $"Test {role}",
        email = email ?? UniqueEmail(),
        password = ApiFactory.Password,
        role,
        organiserCode = role == "Organiser" ? ApiFactory.OrganiserCode : null
    };

    /// <summary>Registers and logs in a new user; the returned client carries the session cookie.</summary>
    protected async Task<TestUser> LoginAs(string role)
    {
        var client = Factory.CreateClient();
        var email = UniqueEmail();

        var reg = await client.PostAsJsonAsync("/api/auth/register", RegisterBody(role, email));
        reg.EnsureSuccessStatusCode();
        var userId = (await reg.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("userId").GetInt32();

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = ApiFactory.Password });
        login.EnsureSuccessStatusCode();
        return new TestUser(client, userId, email);
    }

    protected static object EventBody(string name = "Soweto 10K", string type = "Run", decimal km = 10) => new
    {
        name,
        description = "A friendly community race.",
        eventDate = DateTime.UtcNow.AddMonths(2),
        location = "Soweto, Johannesburg",
        distanceKm = km,
        eventType = type
    };

    protected static async Task<int> IdOf(HttpResponseMessage response, string property)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty(property).GetInt32();
    }

    protected static async Task<int> CreateEvent(HttpClient organiser, string name = "Soweto 10K")
    {
        var res = await organiser.PostAsJsonAsync("/api/events", EventBody(name));
        res.EnsureSuccessStatusCode();
        return await IdOf(res, "eventId");
    }

    protected static async Task<int> CreateCategory(HttpClient organiser, int eventId, string name = "Senior")
    {
        var res = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories", new { name });
        res.EnsureSuccessStatusCode();
        return await IdOf(res, "categoryId");
    }
}
