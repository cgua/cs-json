using System;

namespace Tashan.Json
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public class JsonKeyAttribute : Attribute
    {
        /// <summary>
        /// 标识 json 键名
        /// </summary>
        public string KeyName { get; set; }
    }
}
