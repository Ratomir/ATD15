import * as azure from "@pulumi/azure";
import * as k8s from '@pulumi/kubernetes';

export function getk8sConfigForAzure(resourceGroupName: string, clusterName: string): k8s.Provider {
    const aksCluster = azure.containerservice.getKubernetesCluster({ name: clusterName, resourceGroupName: resourceGroupName });

    return new k8s.Provider("aks", {
        kubeconfig: aksCluster.kubeConfigRaw
    });
}

export function getContainerRegistryForAzure(resourceGroupName: string, registryName: string) {
    return azure.containerservice.getRegistry({
        name: "atd15cr",
        resourceGroupName: "atd15-rg",
    });
}

export function getConnectionStorageForAzure(resourceGroupName: string, storageName: string): string {
    const storage = azure.storage.getAccount({
        name: storageName,
        resourceGroupName: resourceGroupName,
    });

    return storage.primaryBlobConnectionString;
}

export function getConnectionCosmosDbForAzure(resourceGroupName: string, cosmosDbAccount: string): string {
    const storage = azure.cosmosdb.getAccount({
        name: cosmosDbAccount,
        resourceGroupName: resourceGroupName,
    });

    return `DefaultEndpointsProtocol=https;AccountName=${cosmosDbAccount};AccountKey=${storage.primaryMasterKey};TableEndpoint=https://${cosmosDbAccount}.table.cosmos.azure.com:443/;`;
}