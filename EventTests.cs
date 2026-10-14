using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public class EventTests : ApiTestBase
{
    public EventTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Organiser_CanCreateEvent_Returns201()
    {
        var org = await LoginAs("Organiser");
        var res = await org.Client.PostAsJsonAsync("/api/events", EventBody());
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent_Returns403()
    {
        var p = await LoginAs("Participant");
        var res = await p.Client.PostAsJsonAsync("/api/events", EventBody());
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Anonymous_CannotCreateEvent_Returns401()
    {
        var res = await Factory.CreateClient().PostAsJsonAsync("/api/events", EventBody());
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidDistance_Returns400()
    {
        var org = await LoginAs("Organiser");
        var res = await org.Client.PostAsJsonAsync("/api/events", EventBody(km: 0));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidType_Returns400()
    {
        var org = await LoginAs("Organiser");
        var res = await org.Client.PostAsJsonAsync("/api/events", EventBody(type: "Swim"));
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task BothRoles_CanViewEvents()
    {
        var org = await LoginAs("Organiser");
        var id = await CreateEvent(org.Client, "Visible Event");
        var p = await LoginAs("Participant");

        Assert.Equal(HttpStatusCode.OK, (await org.Client.GetAsync("/api/events")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await p.Client.GetAsync("/api/events")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await p.Client.GetAsync($"/api/events/{id}")).StatusCode);
    }

    [Fact]
    public async Task GetEvent_ThatDoesNotExist_Returns404()
    {
        var p = await LoginAs("Participant");
        Assert.Equal(HttpStatusCode.NotFound, (await p.Client.GetAsync("/api/events/999999")).StatusCode);
    }

    [Fact]
    public async Task Organiser_CanUpdateOwnEvent()
    {
        var org = await LoginAs("Organiser");
        var id = await CreateEvent(org.Client);

        var res = await org.Client.PutAsJsonAsync($"/api/events/{id}", EventBody("Renamed Event", "Cycle", 40));
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var ev = await org.Client.GetFromJsonAsync<System.Text.Json.JsonElement>($"/api/events/{id}");
        Assert.Equal("Renamed Event", ev.GetProperty("name").GetString());
        Assert.Equal("Cycle", ev.GetProperty("eventType").GetString());
    }

    [Fact]
    public async Task Organiser_CannotUpdateAnotherOrganisersEvent_Returns403()
    {
        var owner = await LoginAs("Organiser");
        var other = await LoginAs("Organiser");
        var id = await CreateEvent(owner.Client);

        var res = await other.Client.PutAsJsonAsync($"/api/events/{id}", EventBody("Hijacked"));
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotUpdateOrDeleteEvent_Returns403()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var id = await CreateEvent(org.Client);

        Assert.Equal(HttpStatusCode.Forbidden, (await p.Client.PutAsJsonAsync($"/api/events/{id}", EventBody("X"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await p.Client.DeleteAsync($"/api/events/{id}")).StatusCode);
    }

    [Fact]
    public async Task Organiser_CanDeleteOwnEvent()
    {
        var org = await LoginAs("Organiser");
        var id = await CreateEvent(org.Client);

        Assert.Equal(HttpStatusCode.NoContent, (await org.Client.DeleteAsync($"/api/events/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await org.Client.GetAsync($"/api/events/{id}")).StatusCode);
    }

    [Fact]
    public async Task Organiser_CanManageCategories_ParticipantCannot()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var eventId = await CreateEvent(org.Client);

        Assert.Equal(HttpStatusCode.Created,
            (await org.Client.PostAsJsonAsync($"/api/events/{eventId}/categories", new { name = "21km" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await p.Client.PostAsJsonAsync($"/api/events/{eventId}/categories", new { name = "Under 20" })).StatusCode);

        // Both roles can view categories.
        Assert.Equal(HttpStatusCode.OK, (await p.Client.GetAsync($"/api/events/{eventId}/categories")).StatusCode);
    }

    [Fact]
    public async Task DuplicateCategoryName_Returns409()
    {
        var org = await LoginAs("Organiser");
        var eventId = await CreateEvent(org.Client);
        await CreateCategory(org.Client, eventId, "Senior");

        var res = await org.Client.PostAsJsonAsync($"/api/events/{eventId}/categories", new { name = "Senior" });
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanDeleteEvent_ThatHasCategoriesAndEnrolments()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var eventId = await CreateEvent(org.Client);
        var categoryId = await CreateCategory(org.Client, eventId);
        await p.Client.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });

        Assert.Equal(HttpStatusCode.NoContent, (await org.Client.DeleteAsync($"/api/events/{eventId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await org.Client.GetAsync($"/api/events/{eventId}")).StatusCode);
    }
}
