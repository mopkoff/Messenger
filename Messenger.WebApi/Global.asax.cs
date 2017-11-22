using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Messenger.WebApi.Models;
using Messenger.WebApi.Filters;
using System.Data.Entity;
using Microsoft.Extensions.DependencyInjection;


namespace Messenger.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Database.SetInitializer(new ChatDbInitializer());
            //GlobalConfiguration.Configuration.MessageHandlers.Add(new OptionsHttpMessageHandler());
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }


        /* public static void RegisterGlobalFilters(GlobalFilterCollection filters)
         {
             filters.Add(new HandleErrorAttribute());
             filters.Add(new System.Web.Mvc.AuthorizeAttribute());
         }
        public void ConfigureServices(IServiceCollection services)
        {
            services.Add();
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowSpecificOrigin"));
            });
        }*/
    }
}
