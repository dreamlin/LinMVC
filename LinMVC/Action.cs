using System;

namespace LinMVC
{
    /// <summary>
    /// 注解：用于URLRewrite
    /// </summary>
    public class Action : Attribute
    {
        public string Url = String.Empty;
        /// <summary>
        /// GET、POST请求类型
        /// </summary>
        public string RequestType = String.Empty;
    }
}
