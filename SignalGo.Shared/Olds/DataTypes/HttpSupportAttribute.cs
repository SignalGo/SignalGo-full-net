using System;

namespace SignalGo.Shared.DataTypes
{
    //public class HttpSupportAttribute : ServiceContractAttribute
    //{
    //    public List<string> Addresses { get; set; } = new List<string>();
    //    public HttpSupportAttribute(string address)
    //    {
    //        Addresses.Add(address);
    //    }

    //    public HttpSupportAttribute(string[] addresses)
    //    {
    //        Addresses.AddRange(addresses);
    //    }
    //}

    public enum HttpKeyType
    {
        Cookie = 0,
        ParameterName = 1,
        ExpireField = 2
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class HttpKeyAttribute : Attribute
    {
        /// <summary>
        /// name of header when client request
        /// </summary>
        public string RequestHeaderName { get; set; } = "Cookie";
        /// <summary>
        /// name of header when client getting response
        /// </summary>
        public string ResponseHeaderName { get; set; } = "Set-Cookie";
        /// <summary>
        /// separate char for value of header for example for Set-Cookie header is ';'
        /// </summary>
        public string HeaderValueSeparate { get; set; } = ";";
        /// <summary>
        /// separate char between key and value of header for example for Set-Cookie header is '='
        /// </summary>
        public string HeaderKeyValueSeparate { get; set; } = "=";
        /// <summary>
        /// name of key that you saves your session id
        /// </summary>
        public string KeyName { get; set; } = "_session";
        /// <summary>
        /// add perfix to last of header value
        /// </summary>
        public string Perfix { get; set; } = "; path=/";
        /// <summary>
        /// type of your key
        /// </summary>
        public HttpKeyType KeyType { get; set; } = HttpKeyType.Cookie;
        /// <summary>
        /// name of key parameter when your keytype is ParameterName
        /// </summary>
        public string KeyParameterName { get; set; }

        public HttpKeyAttribute()
        {

        }

        public virtual bool CheckIsExpired(object value)
        {
            if (value is DateTime && (DateTime)value > DateTime.Now)
                return false;
            return true;
        }
    }
}
