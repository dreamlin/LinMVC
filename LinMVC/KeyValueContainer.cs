using System.Collections.Generic;
using System.Text;
using System.Web;

namespace LinMVC
{
    public class KeyValueContainer
    {
        private IDictionary<string, string> _itemMap;

        public KeyValueContainer(string encodeString)
        {
            this._itemMap = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(encodeString))
                return;

            //此处，也可用系统自带的方法处理System.Collections.Specialized.NameValueCollection pramas = HttpUtility.ParseQueryString(Request.Url.Query);
            string[] list = encodeString.Split('&');
            foreach (string str in list)
            {
                string[] items = str.Split('=');
                string key = items[0];
                string value = HttpUtility.UrlDecode(items[1]);
                this._itemMap[key] = value;
            }
        }

        public int GetCount()
        {
            return this._itemMap.Count;
        }

        public string Get(string key)
        {
            return this._itemMap.ContainsKey(key) ? this._itemMap[key] : null;
        }

        public void Set(string key, string value)
        {
            this._itemMap[key] = value;
        }

        public void Remove(string key)
        {
            this._itemMap.Remove(key);
        }

        public string ToEncodeString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in this._itemMap)
            {
                if (sb.Length != 0)
                    sb.Append("&");
                sb.Append(string.Format("{0}={1}", pair.Key, HttpUtility.UrlEncode(pair.Value)));
            }
            return sb.ToString();
        }
    }
}
