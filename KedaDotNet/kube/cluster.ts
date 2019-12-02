import * as azure from "@pulumi/azure";
import * as azuread from "@pulumi/azuread";
import * as k8s from "@pulumi/kubernetes";
import * as pulumi from "@pulumi/pulumi";

export class AksCluster {

    public clusterRawConfig: string;

    /**
     *
     */
    constructor(resourceGroupName: string, clusterName: string) {
        
        const aksCluster = azure.containerservice.getKubernetesCluster({name: clusterName, resourceGroupName: resourceGroupName});
        this.clusterRawConfig = aksCluster.kubeConfigRaw;
    }

}