using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Api.Tests;

public class EnrolmentTests : ApiTestBase
{
    public EnrolmentTests(ApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Participant_CanEnrol_AndEnrolmentIsRecorded()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var eventId = await CreateEvent(org.Client);
        var categoryId = await CreateCategory(org.Client, eventId, "10km");

        var res = await p.Client.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        // The link between participant, event and category is stored and visible to the participant...
        var mine = await p.Client.GetFromJsonAsync<JsonElement>("/api/enrolments/my");
        var enrolment = Assert.Single(mine.EnumerateArray());
        Assert.Equal(eventId, enrolment.GetProperty("eventId").GetInt32());
        Assert.Equal(categoryId, enrolment.GetProperty("categoryId").GetInt32());
        Assert.Equal(p.UserId, enrolment.GetProperty("participantId").GetInt32());

        // ...and to the organiser of that event.
        var forEvent = await org.Client.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/enrolments");
        Assert.Single(forEvent.EnumerateArray());
    }

    [Fact]
    public async Task Organiser_CannotEnrol_Returns403()
    {
        var org = await LoginAs("Organiser");
        var eventId = await CreateEvent(org.Client);
        var categoryId = await CreateCategory(org.Client, eventId);

        var res = await org.Client.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Anonymous_CannotEnrol_Returns401()
    {
        var res = await Factory.CreateClient().PostAsJsonAsync("/api/events/1/enrolments", new { categoryId = 1 });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Enrol_Twice_Returns409()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var eventId = await CreateEvent(org.Client);
        var categoryId = await CreateCategory(org.Client, eventId);

        await p.Client.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });
        var again = await p.Client.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task Enrol_WithCategoryFromAnotherEvent_Returns400()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var eventA = await CreateEvent(org.Client, "Event A");
        var eventB = await CreateEvent(org.Client, "Event B");
        var categoryOfB = await CreateCategory(org.Client, eventB);

        var res = await p.Client.PostAsJsonAsync($"/api/events/{eventA}/enrolments", new { categoryId = categoryOfB });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Enrol_InMissingEvent_Returns404()
    {
        var p = await LoginAs("Participant");
        var res = await p.Client.PostAsJsonAsync("/api/events/999999/enrolments", new { categoryId = 1 });
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotViewEventEnrolments_Returns403()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var eventId = await CreateEvent(org.Client);

        var res = await p.Client.GetAsync($"/api/events/{eventId}/enrolments");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotViewAnotherOrganisersEnrolments_Returns403()
    {
        var owner = await LoginAs("Organiser");
        var other = await LoginAs("Organiser");
        var eventId = await CreateEvent(owner.Client);

        var res = await other.Client.GetAsync($"/api/events/{eventId}/enrolments");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Participant_CanWithdraw_OwnEnrolment()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var eventId = await CreateEvent(org.Client);
        var categoryId = await CreateCategory(org.Client, eventId);
        var enrol = await p.Client.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });
        var enrolmentId = await IdOf(enrol, "enrolmentId");

        Assert.Equal(HttpStatusCode.NoContent, (await p.Client.DeleteAsync($"/api/enrolments/{enrolmentId}")).StatusCode);
    }

    [Fact]
    public async Task Participant_CannotWithdraw_SomeoneElsesEnrolment()
    {
        var org = await LoginAs("Organiser");
        var owner = await LoginAs("Participant");
        var other = await LoginAs("Participant");
        var eventId = await CreateEvent(org.Client);
        var categoryId = await CreateCategory(org.Client, eventId);
        var enrol = await owner.Client.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });
        var enrolmentId = await IdOf(enrol, "enrolmentId");

        Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.DeleteAsync($"/api/enrolments/{enrolmentId}")).StatusCode);
    }
}
