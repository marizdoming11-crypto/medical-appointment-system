namespace MedicalAppointment.WinForms;

public static class Session
{
    public static string ApiBaseUrl { get; set; } = "http://127.0.0.1:8001";
    public static string AccessToken { get; set; } = "";
    public static int UserId { get; set; }
    public static string FullName { get; set; } = "";
    public static string Username { get; set; } = "";
    public static string Role { get; set; } = "";

    public static void Clear()
    {
        AccessToken = "";
        UserId = 0;
        FullName = "";
        Username = "";
        Role = "";
    }
}
