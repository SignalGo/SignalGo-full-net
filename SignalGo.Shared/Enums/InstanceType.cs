// Licensed to the ali.visual.studio@gmail.com under one or more agreements.
// The license this file to you under the GNU license.
// See the LICENSE file in the project root for more information.
//https://github.com/Ali-YousefiTelori
//https://github.com/SignalGo/SignalGo-full-net

using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.Shared.Enums
{
    /// <summary>
    /// type of instance
    /// when cllient connect to servevr and registering service, service class get new instance
    /// </summary>
    public enum InstanceType
    {
        /// <summary>
        /// single instance for all of user
        /// </summary>
        SingleInstance = 1,
        /// <summary>
        /// create new instance per user connection
        /// </summary>
        MultipeInstance = 2,
    }

}
