using System;
using System.Web;
using System.Web.SessionState;

namespace LinMVC
{
    public class HttpBase
    {
        private HttpContext context;
        private KeyValueContainer urlKeyValue;
        private HttpRequest request;
        private HttpResponse response;
        private HttpSessionState session;
        public HttpBase(HttpContext context, HttpApplication application)
        {
            object urlKeyValue = context.Items["UrlKeyValue"];
            if (urlKeyValue != null)
            {
                this.urlKeyValue = (KeyValueContainer)urlKeyValue;
            }
            this.context = context;
            if (application != null)
            {
                this.request = application.Request;
                this.response = application.Response;
            }
            else
            {
                this.request = context.Request;
                this.response = context.Response;
                this.session = context.Session;
            }
        }

        public HttpContext Context
        {
            get
            {
                return context;
            }
        }

        public HttpRequest Request
        {
            get
            {
                return request;
            }
        }

        public HttpResponse Response
        {
            get
            {
                return response;
            }
        }

        public HttpSessionState Session
        {
            get
            {
                return session;
            }
        }

        public System.Collections.IDictionary Items
        {
            get
            {
                return context.Items;
            }
        }

        public string Url
        {
            get
            {
                return request.Url.ToString();
            }
        }

        public string FilePath
        {
            get
            {
                return request.FilePath.ToString();
            }
        }

        public string GetString(string key, string defaultValue = "")
        {
            string value = request[key];
            if (string.IsNullOrEmpty(value))
            {
                if (urlKeyValue != null)
                {
                    value = urlKeyValue.Get(key);
                }
                if (string.IsNullOrEmpty(value))
                {
                    return defaultValue;
                }
            }
            return value.Trim();
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            string value = GetString(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            int result = 0;
            int.TryParse(value, out result);
            return result;
        }

        public decimal GetDecimal(string key, decimal defaultValue = 0)
        {
            string value = GetString(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            decimal result = 0;
            decimal.TryParse(value, out result);
            return result;
        }

        public string GetCookie(string name)
        {
            HttpCookie cookie = request.Cookies.Get(name);
            if (cookie == null)
                return string.Empty;
            string value = cookie.Value;
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return HttpUtility.UrlDecode(value);
        }

        public void SetCookie(string name, string value, int expires = 0, bool httpOnly = true)
        {
            HttpCookie cookie = new HttpCookie(name);
            cookie.Value = HttpUtility.UrlEncode(value);
            if (expires != 0)
            {
                cookie.Expires = DateTime.Now.AddDays(expires);
            }
            cookie.HttpOnly = httpOnly;
            response.Cookies.Add(cookie);
        }

        public void RemoveCookie(string name)
        {
            SetCookie(name, string.Empty, -1);
        }

        public string GetIP()
        {
            string ip = null;
            try
            {
                ip = request.Headers["Client_ip"];
                if (String.IsNullOrEmpty(ip))
                {
                    ip = request.ServerVariables["CLIENT_IP"];
                    if (String.IsNullOrEmpty(ip))
                    {
                        ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (String.IsNullOrEmpty(ip))
                        {
                            ip = request.ServerVariables["HTTP_CLIENT_IP"];
                            if (String.IsNullOrEmpty(ip))
                            {
                                ip = request.ServerVariables["REMOTE_ADDR"];
                            }
                        }
                    }
                }
                if (String.IsNullOrEmpty(ip))
                {
                    ip = request.UserHostAddress;
                }
                else
                {
                    int x = ip.LastIndexOf(",");
                    if (x > 0)
                    {
                        ip = ip.Substring(0, x);
                    }
                }
            }
            catch
            {
                ip = "0.0.0.0";
            }
            return ip;
        }

        public void Write(string text, string contentType = "text/html", bool endResponse = true)
        {
            response.ClearContent();
            response.ContentType = contentType;
            response.Write(text);
            if (endResponse)
            {
                response.End();
            }
        }

        public void WriteJson(string text, bool endResponse = true)
        {
            Write(text, "application/json", endResponse);
        }

        public void WriteJavascript(string text, bool endResponse = true)
        {
            Write(text, "application/x-javascript", endResponse);
        }

        public void Redirect(string url, bool endResponse = true)
        {
            response.Redirect(url, endResponse);
        }

        public void End()
        {
            response.End();
        }
    }
}
