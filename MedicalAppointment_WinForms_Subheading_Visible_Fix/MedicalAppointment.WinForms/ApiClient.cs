using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MedicalAppointment.WinForms;

public class ApiClient
{
    private readonly HttpClient _http = new();
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    private string Url(string path) => Session.ApiBaseUrl.TrimEnd('/') + path;

    private HttpRequestMessage Request(HttpMethod method, string path, object? body = null, bool auth = true)
    {
        var req = new HttpRequestMessage(method, Url(path));
        if (auth && !string.IsNullOrWhiteSpace(Session.AccessToken))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Session.AccessToken);
        if (body != null)
        {
            string json = JsonSerializer.Serialize(body, _json);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        return req;
    }

    private async Task<T?> Send<T>(HttpMethod method, string path, object? body = null, bool auth = true)
    {
        var res = await _http.SendAsync(Request(method, path, body, auth));
        var text = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception(ExtractError(text, res.StatusCode.ToString()));
        if (string.IsNullOrWhiteSpace(text)) return default;
        return JsonSerializer.Deserialize<T>(text, _json);
    }

    private async Task Send(HttpMethod method, string path, object? body = null, bool auth = true)
    {
        var res = await _http.SendAsync(Request(method, path, body, auth));
        var text = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception(ExtractError(text, res.StatusCode.ToString()));
    }

    private string ExtractError(string text, string fallback)
    {
        try
        {
            var msg = JsonSerializer.Deserialize<ApiMessage>(text, _json);
            if (!string.IsNullOrWhiteSpace(msg?.detail)) return msg.detail;
            if (!string.IsNullOrWhiteSpace(msg?.message)) return msg.message;
        }
        catch { }
        return string.IsNullOrWhiteSpace(text) ? fallback : text;
    }

    public Task<LoginResponse?> Login(string username, string password) => Send<LoginResponse>(HttpMethod.Post, "/api/Auth/login", new LoginRequest { username = username, password = password }, false);
    public Task<UserResponse?> Register(string fullName, string username, string password) => Send<UserResponse>(HttpMethod.Post, "/api/Auth/register", new RegisterRequest { fullName = fullName, username = username, password = password }, false);

    public async Task<List<Doctor>> GetDoctors(string search = "") => await Send<List<Doctor>>(HttpMethod.Get, "/api/Doctors" + (string.IsNullOrWhiteSpace(search) ? "" : "?search=" + Uri.EscapeDataString(search)), null, false) ?? new();
    public Task<Doctor?> GetDoctor(int id) => Send<Doctor>(HttpMethod.Get, $"/api/Doctors/{id}");
    public Task CreateDoctor(DoctorCreate dto) => Send(HttpMethod.Post, "/api/Doctors", dto);
    public Task UpdateDoctor(int id, DoctorCreate dto) => Send(HttpMethod.Put, $"/api/Doctors/{id}", dto);
    public Task DeleteDoctor(int id) => Send(HttpMethod.Delete, $"/api/Doctors/{id}");

    public async Task<List<Schedule>> GetSchedules() => await Send<List<Schedule>>(HttpMethod.Get, "/api/DoctorSchedules", null, false) ?? new();
    public Task CreateSchedule(ScheduleCreate dto) => Send(HttpMethod.Post, "/api/DoctorSchedules", dto);
    public Task UpdateSchedule(int id, ScheduleCreate dto) => Send(HttpMethod.Put, $"/api/DoctorSchedules/{id}", dto);
    public Task DeleteSchedule(int id) => Send(HttpMethod.Delete, $"/api/DoctorSchedules/{id}");

    public async Task<List<Appointment>> GetAllAppointments(string status = "") => await Send<List<Appointment>>(HttpMethod.Get, "/api/Appointments" + (string.IsNullOrWhiteSpace(status) ? "" : "?status=" + Uri.EscapeDataString(status))) ?? new();
    public async Task<List<Appointment>> GetMyAppointments(int userId) => await Send<List<Appointment>>(HttpMethod.Get, $"/api/Appointments/user/{userId}") ?? new();
    public Task CreateAppointment(AppointmentCreate dto) => Send(HttpMethod.Post, "/api/Appointments", dto);
    public Task UpdateMyAppointment(int id, AppointmentUpdate dto) => Send(HttpMethod.Put, $"/api/Appointments/update/{id}", dto);
    public Task CancelMyAppointment(int id) => Send(HttpMethod.Put, $"/api/Appointments/cancel/{id}");
    public Task AdminSetAppointmentStatus(int id, string status) => Send(HttpMethod.Put, $"/api/Admin/Appointments/{id}/status/{status}");

    public Task<ReportSummary?> GetReportSummary() => Send<ReportSummary>(HttpMethod.Get, "/api/Reports/summary");
}
