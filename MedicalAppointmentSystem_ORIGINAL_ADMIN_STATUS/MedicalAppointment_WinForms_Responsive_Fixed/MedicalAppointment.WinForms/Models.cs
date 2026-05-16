namespace MedicalAppointment.WinForms;

public class LoginRequest { public string username { get; set; } = ""; public string password { get; set; } = ""; }
public class RegisterRequest { public string fullName { get; set; } = ""; public string username { get; set; } = ""; public string password { get; set; } = ""; }
public class LoginResponse { public string access_token { get; set; } = ""; public string token_type { get; set; } = ""; public int id { get; set; } public string fullName { get; set; } = ""; public string username { get; set; } = ""; public string role { get; set; } = ""; }
public class UserResponse { public int id { get; set; } public string fullName { get; set; } = ""; public string username { get; set; } = ""; public string role { get; set; } = ""; }
public class Doctor { public int id { get; set; } public string fullName { get; set; } = ""; public string specialization { get; set; } = ""; public string contactNumber { get; set; } = ""; public override string ToString() => $"{fullName} - {specialization}"; }
public class DoctorCreate { public string fullName { get; set; } = ""; public string specialization { get; set; } = ""; public string contactNumber { get; set; } = ""; }
public class Schedule { public int id { get; set; } public int doctorId { get; set; } public string scheduleDate { get; set; } = ""; public string startTime { get; set; } = ""; public string endTime { get; set; } = ""; public bool isAvailable { get; set; } public Doctor? doctor { get; set; } }
public class ScheduleCreate { public int doctorId { get; set; } public string scheduleDate { get; set; } = ""; public string startTime { get; set; } = ""; public string endTime { get; set; } = ""; public bool isAvailable { get; set; } = true; }
public class Appointment { public int id { get; set; } public int userId { get; set; } public int doctorId { get; set; } public string appointmentDate { get; set; } = ""; public string reason { get; set; } = ""; public string status { get; set; } = ""; public UserResponse? user { get; set; } public Doctor? doctor { get; set; } }
public class AppointmentCreate { public int userId { get; set; } public int doctorId { get; set; } public string appointmentDate { get; set; } = ""; public string reason { get; set; } = ""; }
public class AppointmentUpdate { public int doctorId { get; set; } public string appointmentDate { get; set; } = ""; public string reason { get; set; } = ""; }
public class ReportSummary { public int users { get; set; } public int doctors { get; set; } public int schedules { get; set; } public int appointments { get; set; } public int pending { get; set; } public int approved { get; set; } public int cancelled { get; set; } }
public class ApiMessage { public string message { get; set; } = ""; public string detail { get; set; } = ""; }
