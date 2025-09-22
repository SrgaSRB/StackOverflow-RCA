using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthStatusServiceWeb
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            var _ = RoleEnvironment.GetConfigurationSettingValue("AzureStorage");
            return base.OnStart();
        }

        public override void OnStop()
        {
            base.OnStop();
        }
    }
}
