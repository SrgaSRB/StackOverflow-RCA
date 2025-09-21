using StackOverflowServiceWeb.App_Start;
using System.Web.Http;

namespace StackOverflowServiceWeb
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);  
            UnityConfig.RegisterComponents();                    
        }

    }
}
