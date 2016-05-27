using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace LinMVC
{
    public class SiteConfig
    {
        private static Mutex _Mutex = new Mutex();
        private static SiteConfig _MeObj;
        public static SiteConfig Create(string serverPath, string templateDirectory, string namespaceString = null, bool isDebug = false)
        {
            if (_MeObj != null)
                return _MeObj;

            _Mutex.WaitOne();
            if (_MeObj == null)
                _MeObj = new SiteConfig(serverPath, templateDirectory, namespaceString, isDebug);
            _Mutex.ReleaseMutex();

            return _MeObj;
        }

        public static SiteConfig Get()
        {
            return _MeObj;
        }

        private string _serverPath;
        private string _templateDirectory;
        private string _namespaceString;
        private bool _isDebug;
        private Dictionary<string, List<ControllerItem>> _controller = new Dictionary<string, List<ControllerItem>>();
        private Dictionary<string, ControllerItem> _urLController = new Dictionary<string, ControllerItem>();

        private SiteConfig(string serverPath, string templateDirectory, string namespaceString, bool isDebug)
        {
            this._serverPath = serverPath;
            this._templateDirectory = templateDirectory;
            this._namespaceString = namespaceString;
            this._isDebug = isDebug;
            LoadConfig();
            if (!string.IsNullOrEmpty(namespaceString))
            {
                LoadController();
            }
        }

        public string ServerPath
        {
            get
            {
                return this._serverPath;
            }
        }

        public string TemplateDirectory
        {
            get
            {
                return this._templateDirectory;
            }
        }

        #region LoadConfig
        private List<string> _exts = new List<string>();
        private List<FilterItem> _filterList = new List<FilterItem>();
        private Dictionary<string, string> _rewriteData = new Dictionary<string, string>();
        private string _defaultController;
        private string _errorUrl;

        private void LoadConfig()
        {
            string path = this._serverPath + "App_Data/config.xml";
            if (!System.IO.File.Exists(path))
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            #region UrlRewrite
            XmlNode rewrites = doc.SelectSingleNode("/config/Rewrites");
            if (rewrites != null)
            {
                XmlNodeList xnl = rewrites.ChildNodes;
                foreach (XmlNode xn in xnl)
                {
                    XmlElement xe = (XmlElement)xn;
                    string url = xe.GetAttribute("Url");
                    string regex = xe.GetAttribute("Regex");
                    string toUrl = xe.GetAttribute("ToUrl");
                    if (!string.IsNullOrEmpty(toUrl))
                    {
                        if (!string.IsNullOrEmpty(url))
                        {
                            this._rewriteData.Add(url.Trim().ToLower(), toUrl);
                        }
                        else if (!string.IsNullOrEmpty(regex))
                        {
                            this._rewriteData.Add("^" + regex.Trim().ToLower() + "$", toUrl);
                        }
                    }
                }
            }
            #endregion

            #region Ext
            XmlNode exts = doc.SelectSingleNode("/config/Exts");
            if (exts != null)
            {
                XmlNodeList xnl = exts.ChildNodes;
                foreach (XmlNode xn in xnl)
                {
                    XmlElement xe = (XmlElement)xn;
                    string ext = xe.InnerText;
                    if (!string.IsNullOrEmpty(ext))
                    {
                        this._exts.Add(ext.Trim().ToLower());
                    }
                }
            }
            #endregion

            #region Filter
            XmlNode filters = doc.SelectSingleNode("/config/Filters");
            if (filters != null)
            {
                XmlNodeList xnl = filters.ChildNodes;
                foreach (XmlNode xn in xnl)
                {
                    XmlElement xe = (XmlElement)xn;
                    string className = xe.GetAttribute("ClassName");
                    string methodName = xe.GetAttribute("MethodName");
                    string toUrl = xe.GetAttribute("ToUrl");
                    if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(methodName) && !string.IsNullOrEmpty(toUrl))
                    {
                        FilterItem filter = new FilterItem();
                        filter.ClassName = className.Trim();
                        filter.MethodName = methodName.Trim();
                        filter.ToUrl = toUrl;
                        this._filterList.Add(filter);
                    }
                }
            }
            #endregion

            #region DefaultController
            XmlNode defaultController = doc.SelectSingleNode("/config/DefaultController");
            if (defaultController != null)
            {
                if (!string.IsNullOrEmpty(defaultController.InnerText))
                {
                    this._defaultController = defaultController.InnerText.Trim().ToLower();
                }
            }
            if (string.IsNullOrEmpty(this._defaultController))
            {
                this._defaultController = "root";
            }
            #endregion

            #region ErrorUrl
            XmlNode errorUrl = doc.SelectSingleNode("/config/ErrorUrl");
            if (errorUrl != null)
            {
                if (!string.IsNullOrEmpty(errorUrl.InnerText))
                {
                    this._errorUrl = errorUrl.InnerText.Trim().ToLower();
                }
            }
            if (string.IsNullOrEmpty(this._errorUrl))
            {
                this._errorUrl = "/";
            }
            #endregion
        }

        public List<string> Exts
        {
            get
            {
                return this._exts;
            }
        }

        public string DefaultController
        {
            get
            {
                return this._defaultController;
            }
        }

        public List<FilterItem> FilterList
        {
            get
            {
                return this._filterList;
            }
        }

        public string ErrorUrl
        {
            get
            {
                return this._errorUrl;
            }
        }
        #endregion

        public string URLRewrite(string filePath)
        {
            foreach (string key in this._rewriteData.Keys)
            {
                Regex r = new Regex(key);
                int[] gnums = r.GetGroupNumbers();
                if (gnums.Length == 1)
                {
                    if (key == filePath)
                    {
                        return this._rewriteData[key];
                    }
                }
                else
                {
                    Match m = r.Match(filePath);
                    string turnUrl = this._rewriteData[key];
                    if (m.Success)
                    {
                        int ci = m.Groups.Count;
                        for (int i = 1; i < ci; i++)
                        {
                            turnUrl = turnUrl.Replace("$" + i, m.Groups[i].Value);
                        }
                        return turnUrl;
                    }
                }
            }
            return null;
        }

        private Assembly _assembly;

        private void LoadController()
        {
            List<string> filter = new List<string>();
            filter.Add("ToString");
            filter.Add("Equals");
            filter.Add("GetHashCode");
            filter.Add("GetType");
            string controllerNamespaceString = this._namespaceString + ".Controller";

            this._assembly = Assembly.Load(this._namespaceString);
            Type[] types = this._assembly.GetTypes();
            foreach (Type tp in types)
            {
                if (tp.Namespace != controllerNamespaceString)
                    continue;

                MethodInfo[] mis = tp.GetMethods();
                foreach (MethodInfo mi in mis)
                {
                    if (filter.Contains(mi.Name))
                        continue;

                    if (tp.BaseType.Name == "Object")
                    {
                        ControllerItem ci = new ControllerItem();
                        ci.NamespaceName = tp.Namespace;
                        ci.ClassName = tp.Name;
                        ci.MethodName = mi.Name;
                        ci.Compare = mi.Name.ToLower();
                        ci.FullName = ci.NamespaceName + "." + ci.ClassName;

                        //获取注解，保存至UrlController中
                        Object[] attr = mi.GetCustomAttributes(false);
                        if (attr.Length > 0)
                        {
                            Action action = attr[0] as Action;
                            string requestType = action.RequestType.Trim().ToUpper();
                            if (requestType == "GET")
                            {
                                ci.RequestType = "GET";
                            }
                            else if (requestType == "POST")
                            {
                                ci.RequestType = "POST";
                            }
                            else
                            {
                                ci.RequestType = string.Empty;
                            }
                            if (!string.IsNullOrEmpty(action.Url))
                            {
                                string url = action.Url.Trim().ToLower();
                                if (url.Length > 1 && !action.Url.StartsWith("/"))
                                {
                                    url = "/" + url;
                                }
                                _urLController[url] = ci;

                                if (url.Length > 1 && url.EndsWith("/"))
                                {
                                    url = url.Substring(0, url.Length - 1);
                                    _urLController[url] = ci;
                                }
                                else if (url.Length > 1 && !url.EndsWith("/"))
                                {
                                    url = url + "/";
                                    _urLController[url] = ci;
                                }
                            }
                        }
                        else
                        {
                            ci.RequestType = string.Empty;
                        }
                        //保存至Controller中
                        string className = tp.Name.ToLower();
                        if (!this._controller.ContainsKey(className))
                        {
                            this._controller[className] = new List<ControllerItem>();
                        }
                        this._controller[className].Add(ci);
                    }
                }
            }
        }

        /// <summary>
        /// 通过【缓存】或【Action注解的Url】获取ControllerItem
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ControllerItem GetController(string url)
        {
            if (this._urLController.ContainsKey(url))
            {
                return this._urLController[url];
            }
            return null;
        }

        /// <summary>
        /// 缓存ControllerItem
        /// </summary>
        /// <param name="url"></param>
        /// <param name="controllerItem"></param>
        public void AddController(string url, ControllerItem controllerItem)
        {
            if (this._urLController.ContainsKey(url))
            {
                return;
            }

            _Mutex.WaitOne();
            if (!this._urLController.ContainsKey(url))
            {
                this._urLController[url] = controllerItem;
                if (url.Length > 1 && url.EndsWith("/"))
                {
                    url = url.Substring(0, url.Length - 1);
                    _urLController[url] = controllerItem;
                }
                else if (url.Length > 1 && !url.EndsWith("/"))
                {
                    url = url + "/";
                    _urLController[url] = controllerItem;
                }
            }
            _Mutex.ReleaseMutex();
        }

        public ControllerItem GetController(string controllerClassName, string controllerMethodName)
        {
            if (this._isDebug)
            {
                List<ControllerItem> list = new List<ControllerItem>();
                if (this._controller.ContainsKey(controllerClassName))
                {
                    foreach (ControllerItem item in this._controller[controllerClassName])
                    {
                        if (item.Compare == controllerMethodName)
                        {
                            list.Add(item);
                        }
                    }
                }
                if (list.Count == 1)
                {
                    return list[0];
                }
                else if (list.Count == 0)
                {
                    return null;
                }
                else
                {
                    //找到多个，抛出异常，显示所在命名空间
                    System.Text.StringBuilder message = new System.Text.StringBuilder();
                    message.AppendLine("在多个namespace中找到相同的Controller、Action，分别在：");
                    foreach (ControllerItem item in list)
                    {
                        message.AppendLine(item.NamespaceName);
                    }
                    throw new Exception(message.ToString());
                }
            }
            else
            {
                if (this._controller.ContainsKey(controllerClassName))
                {
                    foreach (ControllerItem item in this._controller[controllerClassName])
                    {
                        if (item.Compare == controllerMethodName)
                        {
                            return item;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 反射调用方法
        /// </summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object Reflection(string className, string methodName, params object[] parameters)
        {
            Type t = this._assembly.GetType(className);
            if (t == null)
                return null;

            MethodInfo mInfo = t.GetMethod(methodName);
            if (mInfo == null)
                return null;

            object obj = Activator.CreateInstance(t);
            return mInfo.Invoke(obj, parameters);
        }
    }
}
