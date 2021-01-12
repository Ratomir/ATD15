using System;

namespace producer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.EventHubs;
    using Producer;

    public class Program
    {
        private static EventHubClient eventHubClient;
        private const string EventHubNamespace = "evhns-rv187";
        private const string EventHubName = "evh-rv187";
        private const string EventHubKey = "XXX";
        private const string EventHubConnectionString = "Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={1}";
        private static int jsonOrMessage;

        public static void Main(string[] args)
        {
            Console.Write("1. Json\n2. Messsage\n > ");
            jsonOrMessage = Convert.ToInt32(Console.ReadLine());
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            // Creates an EventHubsConnectionStringBuilder object from the connection string, and sets the EntityPath.
            // Typically, the connection string should have the entity path in it, but for the sake of this simple scenario
            // we are using the connection string from the namespace.
            var connectionStringBuilder = new EventHubsConnectionStringBuilder(String.Format(EventHubConnectionString, EventHubNamespace, EventHubKey))
            {
                EntityPath = EventHubName
            };

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

            // Sending 50 messages to Event Hub
            await SendMessagesToEventHub(2);
            await eventHubClient.CloseAsync();

            Console.WriteLine("Press ENTER to exit.");
            Console.ReadLine();
        }

        // Creates an event hub client and sends 100 messages to the event hub.
        private static async Task SendMessagesToEventHub(int numMessagesToSend)
        {
            var lstOfSensors = new Dictionary<int, string>()
            {
                { 1, "Ratomir".ToUpper() },{ 2, "ATD15".ToUpper() },{ 3, "GreenRoom".ToUpper() }
            };

            var lstOfSectors = new Dictionary<int, string>()
            {
                { 1, "Development".ToUpper() },{ 2, "Testing".ToUpper() },{ 3, "Painting".ToUpper() }
            };

            string jsonString = File.ReadAllText("data_json.json");
            var lstConutries = JsonSerializer.Deserialize<List<CountryCodeModel>>(jsonString);
            int maxCountries = lstConutries.Count - 1;

            Random rNum = new Random();
            for (var i = 0; i < numMessagesToSend; i++)
            {
                try
                {
                    var temperature = Math.Round(rNum.NextDouble() * 10, 3);
                    string device = lstOfSensors[rNum.Next(1, 3)];
                    string sector = lstOfSectors[rNum.Next(1, 3)];
                    string country = lstConutries[rNum.Next(0, maxCountries)].Code;

                    var message = $"{sector}-{device}-{temperature}-{country}";
                    Console.WriteLine($"Sending message: {message}");
                    switch (jsonOrMessage)
                    {
                        case 1:
                            var jsonObject = new
                            {
                                device,
                                sector,
                                temperature,
                                country
                            };
                            await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(jsonObject))));
                            break;
                        case 2:
                            await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
                }

                await Task.Delay(10);
            }

            Console.WriteLine($"{numMessagesToSend} messages sent.");
        }
    }
}
