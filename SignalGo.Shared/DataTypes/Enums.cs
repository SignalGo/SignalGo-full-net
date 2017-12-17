using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo
{
    /// <summary>
    /// methods call and callback type
    /// </summary>
    public enum DataType : byte
    {
        /// <summary>
        /// correct byte
        /// </summary>
        Unkwnon = 0,
        /// <summary>
        /// method must call
        /// </summary>
        CallMethod = 1,
        /// <summary>
        /// response called method
        /// </summary>
        ResponseCallMethod = 2,
        /// <summary>
        /// register a file connection for download
        /// </summary>
        RegisterFileDownload = 3,
        /// <summary>
        /// register a file connection for upload
        /// </summary>
        RegisterFileUpload = 4,
        /// <summary>
        /// ping pong between client and server
        /// </summary>
        PingPong = 5,
        /// <summary>
        /// get details of service like methods
        /// </summary>
        GetServiceDetails = 6,
        /// <summary>
        /// get details of method parameters
        /// </summary>
        GetMethodParameterDetails = 7,
        /// <summary>
        /// flush stream for client side to get position of upload file
        /// </summary>
        FlushStream = 8,
    }

    /// <summary>
    /// compress mode byte
    /// </summary>
    public enum CompressMode : byte
    {
        /// <summary>
        /// no compress
        /// </summary>
        None = 0,
        /// <summary>
        /// zip compress
        /// </summary>
        Zip = 1
    }

    /// <summary>
    /// mode of security
    /// </summary>
    public enum SecurityMode : byte
    {
        /// <summary>
        /// none security
        /// </summary>
        None = 0,
        /// <summary>
        /// rsa and aes security encryption data
        /// </summary>
        RSA_AESSecurity = 1,
    }

    /// <summary>
    /// ignore a property or class in call or receive method
    /// </summary>
    public enum LimitExchangeType : byte
    {
        /// <summary>
        /// Limit this in all incoming calls
        /// for example: you calling server method from client, if client sent value server skip this and set to null
        /// </summary>
        IncomingCall = 0,
        /// <summary>
        /// Limit this in all outgoing call
        /// for example: you calling client method from server this is one outgoig call
        /// </summary>
        OutgoingCall = 1,
        /// <summary>
        /// Limit all incoming and outgoing call
        /// </summary>
        Both = 2
    }

    /// <summary>
    /// type of custom data exchanger
    /// </summary>
    public enum CustomDataExchangerType
    {
        /// <summary>
        /// if use take system will take properties for serialize
        /// </summary>
        Take,
        /// <summary>
        /// if use ignore system ignore properties for serialize
        /// </summary>
        Ignore
    }
}
