using System;
using System.Web;

namespace LinMVC
{
    public class FrontApp
    {
        public static void Handle(HttpContext context, HttpApplication application = null)
        {
            string filePath = context.Request.FilePath.ToString().ToLower();
            string fileExt = VirtualPathUtility.GetExtension(filePath);

            //只处理无后缀，或者exts中包含的后缀
            if (fileExt != "")
            {
                if (!SiteConfig.Get().Exts.Contains(fileExt))
                {
                    return;
                }
            }

            //过滤器
            foreach (FilterItem item in SiteConfig.Get().FilterList)
            {
                object result = SiteConfig.Get().Reflection(item.ClassName, item.MethodName, new HttpBase(context, application));
                if (result != null)
                {
                    if (!Convert.ToBoolean(result))
                    {
                        context.Response.Redirect(item.ToUrl, true);
                    }
                }
            }

            string url = SiteConfig.Get().URLRewrite(filePath);
            string content = string.Empty;
            if (string.IsNullOrEmpty(url))
            {
                if (!string.IsNullOrEmpty(fileExt))
                {
                    content = GetContent(filePath.Replace(fileExt, ""), context, application);
                }
                else
                {
                    content = GetContent(filePath, context, application);
                }
            }
            else
            {
                //当使用URLRewrite时
                content = GetContent(url, context, application, true);
            }

            if (string.IsNullOrEmpty(content))
            {
                context.Response.Redirect(SiteConfig.Get().ErrorUrl);
            }
            else
            {
                context.Items["UrlKeyValue"] = null;
                context.Response.AddHeader("Content-Type", "text/html; charset=utf-8");
                context.Response.Write(content);
            }
            context.Response.End();
        }

        private static string GetContent(string filePath, HttpContext context, HttpApplication application, bool rewrite = false)
        {
            string url;
            if (rewrite)
            {
                string[] filePathArray = filePath.Split('?');
                url = filePathArray[0].ToLower();
                if (filePathArray.Length > 1)
                {
                    context.Items["UrlKeyValue"] = new KeyValueContainer(filePathArray[1]);
                }
            }
            else
            {
                url = filePath;
            }

            ControllerItem ci = SiteConfig.Get().GetController(url);
            if (ci == null)
            {
                string[] param = url.Split('/');
                string className;
                string methodName;

                if (param.Length == 2)
                {
                    className = SiteConfig.Get().DefaultController;
                    methodName = param[1];
                }
                else if (param.Length > 2)
                {
                    if (string.IsNullOrEmpty(param[2]))
                    {
                        className = SiteConfig.Get().DefaultController;
                        methodName = param[1];
                    }
                    else
                    {
                        className = param[1];
                        methodName = param[2];
                    }
                }
                else
                {
                    return null;
                }
                ci = SiteConfig.Get().GetController(className, methodName);
                //把找到的ControllerItem放入缓存，以便下次直接从缓存中读取
                if (ci != null)
                {
                    SiteConfig.Get().AddController(url, ci);
                }
            }

            if (ci == null)
                return null;

            if (!string.IsNullOrEmpty(ci.RequestType))
            {
                if (context.Request.RequestType != ci.RequestType)
                    return null;
            }

            object content = SiteConfig.Get().Reflection(ci.FullName, ci.MethodName, new HttpBase(context, application));
            if (content == null)
                return null;
            return (string)content;
        }
    }
}
