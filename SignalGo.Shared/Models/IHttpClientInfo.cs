using SignalGo.Shared.Http;

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
        WebHeaderCollection RequestHeaders { get; set; }
        /// <summary>
        /// reponse headers to client
        /// </summary>
        WebHeaderCollection ResponseHeaders { get; set; }
        /// <summary>
        /// ip address of client
        /// </summary>
        string IPAddress { get; set; }
        void SetFirstFile(HttpPostedFileInfo fileInfo);
        HttpPostedFileInfo TakeNextFile();
    }
}
