using System;

namespace producer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.EventHubs;

    public class Program
    {
        private static EventHubClient eventHubClient;
        private const string EventHubNamespace = "atd15ehns-v1";
        private const string EventHubName = "atd15eh-v1";
        private const string EventHubKey = "XXX";
        private const string EventHubConnectionString = "Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={1}";

        public static void Main(string[] args)
        {
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

            Random rNum = new Random();
            for (var i = 0; i < numMessagesToSend; i++)
            {
                try
                {
                    var temperature = Math.Round(rNum.NextDouble() * 10, 3);
                    string device = lstOfSensors[rNum.Next(1, 3)];
                    string sector = lstOfSectors[rNum.Next(1, 3)];

                    var message = $"{sector}-{device}-{temperature}";
                    Console.WriteLine($"Sending message: {message}");
                    await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
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
