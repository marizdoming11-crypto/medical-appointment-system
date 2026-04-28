using System.Net.Http;

namespace Medical.ConsoleApp
{
    public class ApiClient
    {
        static HttpClient client = new HttpClient();

        public static async Task GetAppointments()
        {
            var res = await client.GetStringAsync("http://localhost:5000/api/appointments/all");
            Console.WriteLine(res);
        }
    }
}