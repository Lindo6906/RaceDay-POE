using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests;

public class AuthTests : ApiTestBase
{
    public AuthTests(ApiFactory factory) : base(factory) { }

    [Theory]
    [InlineData("Participant")]
    [InlineData("Organiser")]
    public async Task Register_WithValidDetails_Returns201(string role)
    {
        var client = Factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", RegisterBody(role));
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }

    [Fact]
    public async Task Register_Response_DoesNotExposePassword()
    {
        var client = Factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", RegisterBody("Participant"));
        var body = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain(ApiFactory.Password, body);
        Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = Factory.CreateClient();
        var email = UniqueEmail();
        await client.PostAsJsonAsync("/api/auth/register", RegisterBody("Participant", email));

        var res = await client.PostAsJsonAsync("/api/auth/register", RegisterBody("Participant", email));
        Assert.Equal(HttpStatusCode.Conflict, res.StatusCode);
    }

    [Fact]
    public async Task Register_Organiser_WithWrongCode_Returns403()
    {
        var client = Factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Bad Actor", email = UniqueEmail(), password = ApiFactory.Password,
            role = "Organiser", organiserCode = "WRONG"
        });
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Register_WithShortPassword_Returns400()
    {
        var client = Factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "A B", email = UniqueEmail(), password = "short", role = "Participant"
        });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingRole_Returns400()
    {
        var client = Factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "A B", email = UniqueEmail(), password = ApiFactory.Password
        });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_Returns200AndRole()
    {
        var user = await LoginAs("Participant");
        var res = await user.Client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = Factory.CreateClient();
        var email = UniqueEmail();
        await client.PostAsJsonAsync("/api/auth/register", RegisterBody("Participant", email));

        var res = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "WrongPassword1" });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var client = Factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login", new { email = UniqueEmail(), password = ApiFactory.Password });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutLogin_Returns401()
    {
        var client = Factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/profile")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/events")).StatusCode);
    }

    [Fact]
    public async Task Logout_EndsTheSession()
    {
        var user = await LoginAs("Participant");
        await user.Client.PostAsync("/api/auth/logout", null);

        var res = await user.Client.GetAsync("/api/profile");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Profile_CanBeUpdated()
    {
        var user = await LoginAs("Participant");
        var put = await user.Client.PutAsJsonAsync("/api/profile", new { fullName = "Lindo Runner", phoneNumber = "0821234567" });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var get = await user.Client.GetFromJsonAsync<System.Text.Json.JsonElement>("/api/profile");
        Assert.Equal("Lindo Runner", get.GetProperty("fullName").GetString());
    }
}
