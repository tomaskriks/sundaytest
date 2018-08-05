using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace TaxProviderService
{
    public partial class TaxProviderService : ServiceBase
    {
        public TaxProviderService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:50795");

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "GetRate",
                routeTemplate: "api/Tax/{municipalityName}/{dateString}",
                defaults: new { controller = "Tax" }
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

            string executable = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = (System.IO.Path.GetDirectoryName(executable));
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            HttpSelfHostServer server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }

        protected override void OnStop()
        {
        }
    }
}
