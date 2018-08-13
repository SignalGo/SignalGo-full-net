using Newtonsoft.Json.Linq;
using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SignalGo.Server.ServiceManager.Providers
{
    /// <summary>
    /// manage data providing of http and https services
    /// </summary>
    public class HttpProvider : BaseProvider
    {
        public static void StartToReadingClientData(TcpClient tcpClient, ServerBase serverBase, CustomStreamReader reader, string headerResponse)
        {
            ClientInfo client = null;
            try
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    headerResponse += line;
                    if (line == "\r\n")
                        break;
                }
                if (headerResponse.Contains("Sec-WebSocket-Key"))
                {
                    client = serverBase.ServerDataProvider.CreateClientInfo(false, tcpClient);
                    client.IsWebSocket = true;
                    var key = headerResponse.Replace("ey:", "`").Split('`')[1].Replace("\r", "").Split('\n')[0].Trim();
                    var acceptKey = AcceptKey(ref key);
                    var newLine = "\r\n";

                    //var response = "HTTP/1.1 101 Switching Protocols" + newLine
                    var response = "HTTP/1.0 101 Switching Protocols" + newLine
                     + "Upgrade: websocket" + newLine
                     + "Connection: Upgrade" + newLine
                     + "Sec-WebSocket-Accept: " + acceptKey + newLine + newLine;
                    var bytes = System.Text.Encoding.UTF8.GetBytes(response);
                    client.ClientStream.Write(bytes, 0, bytes.Length);
                    SignalGoDuplexServiceProvider.StartToReadingClientData(client, serverBase);
                }
                else
                {
                    client = serverBase.ServerDataProvider.CreateClientInfo(true, tcpClient);

                    string[] lines = null;
                    if (headerResponse.Contains("\r\n\r\n"))
                        lines = headerResponse.Substring(0, headerResponse.IndexOf("\r\n\r\n")).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    else
                        lines = headerResponse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var newLine = "\r\n";
                    string response = "";
                    if (lines.Length > 0)
                    {
                        var methodName = GetHttpMethodName(lines[0]);
                        var address = GetHttpAddress(lines[0]);
                        if (methodName.ToLower() == "get" && !string.IsNullOrEmpty(address) && address != "/")
                        {
                            var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                            if (headers["content-type"] != null && headers["content-type"] == "SignalGo Service Reference")
                            {
                                var doClient = (HttpClientInfo)client;
                                doClient.RequestHeaders = headers;
                                SendSignalGoServiceReference(doClient, serverBase);
                            }
                            else
                                RunHttpRequest(serverBase, address, "GET", "", headers, (HttpClientInfo)client);
                            serverBase.DisposeClient(client, "AddClient finish get call");
                        }
                        else if (methodName.ToLower() == "post" && !string.IsNullOrEmpty(address) && address != "/")
                        {
                            var indexOfStartedContent = headerResponse.IndexOf("\r\n\r\n");
                            string content = "";
                            if (indexOfStartedContent > 0)
                            {
                                indexOfStartedContent += 4;
                                content = headerResponse.Substring(indexOfStartedContent, headerResponse.Length - indexOfStartedContent);
                            }
                            var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                            if (headers["content-type"] != null && headers["content-type"].ToLower().Contains("multipart/form-data"))
                            {
                                RunPostHttpRequestFile(address, "POST", content, headers, (HttpClientInfo)client);
                            }
                            else if (headers["content-type"] != null && headers["content-type"] == "SignalGo Service Reference")
                            {
                                SendSignalGoServiceReference((HttpClientInfo)client, serverBase);
                            }
                            else
                            {
                                RunHttpRequest(serverBase, address, "POST", content, headers, (HttpClientInfo)client);
                            }
                            serverBase.DisposeClient(client, "AddClient finish post call");
                        }
                        else if (methodName.ToLower() == "options" && !string.IsNullOrEmpty(address) && address != "/")
                        {
                            string settingHeaders = "";
                            var headers = GetHttpHeaders(lines.Skip(1).ToArray());

                            if (serverBase.ProviderSetting.HttpSetting.HandleCrossOriginAccess)
                            {
                                settingHeaders = "Access-Control-Allow-Origin: " + headers["origin"] + newLine +
                                "Access-Control-Allow-Credentials: true" + newLine
                                //"Access-Control-Allow-Methods: " + "POST,GET,OPTIONS" + newLine
                                ;

                                if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                                {
                                    settingHeaders += "Access-Control-Allow-Headers: " + headers["Access-Control-Request-Headers"] + newLine;
                                }
                            }
                            string message = newLine + $"Success" + newLine;
                            response = $"HTTP/1.1 {(int)HttpStatusCode.OK} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.OK)}" + newLine
                                + "Content-Type: text/html; charset=utf-8" + newLine
                                + settingHeaders
                                + "Connection: Close" + newLine;
                            var bytesResult = System.Text.Encoding.UTF8.GetBytes(response + message);
                            client.ClientStream.Write(bytesResult, 0, bytesResult.Length);
                            serverBase.DisposeClient(client, "AddClient finish post call");
                        }
                        else if (serverBase.RegisteredServiceTypes.ContainsKey("") && (string.IsNullOrEmpty(address) || address == "/"))
                        {
                            var headers = GetHttpHeaders(lines.Skip(1).ToArray());
                            RunIndexHttpRequest(headers, (HttpClientInfo)client);
                            serverBase.DisposeClient(client, "Index Page call");
                        }
                    }

                    response = "HTTP/1.1 200 OK" + newLine
                         + "Content-Type: text/html" + newLine
                         + "Connection: Close" + newLine;
                    var bytes = System.Text.Encoding.ASCII.GetBytes(response + newLine + "SignalGo Server OK" + newLine);
                    client.ClientStream.Write(bytes, 0, bytes.Length);
                    serverBase.DisposeClient(client, "AddClient http ok signalGo");
                }
            }
            catch (Exception ex)
            {
                serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase HttpProvider StartToReadingClientData");
                serverBase.DisposeClient(client, "HttpProvider StartToReadingClientData exception");
            }
        }


        /// <summary>
        /// Guid for web socket client connection
        /// </summary>
        private static string _guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        /// <summary>
        /// Accept key for websoket client
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string AcceptKey(ref string key)
        {
            string longKey = key + _guid;
            byte[] hashBytes = ComputeHash(longKey);
            return Convert.ToBase64String(hashBytes);
        }

        static SHA1 _sha1 = SHA1.Create();
        /// <summary>
        /// Compute sha1 hash
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static byte[] ComputeHash(string str)
        {
            return _sha1.ComputeHash(Encoding.ASCII.GetBytes(str));
        }

        /// <summary>
        /// get method name of http response
        /// </summary>
        /// <param name="reponse">response string</param>
        /// <returns>method name like "GET"</returns>
        private static string GetHttpMethodName(string reponse)
        {
            var lines = reponse.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
                return lines[0];
            return "";
        }

        /// <summary>
        /// get http address from response
        /// </summary>
        /// <param name="reponse">response string</param>
        /// <returns>address</returns>
        private static string GetHttpAddress(string reponse)
        {
            var lines = reponse.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 1)
                return lines[1];
            return "";
        }

        /// <summary>
        /// get http header from response
        /// </summary>
        /// <param name="lines">lines of headers</param>
        /// <returns>http headers</returns>
        private static Shared.Http.WebHeaderCollection GetHttpHeaders(string[] lines)
        {
            Shared.Http.WebHeaderCollection result = new Shared.Http.WebHeaderCollection();
            foreach (var item in lines)
            {
                var keyValues = item.Split(new[] { ':' }, 2);
                if (keyValues.Length > 1)
                {
                    result.Add(keyValues[0], keyValues[1].TrimStart());
                }
            }
            return result;
        }

        /// <summary>
        /// send service reference data to client
        /// </summary>
        /// <param name="client"></param>
        static void SendSignalGoServiceReference(HttpClientInfo client, ServerBase serverBase)
        {
            var stream = client.ClientStream;
            StringBuilder headers = new StringBuilder();

            var referenceData = new ServiceReferenceHelper().GetServiceReferenceCSharpCode(client.RequestHeaders["servicenamespace"], serverBase);
            var reault = Encoding.UTF8.GetBytes(ServerSerializationHelper.SerializeObject(referenceData, serverBase));
            headers.AppendLine($"HTTP/1.1 {(int)HttpStatusCode.OK} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.OK)}");
            headers.AppendLine("Content-Length: " + reault.Length);
            headers.AppendLine("Content-Type: SignalGoServiceType");
            headers.AppendLine();
            var headBytes = Encoding.ASCII.GetBytes(headers.ToString());
            stream.Write(headBytes, 0, headBytes.Length);

            stream.Write(reault, 0, reault.Length);

            var bytes = new byte[1024 * 1024];
            var readCount = stream.Read(bytes, 0, bytes.Length);
            serverBase.DisposeClient(client, "SendSignalGoServiceReference finished");
        }


        /// <summary>
        /// run method of server http class with address and headers
        /// </summary>
        /// <param name="address">address</param>
        /// <param name="headers">headers</param>
        /// <param name="client">client</param>
        private static void RunHttpRequest(ServerBase serverBase, string address, string httpMethod, string content, Shared.Http.WebHeaderCollection headers, HttpClientInfo client)
        {
            var newLine = "\r\n";
            Action<string> sendInternalErrorMessage = (msg) =>
            {
                try
                {
                    //{ 500} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.InternalServerError)}
                    string settingHeaders = "";
                    if (serverBase.ProviderSetting.HttpSetting.HandleCrossOriginAccess)
                    {
                        settingHeaders = "Access-Control-Allow-Origin: " + headers["origin"] + newLine +
                                            "Access-Control-Allow-Credentials: true" + newLine;
                        if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                        {
                            settingHeaders += "Access-Control-Allow-Headers: " + headers["Access-Control-Request-Headers"] + newLine;
                        }
                    }
                    string message = newLine + $"Internal Server Error: {msg}" + newLine;
                    var response = $"HTTP/1.1 {(int)HttpStatusCode.InternalServerError} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.InternalServerError)}" + newLine
                        + "Content-Type: text/html; charset=utf-8" + newLine
                        + settingHeaders +
                        "Content-Length: " + (message.Length - 2) + newLine
                        + "Connection: Close" + newLine;
                    var bytes = System.Text.Encoding.UTF8.GetBytes(response + message);
                    client.ClientStream.Write(bytes, 0, bytes.Length);
                }
                catch (SocketException)
                {

                }
                catch (Exception ex)
                {
                    serverBase.AutoLogger.LogError(ex, "RunHttpGETRequest sendErrorMessage");
                }
            };
            string fullAddress = address;
            address = address.Trim('/');
            var lines = address.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //if (lines.Count <= 1)
            //{
            //    string data = newLine + "SignalGo Error: method not found from address: " + address + newLine;
            //    sendInternalErrorMessage(data);
            //    AutoLogger.LogText(data);
            //}
            //else
            //{
            var methodName = lines.Last();
            if (methodName == address)
                address = "";
            string parameters = "";
            Dictionary<string, string> multiPartParameter = new Dictionary<string, string>();
            if (httpMethod == "GET")
            {
                if (methodName.Contains("?"))
                {
                    var sp = methodName.Split(new[] { '?' }, 2);
                    methodName = sp.First();
                    parameters = sp.Last();
                }
            }
            else if (httpMethod == "POST")
            {
                var len = int.Parse(headers["content-length"]);
                if (content.Length < len)
                {
                    List<byte> resultBytes = new List<byte>();
                    int readedCount = 0;
                    while (readedCount < len)
                    {
                        byte[] buffer = new byte[len - content.Length];
                        var readCount = client.ClientStream.Read(buffer, 0, len - content.Length);
                        if (readCount == 0)
                            throw new Exception("zero byte readed socket disconnected!");
                        resultBytes.AddRange(buffer.ToList().GetRange(0, readCount));
                        readedCount += readCount;
                    }
                    var postResponse = Encoding.UTF8.GetString(resultBytes.ToArray(), 0, resultBytes.Count);
                    content = postResponse;
                }

                methodName = lines.Last();
                parameters = content;
                if (methodName.Contains("?"))
                {
                    var sp = methodName.Split(new[] { '?' }, 2);
                    methodName = sp.First();
                    parameters = sp.Last();
                }
                else if (parameters.StartsWith("----") && parameters.ToLower().Contains("content-disposition"))
                {
                    var boundary = parameters.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    var pValues = parameters.Split(new string[] { boundary }, StringSplitOptions.RemoveEmptyEntries);
                    string name = "";
                    foreach (var valueData in pValues)
                    {
                        if (valueData.ToLower().Contains("content-disposition"))
                        {
                            if (valueData.Replace(" ", "").Contains(";name="))
                            {
                                int index = valueData.ToLower().IndexOf("content-disposition");
                                var header = valueData.Substring(index);
                                var headLen = header.IndexOf("\r\n");
                                header = valueData.Substring(index, headLen);
                                var newData = valueData.Substring(index + headLen + 2);
                                //newData = newData.Split(new string[] { boundary }, StringSplitOptions.RemoveEmptyEntries);
                                if (header.ToLower().IndexOf("content-disposition:") == 0)
                                {
                                    var disp = new CustomContentDisposition(header);
                                    if (disp.Parameters.ContainsKey("name"))
                                        name = disp.Parameters["name"];
                                    newData = newData.Substring(2, newData.Length - 4);
                                    multiPartParameter.Add(name, newData);
                                }
                            }
                        }
                    }
                }
            }


            methodName = methodName.ToLower();

            lines.RemoveAt(lines.Count - 1);
            address = "";
            foreach (var item in lines)
            {
                address += item + "/";
            }
            address = address.TrimEnd('/').ToLower();
            string callGuid = Guid.NewGuid().ToString();
            string data = null;
            try
            {
                if (!string.IsNullOrEmpty(address) && serverBase.RegisteredServiceTypes.ContainsKey(address))
                {
                    //var methods = (from x in RegisteredHttpServiceTypes[address].GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) where x.Name.ToLower() == methodName && x.IsPublic && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) select x).ToList();
                    //if (methods.Count == 0)
                    //{
                    //    data = newLine + "SignalGo Error: Method name not found in method list: " + methodName + newLine;
                    //    sendInternalErrorMessage(data);
                    //    AutoLogger.LogText(data);
                    //    return;
                    //}

                    List<Shared.Models.ParameterInfo> values = new List<Shared.Models.ParameterInfo>();
                    if (multiPartParameter.Count > 0)
                    {
                        foreach (var item in multiPartParameter)
                        {
                            values.Add(new Shared.Models.ParameterInfo() { Name = item.Key, Value = item.Value });
                        }
                    }
                    else if (headers["content-type"] == "application/json")
                    {
                        try
                        {
                            JObject des = JObject.Parse(parameters);
                            //if (IsMethodInfoOfJsonParameters(methods, des.Properties().Select(x => x.Name).ToList()))
                            //{
                            foreach (var item in des.Properties())
                            {
                                var value = des.GetValue(item.Name);
                                values.Add(new Shared.Models.ParameterInfo() { Name = item.Name, Value = value.ToString() });
                            }
                            //}
                            //else
                            //    values.Add(new Shared.Models.ParameterInfo() { Name = "", Value = parameters });
                        }
                        catch (Exception ex)
                        {
                            serverBase.AutoLogger.LogError(ex, $"Parse json exception: {parameters}");
                        }
                    }
                    else
                    {
                        parameters = parameters.Trim('&');
                        if (!string.IsNullOrEmpty(parameters))
                        {
                            foreach (var item in parameters.Split(new[] { '&' }))
                            {
                                var keyValue = item.Split(new[] { '=' }, 2);
                                values.Add(new Shared.Models.ParameterInfo() { Name = keyValue.Length == 2 ? keyValue[0] : "", Value = Uri.UnescapeDataString(keyValue.Last()) });
                            }
                        }
                        //                            if (!string.IsNullOrEmpty(content))
                        //                            {
                        //#if (NET35)
                        //#else
                        //                                content = System.Net.WebUtility.HtmlDecode(content);
                        //#endif
                        //                            }
                    }
                    var result = CallMethod(address, _guid, methodName, values.ToArray(), client, "", serverBase, out List<HttpKeyAttribute> httpKeyAttributes);

                    if (result.IsException || result.IsAccessDenied)
                    {
                        //#if (NET35)
                        //                        data = newLine + $"SignalGo Error: Method name not found: " + methodName + $" values : {values.Count}" + newLine;
                        //#else
                        //                        data = newLine + $"SignalGo Error: Method name not found: " + methodName + $" values : {string.Join(",", values)}" + newLine;
                        //#endif
                        data = newLine + result.Data + newLine;

                        sendInternalErrorMessage(data);
                        serverBase.AutoLogger.LogText(data);
                        return;
                    }

                    //MethodsCallHandler.BeginHttpMethodCallAction?.Invoke(client, callGuid, address, method, valueitems);
                    //service = Activator.CreateInstance(RegisteredHttpServiceTypes[address]);
                    //if (service is IHttpClientInfo)
                    //{
                    //    ((IHttpClientInfo)service).RequestHeaders = client.RequestHeaders = headers;
                    //    ((IHttpClientInfo)service).ResponseHeaders = client.ResponseHeaders;
                    //    ((IHttpClientInfo)service).IPAddress = client.IPAddress;
                    //}
                    //client.RequestHeaders = headers;
                    if (serverBase.ProviderSetting.HttpSetting.HandleCrossOriginAccess)
                    {
                        client.ResponseHeaders.Add("Access-Control-Allow-Origin", headers["origin"]);
                        client.ResponseHeaders.Add("Access-Control-Allow-Credentials", "true");
                        if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                        {
                            client.ResponseHeaders.Add("Access-Control-Allow-Headers", headers["Access-Control-Request-Headers"]);
                        }
                    }
                    //var securityAttributes = method.GetCustomAttributes(typeof(SecurityContractAttribute), true).ToList();
                    //foreach (SecurityContractAttribute attrib in securityAttributes)
                    //{
                    //    if (!attrib.CheckHttpPermission(client, (service is IHttpClientInfo) ? (IHttpClientInfo)service : null, address, methodName, fullAddress, resultParameters))
                    //    {
                    //        result = attrib.GetHttpValueWhenDenyPermission(client, (service is IHttpClientInfo) ? (IHttpClientInfo)service : null, address, methodName, fullAddress, resultParameters);
                    //        if (result == null)
                    //        {
                    //            data = newLine + $"result from method invoke {methodName}, is null or is not ActionResult type" + address + newLine;
                    //            sendInternalErrorMessage(data);
                    //            AutoLogger.LogText("RunHttpGETRequest : " + data);
                    //        }
                    //        else
                    //        {
                    //            RunHttpActionResult(client, result, client);
                    //        }
                    //        return;
                    //    }
                    //}


                    FillReponseHeaders(client, httpKeyAttributes);

                    if (result.Data == null)
                    {
                        data = newLine + $"result from method invoke {methodName}, is null " + address + newLine;
                        sendInternalErrorMessage(data);
                        serverBase.AutoLogger.LogText("RunHttpGETRequest : " + data);
                    }
                    else
                    {
                        RunHttpActionResult(client, result, client);
                    }
                }
                else
                {
                    method = (from x in RegisteredHttpServiceTypes[""].GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) where x.IsPublic && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) && x.GetCustomAttributes<HomePageAttribute>().Count() > 0 select x).FirstOrDefault();
                    if (method == null)
                    {
                        data = newLine + "SignalGo Error: Index Method name not found!" + newLine;
                        sendInternalErrorMessage(data);
                        AutoLogger.LogText(data);
                        return;
                    }

                    var clientLimitationAttribute = method.GetCustomAttributes(typeof(ClientLimitationAttribute), true).ToList();

                    foreach (ClientLimitationAttribute attrib in clientLimitationAttribute)
                    {
                        var allowAddresses = attrib.GetAllowAccessIpAddresses();
                        if (allowAddresses != null && allowAddresses.Length > 0)
                        {
                            if (!allowAddresses.Contains(client.IPAddress))
                            {
                                data = newLine + $"Client IP Have Not Access To Call Method: {client.IPAddress}" + newLine;
                                sendInternalErrorMessage(data);
                                AutoLogger.LogText(data);
                                return;
                            }
                        }
                        else
                        {
                            var denyAddresses = attrib.GetDenyAccessIpAddresses();
                            if (denyAddresses != null && denyAddresses.Length > 0)
                            {
                                if (denyAddresses.Contains(client.IPAddress))
                                {
                                    data = newLine + $"Client IP Is Deny Access To Call Method: {client.IPAddress}" + newLine;
                                    sendInternalErrorMessage(data);
                                    AutoLogger.LogText(data);
                                    return;
                                }
                            }
                        }
                    }


                    MethodsCallHandler.BeginHttpMethodCallAction?.Invoke(client, callGuid, "", method, null);
                    service = Activator.CreateInstance(RegisteredHttpServiceTypes[""]);
                    if (service is IHttpClientInfo)
                    {
                        ((IHttpClientInfo)service).RequestHeaders = client.RequestHeaders = headers;
                        ((IHttpClientInfo)service).ResponseHeaders = client.ResponseHeaders;
                        ((IHttpClientInfo)service).IPAddress = client.IPAddress;
                    }
                    client.RequestHeaders = headers;
                    if (HttpProtocolSetting != null)
                    {
                        if (HttpProtocolSetting.HandleCrossOriginAccess)
                        {
                            client.ResponseHeaders.Add("Access-Control-Allow-Origin", headers["origin"]);
                            client.ResponseHeaders.Add("Access-Control-Allow-Credentials", "true");
                            if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                            {
                                client.ResponseHeaders.Add("Access-Control-Allow-Headers", headers["Access-Control-Request-Headers"]);
                            }
                        }
                    }

                    var securityAttributes = method.GetCustomAttributes(typeof(SecurityContractAttribute), true).ToList();
                    foreach (SecurityContractAttribute attrib in securityAttributes)
                    {
                        if (!attrib.CheckHttpPermission(client, (service is IHttpClientInfo) ? (IHttpClientInfo)service : null, "", method.Name, "", null))
                        {
                            result = attrib.GetHttpValueWhenDenyPermission(client, (service is IHttpClientInfo) ? (IHttpClientInfo)service : null, "", method.Name, "", null);
                            if (result == null)
                            {
                                data = newLine + $"result from method invoke {method.Name}, is null or is not ActionResult type" + newLine;
                                sendInternalErrorMessage(data);
                                AutoLogger.LogText("RunHttpGETRequest : " + data);
                            }
                            else
                            {
                                RunHttpActionResult(client, result, client);
                            }
                            return;
                        }
                    }

                    bool isStaticLock = method.GetCustomAttributes(typeof(StaticLockAttribute), true).Count() > 0;
                    if (isStaticLock)
                    {
                        lock (StaticLockObject)
                        {
                            result = method.Invoke(service, new object[] { address, methodName, valueitems }).ToActionResult();
                        }
                    }
                    else
                        result = method.Invoke(service, new object[] { address, methodName, valueitems }).ToActionResult();

                    List<HttpKeyAttribute> httpKeyAttributes = new List<HttpKeyAttribute>();
                    var httpKeyOnMethod = (HttpKeyAttribute)method.GetCustomAttributes(typeof(HttpKeyAttribute), true).FirstOrDefault();
                    if (httpKeyOnMethod != null)
                        httpKeyAttributes.Add(httpKeyOnMethod);
                    if (InternalSetting.HttpKeyResponses != null)
                    {
                        httpKeyAttributes.AddRange(InternalSetting.HttpKeyResponses);
                    }

                    FillReponseHeaders(client, httpKeyAttributes);

                    if (result == null)
                    {
                        data = newLine + $"result from index method invoke, is null " + newLine;
                        sendInternalErrorMessage(data);
                        AutoLogger.LogText("RunHttpGETRequest : " + data);
                    }
                    else
                    {
                        RunHttpActionResult(client, result, client);
                    }
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                if (HTTPErrorHandlingFunction != null)
                {
                    result = HTTPErrorHandlingFunction(ex).ToActionResult();
                    RunHttpActionResult(client, result, client);
                }
                else
                {
                    data = newLine + ex.ToString() + address + newLine;
                    sendInternalErrorMessage(data);
                }
                if (!(ex is SocketException))
                    AutoLogger.LogError(ex, "RunHttpRequest");
            }
            finally
            {
                ClientConnectedCallingCount--;
                MethodsCallHandler.EndHttpMethodCallAction?.Invoke(client, callGuid, address, method, valueitems, result, exception);
            }
        }

        static void FillReponseHeaders(HttpClientInfo client, List<HttpKeyAttribute> httpKeyAttributes)
        {
            foreach (var item in httpKeyAttributes)
            {
                if (item.SettingType == null)
                    throw new Exception("you made HttpKeyAttribute top of your method but this have not fill SettingType property");
                var contextResult = OperationContextBase.GetCurrentSetting(item.SettingType);

                if (contextResult != null)
                {
                    var property = contextResult.GetType().GetListOfProperties().Select(x => new { Info = x, Attribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => !y.IsExpireField), ExpiredAttribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => y.IsExpireField) }).FirstOrDefault(x => x.Attribute != null);
                    if (property != null)
                    {
                        if (!client.ResponseHeaders.ExistHeader(property.Attribute.ResponseHeaderName))
                        {
                            client.ResponseHeaders[property.Attribute.ResponseHeaderName] = OperationContextBase.IncludeValue((string)property.Info.GetValue(contextResult, null), property.Attribute.KeyName, property.Attribute.HeaderValueSeparate, property.Attribute.HeaderKeyValueSeparate) + property.Attribute.Perfix;
                        }
                    }
                }
            }

        }
        /// <summary>
        /// run method of server http class with address and headers
        /// </summary>
        /// <param name="address">address</param>
        /// <param name="headers">headers</param>
        /// <param name="client">client</param>
        private static void RunPostHttpRequestFile(string address, string httpMethod, string content, Shared.Http.WebHeaderCollection headers, HttpClientInfo client)
        {
            var newLine = "\r\n";
            Action<string> sendInternalErrorMessage = (data) =>
            {
                try
                {
                    //{ 500} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.InternalServerError)}
                    string settingHeaders = "";
                    if (HttpProtocolSetting != null)
                    {
                        if (HttpProtocolSetting.HandleCrossOriginAccess)
                        {
                            settingHeaders = "Access-Control-Allow-Origin: " + headers["origin"] + newLine +
                                                "Access-Control-Allow-Credentials: true" + newLine;
                            if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                            {
                                settingHeaders += "Access-Control-Allow-Headers: " + headers["Access-Control-Request-Headers"] + newLine;
                            }
                        }
                    }
                    string message = newLine + $"Internal Server Error: {data}" + newLine;

                    var response = $"HTTP/1.1 {(int)HttpStatusCode.InternalServerError} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.InternalServerError)}" + newLine
                                    + "Content-Type: text/html; charset=utf-8" + newLine
                                    + settingHeaders +
                                    "Content-Length: " + (message.Length - 2) + newLine
                                    + "Connection: Close" + newLine;
                    //Console.WriteLine(response + message);
                    client.ClientStream.Send(System.Text.Encoding.UTF8.GetBytes(response + message));
                    Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "RunHttpGETRequest sendErrorMessage");
                }
            };
            string fullAddress = address;
            address = address.Trim('/');
            var lines = address.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lines.Count <= 1)
            {
                string data = newLine + "SignalGo Error: method not found from address: " + address + newLine;
                sendInternalErrorMessage(data);
                AutoLogger.LogText(data);
            }
            else
            {
                var methodName = lines.Last();
                string parameters = "";
                if (methodName.Contains("?"))
                {
                    var sp = methodName.Split(new[] { '?' }, 2);
                    methodName = sp.First();
                    parameters = sp.Last();
                }
                Dictionary<string, string> multiPartParameter = new Dictionary<string, string>();

                var len = int.Parse(headers["content-length"]);
                HttpPostedFileInfo fileInfo = null;
                if (content.Length < len)
                {
                    var boundary = headers["content-type"].Split('=').Last();
                    if (!boundary.Contains("--"))
                        boundary = null;
                    var fileHeaderCount = 0;
                    string response = "";
                    fileHeaderCount = GetHttpFileFileHeader(client.ClientStream, ref boundary, len, out response);
                    //boundary = boundary.TrimStart('-');
                    string contentType = "";
                    string fileName = "";
                    string name = "";
                    bool findFile = false;
                    var lineBreaks = new string[] { boundary.Replace("\"", ""), boundary.Replace("\"", "") + "--", "--" + boundary.Replace("\"", ""), "--" + boundary.Replace("\"", "") + "--" };
                    foreach (var data in response.Split(lineBreaks, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (data.ToLower().Contains("content-disposition"))
                        {
                            if (data.Replace(" ", "").Contains(";filename="))
                            {
                                foreach (var header in data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var index = header.ToLower().IndexOf("content-type: ");
                                    if (index == 0)
                                    {
                                        var ctypeLen = "content-type: ".Length;
                                        contentType = header.Substring(ctypeLen, header.Length - ctypeLen);
                                    }
                                    else if (header.ToLower().IndexOf("content-disposition:") == 0)
                                    {
                                        var disp = new CustomContentDisposition(header);
                                        if (disp.Parameters.ContainsKey("filename"))
                                            fileName = disp.Parameters["filename"];
                                        if (disp.Parameters.ContainsKey("name"))
                                            name = disp.Parameters["name"];
                                    }
                                    findFile = true;
                                }
                                break;
                            }
                            else if (data.ToLower().Contains("content-disposition"))
                            {
                                if (data.Replace(" ", "").Contains(";name="))
                                {
                                    var sp = data.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    var contentHeaders = sp.FirstOrDefault();
                                    var datas = sp.LastOrDefault();
                                    int index = contentHeaders.ToLower().IndexOf("content-disposition");
                                    var header = contentHeaders.Substring(index);
                                    var headLen = data.IndexOf("\r\n");
                                    //var headLen = data.IndexOf("\r\n\r\n");
                                    //header = sp.Length > 1 ? datas : data.Substring(index, headLen);
                                    //var byteData = GoStreamReader.ReadBlockSize(client.TcpClient.GetStream(), (ulong)(len - content.Length - fileHeaderCount));
                                    string newData = sp.Length > 1 ? datas : data.Substring(headLen + 4);//+ 4 Encoding.UTF8.GetString(byteData);
                                    newData = newData.Trim(Environment.NewLine.ToCharArray());
                                    //var newData = text.Substring(0, text.IndexOf(boundary) - 4);
                                    if (header.ToLower().IndexOf("content-disposition:") == 0)
                                    {
                                        var disp = new CustomContentDisposition(header.Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
                                        if (disp.Parameters.ContainsKey("name"))
                                            name = disp.Parameters["name"];
                                        //StringBuilder build = new StringBuilder();
                                        //using (var reader = new StringReader(newData))
                                        //{
                                        //    while (true)
                                        //    {
                                        //        var line = reader.ReadLine();
                                        //        if (line == null)
                                        //            break;
                                        //        else if (lineBreaks.Contains(line))
                                        //            continue;
                                        //        build.AppendLine(line);
                                        //    }
                                        //}
                                        multiPartParameter.Add(name, newData);
                                    }
                                }
                            }
                            var keyValue = data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            if (keyValue.Length == 2)
                            {
                                if (!string.IsNullOrEmpty(parameters))
                                {
                                    parameters += "&";
                                }
                                var disp = new CustomContentDisposition(keyValue[0]);
                                foreach (var prm in disp.Parameters)
                                {
                                    parameters += prm.Key;
                                    parameters += "=" + prm.Value;
                                }

                            }
                        }
                    }
                    if (findFile)
                    {
                        var stream = new StreamGo(client.ClientStream);
                        stream.SetOfStreamLength(len - content.Length - fileHeaderCount, boundary.Length + 12 - 6);// + 6 ; -6 ezafe shode
                        fileInfo = new HttpPostedFileInfo()
                        {
                            Name = name,
                            ContentLength = stream.Length,
                            ContentType = contentType,
                            FileName = fileName,
                            InputStream = stream
                        };
                    }


                    //byte[] buffer = new byte[len * 5];
                    //var readCount = client.TcpClient.Client.Receive(buffer);
                    //// I dont know why 44 bytes(overplus) always sent
                    //var postResponse = Encoding.UTF8.GetString(buffer.ToList().GetRange(0, readCount).ToArray());
                    //content = postResponse;
                }




                methodName = methodName.ToLower();
                lines.RemoveAt(lines.Count - 1);
                address = "";
                foreach (var item in lines)
                {
                    address += item + "/";
                }
                address = address.TrimEnd('/').ToLower();
                if (RegisteredHttpServiceTypes.ContainsKey(address))
                {
                    object result = null;
                    MethodInfo method = null;
                    List<string> valueitems = null;
                    Exception exception = null;
                    string callGuid = Guid.NewGuid().ToString();
                    object service = null;
                    try
                    {
                        ClientConnectedCallingCount++;
                        var methods = (from x in RegisteredHttpServiceTypes[address].GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) where x.Name.ToLower() == methodName && x.IsPublic && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) select x).ToList();
                        if (methods.Count == 0)
                        {
                            string data = newLine + "SignalGo Error: Method name not found in method list: " + methodName + newLine;
                            sendInternalErrorMessage(data);
                            AutoLogger.LogText(data);
                            return;
                        }

                        List<Tuple<string, string>> values = new List<Tuple<string, string>>();
                        if (multiPartParameter.Count > 0)
                        {
                            foreach (var item in multiPartParameter)
                            {
                                values.Add(new Tuple<string, string>(item.Key, item.Value));
                            }
                        }
                        else if (headers["content-type"] == "application/json")
                        {
                            JObject des = JObject.Parse(parameters);
                            foreach (var item in des.Properties())
                            {
                                var value = des.GetValue(item.Name);
                                //values.Add(new Tuple<string, string>(item.Name, Uri.UnescapeDataString(value.Value<string>())));
                                values.Add(new Tuple<string, string>(item.Name, value.ToString()));
                            }
                        }
                        else
                        {
                            parameters = parameters.Trim('&');
                            if (!string.IsNullOrEmpty(parameters))
                            {
                                foreach (var item in parameters.Split(new[] { '&' }))
                                {
                                    var keyValue = item.Split(new[] { '=' }, 2);
                                    values.Add(new Tuple<string, string>(keyValue.Length == 2 ? keyValue[0] : "", Uri.UnescapeDataString(keyValue.Last())));
                                }
                            }
                        }


                        method = FindMethodInfo(methods, values);


                        if (method == null)
                        {
#if (NET35)
                            string data = newLine + $"SignalGo Error: Method name not found: " + methodName + $" values : {values.Count}" + newLine;
#else
                            string data = newLine + $"SignalGo Error: Method name not found: " + methodName + $" values : {string.Join(",", values)}" + newLine;
#endif

                            sendInternalErrorMessage(data);
                            AutoLogger.LogText(data);
                            return;
                        }

                        var clientLimitationAttribute = method.GetCustomAttributes(typeof(ClientLimitationAttribute), true).ToList();

                        foreach (ClientLimitationAttribute attrib in clientLimitationAttribute)
                        {
                            var allowAddresses = attrib.GetAllowAccessIpAddresses();
                            if (allowAddresses != null && allowAddresses.Length > 0)
                            {
                                if (!allowAddresses.Contains(client.IPAddress))
                                {
                                    string data = newLine + $"Client IP Have Not Access To Call Method: {client.IPAddress}" + newLine;
                                    sendInternalErrorMessage(data);
                                    AutoLogger.LogText(data);
                                    return;
                                }
                            }
                            else
                            {
                                var denyAddresses = attrib.GetDenyAccessIpAddresses();
                                if (denyAddresses != null && denyAddresses.Length > 0)
                                {
                                    if (denyAddresses.Contains(client.IPAddress))
                                    {
                                        string data = newLine + $"Client IP Is Deny Access To Call Method: {client.IPAddress}" + newLine;
                                        sendInternalErrorMessage(data);
                                        AutoLogger.LogText(data);
                                        return;
                                    }
                                }
                            }
                        }

                        service = Activator.CreateInstance(RegisteredHttpServiceTypes[address]);
                        if (service is IHttpClientInfo)
                        {
                            ((IHttpClientInfo)service).RequestHeaders = client.RequestHeaders = headers;
                            ((IHttpClientInfo)service).ResponseHeaders = client.ResponseHeaders;
                            ((IHttpClientInfo)service).IPAddress = client.IPAddress;
                        }
                        if (HttpProtocolSetting != null)
                        {
                            if (HttpProtocolSetting.HandleCrossOriginAccess)
                            {
                                client.ResponseHeaders.Add("Access-Control-Allow-Origin", headers["origin"]);
                                client.ResponseHeaders.Add("Access-Control-Allow-Credentials", "true");
                                if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                                {
                                    client.ResponseHeaders.Add("Access-Control-Allow-Headers", headers["Access-Control-Request-Headers"]);
                                }
                            }
                        }
                        if (service is IHttpClientInfo)
                        {
                            ((IHttpClientInfo)service).SetFirstFile(fileInfo);
                        }
                        client.RequestHeaders = headers;
                        client.SetFirstFile(fileInfo);


                        var prms = method.GetParameters();
                        List<object> resultParameters = new List<object>();
                        var noParamNameDetected = (from x in values where string.IsNullOrEmpty(x.Item1) select x).Count() > 0;

                        int index = 0;
                        foreach (var item in prms)
                        {
                            Tuple<string, string> currentParam = null;
                            if (!noParamNameDetected)
                            {
                                currentParam = (from x in values where x.Item1.ToLower() == item.Name.ToLower() select x).FirstOrDefault();
                                if (currentParam == null)
                                {
                                    string data = newLine + $"result from method {methodName}, parameter {item.Name} not exist, your params {content} " + newLine;
                                    sendInternalErrorMessage(data);
                                    AutoLogger.LogText("RunHttpGETRequest : " + data);
                                    return;
                                }
                            }
                            else
                                currentParam = values[index];
                            if (string.IsNullOrEmpty(currentParam.Item2))
                                resultParameters.Add(GetDefault(item.ParameterType));
                            else
                            {
                                var customDataExchanger = method.GetCustomAttributes(typeof(CustomDataExchangerAttribute), true).Cast<CustomDataExchangerAttribute>().Where(x => x.GetExchangerByUserCustomization(client)).ToList();
                                customDataExchanger.AddRange(GetMethodParameterBinds(index, method).Where(x => x.GetExchangerByUserCustomization(client)));
                                var obj = ServerSerializationHelper.DeserializeByValidate(currentParam.Item2, item.ParameterType, this, customDataExchanger: customDataExchanger.ToArray());
                                resultParameters.Add(obj);
                            }
                            index++;
                        }
                        valueitems = values.Select(x => x.Item2).ToList();
                        MethodsCallHandler.BeginHttpMethodCallAction?.Invoke(client, callGuid, address, method, valueitems);
                        var securityAttributes = method.GetCustomAttributes(typeof(SecurityContractAttribute), true).ToList();
                        foreach (SecurityContractAttribute attrib in securityAttributes)
                        {
                            if (!attrib.CheckHttpPermission(client, (service is IHttpClientInfo) ? (IHttpClientInfo)service : null, address, methodName, fullAddress, resultParameters))
                            {
                                result = attrib.GetHttpValueWhenDenyPermission(client, (service is IHttpClientInfo) ? (IHttpClientInfo)service : null, address, methodName, fullAddress, resultParameters);
                                if (result == null)
                                {
                                    string data = newLine + $"result from method invoke {methodName}, is null or is not ActionResult type" + address + newLine;
                                    sendInternalErrorMessage(data);
                                    AutoLogger.LogText("RunHttpGETRequest : " + data);
                                }
                                else
                                {
                                    RunHttpActionResult(client, result, client);
                                }
                                return;
                            }
                        }
                        bool isStaticLock = method.GetCustomAttributes(typeof(StaticLockAttribute), true).Count() > 0;
                        if (isStaticLock)
                        {
                            lock (StaticLockObject)
                            {
                                result = method.Invoke(service, resultParameters.ToArray()).ToActionResult();
                            }
                        }
                        else
                            result = method.Invoke(service, resultParameters.ToArray()).ToActionResult();
                        List<HttpKeyAttribute> httpKeyAttributes = new List<HttpKeyAttribute>();
                        var httpKeyOnMethod = (HttpKeyAttribute)method.GetCustomAttributes(typeof(HttpKeyAttribute), true).FirstOrDefault();
                        if (httpKeyOnMethod != null)
                            httpKeyAttributes.Add(httpKeyOnMethod);
                        if (InternalSetting.HttpKeyResponses != null)
                        {
                            httpKeyAttributes.AddRange(InternalSetting.HttpKeyResponses);
                        }

                        FillReponseHeaders(client, httpKeyAttributes);

                        if (result == null)
                        {
                            string data = newLine + $"result from method invoke {methodName}, is null or is not ActionResult type" + address + newLine;
                            sendInternalErrorMessage(data);
                            AutoLogger.LogText("RunHttpGETRequest : " + data);
                        }
                        else
                        {
                            RunHttpActionResult(client, result, client);
                        }
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                        if (HTTPErrorHandlingFunction != null)
                        {
                            result = HTTPErrorHandlingFunction(ex).ToActionResult();
                            RunHttpActionResult(client, result, client);
                        }
                        else
                        {
                            string data = newLine + ex.ToString() + address + newLine;
                            sendInternalErrorMessage(data);
                        }
                        AutoLogger.LogError(ex, "RunPostHttpRequestFile");
                    }
                    finally
                    {
                        ClientConnectedCallingCount--;
                        MethodsCallHandler.EndHttpMethodCallAction?.Invoke(client, callGuid, address, method, valueitems, result, exception);
                    }
                }
                else
                {
                    string data = newLine + "SignalGo Error: address not found in signalGo services: " + address + newLine;
                    sendInternalErrorMessage(data);
                    AutoLogger.LogText(data);
                }
            }
        }

        static void RunIndexHttpRequest(Shared.Http.WebHeaderCollection headers, HttpClientInfo client)
        {
            var newLine = "\r\n";
            Action<string> sendInternalErrorMessage = (data) =>
            {
                try
                {
                    //{ 500} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.InternalServerError)}
                    string settingHeaders = "";
                    if (HttpProtocolSetting != null)
                    {
                        if (HttpProtocolSetting.HandleCrossOriginAccess)
                        {
                            settingHeaders = "Access-Control-Allow-Origin: " + headers["origin"] + newLine +
                                                "Access-Control-Allow-Credentials: true" + newLine;
                            if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                            {
                                settingHeaders += "Access-Control-Allow-Headers: " + headers["Access-Control-Request-Headers"] + newLine;
                            }
                        }
                    }
                    string message = newLine + $"Internal Server Error: {data}" + newLine;
                    var response = $"HTTP/1.1 {(int)HttpStatusCode.InternalServerError} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.InternalServerError)}" + newLine
                        + "Content-Type: text/html; charset=utf-8" + newLine
                        + settingHeaders +
                        "Content-Length: " + (message.Length - 2) + newLine
                        + "Connection: Close" + newLine;
                    client.ClientStream.Send(System.Text.Encoding.UTF8.GetBytes(response + message));
                }
                catch (Exception ex)
                {
                    AutoLogger.LogError(ex, "RunHttpGETRequest sendErrorMessage");
                }
            };



            object result = null;
            MethodInfo method = null;
            Exception exception = null;
            string callGuid = Guid.NewGuid().ToString();
            object service = null;
            try
            {
                ClientConnectedCallingCount++;
                method = (from x in RegisteredHttpServiceTypes[""].GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) where x.IsPublic && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) && x.GetCustomAttributes<HomePageAttribute>().Count() > 0 select x).FirstOrDefault();
                if (method == null)
                {
                    string data = newLine + "SignalGo Error: Index Method name not found!" + newLine;
                    sendInternalErrorMessage(data);
                    AutoLogger.LogText(data);
                    return;
                }

                var clientLimitationAttribute = method.GetCustomAttributes(typeof(ClientLimitationAttribute), true).ToList();

                foreach (ClientLimitationAttribute attrib in clientLimitationAttribute)
                {
                    var allowAddresses = attrib.GetAllowAccessIpAddresses();
                    if (allowAddresses != null && allowAddresses.Length > 0)
                    {
                        if (!allowAddresses.Contains(client.IPAddress))
                        {
                            string data = newLine + $"Client IP Have Not Access To Call Method: {client.IPAddress}" + newLine;
                            sendInternalErrorMessage(data);
                            AutoLogger.LogText(data);
                            return;
                        }
                    }
                    else
                    {
                        var denyAddresses = attrib.GetDenyAccessIpAddresses();
                        if (denyAddresses != null && denyAddresses.Length > 0)
                        {
                            if (denyAddresses.Contains(client.IPAddress))
                            {
                                string data = newLine + $"Client IP Is Deny Access To Call Method: {client.IPAddress}" + newLine;
                                sendInternalErrorMessage(data);
                                AutoLogger.LogText(data);
                                return;
                            }
                        }
                    }
                }


                MethodsCallHandler.BeginHttpMethodCallAction?.Invoke(client, callGuid, "", method, null);
                service = Activator.CreateInstance(RegisteredHttpServiceTypes[""]);
                if (service is IHttpClientInfo)
                {
                    ((IHttpClientInfo)service).RequestHeaders = client.RequestHeaders = headers;
                    ((IHttpClientInfo)service).ResponseHeaders = client.ResponseHeaders;
                    ((IHttpClientInfo)service).IPAddress = client.IPAddress;
                }
                client.RequestHeaders = headers;
                if (HttpProtocolSetting != null)
                {
                    if (HttpProtocolSetting.HandleCrossOriginAccess)
                    {
                        client.ResponseHeaders.Add("Access-Control-Allow-Origin", headers["origin"]);
                        client.ResponseHeaders.Add("Access-Control-Allow-Credentials", "true");
                        if (!string.IsNullOrEmpty(headers["Access-Control-Request-Headers"]))
                        {
                            client.ResponseHeaders.Add("Access-Control-Allow-Headers", headers["Access-Control-Request-Headers"]);
                        }
                    }
                }

                var securityAttributes = method.GetCustomAttributes(typeof(SecurityContractAttribute), true).ToList();
                foreach (SecurityContractAttribute attrib in securityAttributes)
                {
                    if (!attrib.CheckHttpPermission(client, (service is IHttpClientInfo) ? (IHttpClientInfo)service : null, "", method.Name, "", null))
                    {
                        result = attrib.GetHttpValueWhenDenyPermission(client, (service is IHttpClientInfo) ? (IHttpClientInfo)service : null, "", method.Name, "", null);
                        if (result == null)
                        {
                            string data = newLine + $"result from method invoke {method.Name}, is null or is not ActionResult type" + newLine;
                            sendInternalErrorMessage(data);
                            AutoLogger.LogText("RunHttpGETRequest : " + data);
                        }
                        else
                        {
                            RunHttpActionResult(client, result, client);
                        }
                        return;
                    }
                }

                bool isStaticLock = method.GetCustomAttributes(typeof(StaticLockAttribute), true).Count() > 0;
                if (isStaticLock)
                {
                    lock (StaticLockObject)
                    {
                        result = method.Invoke(service, new object[] { "", "", null }).ToActionResult();
                    }
                }
                else
                    result = method.Invoke(service, new object[] { "", "", null }).ToActionResult();

                List<HttpKeyAttribute> httpKeyAttributes = new List<HttpKeyAttribute>();
                var httpKeyOnMethod = (HttpKeyAttribute)method.GetCustomAttributes(typeof(HttpKeyAttribute), true).FirstOrDefault();
                if (httpKeyOnMethod != null)
                    httpKeyAttributes.Add(httpKeyOnMethod);
                if (InternalSetting.HttpKeyResponses != null)
                {
                    httpKeyAttributes.AddRange(InternalSetting.HttpKeyResponses);
                }

                FillReponseHeaders(client, httpKeyAttributes);

                if (result == null)
                {
                    string data = newLine + $"result from index method invoke, is null " + newLine;
                    sendInternalErrorMessage(data);
                    AutoLogger.LogText("RunHttpGETRequest : " + data);
                }
                else
                {
                    RunHttpActionResult(client, result, client);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                if (HTTPErrorHandlingFunction != null)
                {
                    result = HTTPErrorHandlingFunction(ex).ToActionResult();
                    RunHttpActionResult(client, result, client);
                }
                else
                {
                    string data = newLine + ex.ToString() + newLine;
                    sendInternalErrorMessage(data);
                }
                AutoLogger.LogError(ex, "RunHttpRequest");
            }
            finally
            {
                ClientConnectedCallingCount--;
                MethodsCallHandler.EndHttpMethodCallAction?.Invoke(client, callGuid, "", method, null, result, exception);
            }

        }

        bool IsMethodInfoOfJsonParameters(IEnumerable<MethodInfo> methods, List<string> names)
        {
            bool isFind = false;
            foreach (var method in methods)
            {
                int fakeParameterCount = 0;
                var findCount = method.GetCustomAttributes<FakeParameterAttribute>().Count();
                fakeParameterCount += findCount;
                if (method.GetParameters().Length == names.Count - fakeParameterCount)
                {
                    for (int i = 0; i < fakeParameterCount; i++)
                    {
                        if (names.Count > 0)
                            names.RemoveAt(names.Count - 1);
                    }
                }
                if (method.GetParameters().Count(x => names.Any(y => y.ToLower() == x.Name.ToLower())) == names.Count)
                {
                    isFind = true;
                    break;
                }
            }
            return isFind;
        }


        static void RunHttpActionResult(IHttpClientInfo controller, object result, ClientInfo client, ServerBase serverBase)
        {
            var newLine = "\r\n";

            var response = $"HTTP/1.1 {(int)controller.Status} {HttpRequestController.GetStatusDescription((int)controller.Status)}" + newLine;

            //foreach (string item in headers)
            //{
            //    response += item + ": " + headers[item] + newLine;
            //}

            if (result is FileActionResult && controller.Status == HttpStatusCode.OK)
            {
                response += controller.ResponseHeaders.ToString();
                var file = (FileActionResult)result;
                long fileLength = -1;
                //string len = "";
                try
                {
                    fileLength = file.FileStream.Length;
                    //len = "Content-Length: " + fileLength;
                }
                catch { }
                //response += len + newLine;
                var bytes = System.Text.Encoding.ASCII.GetBytes(response);
                client.ClientStream.Write(bytes, 0, bytes.Length);
                List<byte> allb = new List<byte>();
                //if (file.FileStream.CanSeek)
                //    file.FileStream.Seek(0, System.IO.SeekOrigin.Begin);
                while (fileLength != file.FileStream.Position)
                {
                    byte[] data = new byte[1024 * 20];
                    var readCount = file.FileStream.Read(data, 0, data.Length);
                    if (readCount == 0)
                        break;
                    bytes = data.ToList().GetRange(0, readCount).ToArray();
                    client.ClientStream.Write(bytes, 0, bytes.Length);
                }
                file.FileStream.Dispose();
            }
            else
            {
                byte[] dataBytes = null;
                if (result is ActionResult)
                {
                    var data = (((ActionResult)result).Data is string ? ((ActionResult)result).Data.ToString() : ServerSerializationHelper.SerializeObject(((ActionResult)result).Data, serverBase));
                    dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
                    if (controller.ResponseHeaders["content-length"] == null)
                        controller.ResponseHeaders.Add("Content-Length", (dataBytes.Length).ToString());

                    if (controller.ResponseHeaders["Content-Type"] == null)
                    {
                        if (((ActionResult)result).Data is string)
                            controller.ResponseHeaders.Add("Content-Type", "text/html; charset=utf-8");
                        else
                            controller.ResponseHeaders.Add("Content-Type", "application/json; charset=utf-8");
                    }
                }
                else
                {
                    var data = ServerSerializationHelper.SerializeObject(result, serverBase);
                    dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
                    if (controller.ResponseHeaders["content-length"] == null)
                        controller.ResponseHeaders.Add("Content-Length", (dataBytes.Length).ToString());

                    if (controller.ResponseHeaders["Content-Type"] == null)
                    {
                        //if (result.Data is string)
                        //    controller.ResponseHeaders.Add("Content-Type", "text/html; charset=utf-8");
                        //else
                        controller.ResponseHeaders.Add("Content-Type", "application/json; charset=utf-8");
                    }
                }

                if (controller.ResponseHeaders["Connection"] == null)
                    controller.ResponseHeaders.Add("Connection", "close");

                response += controller.ResponseHeaders.ToString();

                var bytes = System.Text.Encoding.UTF8.GetBytes(response);
                client.ClientStream.Write(bytes, 0, bytes.Length);

                //response += "Content-Type: text/html" + newLine + "Connection: Close" + newLine;
                client.ClientStream.Write(dataBytes, 0, dataBytes.Length);
                client.ClientStream.Flush();
                System.Threading.Thread.Sleep(100);
            }
        }

    }
}
