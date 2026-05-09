using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

string baseUrl = "http://127.0.0.1:8001/api";
using HttpClient client = new HttpClient();
User? currentUser = null;

while (true)
{
    if (currentUser == null)
    {
        Console.WriteLine("\n=== MEDICAL APPOINTMENT SYSTEM ===");
        Console.WriteLine("1. Register Patient");
        Console.WriteLine("2. Login");
        Console.WriteLine("3. Exit");
        Console.Write("Choose: ");
        var choice = Console.ReadLine();
        if (choice == "1") await RegisterPatient();
        else if (choice == "2")
        {
            currentUser = await Login();
            if (currentUser != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", currentUser.AccessToken);
                Console.WriteLine($"Welcome, {currentUser.FullName}");
            }
        }
        else if (choice == "3") break;
    }
    else if (currentUser.Role == "Admin")
    {
        await AdminDashboard();
        currentUser = null;
        client.DefaultRequestHeaders.Authorization = null;
    }
    else
    {
        await UserDashboard();
        currentUser = null;
        client.DefaultRequestHeaders.Authorization = null;
    }
}

async Task RegisterPatient()
{
    Console.Write("Full Name: "); var fullName = Console.ReadLine() ?? "";
    Console.Write("Username: "); var username = Console.ReadLine() ?? "";
    Console.Write("Password: "); var password = Console.ReadLine() ?? "";
    var res = await client.PostAsJsonAsync($"{baseUrl}/Auth/register", new RegisterDto(fullName, username, password));
    Console.WriteLine(res.IsSuccessStatusCode ? "Registration successful." : await res.Content.ReadAsStringAsync());
}

async Task<User?> Login()
{
    Console.Write("Username: "); var username = Console.ReadLine() ?? "";
    Console.Write("Password: "); var password = Console.ReadLine() ?? "";
    var res = await client.PostAsJsonAsync($"{baseUrl}/Auth/login", new LoginDto(username, password));
    if (!res.IsSuccessStatusCode) { Console.WriteLine("Invalid login."); return null; }
    return await res.Content.ReadFromJsonAsync<User>();
}

async Task AdminDashboard()
{
    while (true)
    {
        Console.WriteLine("\n=== ADMIN DASHBOARD ===");
        Console.WriteLine("1. View Doctors");
        Console.WriteLine("2. Add Doctor");
        Console.WriteLine("3. View Appointments");
        Console.WriteLine("4. Accept Appointment");
        Console.WriteLine("5. Cancel Appointment");
        Console.WriteLine("6. Sign Out");
        Console.Write("Choose: ");
        var c = Console.ReadLine();
        if (c == "1") await ViewDoctors();
        else if (c == "2") await AddDoctor();
        else if (c == "3") await ViewAllAppointments();
        else if (c == "4") await AdminSetAppointmentStatus("Approved");
        else if (c == "5") await AdminSetAppointmentStatus("Cancelled");
        else if (c == "6") break;
    }
}

async Task UserDashboard()
{
    while (true)
    {
        Console.WriteLine("\n=== USER DASHBOARD ===");
        Console.WriteLine("1. View Doctors");
        Console.WriteLine("2. Make Appointment");
        Console.WriteLine("3. View My Appointments");
        Console.WriteLine("4. Cancel Appointment");
        Console.WriteLine("5. Sign Out");
        Console.Write("Choose: ");
        var c = Console.ReadLine();
        if (c == "1") await ViewDoctors();
        else if (c == "2") await MakeAppointment();
        else if (c == "3") await ViewMyAppointments();
        else if (c == "4") await UserCancelAppointment();
        else if (c == "5") break;
    }
}

async Task ViewDoctors()
{
    var doctors = await client.GetFromJsonAsync<List<Doctor>>($"{baseUrl}/Doctors") ?? new();
    foreach (var d in doctors) Console.WriteLine($"{d.Id}. Dr. {d.FullName} - {d.Specialization} - {d.ContactNumber}");
    if (!doctors.Any()) Console.WriteLine("No doctors found.");
}

