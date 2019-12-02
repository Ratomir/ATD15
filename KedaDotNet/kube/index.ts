import * as k8s from "@pulumi/kubernetes";
import * as kx from "@pulumi/kubernetesx";
import * as kubehelper from "./k8provider";
import * as azure from "@pulumi/azure";
import * as docker from "@pulumi/docker";
import * as pulumi from "@pulumi/pulumi";
import { Secret } from "@pulumi/kubernetes/core/v1";

const projectName = "atd15";

const nameApp = `${projectName}-keda`;

const resourceGroup = `${projectName}-rg`;
const clusterName = `${projectName}aks`;
const containerRegistry = `${projectName}cr`;
const eventHub = `${projectName}eh-v1`;
const ehNameNamespace = `${projectName}ehns-v1`;
const storageaccount = `${projectName}store`;
const cosmosdb = `${projectName}cd`;

const kube = kubehelper.getk8sConfigForAzure(resourceGroup, clusterName);
const provider = { provider: kube };
const acr = kubehelper.getContainerRegistryForAzure(resourceGroup, containerRegistry);

const dockercfg = pulumi.all([acr.loginServer, acr.adminUsername, acr.adminPassword])
    .apply(([server, username, password]) => {
        const r: any = {};
        r[server] = {
            email: "ratomir@live.com",
            username,
            password,
        };
        return r;
    });

const secretRegistry = applySecret("docker-secret", {
    metadata: {
        name: "docker-secret"
    },
    data: {
        ".dockercfg": dockercfg.apply(c => Buffer.from(JSON.stringify(c)).toString("base64")),
    },
    type: "kubernetes.io/dockercfg",
});

const dataSecret = applySecret("kedaconfig", {
    apiVersion: "v1",
    metadata: {
        name: "kedaconfig",
        namespace: "default"
    },
    data: {
        "FUNCTIONS_WORKER_RUNTIME": pulumi.interpolate`dotnet`
            .apply(c => Buffer.from(c).toString("base64")),
        "EventHub": pulumi.interpolate`${getEventHubNamespace(resourceGroup, ehNameNamespace)
            .defaultPrimaryConnectionString};EntityPath=${eventHub}`
            .apply(c => Buffer.from(c).toString("base64")),
        "AzureWebJobsStorage": pulumi.interpolate`${kubehelper.getConnectionStorageForAzure(resourceGroup, storageaccount)}`
            .apply(c => Buffer.from(c).toString("base64")),
        "TableStorage": pulumi.interpolate`${kubehelper.getConnectionCosmosDbForAzure(resourceGroup, cosmosdb)}`
            .apply(c => Buffer.from(c).toString("base64"))
    }
});

const registrySecretName = secretRegistry.metadata.name;

const appLabels = { app: nameApp };
const deployment = new k8s.apps.v1.Deployment(nameApp, {
    apiVersion: "apps/v1",
    kind: "Deployment",
    metadata: {
        name: nameApp,
        namespace: "default",
        labels: appLabels,
    },
    spec: {
        replicas: 2,
        selector: {
            matchLabels: appLabels
        },
        template: {
            metadata: {
                labels: appLabels,
            },
            spec: {
                containers: [{
                    name: nameApp,
                    image: "atd15cr.azurecr.io/kedadotnet:v2",
                    resources: {
                        limits: {
                            cpu: "400m",
                            memory: "256Mi"
                        },
                        requests: {
                            cpu: "200m",
                            memory: "64Mi"
                        }
                    },
                    env: [{
                        name: "AzureFunctionsJobHost__functions__0",
                        value: "EventHubFunction"
                    }],
                    envFrom: [{
                        secretRef: {
                            name: "kedaconfig"
                        }
                    }]
                }],
                imagePullSecrets: [{ name: registrySecretName }],
            },
        },
    },
}, provider);

const scaledObject = new k8s.apiextensions.CustomResource("scaledobject", {
    apiVersion: "keda.k8s.io/v1alpha1",
    kind: "ScaledObject",
    metadata: {
        name: nameApp,
        namespace: "default",
        labels: {
            deploymentName: nameApp
        },
    },
    spec: {
        scaleTargetRef: {
            deploymentName: nameApp,
        },
        triggers: [{
            type: "azure-eventhub",
            metadata: {
                type: "eventHubTrigger",
                connection: "EventHub",
                eventHubName: "atd15eh-v1",
                name: "events",
            },
        }],
        minReplicas: 1,
        maxReplicas: 15
    },
}, provider);

const hpa = new k8s.autoscaling.v1.HorizontalPodAutoscaler("hpa", {
    apiVersion: "autoscaling/v1",
    kind: "HorizontalPodAutoscaler",
    metadata: {
        name: nameApp + "-hpa",
        namespace: "default",
        labels: {
            deploymentName: nameApp
        },
    },
    spec: {
        scaleTargetRef: {
            kind: "Deployment",
            name: nameApp,
            apiVersion: "apps/v1",
        },
        minReplicas: 1,
        maxReplicas: 15
    },
}, provider);

function applySecret(name: string, args?: k8s.types.input.core.v1.Secret | undefined, opts?: pulumi.CustomResourceOptions | undefined): k8s.core.v1.Secret {
    return new k8s.core.v1.Secret(name, args, provider);
}

function getEventHubNamespace(resourceGroup: string, eventHubNamespace: string) {
    return azure.eventhub.getNamespace({
        name: eventHubNamespace,
        resourceGroupName: resourceGroup,
    });
}

export const name = deployment.metadata.name;
