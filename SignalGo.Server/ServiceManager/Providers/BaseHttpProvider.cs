using Newtonsoft.Json.Linq;
using SignalGo.Server.DataTypes;
using SignalGo.Server.Helpers;
using SignalGo.Server.IO;
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
using System.Text;
using System.Threading.Tasks;
using WebHeaderCollection = SignalGo.Shared.Http.WebHeaderCollection;

namespace SignalGo.Server.ServiceManager.Providers
{
    public class BaseHttpProvider : BaseProvider
    {
        /// <summary>
        /// Guid for web socket client connection
        /// </summary>
        internal static readonly string _guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        internal static void HandleHttpRequest(string methodName, string address, ServerBase serverBase, HttpClientInfo client)
        {
            string newLine = "\r\n";
            string headerResponse = client.RequestHeaders.ToString();
            if (methodName.ToLower() == "get" && !string.IsNullOrEmpty(address) && address != "/")
            {
                if (client.RequestHeaders.ContainsKey("content-type") && client.GetRequestHeaderValue("content-type") == "SignalGo Service Reference")
                {
                    SendSignalGoServiceReference(client, serverBase);
                }
                else
                    RunHttpRequest(serverBase, address, "GET", "", client);
                serverBase.DisposeClient(client, "AddClient finish get call");
            }
            else if (methodName.ToLower() == "post" && !string.IsNullOrEmpty(address) && address != "/")
            {
                int indexOfStartedContent = headerResponse.IndexOf("\r\n\r\n");
                string content = "";
                if (indexOfStartedContent > 0)
                {
                    indexOfStartedContent += 4;
                    content = headerResponse.Substring(indexOfStartedContent, headerResponse.Length - indexOfStartedContent);
                }
                if (client.RequestHeaders["content-type"] != null && client.GetRequestHeaderValue("content-type").ToLower().Contains("multipart/form-data"))
                {
                    RunPostHttpRequestFile(address, "POST", content, client, serverBase);
                }
                else if (client.RequestHeaders["content-type"] != null && client.GetRequestHeaderValue("content-type") == "SignalGo Service Reference")
                {
                    SendSignalGoServiceReference(client, serverBase);
                }
                else
                {
                    RunHttpRequest(serverBase, address, "POST", content, client);
                }
                serverBase.DisposeClient(client, "AddClient finish post call");
            }
            else if (methodName.ToLower() == "options" && !string.IsNullOrEmpty(address) && address != "/")
            {
                Shared.Http.WebHeaderCollection responseHeaders = new WebHeaderCollection();

                if (serverBase.ProviderSetting.HttpSetting.HandleCrossOriginAccess)
                {
                    responseHeaders.Add("Access-Control-Allow-Origin", client.RequestHeaders["origin"]);
                    responseHeaders.Add("Access-Control-Allow-Credentials", "true");

                    if (!string.IsNullOrEmpty(client.GetRequestHeaderValue("Access-Control-Request-Headers")))
                    {
                        responseHeaders.Add("Access-Control-Allow-Headers", client.RequestHeaders["Access-Control-Request-Headers"]);
                    }
                }
                string message = newLine + $"Success" + newLine;
                responseHeaders.Add("Content-Type", "text/html; charset=utf-8");
                responseHeaders.Add("Connection", "Close");
                responseHeaders.Add("Content-Length", (System.Text.Encoding.UTF8.GetByteCount(message)).ToString().Split(','));

                SendResponseHeadersToClient(HttpStatusCode.OK, responseHeaders, client);
                SendResponseDataToClient(message, client);
                serverBase.DisposeClient(client, "AddClient finish post call");
            }
            else if (serverBase.RegisteredServiceTypes.ContainsKey("") && (string.IsNullOrEmpty(address) || address == "/"))
            {
                RunIndexHttpRequest(client, serverBase);
                serverBase.DisposeClient(client, "Index Page call");
            }
            else
            {
                Shared.Http.WebHeaderCollection responseHeaders = new WebHeaderCollection();
                responseHeaders.Add("Content-Type", "text/html");
                responseHeaders.Add("Connection", "Close");
                SendResponseHeadersToClient(HttpStatusCode.OK, responseHeaders, client);
                SendResponseDataToClient(newLine + "SignalGo Server OK" + newLine, client);
                serverBase.DisposeClient(client, "AddClient http ok signalGo");
            }
        }
        /// <summary>
        /// run method of server http class with address and headers
        /// </summary>
        /// <param name="address">address</param>
        /// <param name="headers">headers</param>
        /// <param name="client">client</param>
#if (NET35 || NET40)
        internal static void RunHttpRequest(ServerBase serverBase, string address, string httpMethod, string content, HttpClientInfo client)
#else
        internal static void RunHttpRequest(ServerBase serverBase, string address, string httpMethod, string content, HttpClientInfo client)
#endif
        {
            string newLine = "\r\n";

            string fullAddress = address;
            address = address.Trim('/');
            List<string> lines = address.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            //if (lines.Count <= 1)
            //{
            //    string data = newLine + "SignalGo Error: method not found from address: " + address + newLine;
            //    sendInternalErrorMessage(data);
            //    AutoLogger.LogText(data);
            //}
            //else
            //{
            string methodName = lines.Last();
            if (methodName == address)
                address = "";
            string parameters = "";
            Dictionary<string, string> multiPartParameter = new Dictionary<string, string>();
            if (httpMethod == "GET")
            {
                if (methodName.Contains("?"))
                {
                    string[] sp = methodName.Split(new[] { '?' }, 2);
                    methodName = sp.First();
                    parameters = sp.Last();
                }
            }
            else if (httpMethod == "POST")
            {
                int len = int.Parse(client.GetRequestHeaderValue("content-length"));
                if (content.Length < len)
                {
                    List<byte> resultBytes = new List<byte>();
                    int readedCount = 0;
                    while (readedCount < len)
                    {
                        try
                        {
                            byte[] buffer = client.ClientStream.Read(len - content.Length, out int readCount);
                            //#if (NET35 || NET40)
                            //                            int readCount = client.ClientStream.Read(buffer, 0, len - content.Length);
                            //#else
                            //buffer = 
                            //#endif
                            if (readCount == 0)
                                throw new Exception("zero byte readed socket disconnected!");
                            resultBytes.AddRange(buffer.ToList().GetRange(0, readCount));
                            readedCount += readCount;
                        }
                        catch
                        {
                            serverBase.DisposeClient(client, "HttpProvider RunHttpRequest exception");
                            return;
                        }
                    }
                    string postResponse = Encoding.UTF8.GetString(resultBytes.ToArray(), 0, resultBytes.Count);
                    content = postResponse;
                }

                methodName = lines.Last();
                parameters = content;
                if (methodName.Contains("?"))
                {
                    string[] sp = methodName.Split(new[] { '?' }, 2);
                    methodName = sp.First();
                    parameters = sp.Last();
                }
                else if (parameters.StartsWith("----") && parameters.ToLower().Contains("content-disposition"))
                {
                    string boundary = parameters.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                    string[] pValues = parameters.Split(new string[] { boundary }, StringSplitOptions.RemoveEmptyEntries);
                    string name = "";
                    foreach (string valueData in pValues)
                    {
                        if (valueData.ToLower().Contains("content-disposition"))
                        {
                            if (valueData.Replace(" ", "").Contains(";name="))
                            {
                                int index = valueData.ToLower().IndexOf("content-disposition");
                                string header = valueData.Substring(index);
                                int headLen = header.IndexOf("\r\n");
                                header = valueData.Substring(index, headLen);
                                string newData = valueData.Substring(index + headLen + 2);
                                //newData = newData.Split(new string[] { boundary }, StringSplitOptions.RemoveEmptyEntries);
                                if (header.ToLower().IndexOf("content-disposition:") == 0)
                                {
                                    CustomContentDisposition disp = new CustomContentDisposition(header);
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
            foreach (string item in lines)
            {
                address += item + "/";
            }
            address = address.TrimEnd('/').ToLower();
            string callGuid = Guid.NewGuid().ToString();
            string data = null;
            Type serviceType = null;
            MethodInfo method = null;
            try
            {
                if (!string.IsNullOrEmpty(address) && serverBase.RegisteredServiceTypes.ContainsKey(address))
                {
                    List<Shared.Models.ParameterInfo> values = new List<Shared.Models.ParameterInfo>();
                    if (multiPartParameter.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> item in multiPartParameter)
                        {
                            values.Add(new Shared.Models.ParameterInfo() { Name = item.Key, Value = item.Value });
                        }
                    }
                    else if (client.GetRequestHeaderValue("content-type") == "application/json")
                    {
                        try
                        {
                            JObject des = JObject.Parse(parameters);
                            //if (IsMethodInfoOfJsonParameters(methods, des.Properties().Select(x => x.Name).ToList()))
                            //{
                            foreach (JProperty item in des.Properties())
                            {
                                JToken value = des.GetValue(item.Name);
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
                            foreach (string item in parameters.Split(new[] { '&' }))
                            {
                                string[] keyValue = item.Split(new[] { '=' }, 2);
                                values.Add(new Shared.Models.ParameterInfo() { Name = keyValue.Length == 2 ? keyValue[0] : "", Value = Uri.UnescapeDataString(keyValue.Last()) });
                            }
                        }
                    }
                    CallHttpMethod(client, address, methodName, values, serverBase, method, data, newLine, null, null, out serviceType, out object serviceInstance);
                }
                else
                {
                    CallHttpMethod(client, address, methodName, null, serverBase, method, data, newLine, null, null, out serviceType, out object serviceInstance);
                }
            }
            catch (Exception ex)
            {
                // exception = ex;
                if (serverBase.ErrorHandlingFunction != null)
                {
                    ActionResult result = serverBase.ErrorHandlingFunction(ex, serviceType, method).ToActionResult();
                    RunHttpActionResult(client, result.Data, client, serverBase);
                }
                else
                {
                    data = newLine + ex.ToString() + address + newLine;
                    SendInternalErrorMessage(data, serverBase, client, newLine, HttpStatusCode.InternalServerError);
                }
                if (!(ex is SocketException))
                    serverBase.AutoLogger.LogError(ex, "RunHttpRequest");
            }
            finally
            {
                //ClientConnectedCallingCount--;
                //MethodsCallHandler.EndHttpMethodCallAction?.Invoke(client, callGuid, address, method, valueitems, result, exception);
            }
        }


        internal static void RunIndexHttpRequest(HttpClientInfo client, ServerBase serverBase)
        {
            string newLine = "\r\n";

            MethodInfo method = null;
            Type serviceType = null;

            try
            {
                CallHttpMethod(client, "", "-noName-", null, serverBase, method, null, newLine, null, x => x.GetCustomAttributes<HomePageAttribute>().Count() > 0, out serviceType, out object serviceInstance);
            }
            catch (Exception ex)
            {
                // exception = ex;
                if (serverBase.ErrorHandlingFunction != null)
                {
                    ActionResult result = serverBase.ErrorHandlingFunction(ex, serviceType, method).ToActionResult();
                    RunHttpActionResult(client, result, client, serverBase);
                }
                else
                {
                    string data = newLine + ex.ToString() + "" + newLine;
                    SendInternalErrorMessage(data, serverBase, client, newLine, HttpStatusCode.InternalServerError);
                }
                if (!(ex is SocketException))
                    serverBase.AutoLogger.LogError(ex, "RunPostHttpRequestFile");
            }
            finally
            {
                //ClientConnectedCallingCount--;
                //MethodsCallHandler.EndHttpMethodCallAction?.Invoke(client, callGuid, "", method, null, result, exception);
            }

        }
        internal static void CallHttpMethod(HttpClientInfo client, string address, string methodName, IEnumerable<Shared.Models.ParameterInfo> values, ServerBase serverBase, MethodInfo method
            , string data, string newLine, HttpPostedFileInfo fileInfo, Func<MethodInfo, bool> canTakeMethod, out Type serviceType, out object serviceInstance)
        {
            try
            {
                serverBase.AddTask(Task.CurrentId.GetValueOrDefault(), client.ClientId);

                if (values != null)
                {
                    foreach (Shared.Models.ParameterInfo item in values.Where(x => x.Value == "null"))
                    {
                        item.Value = null;
                    }
                }
                MethodCallbackInfo result = CallMethod(address, _guid, methodName, values == null ? null : values.ToArray(), client, "", serverBase, fileInfo, canTakeMethod, out IStreamInfo streamInfo, out List<HttpKeyAttribute> httpKeyAttributes, out serviceType, out method, out serviceInstance, out FileActionResult fileActionResult);

                if (result.IsException || result.IsAccessDenied)
                {
                    data = newLine + result.Data + newLine;

                    SendInternalErrorMessage(data, serverBase, client, newLine, (result.IsAccessDenied ? serverBase.ProviderSetting.HttpSetting.DefaultAccessDenidHttpStatusCode : HttpStatusCode.InternalServerError));
                    serverBase.AutoLogger.LogText(data);
                    return;
                }

                //MethodsCallHandler.BeginHttpMethodCallAction?.Invoke(client, callGuid, address, method, valueitems);
                //service = Activator.CreateInstance(RegisteredHttpServiceTypes[address]);
                if (serviceInstance is IHttpClientInfo)
                {
                    ((IHttpClientInfo)serviceInstance).RequestHeaders = client.RequestHeaders;
                    ((IHttpClientInfo)serviceInstance).ResponseHeaders = client.ResponseHeaders;
                    ((IHttpClientInfo)serviceInstance).IPAddress = client.IPAddress;
                }
                if (serverBase.ProviderSetting.HttpSetting.HandleCrossOriginAccess)
                {
                    client.ResponseHeaders.Add("Access-Control-Allow-Origin", client.RequestHeaders["origin"]);
                    client.ResponseHeaders.Add("Access-Control-Allow-Credentials", new string[] { "true" });
                    if (!string.IsNullOrEmpty(client.GetRequestHeaderValue("Access-Control-Request-Headers")))
                    {
                        client.ResponseHeaders.Add("Access-Control-Allow-Headers", client.RequestHeaders["Access-Control-Request-Headers"]);
                    }
                }
                HttpKeyAttribute httpKeyOnMethod = (HttpKeyAttribute)method.GetCustomAttributes(typeof(HttpKeyAttribute), true).FirstOrDefault();
                if (httpKeyOnMethod != null)
                    httpKeyAttributes.Add(httpKeyOnMethod);
                if (serverBase.ProviderSetting.HttpKeyResponses != null)
                {
                    httpKeyAttributes.AddRange(serverBase.ProviderSetting.HttpKeyResponses);
                }

                FillReponseHeaders(client, httpKeyAttributes);
                if (fileActionResult != null)
                    RunHttpActionResult(client, fileActionResult, client, serverBase);
                else if (result.Data == null)
                {
                    data = newLine + $"result from method invoke {methodName}, is null " + address + newLine;
                    SendInternalErrorMessage(data, serverBase, client, newLine, HttpStatusCode.InternalServerError);
                    serverBase.AutoLogger.LogText("RunHttpGETRequest : " + data);
                }
                else
                {
                    RunHttpActionResult(client, result.Data, client, serverBase);
                }
            }
            finally
            {
                serverBase.RemoveTask(Task.CurrentId.GetValueOrDefault());
            }
        }

        internal static void RunHttpActionResult(IHttpClientInfo controller, object result, ClientInfo client, ServerBase serverBase)
        {
            try
            {

                //string response = $"HTTP/1.1 {(int)controller.Status} {HttpRequestController.GetStatusDescription((int)controller.Status)}" + newLine;

                //foreach (string item in headers)
                //{
                //    response += item + ": " + headers[item] + newLine;
                //}

                if (result is FileActionResult && controller.Status == HttpStatusCode.OK)
                {
                    //response += controller.ResponseHeaders.ToString();
                    FileActionResult file = (FileActionResult)result;
                    long fileLength = -1;
                    //string len = "";
                    try
                    {
                        fileLength = file.FileStream.Length;
                        //len = "Content-Length: " + fileLength;
                    }
                    catch { }
                    //response += len + newLine;
                    //byte[] bytes = System.Text.Encoding.ASCII.GetBytes(response);
                    SendResponseHeadersToClient(controller.Status, controller.ResponseHeaders, client);
                    //List<byte> allb = new List<byte>();
                    //if (file.FileStream.CanSeek)
                    //    file.FileStream.Seek(0, System.IO.SeekOrigin.Begin);
                    while (fileLength != file.FileStream.Position)
                    {
                        byte[] data = new byte[1024 * 20];
                        int readCount = file.FileStream.Read(data, 0, data.Length);
                        if (readCount == 0)
                            break;
                        byte[] bytes = data.ToList().GetRange(0, readCount).ToArray();
                        client.ClientStream.Write(bytes);
                    }
                    file.FileStream.Dispose();
                }
                else
                {
                    string data = null;
                    if (result is ActionResult)
                    {
                        data = (((ActionResult)result).Data is string ? ((ActionResult)result).Data.ToString() : ServerSerializationHelper.SerializeObject(((ActionResult)result).Data, serverBase));
                        if (!controller.ResponseHeaders.ContainsKey("content-length"))
                            controller.ResponseHeaders.Add("Content-Length", (System.Text.Encoding.UTF8.GetByteCount(data)).ToString().Split(','));

                        if (!controller.ResponseHeaders.ContainsKey("Content-Type"))
                        {
                            if (((ActionResult)result).Data is string)
                                controller.ResponseHeaders.Add("Content-Type", "text/html; charset=utf-8".Split(','));
                            else
                                controller.ResponseHeaders.Add("Content-Type", "application/json; charset=utf-8".Split(','));
                        }
                    }
                    else
                    {
                        data = result is string ? (string)result : ServerSerializationHelper.SerializeObject(result, serverBase);
                        if (!controller.ResponseHeaders.ContainsKey("content-length"))
                            controller.ResponseHeaders.Add("Content-Length", (System.Text.Encoding.UTF8.GetByteCount(data)).ToString().Split(','));

                        if (!controller.ResponseHeaders.ContainsKey("Content-Type"))
                        {
                            //if (result.Data is string)
                            //    controller.ResponseHeaders.Add("Content-Type", "text/html; charset=utf-8");
                            //else
                            controller.ResponseHeaders.Add("Content-Type", "application/json; charset=utf-8".Split(','));
                        }
                    }

                    if (!controller.ResponseHeaders.ContainsKey("Connection"))
                        controller.ResponseHeaders.Add("Connection", "close".Split(','));

                    SendResponseHeadersToClient(controller.Status, controller.ResponseHeaders, client);
                    SendResponseDataToClient(data, client);
                }
            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// run method of server http class with address and headers
        /// </summary>
        /// <param name="address">address</param>
        /// <param name="headers">headers</param>
        /// <param name="client">client</param>
        internal static void RunPostHttpRequestFile(string address, string httpMethod, string content, HttpClientInfo client, ServerBase serverBase)
        {
            string newLine = "\r\n";
            string fullAddress = address;
            address = address.Trim('/');
            List<string> lines = address.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lines.Count <= 1)
            {
                string msg = newLine + "SignalGo Error: method not found from address: " + address + newLine;
                SendInternalErrorMessage(msg, serverBase, client, newLine, HttpStatusCode.InternalServerError);
                serverBase.AutoLogger.LogText(msg);
            }
            else
            {
                string methodName = lines.Last();
                string parameters = "";
                if (methodName.Contains("?"))
                {
                    string[] sp = methodName.Split(new[] { '?' }, 2);
                    methodName = sp.First();
                    parameters = sp.Last();
                }
                Dictionary<string, string> multiPartParameter = new Dictionary<string, string>();

                int len = int.Parse(client.GetRequestHeaderValue("content-length"));
                HttpPostedFileInfo fileInfo = null;
                if (content.Length < len)
                {
                    string boundary = client.GetRequestHeaderValue("content-type").Split('=').Last();
                    if (!boundary.Contains("--"))
                        boundary = null;
                    int fileHeaderCount = 0;
                    string response = "";
                    fileHeaderCount = GetHttpFileFileHeader(client.ClientStream, ref boundary, len, out response);
                    //boundary = boundary.TrimStart('-');
                    string contentType = "";
                    string fileName = "";
                    string name = "";
                    bool findFile = false;
                    string[] lineBreaks = new string[] { boundary.Replace("\"", ""), boundary.Replace("\"", "") + "--", "--" + boundary.Replace("\"", ""), "--" + boundary.Replace("\"", "") + "--" };
                    foreach (string httpData in response.Split(lineBreaks, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (httpData.ToLower().Contains("content-disposition"))
                        {
                            if (httpData.Replace(" ", "").Contains(";filename="))
                            {
                                foreach (string header in httpData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int index = header.ToLower().IndexOf("content-type: ");
                                    if (index == 0)
                                    {
                                        int ctypeLen = "content-type: ".Length;
                                        contentType = header.Substring(ctypeLen, header.Length - ctypeLen);
                                    }
                                    else if (header.ToLower().IndexOf("content-disposition:") == 0)
                                    {
                                        CustomContentDisposition disp = new CustomContentDisposition(header);
                                        if (disp.Parameters.ContainsKey("filename"))
                                            fileName = disp.Parameters["filename"];
                                        if (disp.Parameters.ContainsKey("name"))
                                            name = disp.Parameters["name"];
                                    }
                                    findFile = true;
                                }
                                break;
                            }
                            else if (httpData.ToLower().Contains("content-disposition"))
                            {
                                if (httpData.Replace(" ", "").Contains(";name="))
                                {
                                    string[] sp = httpData.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    string contentHeaders = sp.FirstOrDefault();
                                    string datas = sp.LastOrDefault();
                                    int index = contentHeaders.ToLower().IndexOf("content-disposition");
                                    string header = contentHeaders.Substring(index);
                                    int headLen = httpData.IndexOf("\r\n");
                                    //var headLen = data.IndexOf("\r\n\r\n");
                                    //header = sp.Length > 1 ? datas : data.Substring(index, headLen);
                                    //var byteData = GoStreamReader.ReadBlockSize(client.TcpClient.GetStream(), (ulong)(len - content.Length - fileHeaderCount));
                                    string newData = sp.Length > 1 ? datas : httpData.Substring(headLen + 4);//+ 4 Encoding.UTF8.GetString(byteData);
                                    newData = newData.Trim(Environment.NewLine.ToCharArray());
                                    //var newData = text.Substring(0, text.IndexOf(boundary) - 4);
                                    if (header.ToLower().IndexOf("content-disposition:") == 0)
                                    {
                                        CustomContentDisposition disp = new CustomContentDisposition(header.Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault());
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
                            string[] keyValue = httpData.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            if (keyValue.Length == 2)
                            {
                                if (!string.IsNullOrEmpty(parameters))
                                {
                                    parameters += "&";
                                }
                                CustomContentDisposition disp = new CustomContentDisposition(keyValue[0]);
                                foreach (KeyValuePair<string, string> prm in disp.Parameters)
                                {
                                    parameters += prm.Key;
                                    parameters += "=" + prm.Value;
                                }

                            }
                        }
                    }
                    if (findFile)
                    {
                        StreamGo stream = new StreamGo(client.ClientStream);
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
                foreach (string item in lines)
                {
                    address += item + "/";
                }
                address = address.TrimEnd('/').ToLower();
                //if (RegisteredHttpServiceTypes.ContainsKey(address))
                //{
                MethodInfo method = null;
                string callGuid = Guid.NewGuid().ToString();
                object serviceInstance = null;
                Type serviceType = null;
                string data = null;
                try
                {
                    List<Shared.Models.ParameterInfo> values = new List<Shared.Models.ParameterInfo>();

                    //var methods = (from x in RegisteredHttpServiceTypes[address].GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance) where x.Name.ToLower() == methodName && x.IsPublic && !(x.IsSpecialName && (x.Name.StartsWith("set_") || x.Name.StartsWith("get_"))) select x).ToList();
                    //if (methods.Count == 0)
                    //{
                    //    string data = newLine + "SignalGo Error: Method name not found in method list: " + methodName + newLine;
                    //    sendInternalErrorMessage(data);
                    //    serverBase.AutoLogger.LogText(data);
                    //    return;
                    //}

                    //List<Tuple<string, string>> values = new List<Tuple<string, string>>();
                    if (multiPartParameter.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> item in multiPartParameter)
                        {
                            values.Add(new Shared.Models.ParameterInfo() { Name = item.Key, Value = item.Value });
                        }
                    }
                    else if (client.GetRequestHeaderValue("content-type") == "application/json")
                    {
                        JObject des = JObject.Parse(parameters);
                        foreach (JProperty item in des.Properties())
                        {
                            JToken value = des.GetValue(item.Name);
                            //values.Add(new Tuple<string, string>(item.Name, Uri.UnescapeDataString(value.Value<string>())));
                            values.Add(new Shared.Models.ParameterInfo() { Name = item.Name, Value = value.ToString() });
                        }
                    }
                    else
                    {
                        parameters = parameters.Trim('&');
                        if (!string.IsNullOrEmpty(parameters))
                        {
                            foreach (string item in parameters.Split(new[] { '&' }))
                            {
                                string[] keyValue = item.Split(new[] { '=' }, 2);
                                values.Add(new Shared.Models.ParameterInfo() { Name = keyValue.Length == 2 ? keyValue[0] : "", Value = Uri.UnescapeDataString(keyValue.Last()) });
                            }
                        }
                    }



                    CallHttpMethod(client, address, methodName, values, serverBase, method, data, newLine, fileInfo, null, out serviceType, out serviceInstance);


                    //valueitems = values.Select(x => x.Item2).ToList();
                    //MethodsCallHandler.BeginHttpMethodCallAction?.Invoke(client, callGuid, address, method, valueitems);


                }
                catch (Exception ex)
                {
                    // exception = ex;
                    if (serverBase.ErrorHandlingFunction != null)
                    {
                        ActionResult result = serverBase.ErrorHandlingFunction(ex, serviceType, method).ToActionResult();
                        RunHttpActionResult(client, result, client, serverBase);
                    }
                    else
                    {
                        data = newLine + ex.ToString() + address + newLine;
                        SendInternalErrorMessage(data, serverBase, client, newLine, HttpStatusCode.InternalServerError);
                    }
                    if (!(ex is SocketException))
                        serverBase.AutoLogger.LogError(ex, "RunPostHttpRequestFile");
                }
                finally
                {
                    //ClientConnectedCallingCount--;
                    //MethodsCallHandler.EndHttpMethodCallAction?.Invoke(client, callGuid, address, method, valueitems, result, exception);
                }
                //}
                //else
                //{
                //    string data = newLine + "SignalGo Error: address not found in signalGo services: " + address + newLine;
                //    sendInternalErrorMessage(data);
                //    AutoLogger.LogText(data);
                //}
            }
        }


        /// <summary>
        /// send service reference data to client
        /// </summary>
        /// <param name="client"></param>
        internal static void SendSignalGoServiceReference(HttpClientInfo client, ServerBase serverBase)
        {
            PipeNetworkStream stream = client.ClientStream;

            Shared.Models.ServiceReference.NamespaceReferenceInfo referenceData = new ServiceReferenceHelper().GetServiceReferenceCSharpCode(client.GetRequestHeaderValue("servicenamespace"), serverBase);
            string result = ServerSerializationHelper.SerializeObject(referenceData, serverBase);
            Shared.Http.WebHeaderCollection responseHeaders = new WebHeaderCollection();
            responseHeaders.Add("Content-Length", result.Length.ToString());
            responseHeaders.Add("Content-Type", "SignalGoServiceType");
            SendResponseHeadersToClient(HttpStatusCode.OK, responseHeaders, client);
            SendResponseDataToClient(result, client);
            System.Threading.Thread.Sleep(100);
            serverBase.DisposeClient(client, "SendSignalGoServiceReference finished");
        }



        internal static int GetHttpFileFileHeader(PipeNetworkStream stream, ref string boundary, int maxLen, out string response)
        {
            List<byte> bytes = new List<byte>();
            byte findNextlvl = 0;
            while (true)
            {
                byte singleByte = stream.ReadOneByte();
                bytes.Add(singleByte);
                if (bytes.Count >= maxLen)
                {
                    string data = Encoding.UTF8.GetString(bytes.ToArray());
                    response = data;
                    if (response.Contains("--") && string.IsNullOrEmpty(boundary))
                    {
                        string[] split = response.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string item in split)
                        {
                            if (response.Contains("--"))
                            {
                                boundary = item;
                                break;
                            }
                        }
                    }
                    return bytes.Count;

                }
                if (findNextlvl > 0)
                {
                    if (findNextlvl == 1 && singleByte == 10)
                        findNextlvl++;
                    else if (findNextlvl == 2 && singleByte == 13)
                        findNextlvl++;
                    else if (findNextlvl == 3 && singleByte == 10)
                    {
                        string data = Encoding.UTF8.GetString(bytes.ToArray());
                        string res = data.Replace(" ", "").ToLower();

                        string[] lines = res.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        bool canBreak = false;
                        foreach (string item in lines)
                        {
                            if (item.Trim().StartsWith("content-disposition:") && item.Contains("filename="))
                            {
                                canBreak = true;
                                break;
                            }
                        }
                        if (canBreak)
                            break;
                        findNextlvl = 0;
                    }
                    else
                        findNextlvl = 0;
                }
                else
                {
                    if (singleByte == 13)
                        findNextlvl++;
                }
            }
            response = Encoding.UTF8.GetString(bytes.ToArray());
            if (response.Contains("--") && string.IsNullOrEmpty(boundary))
            {
                string[] split = response.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in split)
                {
                    if (response.Contains("--"))
                    {
                        boundary = item;
                        break;
                    }

                }
                //if (lastEnter > 0)
                //{
                //    var startindex = response.LastIndexOf("--") + 2;
                //    boundary = response.Substring(startindex, lastEnter - startindex - 6);
                //}
            }
            return bytes.Count;
        }

        internal static new void SendInternalErrorMessage(string msg, ServerBase serverBase, HttpClientInfo client, string newLine, HttpStatusCode httpStatusCode)
        {
            try
            {
                //{ 500} {HttpRequestController.GetStatusDescription((int)HttpStatusCode.InternalServerError)}
                Shared.Http.WebHeaderCollection responseHeaders = new WebHeaderCollection();
                if (serverBase.ProviderSetting.HttpSetting.HandleCrossOriginAccess)
                {
                    responseHeaders.Add("Access-Control-Allow-Origin", client.RequestHeaders["origin"]);
                    responseHeaders.Add("Access-Control-Allow-Credentials", "true");

                    if (!string.IsNullOrEmpty(client.GetRequestHeaderValue("Access-Control-Request-Headers")))
                    {
                        responseHeaders.Add("Access-Control-Allow-Headers", client.RequestHeaders["Access-Control-Request-Headers"]);
                    }
                }
                string message = newLine + $"{msg}" + newLine;

                responseHeaders.Add("Content-Type", "text/html; charset=utf-8");
                responseHeaders.Add("Content-Length", (message.Length - 2).ToString());
                responseHeaders.Add("Connection", "Close");

                SendResponseHeadersToClient(httpStatusCode, responseHeaders, client);
                SendResponseDataToClient(message, client);
            }
            catch (SocketException)
            {

            }
            catch (Exception ex)
            {
                serverBase.AutoLogger.LogError(ex, "RunHttpGETRequest sendErrorMessage");
            }
        }


        private static void FillReponseHeaders(HttpClientInfo client, List<HttpKeyAttribute> httpKeyAttributes)
        {
            foreach (HttpKeyAttribute item in httpKeyAttributes)
            {
                if (item.SettingType == null)
                    throw new Exception("you made HttpKeyAttribute top of your method but this have not fill SettingType property");
                object contextResult = OperationContextBase.GetCurrentSetting(item.SettingType);

                if (contextResult != null)
                {
                    var property = contextResult.GetType().GetListOfProperties().Select(x => new { Info = x, Attribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => !y.IsExpireField), ExpiredAttribute = x.GetCustomAttributes<HttpKeyAttribute>().FirstOrDefault(y => y.IsExpireField) }).FirstOrDefault(x => x.Attribute != null);
                    if (property != null)
                    {
                        if (!client.ResponseHeaders.ContainsKey(property.Attribute.ResponseHeaderName))
                        {
                            client.ResponseHeaders[property.Attribute.ResponseHeaderName] = new string[] { OperationContextBase.IncludeValue((string)property.Info.GetValue(contextResult, null), property.Attribute.KeyName, property.Attribute.HeaderValueSeparate, property.Attribute.HeaderKeyValueSeparate) + property.Attribute.Perfix };
                        }
                    }
                }
            }
        }


        internal static void SendResponseHeadersToClient(HttpStatusCode httpStatusCode, IDictionary<string, string[]> webResponseHeaderCollection, ClientInfo client)
        {
            if (client.IsOwinClient)
                return;
            string newLine = "\r\n";
            string firstLine = $"HTTP/1.1 {(int)httpStatusCode} {HttpRequestController.GetStatusDescription((int)httpStatusCode)}" + newLine;

            byte[] headBytes = Encoding.ASCII.GetBytes(firstLine + webResponseHeaderCollection.ToString());
            client.StreamHelper.WriteToStream(client.ClientStream, headBytes);
        }


        internal static void SendResponseDataToClient(string dataResult, ClientInfo client)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(dataResult);

            client.StreamHelper.WriteToStream(client.ClientStream, dataBytes);
        }

    }
}
