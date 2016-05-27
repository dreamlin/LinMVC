using System;
using System.IO;
using NVelocity;
using NVelocity.Context;

namespace LinMVC
{
    public class TemplatePage
    {
        private IContext _IContext;
        private string _templatePath;

        public TemplatePage(string templatePath)
        {
            this._IContext = new VelocityContext();
            this._templatePath = templatePath + ".vm";
        }

        public void SetTitle(string str)
        {
            this._IContext.Put("title", str);
        }

        public void SetKeywords(string str)
        {
            this._IContext.Put("keywords", str);
        }

        public void SetDescription(string str)
        {
            this._IContext.Put("description", str);
        }

        public void Add(string key, object value)
        {
            if (this._IContext.ContainsKey(key))
                this._IContext.Remove(key);
            this._IContext.Put(key, value);
        }

        public string ToHtml()
        {
            try
            {
                return TemplateEngine.Create().GetHtml(this._IContext, this._templatePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteHtml(string fileName)
        {
            try
            {
                StreamWriter rw = new StreamWriter(SiteConfig.Get().ServerPath + fileName, false, System.Text.Encoding.GetEncoding("UTF-8"));
                rw.WriteLine(TemplateEngine.Create().GetHtml(this._IContext, this._templatePath));
                rw.WriteLine("<!-- " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " -->");
                rw.Write("<!-- OK -->");
                rw.Flush();
                rw.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}