using Pulumi;
using Pulumi.Azure.Authorization;
using Pulumi.Azure.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infra.Base
{
    public class BaseResourceModel
    {
        public ResourceGroup ResourceGroup { get; set; }
        public string ResourceName { get; set; }

        public bool RoleAssiniment(Input<string> principalId, string provider, string resourceName, string roleDefinitionName)
        {
            try
            {
                string subscriptionId = Pulumi.Azure.Config.SubscriptionId;
                var role = new Assignment("supernewassign", new AssignmentArgs()
                {
                    RoleDefinitionName = roleDefinitionName,
                    PrincipalId = principalId,
                    Scope = "/subscriptions/" + subscriptionId + "/resourcegroups/" + ResourceGroup.GetResourceName() + "/providers/" + provider + "/" + resourceName
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("RoleAssignment => " + ex.Message);
                throw;
            }
        }
    }
}
