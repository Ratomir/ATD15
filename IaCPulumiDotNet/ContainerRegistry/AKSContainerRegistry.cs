using System;
using System.Collections.Generic;
using System.Text;
using Infra.Base;
using Pulumi.Azure.ContainerService;
using Pulumi.Azure.Core;

namespace Infra.ContainerRegistry
{
    public class AKSContainerRegistry
    {
        public Registry ContainerRegsitry { get; set; }
        public AKSContainerRegistry(ResourceGroup resourceGroup)
        {
            ContainerRegsitry = new Registry("containerRegistry", new RegistryArgs()
            {
                AdminEnabled = true,
                Sku = "Basic",
                Location = ConfigModel.ConfigValues["location"].ToString(),
                Name = ConfigModel.ConfigValues["projectname"].ToString() + "cr",
                ResourceGroupName = resourceGroup.Name
            });
        }
    }
}
