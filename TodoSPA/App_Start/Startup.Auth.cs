using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace TodoSPA
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app) {

            var audience = ConfigurationManager.AppSettings["ida:Audience"];
            var tenant = ConfigurationManager.AppSettings["ida:Tenant"];

            app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Audience = audience,
                    Tenant = tenant,
                });
        }
    }
}