using Microsoft.WindowsAzure.ServiceRuntime;
using StackOverflowServiceWeb.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.WebApi;

namespace StackOverflowServiceWeb.App_Start
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // 1) čitanje connection string-a iz .cscfg
            var cs = RoleEnvironment.GetConfigurationSettingValue("AzureStorage");

            // 2) registracije (ono što ti je bilo u Program.cs)
            container.RegisterType<VoteService>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(cs));

            container.RegisterType<UserService>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(cs));

            container.RegisterType<NotificationQueueService>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(cs));

            container.RegisterFactory<QuestionService>(c =>
            {
                var vote = c.Resolve<VoteService>();
                return new QuestionService(cs, vote);
            }, new ContainerControlledLifetimeManager());

            container.RegisterFactory<CommentService>(c =>
            {
                var user = c.Resolve<UserService>();
                var vote = c.Resolve<VoteService>();
                return new CommentService(cs, user, vote);
            }, new ContainerControlledLifetimeManager());

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}