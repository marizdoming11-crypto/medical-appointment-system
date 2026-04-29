using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

string baseUrl = "http://localhost:8001/api";
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
        string? choice = Console.ReadLine();
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

static string ReadRequired(string label)
{
    while (true)
    {
        Console.Write(label);
        string? value = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
        Console.WriteLine("This field is required.");
    }
}

async Task RegisterPatient()
{
    RegisterDto dto = new RegisterDto
    {
        FullName = ReadRequired("Full Name: "),
        Username = ReadRequired("Username: "),
        Password = ReadRequired("Password: ")
    };
    var response = await client.PostAsJsonAsync($"{baseUrl}/Auth/register", dto);
    Console.WriteLine(response.IsSuccessStatusCode ? "Registration successful." : await response.Content.ReadAsStringAsync());
}

async Task<User?> Login()
{
    LoginDto dto = new LoginDto
    {
        Username = ReadRequired("Username: "),
        Password = ReadRequired("Password: ")
    };
    var response = await client.PostAsJsonAsync($"{baseUrl}/Auth/login", dto);
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        return null;
    }
    return await response.Content.ReadFromJsonAsync<User>();
}

async Task AdminDashboard()
{
    while (true)
    {
        Console.WriteLine("\n=== ADMIN DASHBOARD ===");
        Console.WriteLine("1. Manage Doctors");
        Console.WriteLine("2. Manage Schedules");
        Console.WriteLine("3. View Appointments");
        Console.WriteLine("4. Sign Out");
        Console.Write("Choose: ");
        string? choice = Console.ReadLine();
        if (choice == "1") await ManageDoctors();
        else if (choice == "2") await ManageSchedules();
        else if (choice == "3") await ViewAllAppointments();
        else if (choice == "4") break;
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
        string? choice = Console.ReadLine();
        if (choice == "1") await ViewDoctors();
        else if (choice == "2") await MakeAppointment();
        else if (choice == "3") await ViewMyAppointments();
        else if (choice == "4") await CancelAppointment();
        else if (choice == "5") break;
    }
}

async Task ManageDoctors()
{
    while (true)
    {
        Console.WriteLine("\n1. View 2. Add 3. Update 4. Delete 5. Back");
        Console.Write("Choose: ");
        string? choice = Console.ReadLine();
        if (choice == "1") await ViewDoctors();
        else if (choice == "2")
        {
            var doctor = new Doctor { FullName = ReadRequired("Name: "), Specialization = ReadRequired("Specialization: "), ContactNumber = ReadRequired("Contact: ") };
            var r = await client.PostAsJsonAsync($"{baseUrl}/Doctors", doctor);
            Console.WriteLine(r.IsSuccessStatusCode ? "Doctor added." : await r.Content.ReadAsStringAsync());
        }
        else if (choice == "3")
        {
            await ViewDoctors();
            int id = int.Parse(ReadRequired("Doctor ID: "));
            var doctor = new Doctor { FullName = ReadRequired("New name: "), Specialization = ReadRequired("New specialization: "), ContactNumber = ReadRequired("New contact: ") };
            var r = await client.PutAsJsonAsync($"{baseUrl}/Doctors/{id}", doctor);
            Console.WriteLine(r.IsSuccessStatusCode ? "Doctor updated." : await r.Content.ReadAsStringAsync());
        }
        else if (choice == "4")
        {
            await ViewDoctors();
            int id = int.Parse(ReadRequired("Doctor ID: "));
            var r = await client.DeleteAsync($"{baseUrl}/Doctors/{id}");
            Console.WriteLine(r.IsSuccessStatusCode ? "Doctor deleted." : await r.Content.ReadAsStringAsync());
        }
        else if (choice == "5") break;
    }
}

async Task ManageSchedules()
{
    while (true)
    {
        Console.WriteLine("\n1. View 2. Add 3. Update 4. Delete 5. Back");
        Console.Write("Choose: ");
        string? choice = Console.ReadLine();
        if (choice == "1") await ViewSchedules();
        else if (choice == "2")
        {
            await ViewDoctors();
            var s = new DoctorSchedule { DoctorId = int.Parse(ReadRequired("Doctor ID: ")), ScheduleDate = ReadRequired("Date yyyy-mm-dd: "), StartTime = ReadRequired("Display start: "), EndTime = ReadRequired("Display end: "), StartTime24 = ReadRequired("Start HH:mm:ss: "), EndTime24 = ReadRequired("End HH:mm:ss: "), IsAvailable = true };
            var r = await client.PostAsJsonAsync($"{baseUrl}/DoctorSchedules", s);
            Console.WriteLine(r.IsSuccessStatusCode ? "Schedule added." : await r.Content.ReadAsStringAsync());
        }
        else if (choice == "3")
        {
            await ViewSchedules();
            int id = int.Parse(ReadRequired("Schedule ID: "));
            var s = new DoctorSchedule { DoctorId = int.Parse(ReadRequired("Doctor ID: ")), ScheduleDate = ReadRequired("Date yyyy-mm-dd: "), StartTime = ReadRequired("Display start: "), EndTime = ReadRequired("Display end: "), StartTime24 = ReadRequired("Start HH:mm:ss: "), EndTime24 = ReadRequired("End HH:mm:ss: "), IsAvailable = true };
            var r = await client.PutAsJsonAsync($"{baseUrl}/DoctorSchedules/{id}", s);
            Console.WriteLine(r.IsSuccessStatusCode ? "Schedule updated." : await r.Content.ReadAsStringAsync());
        }
        else if (choice == "4")
        {
            await ViewSchedules();
            int id = int.Parse(ReadRequired("Schedule ID: "));
            var r = await client.DeleteAsync($"{baseUrl}/DoctorSchedules/{id}");
            Console.WriteLine(r.IsSuccessStatusCode ? "Schedule deleted." : await r.Content.ReadAsStringAsync());
        }
        else if (choice == "5") break;
    }
}

