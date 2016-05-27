using System.IO;
using System.Threading;
using Commons.Collections;
using NVelocity;
using NVelocity.App;
using NVelocity.Context;
using NVelocity.Runtime;

namespace LinMVC
{
    public class TemplateEngine
    {
        private VelocityEngine _velocit;
        private TemplateEngine(string templateDirectory)
        {
            //创建NVelocity引擎的实例对象
            this._velocit = new VelocityEngine();
            //初始化该实例对象
            ExtendedProperties props = new ExtendedProperties();

            props.AddProperty(RuntimeConstants.RESOURCE_LOADER, "file");
            props.AddProperty(RuntimeConstants.INPUT_ENCODING, "utf-8");
            props.AddProperty(RuntimeConstants.OUTPUT_ENCODING, "utf-8");

            //设置模板所在目录
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_PATH, templateDirectory);
            props.AddProperty(RuntimeConstants.FILE_RESOURCE_LOADER_CACHE, true);
            props.AddProperty("file.resource.loader.modificationCheckInterval", "30");

            this._velocit.Init(props);
        }

        private static Mutex _Mutex = new Mutex();
        private static TemplateEngine _MeObj;
        public static TemplateEngine Create()
        {
            if (_MeObj != null)
                return _MeObj;

            _Mutex.WaitOne();
            if (_MeObj == null)
                _MeObj = new TemplateEngine(SiteConfig.Get().TemplateDirectory);
            _Mutex.ReleaseMutex();

            return _MeObj;
        }

        public string GetHtml(IContext context, string templatePath)
        {
            StringWriter writer = new StringWriter();
            Template template = this._velocit.GetTemplate(templatePath, "utf-8");
            template.Merge(context, writer);
            return writer.ToString();
        }
    }
}