using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SignalGo.Server.DataTypes
{
    /// <summary>
    /// Limit client ip addresses to access
    /// if you want read ip addresses from file you must implement this class and override Get ip addresses method
    /// </summary>
    public class ClientLimitationAttribute : Attribute
    {
        /// <summary>
        /// This list contains IPs allowed to call methods
        /// other ip addresses are denied to call method
        /// </summary>
        public string[] AllowAccessList { get; set; }
        /// <summary>
        /// This list contains IPs not allowed to call methods
        /// other ip addresses are allowed to call method
        /// </summary>
        public string[] DenyAccessList { get; set; }

        //public ClientLimitationAttribute(string[] allowAccessList, string[] denyAccessList)
        //{
        //    AllowAccessList = allowAccessList;
        //    DenyAccessList = denyAccessList;
        //}

        public virtual string[] GetAllowAccessIpAddresses()
        {
            return AllowAccessList;
        }

        public virtual string[] GetDenyAccessIpAddresses()
        {
            return DenyAccessList;
        }
    }
}
