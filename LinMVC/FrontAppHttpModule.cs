using System;
using System.Web;

namespace LinMVC
{
    public class FrontAppHttpModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += new EventHandler(application_BeginRequest);
        }

        public void Dispose()
        {

        }

        private void application_BeginRequest(Object source, EventArgs e)
        {
            HttpApplication application = (HttpApplication)source;
            HttpContext context = application.Context;
            FrontApp.Handle(context, application);
        }
    }
}
