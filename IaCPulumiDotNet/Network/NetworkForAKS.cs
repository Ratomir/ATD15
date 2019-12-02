using Infra.Base;
using Pulumi.Azure.Core;
using Pulumi.Azure.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infra.Network
{
    public class NetworkForAKS
    {

        public VirtualNetwork VirtualNetwork { get; set; }
        public Subnet Subnet { get; set; }

        public NetworkForAKS(ResourceGroup resourceGroup)
        {
            VirtualNetwork = new VirtualNetwork("aksVnet", new VirtualNetworkArgs
            {
                Name = "vn" + ConfigModel.ConfigValues["projectname"] + "aks",
                ResourceGroupName = resourceGroup.Name,
                AddressSpaces = { "10.2.0.0/16" },
            });

            // Create a Subnet for the cluster
            Subnet = new Subnet("aksSubnet", new SubnetArgs
            {
                Name = "vn" + ConfigModel.ConfigValues["projectname"] + "aks001",
                ResourceGroupName = resourceGroup.Name,
                VirtualNetworkName = VirtualNetwork.Name,
                AddressPrefix = "10.2.1.0/24",
            });
        }
    }
}