async Task ViewDoctors()
{
    var doctors = await client.GetFromJsonAsync<List<Doctor>>($"{baseUrl}/Doctors");
    foreach (var d in doctors ?? new()) Console.WriteLine($"{d.Id}. Dr. {d.FullName} - {d.Specialization} - {d.ContactNumber}");
}

async Task ViewSchedules()
{
    var schedules = await client.GetFromJsonAsync<List<DoctorSchedule>>($"{baseUrl}/DoctorSchedules");
    foreach (var s in schedules ?? new()) Console.WriteLine($"{s.Id}. Dr. {s.Doctor?.FullName} - {s.ScheduleDate} - {s.StartTime} to {s.EndTime}");
}

async Task MakeAppointment()
{
    await ViewDoctors();
    var a = new Appointment { UserId = currentUser!.Id, DoctorId = int.Parse(ReadRequired("Doctor ID: ")), AppointmentDate = ReadRequired("Date yyyy-mm-dd: "), AppointmentTime = ReadRequired("Time HH:mm:ss: "), Reason = ReadRequired("Reason: "), Status = "Pending" };
    var r = await client.PostAsJsonAsync($"{baseUrl}/Appointments", a);
    Console.WriteLine(r.IsSuccessStatusCode ? "Appointment created." : await r.Content.ReadAsStringAsync());
}

async Task ViewMyAppointments()
{
    var apps = await client.GetFromJsonAsync<List<Appointment>>($"{baseUrl}/Appointments/user/{currentUser!.Id}");
    foreach (var a in apps ?? new()) Console.WriteLine($"{a.Id}. Dr. {a.Doctor?.FullName} - {a.AppointmentDate} {a.AppointmentTime} - {a.Reason} - {a.Status}");
}

async Task CancelAppointment()
{
    await ViewMyAppointments();
    int id = int.Parse(ReadRequired("Appointment ID: "));
    var r = await client.PutAsync($"{baseUrl}/Appointments/cancel/{id}", null);
    Console.WriteLine(r.IsSuccessStatusCode ? "Appointment cancelled." : await r.Content.ReadAsStringAsync());
}

async Task ViewAllAppointments()
{
    var apps = await client.GetFromJsonAsync<List<Appointment>>($"{baseUrl}/Appointments");
    foreach (var a in apps ?? new()) Console.WriteLine($"{a.Id}. Patient: {a.User?.FullName} | Doctor: {a.Doctor?.FullName} | {a.AppointmentDate} {a.AppointmentTime} | {a.Status}");
}

public class User { public int Id { get; set; } public string FullName { get; set; } = ""; public string Username { get; set; } = ""; public string Role { get; set; } = ""; [JsonPropertyName("access_token")] public string AccessToken { get; set; } = ""; }
public class RegisterDto { public string FullName { get; set; } = ""; public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
public class LoginDto { public string Username { get; set; } = ""; public string Password { get; set; } = ""; }
public class Doctor { public int Id { get; set; } public string FullName { get; set; } = ""; public string Specialization { get; set; } = ""; public string ContactNumber { get; set; } = ""; }
public class DoctorSchedule { public int Id { get; set; } public int DoctorId { get; set; } public Doctor? Doctor { get; set; } public string ScheduleDate { get; set; } = ""; public string StartTime { get; set; } = ""; public string EndTime { get; set; } = ""; public string StartTime24 { get; set; } = ""; public string EndTime24 { get; set; } = ""; public bool IsAvailable { get; set; } }
public class Appointment { public int Id { get; set; } public int UserId { get; set; } public User? User { get; set; } public int DoctorId { get; set; } public Doctor? Doctor { get; set; } public string AppointmentDate { get; set; } = ""; public string AppointmentTime { get; set; } = ""; public string Reason { get; set; } = ""; public string Status { get; set; } = ""; }
