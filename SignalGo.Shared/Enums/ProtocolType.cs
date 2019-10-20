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
    /// protocol type of signalgo
    /// </summary>
    public enum ProtocolType : byte
    {
        /// <summary>
        /// unknown protocol
        /// </summary>
        None = 0,
        /// <summary>
        /// http
        /// </summary>
        Http = 1,
        /// <summary>
        /// http over websocket 
        /// </summary>
        Websocket = 2,
        /// <summary>
        /// signalgo duplex real-time
        /// </summary>
        SignalgoDuplex = 3,
        /// <summary>
        /// signalgo one time per request
        /// </summary>
        SignalgoOneway = 4,
        /// <summary>
        /// signalgo streaming
        /// </summary>
        SignalgoStream = 5
    }
}
