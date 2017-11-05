using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Messenger.WebApi.Models;
using System.Data.Entity;

namespace Messenger.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Database.SetInitializer(new ChatDbInitializer());
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
