using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;


namespace StackOverflowServiceWeb
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