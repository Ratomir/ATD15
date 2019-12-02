using Pulumi.Azure.AD;
using Pulumi.Random;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infra.Authentication
{
    public class ApplicationForAKS
    {
        public Application AksApplication { get; set; }
        public ServicePrincipal AksServicePrincipal { get; set; }
        public ServicePrincipalPassword AksServicePrincipalPassword { get; set; }

        public ApplicationForAKS(string applicationName)
        {
            AksApplication = new Application(applicationName, new ApplicationArgs()
            {
                Name = applicationName,
                AvailableToOtherTenants = false,
                Oauth2AllowImplicitFlow = true,
                Homepage = "https://homepage",
                ReplyUrls = "https://homepage"
            });
            AksServicePrincipal = new ServicePrincipal("aksServicePrincipal", new ServicePrincipalArgs 
            {
                ApplicationId = AksApplication.ApplicationId
            });

            var password = new RandomPassword("password", new RandomPasswordArgs
            {
                Length = 20,
                Special = true,
            }).Result;

            AksServicePrincipalPassword = new ServicePrincipalPassword("aksServicePrincipalPassword", new ServicePrincipalPasswordArgs() 
            {
                ServicePrincipalId = AksServicePrincipal.Id,
                Value = password,
                EndDate = DateTime.Now.AddYears(10).ToString("yyyy-MM-dd") + "T00:00:00Z"
            });
        }
    }
}
