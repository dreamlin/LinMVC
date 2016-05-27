
namespace LinMVC
{
    public class FilterItem
    {
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        /// <summary>
        /// 被过滤器拦截后跳转的地址
        /// </summary>
        public string ToUrl { get; set; }
    }
}
