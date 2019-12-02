using Infra.Base;
using Pulumi.Azure.Core;
using Pulumi.Azure.EventHub;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infra.EH
{
    public class KedaEventHub
    {
        public EventHub EventHub { get; set; }
        public EventHubNamespace EventHubNamespace { get; set; }
        public KedaEventHub(ResourceGroup resourceGroup, int version)
        {
            EventHubNamespace = new EventHubNamespace("ehns", new EventHubNamespaceArgs()
            {
                Name = $"{ConfigModel.ConfigValues["projectname"]}ehns-v{version}",
                Capacity = 1,
                Sku = "Basic",
                KafkaEnabled = false,
                Location = ConfigModel.ConfigValues["location"].ToString(),
                ResourceGroupName = resourceGroup.Name
            });

            EventHub = new EventHub("eh", new EventHubArgs() 
            {
                Name = $"{ConfigModel.ConfigValues["projectname"]}eh-v{version}",
                ResourceGroupName = resourceGroup.Name,
                PartitionCount = 1,
                MessageRetention = 1,
                NamespaceName = EventHubNamespace.Name
            });

        }
    }
}
