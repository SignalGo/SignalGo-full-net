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

    public class HttpKeyAttribute : Attribute
    {
        public Type SettingType { get; set; }
        /// <summary>
        /// name of header when client request
        /// </summary>
        public string RequestHeaderName { get; set; }
        /// <summary>
        /// name of header when client getting response
        /// </summary>
        public string ResponseHeaderName { get; set; }
        /// <summary>
        /// separate char for value of header for example for Set-Cookie header is ';'
        /// </summary>
        public string HeaderValueSeparate { get; set; }
        /// <summary>
        /// separate char between key and value of header for example for Set-Cookie header is '='
        /// </summary>
        public string HeaderKeyValueSeparate { get; set; }
        /// <summary>
        /// name of key that you saves your session id
        /// </summary>
        public string KeyName { get; set; }
        /// <summary>
        /// add perfix to last of header value
        /// </summary>
        public string Perfix { get; set; }
        /// <summary>
        /// is field of expire
        /// </summary>
        public bool IsExpireField { get; set; }


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