async Task AddDoctor()
{
    Console.Write("Doctor Name: "); var name = Console.ReadLine() ?? "";
    Console.Write("Specialization: "); var spec = Console.ReadLine() ?? "";
    Console.Write("Contact: "); var contact = Console.ReadLine() ?? "";
    var res = await client.PostAsJsonAsync($"{baseUrl}/Doctors", new DoctorCreate(name, spec, contact));
    Console.WriteLine(res.IsSuccessStatusCode ? "Doctor added." : await res.Content.ReadAsStringAsync());
}

async Task ViewAllAppointments()
{
    var apps = await client.GetFromJsonAsync<List<Appointment>>($"{baseUrl}/Appointments") ?? new();
    foreach (var a in apps) Console.WriteLine($"{a.Id}. Patient: {a.User?.FullName} | Doctor: {a.Doctor?.FullName} | Date: {a.AppointmentDate:yyyy-MM-dd} | Status: {a.Status}");
    if (!apps.Any()) Console.WriteLine("No appointments found.");
}

async Task AdminSetAppointmentStatus(string status)
{
    await ViewAllAppointments();
    Console.Write("Appointment ID: ");
    if (!int.TryParse(Console.ReadLine(), out int id)) return;
    var res = await client.PutAsync($"{baseUrl}/Admin/Appointments/{id}/status/{status}", null);
    Console.WriteLine(res.IsSuccessStatusCode ? $"Appointment set to {status}." : await res.Content.ReadAsStringAsync());
}

async Task MakeAppointment()
{
    await ViewDoctors();
    Console.Write("Doctor ID: "); int doctorId = int.Parse(Console.ReadLine() ?? "0");
    Console.Write("Date yyyy-mm-dd: "); DateTime date = DateTime.Parse(Console.ReadLine() ?? "");
    Console.Write("Reason: "); string reason = Console.ReadLine() ?? "";
    var res = await client.PostAsJsonAsync($"{baseUrl}/Appointments", new AppointmentCreate(currentUser!.Id, doctorId, date, reason, "Pending"));
    Console.WriteLine(res.IsSuccessStatusCode ? "Appointment created." : await res.Content.ReadAsStringAsync());
}

async Task ViewMyAppointments()
{
    var apps = await client.GetFromJsonAsync<List<Appointment>>($"{baseUrl}/Appointments/user/{currentUser!.Id}") ?? new();
    foreach (var a in apps) Console.WriteLine($"{a.Id}. Dr. {a.Doctor?.FullName} | {a.AppointmentDate:yyyy-MM-dd} | {a.Reason} | {a.Status}");
    if (!apps.Any()) Console.WriteLine("No appointments found.");
}

async Task UserCancelAppointment()
{
    await ViewMyAppointments();
    Console.Write("Appointment ID: ");
    if (!int.TryParse(Console.ReadLine(), out int id)) return;
    var res = await client.PutAsync($"{baseUrl}/Appointments/cancel/{id}", null);
    Console.WriteLine(res.IsSuccessStatusCode ? "Appointment cancelled." : await res.Content.ReadAsStringAsync());
}

record RegisterDto(string FullName, string Username, string Password);
record LoginDto(string Username, string Password);
record DoctorCreate(string FullName, string Specialization, string ContactNumber);
record AppointmentCreate(int UserId, int DoctorId, DateTime AppointmentDate, string Reason, string Status);

class User { public int Id { get; set; } public string FullName { get; set; } = ""; public string Username { get; set; } = ""; public string Role { get; set; } = ""; [JsonPropertyName("access_token")] public string AccessToken { get; set; } = ""; }
class Doctor { public int Id { get; set; } public string FullName { get; set; } = ""; public string Specialization { get; set; } = ""; public string ContactNumber { get; set; } = ""; }
class Appointment { public int Id { get; set; } public int UserId { get; set; } public User? User { get; set; } public int DoctorId { get; set; } public Doctor? Doctor { get; set; } public DateTime AppointmentDate { get; set; } public string Reason { get; set; } = ""; public string Status { get; set; } = ""; }
