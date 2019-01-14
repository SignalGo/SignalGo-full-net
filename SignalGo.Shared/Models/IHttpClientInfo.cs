using SignalGo.Shared.Http;
using System.Collections.Generic;

namespace SignalGo.Shared.Models
{

    public interface IHttpClientInfo
    {
        /// <summary>
        /// status of response for client
        /// </summary>
        System.Net.HttpStatusCode Status { get; set; }
        /// <summary>
        /// headers of request that client sended
        /// </summary>
        IDictionary<string, string[]> RequestHeaders { get; set; }
        /// <summary>
        /// reponse headers to client
        /// </summary>
        IDictionary<string, string[]> ResponseHeaders { get; set; }
        /// <summary>
        /// ip address of client
        /// </summary>
        string IPAddress { get; }
        /// <summary>
        /// bytes of ip address
        /// </summary>
        byte[] IPAddressBytes { get; set; }
        void SetFirstFile(HttpPostedFileInfo fileInfo);
        HttpPostedFileInfo TakeNextFile();
    }
}
