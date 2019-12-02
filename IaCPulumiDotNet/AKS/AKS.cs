using Infra.Base;
using Pulumi.Azure.AD;
using Pulumi.Azure.ContainerService;
using Pulumi.Azure.ContainerService.Inputs;
using Pulumi.Azure.Core;
using Pulumi.Azure.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Pulumi.Tls;

namespace Infra.AKS
{
    public class AKS : BaseResourceModel
    {
        public KubernetesCluster KubernetesCluster { get; set; }

        public AKS(ResourceGroup resourceGroup, Application aksApplication, ServicePrincipalPassword aksServicePrincipalPassword)
        {
            // Now allocate an AKS cluster.
            SetupAKS(resourceGroup, aksApplication, aksServicePrincipalPassword);
        }

        public AKS(ResourceGroup resourceGroup, Subnet subnet, Application aksApplication, ServicePrincipalPassword aksServicePrincipalPassword)
        {
            // Now allocate an AKS cluster.
            SetupAKS(resourceGroup, aksApplication, aksServicePrincipalPassword, subnet);
        }

        public void SetupAKS(ResourceGroup resourceGroup, Application aksApplication, ServicePrincipalPassword aksServicePrincipalPassword, Subnet subnet = null)
        {
            var sshPublicKey = new PrivateKey("ssh-key", new PrivateKeyArgs
            {
                Algorithm = "RSA",
                RsaBits = 4096,
            }).PublicKeyOpenssh;

            if (subnet == null)
            {
                // Now allocate an AKS cluster.
                KubernetesCluster = new KubernetesCluster("aksCluster", new KubernetesClusterArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    Name = ConfigModel.ConfigValues["projectname"].ToString() + "aks",
                    Location = ConfigModel.ConfigValues["location"].ToString(),
                    AgentPoolProfiles =
                    {
                    new KubernetesClusterAgentPoolProfilesArgs
                    {
                        Name = "aksagentpool",
                        Count = 2,
                        VmSize = "Standard_B2s",
                        OsType = "Linux",
                        OsDiskSizeGb = 30
                    }
                    },
                    DnsPrefix = ConfigModel.ConfigValues["projectname"].ToString() + "aks",
                    LinuxProfile = new KubernetesClusterLinuxProfileArgs
                    {
                        AdminUsername = "aksuser",
                        SshKey = new KubernetesClusterLinuxProfileSshKeyArgs
                        {
                            KeyData = sshPublicKey
                        },
                    },
                    ServicePrincipal = new KubernetesClusterServicePrincipalArgs
                    {
                        ClientId = aksApplication.ApplicationId,
                        ClientSecret = aksServicePrincipalPassword.Value,
                    },
                    KubernetesVersion = "1.13.12",
                    RoleBasedAccessControl = new KubernetesClusterRoleBasedAccessControlArgs { Enabled = true },
                });
            }
            else
            {
                // Now allocate an AKS cluster.
                KubernetesCluster = new KubernetesCluster("aksCluster", new KubernetesClusterArgs
                {
                    ResourceGroupName = resourceGroup.Name,
                    Name = ConfigModel.ConfigValues["projectname"].ToString() + "aks",
                    Location = ConfigModel.ConfigValues["location"].ToString(),
                    AgentPoolProfiles =
                    {
                    new KubernetesClusterAgentPoolProfilesArgs
                    {
                        Name = "aksagentpool",
                        Count = 2,
                        VmSize = "Standard_B2s",
                        OsType = "Linux",
                        OsDiskSizeGb = 30,
                        VnetSubnetId = subnet.Id,
                    }
                    },
                    DnsPrefix = ConfigModel.ConfigValues["projectname"].ToString() + "aks",
                    LinuxProfile = new KubernetesClusterLinuxProfileArgs
                    {
                        AdminUsername = "aksuser",
                        SshKey = new KubernetesClusterLinuxProfileSshKeyArgs
                        {
                            KeyData = sshPublicKey
                        },
                    },
                    ServicePrincipal = new KubernetesClusterServicePrincipalArgs
                    {
                        ClientId = aksApplication.ApplicationId,
                        ClientSecret = aksServicePrincipalPassword.Value,
                    },
                    KubernetesVersion = "1.13.12",
                    RoleBasedAccessControl = new KubernetesClusterRoleBasedAccessControlArgs { Enabled = true },
                    NetworkProfile = new KubernetesClusterNetworkProfileArgs
                    {
                        NetworkPlugin = "azure",
                        DnsServiceIp = "10.2.2.254",
                        ServiceCidr = "10.2.2.0/24",
                        DockerBridgeCidr = "172.17.0.1/16",
                    },
                });
            }


        }
    }
}
