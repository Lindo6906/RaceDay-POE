using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Api.Tests;

public class ResultTests : ApiTestBase
{
    public ResultTests(ApiFactory factory) : base(factory) { }

    private async Task<(TestUser org, TestUser participant, int eventId, int enrolmentId)> Arrange()
    {
        var org = await LoginAs("Organiser");
        var p = await LoginAs("Participant");
        var eventId = await CreateEvent(org.Client);
        var categoryId = await CreateCategory(org.Client, eventId);
        var enrol = await p.Client.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId });
        return (org, p, eventId, await IdOf(enrol, "enrolmentId"));
    }

    [Fact]
    public async Task Organiser_CanCaptureResult_AndParticipantCanViewIt()
    {
        var (org, p, _, enrolmentId) = await Arrange();

        var res = await org.Client.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result",
            new { finishTime = "00:52:10", position = 1 });
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);

        var mine = await p.Client.GetFromJsonAsync<JsonElement>("/api/results/my");
        var result = Assert.Single(mine.EnumerateArray());
        Assert.Equal(1, result.GetProperty("position").GetInt32());
        Assert.Equal("00:52:10", result.GetProperty("finishTime").GetString());
    }

    [Fact]
    public async Task Participant_CannotCaptureResult_Returns403()
    {
        var (_, p, _, enrolmentId) = await Arrange();
        var res = await p.Client.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result",
            new { finishTime = "00:50:00", position = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotViewMyResults_Returns403()
    {
        var org = await LoginAs("Organiser");
        Assert.Equal(HttpStatusCode.Forbidden, (await org.Client.GetAsync("/api/results/my")).StatusCode);
    }

    [Fact]
    public async Task CaptureResult_Twice_Returns409()
    {
        var (org, _, _, enrolmentId) = await Arrange();
        await org.Client.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new { finishTime = "00:45:00", position = 1 });

        var again = await org.Client.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new { finishTime = "00:46:00", position = 2 });
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task CaptureResult_WithZeroFinishTime_Returns400()
    {
        var (org, _, _, enrolmentId) = await Arrange();
        var res = await org.Client.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new { finishTime = "00:00:00", position = 1 });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task CaptureResult_ForUnknownEnrolment_Returns404()
    {
        var org = await LoginAs("Organiser");
        var res = await org.Client.PostAsJsonAsync("/api/enrolments/999999/result", new { finishTime = "00:45:00", position = 1 });
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task OtherOrganiser_CannotCaptureResult_Returns403()
    {
        var (_, _, _, enrolmentId) = await Arrange();
        var intruder = await LoginAs("Organiser");

        var res = await intruder.Client.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/result", new { finishTime = "00:45:00", position = 1 });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }
}
