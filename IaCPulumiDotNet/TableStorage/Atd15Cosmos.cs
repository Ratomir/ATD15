using System;
using System.Collections.Generic;
using System.Text;
using Infra.Base;
using Pulumi.Azure;
using Pulumi.Azure.Core;
using Pulumi.Azure.CosmosDB.Inputs;

namespace Infra.TableStorage
{
    public class Atd15Cosmos
    {
        public Pulumi.Azure.CosmosDB.Account CosmosDB { get; set; }

        public Atd15Cosmos(ResourceGroup resourceGroup)
        {
            AccountGeoLocationsArgs accountGeoLocationsArgs = new AccountGeoLocationsArgs()
            {
                FailoverPriority = 0,
                Location = ConfigModel.ConfigValues["location"].ToString()
            };

            var geoLocations = new Pulumi.InputList<AccountGeoLocationsArgs>()
            { accountGeoLocationsArgs };

            var consistencyPolicy = new AccountConsistencyPolicyArgs()
            {
                ConsistencyLevel = "Session",
                MaxIntervalInSeconds = 10,
                MaxStalenessPrefix = 100
            };

            var capabilities = new AccountCapabilitiesArgs()
            {
                Name = "EnableTable"
            };

            CosmosDB = new Pulumi.Azure.CosmosDB.Account("atd15cosmosdb", new Pulumi.Azure.CosmosDB.AccountArgs()
            {
                Location = ConfigModel.ConfigValues["location"].ToString(),
                Name = ConfigModel.ConfigValues["projectname"].ToString() + "cd",
                ResourceGroupName = resourceGroup.Name,
                OfferType = "Standard",
                EnableAutomaticFailover = false,
                GeoLocations = geoLocations,
                ConsistencyPolicy = consistencyPolicy,
                Capabilities = capabilities
            });
        }
    }
}
