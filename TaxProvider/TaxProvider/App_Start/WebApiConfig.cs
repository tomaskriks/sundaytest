using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace TaxProvider
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            

            config.Routes.MapHttpRoute(
                name: "GetRate",
                routeTemplate: "api/Tax/{municipalityName}/{dateString}",
                defaults: new {controller = "Tax"}
            );

            config.Routes.MapHttpRoute(
                name: "MunicipalityCustomRoute",
                routeTemplate: "api/Municipality/{action}",
                defaults: new { controller = "Municipality" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
