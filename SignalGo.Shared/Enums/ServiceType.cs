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
    /// type of your service class that supported
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// service class is implemented server methods
        /// </summary>
        ServerService,
        /// <summary>
        /// service class is implemented client methods
        /// </summary>
        ClientService,
        /// <summary>
        /// service class is implemented server methods that support httpCalls
        /// </summary>
        HttpService,
        /// <summary>
        ///  service class is implemented server stream methods
        /// </summary>
        StreamService,
        /// <summary>
        /// one way signal go service that client will call then close
        /// </summary>
        OneWayService,
    }

}
