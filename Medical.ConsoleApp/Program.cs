namespace Medical.ConsoleApp
{
    class Program
    {
        static async Task Main()
        {
            while (true)
            {
                Console.WriteLine("\n1. Register");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. View Appointments");
                Console.WriteLine("4. Exit");

                var choice = Console.ReadLine();

                if (choice == "3")
                    await ApiClient.GetAppointments();
                else if (choice == "4")
                    break;
            }
        }
    }
}