using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SignalGo.Server.DataTypes
{
    /// <summary>
    /// limit client ip addresses
    /// if you want read ip addresses from file you must implement this class and override Get ip addresses methods
    /// </summary>
    public class ClientLimitationAttribute : Attribute
    {
        /// <summary>
        /// just this list can call
        /// if you fill this property other ip addresses cannot call method
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
