using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Services;

/// <summary>Single point of contact between the MVC Web app and the Part 2 API.</summary>
public class ApiService
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ApiService(HttpClient http) => _http = http;

    // ---------- AUTH ----------
    public async Task<UserVm> RegisterAsync(RegisterVm vm)
    {
        var res = await _http.PostAsJsonAsync("api/auth/register", vm);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<UserVm>(JsonOpts))!;
    }

    public async Task<UserVm> LoginAsync(LoginVm vm)
    {
        var res = await _http.PostAsJsonAsync("api/auth/login", vm);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<UserVm>(JsonOpts))!;
    }

    public async Task LogoutAsync()
    {
        var res = await _http.PostAsync("api/auth/logout", null);
        await EnsureSuccess(res);
    }

    // ---------- PROFILE ----------
    public async Task<UserVm?> GetProfileAsync()
    {
        var res = await _http.GetAsync("api/profile");
        if (res.StatusCode == HttpStatusCode.Unauthorized) return null;
        await EnsureSuccess(res);
        return await res.Content.ReadFromJsonAsync<UserVm>(JsonOpts);
    }

    public async Task<UserVm> UpdateProfileAsync(UpdateProfileVm vm)
    {
        var res = await _http.PutAsJsonAsync("api/profile", vm);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<UserVm>(JsonOpts))!;
    }

    // ---------- EVENTS ----------
    public async Task<List<EventVm>> GetEventsAsync(string? type = null)
    {
        var url = string.IsNullOrWhiteSpace(type) ? "api/events" : $"api/events?type={type}";
        return await GetListAsync<EventVm>(url);
    }

    public async Task<EventVm?> GetEventAsync(int id)
    {
        var res = await _http.GetAsync($"api/events/{id}");
        if (res.StatusCode == HttpStatusCode.NotFound) return null;
        await EnsureSuccess(res);
        return await res.Content.ReadFromJsonAsync<EventVm>(JsonOpts);
    }

    public async Task<EventVm> CreateEventAsync(EventFormVm vm)
    {
        var res = await _http.PostAsJsonAsync("api/events", vm);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<EventVm>(JsonOpts))!;
    }

    public async Task<EventVm> UpdateEventAsync(int id, EventFormVm vm)
    {
        var res = await _http.PutAsJsonAsync($"api/events/{id}", vm);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<EventVm>(JsonOpts))!;
    }

    public async Task DeleteEventAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/events/{id}");
        await EnsureSuccess(res);
    }

    // ---------- CATEGORIES ----------
    public Task<List<CategoryVm>> GetCategoriesAsync(int eventId) =>
        GetListAsync<CategoryVm>($"api/events/{eventId}/categories");

    public async Task<CategoryVm> CreateCategoryAsync(int eventId, string name)
    {
        var res = await _http.PostAsJsonAsync($"api/events/{eventId}/categories", new { name });
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<CategoryVm>(JsonOpts))!;
    }

    public async Task<CategoryVm> UpdateCategoryAsync(int id, string name)
    {
        var res = await _http.PutAsJsonAsync($"api/categories/{id}", new { name });
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<CategoryVm>(JsonOpts))!;
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/categories/{id}");
        await EnsureSuccess(res);
    }

    // ---------- ENROLMENTS ----------
    public async Task<EnrolmentVm> EnrolAsync(int eventId, int categoryId)
    {
        var res = await _http.PostAsJsonAsync($"api/events/{eventId}/enrolments", new { categoryId });
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<EnrolmentVm>(JsonOpts))!;
    }

    public Task<List<EnrolmentVm>> GetMyEnrolmentsAsync() =>
        GetListAsync<EnrolmentVm>("api/enrolments/my");

    public async Task WithdrawAsync(int enrolmentId)
    {
        var res = await _http.DeleteAsync($"api/enrolments/{enrolmentId}");
        await EnsureSuccess(res);
    }

    public Task<List<EnrolmentVm>> GetEventEnrolmentsAsync(int eventId) =>
        GetListAsync<EnrolmentVm>($"api/events/{eventId}/enrolments");

    public Task<List<EnrolmentVm>> GetAllMyEventEnrolmentsAsync() =>
        GetListAsync<EnrolmentVm>("api/enrolments");

    // ---------- RESULTS ----------
    public async Task<ResultVm> CaptureResultAsync(int enrolmentId, ResultFormVm vm)
    {
        var res = await _http.PostAsJsonAsync($"api/enrolments/{enrolmentId}/result", vm);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<ResultVm>(JsonOpts))!;
    }

    public async Task<ResultVm> UpdateResultAsync(int id, ResultFormVm vm)
    {
        var res = await _http.PutAsJsonAsync($"api/results/{id}", vm);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<ResultVm>(JsonOpts))!;
    }

    public Task<List<ResultVm>> GetEventResultsAsync(int eventId) =>
        GetListAsync<ResultVm>($"api/events/{eventId}/results");

    public Task<List<ResultVm>> GetMyResultsAsync() =>
        GetListAsync<ResultVm>("api/results/my");

    // ---------- HELPERS ----------
    private async Task<List<T>> GetListAsync<T>(string url)
    {
        var res = await _http.GetAsync(url);
        await EnsureSuccess(res);
        return (await res.Content.ReadFromJsonAsync<List<T>>(JsonOpts)) ?? new List<T>();
    }

    private static async Task EnsureSuccess(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;

        var message = $"API returned {(int)res.StatusCode}.";
        try
        {
            var doc = await res.Content.ReadFromJsonAsync<JsonElement>();
            if (doc.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                message = m.GetString()!;
        }
        catch { /* body not JSON */ }

        throw new ApiException(res.StatusCode, message);
    }
}

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ApiException(HttpStatusCode statusCode, string message) : base(message) => StatusCode = statusCode;
}