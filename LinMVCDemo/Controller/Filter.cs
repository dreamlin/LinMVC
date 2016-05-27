using LinMVC;
using System;
using System.Web;

namespace LinMVCDemo.Controller
{
    public class Filter
    {
        public bool CheckLogin(HttpBase httpBase)
        {
            return true;
        }
    }
}