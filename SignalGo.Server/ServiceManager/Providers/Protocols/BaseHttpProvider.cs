using SignalGo.Server.Models;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers.Protocols
{
    /// <summary>
    /// provider of http protocol in signalgo server manager
    /// this class will help you to calculate everything comming from http protocol and manage data
    /// </summary>
    internal class BaseHttpProvider : BaseProvider
    {
        /// <summary>
        /// start to read data from client
        /// </summary>
        /// <param name="tcpClient">tcp client connected to your server</param>
        /// <param name="serverBase">server listener of signalgo</param>
        /// <param name="stream">stream to read data from client or write data to client</param>
        /// <param name="client">client of signalgo provider</param>
        /// <returns></returns>
        internal static async Task StartToReadingClientData(ServerBase serverBase, PipeLineStream stream, HttpClientInfo client)
        {
            if (GetHttpMethodName(stream.FirstLine, out string methodName, out string address))
            {
                //check the simple character for the good performance
                //no need to check all of the text

                //the http method is GET
                if (methodName.AsSpan().StartsWith("G"))
                {

                }
                //the http method is POST
                else if (methodName.AsSpan().StartsWith("PO"))
                {

                }
                //the http method is OPTIONS
                else if (methodName.AsSpan().StartsWith("O"))
                {

                }
                //the http method is TRACE and you must check the signalgo SignalGo Service Reference header here
                else if (methodName.AsSpan().StartsWith("T"))
                {

                }
            }
            else
                throw new Exception("Http line not support there is no method name or address!");
        }

        /// <summary>
        /// run method of server http class with address and headers for Get method
        /// </summary>
        internal static async Task RunHttpGetRequest(string address, string content, ServerBase serverBase, PipeLineStream stream, HttpClientInfo client)
        {
            //new line of signalgo because in another OS newline is different
            string newLine = TextHelper.NewLine;

            string fullAddress = address;
            var split = address.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string methodName = split.Length == 1 ? split[0] : split.Last();
            string parameters = "";
            string jsonParameters = null;
            Dictionary<string, string> multiPartParameter = new Dictionary<string, string>();

            string[] sp = methodName.Split(new[] { '?' }, 2);
            if (sp.Length > 1)
            {
                methodName = sp.First();
                parameters = sp.Last();
            }

            string data = null;
            Type serviceType = null;
            MethodInfo method = null;
            try
            {
                foreach (string item in parameters.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] keyValue = item.Split('=', 2);
                    values.Add(new Shared.Models.ParameterInfo() { Name = keyValue.Length == 2 ? keyValue[0] : "", Value = Uri.UnescapeDataString(keyValue.Last()) });
                }

                CallMethodResultInfo<OperationContext> result = await CallHttpMethod(client, address, methodName, values, jsonParameters, serverBase, method, data, newLine, null, null);
                serviceType = result.ServiceType;

            }
            catch (Exception ex)
            {
                // exception = ex;
                if (serverBase.ErrorHandlingFunction != null)
                {
                    ActionResult result = serverBase.ErrorHandlingFunction(ex, serviceType, method, client).ToActionResult();
                    await RunHttpActionResult(client, result.Data, client, serverBase);
                }
                else
                {
                    await SendInternalErrorMessage(ex, address, serviceType, method, serverBase, client, newLine, HttpStatusCode.InternalServerError);
                }
                if (!(ex is SocketException))
                    serverBase.AutoLogger.LogError(ex, "RunHttpRequest");
            }
            finally
            {

            }
        }

        internal static async Task HandleHttpRequest(string methodName, string address, ServerBase serverBase, HttpClientInfo client)
        {
            try
            {
                string newLine = TextHelper.NewLine;
                string headerResponse = client.RequestHeaders.ToString();
                if (methodName.ToLower() == "get" && !string.IsNullOrEmpty(address) && address != "/")
                {
                    if (client.RequestHeaders.ContainsKey("content-type") && client.GetRequestHeaderValue("content-type") == "SignalGo Service Reference")
                    {
                        await SendSignalGoServiceReference(client, serverBase);
                    }
                    else
                    {
                        await RunHttpRequest(serverBase, address, "GET", "", client);
                    }
                    serverBase.DisposeClient(client, null, "AddClient finish get call");
                }
                else if (methodName.ToLower() == "post" && !string.IsNullOrEmpty(address) && address != "/")
                {
                    int indexOfStartedContent = headerResponse.IndexOf(TextHelper.NewLine + TextHelper.NewLine);
                    string content = "";
                    if (indexOfStartedContent > 0)
                    {
                        indexOfStartedContent += 4;
                        content = headerResponse.Substring(indexOfStartedContent, headerResponse.Length - indexOfStartedContent);
                    }

                    if (client.RequestHeaders.ContainsKey("signalgo-servicedetail"))
                    {
                        await GenerateServiceDetails(client, content, serverBase, newLine);
                    }
                    else if (client.RequestHeaders["content-type"] != null && client.GetRequestHeaderValue("content-type").ToLower().Contains("multipart/form-data"))
                    {
                        await RunPostHttpRequestFile(address, "POST", content, client, serverBase);
                    }
                    else if (client.RequestHeaders["content-type"] != null && client.GetRequestHeaderValue("content-type") == "SignalGo Service Reference")
                    {
                        await SendSignalGoServiceReference(client, serverBase);
                    }
                    else
                    {
                        await RunHttpRequest(serverBase, address, "POST", content, client);
                    }
                    serverBase.DisposeClient(client, null, "AddClient finish post call");
                }
                else if (methodName.ToLower() == "options" && !string.IsNullOrEmpty(address) && address != "/")
                {
                    if (serverBase.ProviderSetting.HttpSetting.HandleCrossOriginAccess)
                        AddOriginHeader(client, serverBase);
                    string message = newLine + $"Success" + newLine;
                    client.ResponseHeaders.Add("Content-Type", "text/html; charset=utf-8");
                    client.ResponseHeaders.Add("Connection", "Close");

                    byte[] dataBytes = Encoding.UTF8.GetBytes(message);

                    await SendResponseHeadersToClient(HttpStatusCode.OK, client.ResponseHeaders, client, dataBytes.Length);
                    await SendResponseDataToClient(dataBytes, client);
                    serverBase.DisposeClient(client, null, "AddClient finish post call");
                }
                else if (serverBase.RegisteredServiceTypes.ContainsKey("") && (string.IsNullOrEmpty(address) || address == "/"))
                {
                    await RunIndexHttpRequest(client, serverBase);
                    serverBase.DisposeClient(client, null, "Index Page call");
                }
                else
                {
                    client.ResponseHeaders.Add("Content-Type", "text/html");
                    client.ResponseHeaders.Add("Connection", "Close");

                    byte[] dataBytes = Encoding.UTF8.GetBytes(newLine + "SignalGo Server OK" + newLine);
                    await SendResponseHeadersToClient(HttpStatusCode.OK, client.ResponseHeaders, client, dataBytes.Length);
                    await SendResponseDataToClient(dataBytes, client);
                    serverBase.DisposeClient(client, null, "AddClient http ok signalGo");
                }
            }
            catch (Exception ex)
            {
                if (client.IsOwinClient)
                    throw;
                serverBase.DisposeClient(client, null, "HandleHttpRequest exception");
            }
        }


        /// <summary>
        /// get method name and address of http response
        /// </summary>
        /// <param name="reponse">response string</param>
        /// <param name="methodName">method name of http</param>
        /// <param name="address">address of first line of http</param>
        private static bool GetHttpMethodName(string reponse, out string methodName, out string address)
        {
            string[] lines = reponse.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 1)
            {
                methodName = lines[0];
                address = lines[1];
                return true;
            }
            else
            {
                methodName = "";
                address = "";
                return false;
            }
        }
    }
}
