using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Web.Http;

namespace MandraSoft.PokemonGo.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuration et services API Web
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            // Itinéraires de l'API Web
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                 name: "ControllerOnly",
                 routeTemplate: "api/{controller}"
             );

            // Controller with ID
            // To handle routes like `/api/VTRouting/1`
            config.Routes.MapHttpRoute(
                name: "ControllerAndId",
                routeTemplate: "api/{controller}/{id}",
                defaults: null,
                constraints: new { id = @"^\d+$" } // Only integers 
            );

            // Controllers with Actions
            // To handle routes like `/api/VTRouting/route`
            config.Routes.MapHttpRoute(
                name: "ControllerAndAction",
                routeTemplate: "api/{controller}/{action}"
            );

            GlobalConfiguration.Configuration.MessageHandlers.Insert(0, new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));
        }
    }
}
