using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace KedaDotNet
{
    public static class EventHubFunction
    {
        [FunctionName("EventHubFunction")]
        public static async Task Run([EventHubTrigger("atd15eh-v1", Connection = "EventHub")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();
            string connString = Environment.GetEnvironmentVariable("TableStorage");

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);

                    CloudStorageAccount cloudStorageAccount = Common.CreateStorageAccountFromConnectionString(connString);

                    string[] superArray = messageBody.Split('-');

                    // Create or reference an existing table
                    CloudTable table = await Common.CreateTableAsync(cloudStorageAccount.CreateCloudTableClient(), superArray[0]);

                    DataModel data = new DataModel(superArray[1])
                    {
                        Temperature = superArray[2]
                    };

                    await StorageUtils.InsertOrMergeEntityAsync(table, data);

                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
