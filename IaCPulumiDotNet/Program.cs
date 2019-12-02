using System.Collections.Generic;
using System.Threading.Tasks;

using Pulumi;
using Pulumi.Azure.Core;
using Pulumi.Azure.Storage;
using Pulumi.Azure.ContainerService;
using Infra.Authentication;
using Infra.Base;
using Infra.Network;
using Infra.AKS;
using Infra.ContainerRegistry;
using Infra.EH;
using Infra.TableStorage;

class Program
{
    static Task<int> Main()
    {
        return Deployment.RunAsync(() =>
        {
            int version = 1;
            ConfigModel config = new ConfigModel();

            // Create an Azure Resource Group
            ResourceGroup resourceGroup = new ResourceGroup(ConfigModel.ConfigValues["projectname"] + "-rg", new ResourceGroupArgs()
            {
                Location = ConfigModel.ConfigValues["location"].ToString(),
                Name = ConfigModel.ConfigValues["projectname"] + "-rg"
            });

            ApplicationForAKS applicationForAKS = new ApplicationForAKS("aks" + ConfigModel.ConfigValues["projectname"]);

            //NetworkForAKS networkForAks = new NetworkForAKS(resourceGroup);

            AKS kubernetesCluster = new AKS(resourceGroup, applicationForAKS.AksApplication, 
                applicationForAKS.AksServicePrincipalPassword);

            AKSContainerRegistry containerRegistry = new AKSContainerRegistry(resourceGroup);

            // Create an Azure Storage Account
            var storageAccount = new Account("storage", new AccountArgs
            {
                Name = ConfigModel.ConfigValues["projectname"] + "store",
                ResourceGroupName = resourceGroup.Name,
                AccountReplicationType = "LRS",
                AccountTier = "Standard",
            });

            KedaEventHub eventHub = new KedaEventHub(resourceGroup, version);

            Atd15Cosmos cosmosDb = new Atd15Cosmos(resourceGroup);

            // Export the connection string for the storage account
            return new Dictionary<string, object>
            {
                { "connectionString", storageAccount.PrimaryConnectionString },
                //{ "kubeconfig", kubernetesCluster.KubernetesCluster.KubeConfigRaw }
                { "eventHub", eventHub.EventHubNamespace.DefaultPrimaryConnectionString },
                { "cosmosDb", cosmosDb.CosmosDB.ConnectionStrings }
            };
        });
    }
}
