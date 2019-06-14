// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using System;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// http helper for authenticate with session and cookies
    /// </summary>
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
        public string Prefix { get; set; } = "; path=/";
        /// <summary>
        /// name of key parameter when your keytype is ParameterName
        /// </summary>
        public string KeyParameterName { get; set; }
    }
}
