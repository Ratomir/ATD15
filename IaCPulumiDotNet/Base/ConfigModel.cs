using Pulumi;
using Pulumi.Azure.Core;
using System.Collections.Generic;

namespace Infra.Base
{
    public class ConfigModel
    {
        public static Dictionary<string, object> ConfigValues = new Dictionary<string, object>();
        public ConfigModel()
        {
            Config pulumiConfig = new Config();

            ConfigValues.Add("env", pulumiConfig.Require("env"));
            ConfigValues.Add("location", pulumiConfig.Require("location"));
            ConfigValues.Add("projectname", "ATD15".ToLower());
        }
    }
}
