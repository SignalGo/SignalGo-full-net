using System;

namespace SignalGo.Server.DataTypes
{
    /// <summary>
    /// limit client ip addresses to use methods and services
    /// if you want read ip addresses from file you must implement this class and override Get ip addresses methods
    /// </summary>
    public class ClientLimitationAttribute : Attribute
    {
        /// <summary>
        /// just this list can call
        /// if you fill this property another ip addresses have not access to call method
        /// </summary>
        public string[] AllowAccessList { get; set; }
        /// <summary>
        /// just this list blocked to call
        /// if you fill this property another ip addresses have access to call method
        /// </summary>
        public string[] DenyAccessList { get; set; }

        //public ClientLimitationAttribute(string[] allowAccessList, string[] denyAccessList)
        //{
        //    AllowAccessList = allowAccessList;
        //    DenyAccessList = denyAccessList;
        //}

        /// <summary>
        /// get list of ips can access your methods
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetAllowAccessIpAddresses()
        {
            return AllowAccessList;
        }
        /// <summary>
        /// get list of ips blocked methods calls
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetDenyAccessIpAddresses()
        {
            return DenyAccessList;
        }
    }
}
