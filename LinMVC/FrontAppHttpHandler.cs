using System.Web;
using System.Web.SessionState;

namespace LinMVC
{
    public class FrontAppHttpHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            FrontApp.Handle(context);
        }
    }
}