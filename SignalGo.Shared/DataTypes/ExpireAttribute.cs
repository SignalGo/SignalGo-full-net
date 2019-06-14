// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.DataTypes
{
    /// <summary>
    /// expire key will help developers to check client session is expired or no
    /// </summary>
    public class ExpireAttribute : Attribute
    {
        /// <summary>
        /// check if value of expire is done
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public bool CheckExpired(DateTime dateTime)
        {
            if (dateTime > DateTime.Now)
                return false;
            return true;
        }
    }
}
