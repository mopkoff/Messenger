using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Messenger.WebApi.Filters;
using Messenger.DataLayer.SqlServer;

namespace Messenger.WebApi
{
    public static class WebApiConfig
    {

        private static string ConnectionString = @"Data Source=MSI\MESSENGER;
                Initial Catalog=messenger;
                Integrated Security=True;";

        public static void Register(HttpConfiguration config)
        {
            
            var cors = new EnableCorsAttribute("http://127.0.0.1:9000", "origin, content-type, accept, authorization", "GET, POST, PUT, DELETE, OPTIONS, HEAD");
            config.EnableCors(cors);
            // Конфигурация и службы веб-API
            RepositoryBuilder.ConnectionString = ConnectionString;
            // Маршруты веб-API
            config.MapHttpAttributeRoutes();

          // config.MessageHandlers.Add(new OptionsHttpMessageHandler());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
