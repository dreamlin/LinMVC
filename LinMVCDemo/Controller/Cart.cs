using System;
using System.Collections.Generic;
using System.Web;
using LinMVC;

namespace LinMVCDemo.Controller
{
    public class Cart
    {
        [LinMVC.Action(Url = "/cart")]
        public string Index(HttpBase httpBase)
        {
            TemplatePage templatePage = new TemplatePage("cart");
            return templatePage.ToHtml();
        }
    }
}