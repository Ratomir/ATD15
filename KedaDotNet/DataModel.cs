using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace KedaDotNet
{
    public class DataModel : TableEntity
    {
        public DataModel()
        {

        }

        public DataModel(string device)
        {
            PartitionKey = device;
            RowKey = Guid.NewGuid().ToString();
        }

        public string Temperature { get; set; }
    }
}
