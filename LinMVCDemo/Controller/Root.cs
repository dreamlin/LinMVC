using System.Web;
using System.Text.RegularExpressions;
using LinMVC;

namespace LinMVCDemo.Controller
{
    public class Root
    {
        [Action(Url = "/")]
        public string Index(HttpBase httpBase)
        {
            TemplatePage templatePage = new TemplatePage("index");
            return templatePage.ToHtml();
        }

        public string List(HttpBase httpBase)
        {
            TemplatePage templatePage = new TemplatePage("list");
            return templatePage.ToHtml();
        }

        public string Item(HttpBase httpBase)
        {
            TemplatePage templatePage = new TemplatePage("/item");
            templatePage.Add("context", httpBase.GetString("context", "默认"));
            return templatePage.ToHtml();
        }

        [Action(Url = "/my/login/")]
        public void Login(HttpBase httpBase)
        {
            httpBase.Write("登陆页");
        }
    }
}