
namespace LinMVC
{
    public class ControllerItem
    {
        public string NamespaceName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        /// <summary>
        /// NamespaceName+ClassName
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// GET、POST请求类型
        /// </summary>
        public string RequestType { get; set; }
        /// <summary>
        /// 小写字母的方法名，用于URL比对
        /// </summary>
        public string Compare { get; set; }
    }
}