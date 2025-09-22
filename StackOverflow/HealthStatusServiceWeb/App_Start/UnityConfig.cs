using HealthStatusServiceWeb.Services;
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.WebApi;

namespace HealthStatusServiceWeb.App_Start
{
    public class UnityConfig
    {

        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // 1) čitanje connection string-a iz .cscfg
            var cs = RoleEnvironment.GetConfigurationSettingValue("AzureStorage");

            // 2) registracije (ono što ti je bilo u Program.cs)
            container.RegisterType<HealthCheckService>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(cs));


            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }

    }
}