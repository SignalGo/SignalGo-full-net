using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Client
{
    public class HttpClientResponseBase
    {
        public TcpClient TcpClient { get; set; }
        public PipeNetworkStream Stream { get; set; }
        /// <summary>
        /// status
        /// </summary>
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        /// <summary>
        /// response headers
        /// </summary>
        public SignalGo.Shared.Http.WebHeaderCollection ResponseHeaders { get; set; }
        /// <summary>
        /// get stream info from http response
        /// </summary>
        /// <returns></returns>
        public T GetStream<T>() where T : IStreamInfo
        {
            IStreamInfo streamInfo = (IStreamInfo)Activator.CreateInstance(typeof(T), Stream);
            streamInfo.Status = Status;
            if (ResponseHeaders.ContainsKey("Content-Length"))
                streamInfo.Length = long.Parse(ResponseHeaders["Content-Length"]);
            if (ResponseHeaders.ContainsKey("Content-Type"))
                streamInfo.ContentType = ResponseHeaders["Content-Type"];
            if (ResponseHeaders.ContainsKey("Content-Disposition"))
            {
                try
                {
                    CustomContentDisposition customContentDisposition = new CustomContentDisposition(ResponseHeaders["Content-Disposition"]);
                    streamInfo.FileName = customContentDisposition.FileName;
                }
                catch
                {

                }
            }
            return (T)streamInfo;
        }
    }
    public class HttpClientDataResponse
    {
        public HttpClientDataResponse(byte[] data, Encoding encoding)
        {
            Data = data;
            Encoding = encoding;
        }

        public HttpClientDataResponse(string json)
        {
            Json = json;
        }

        Encoding Encoding { get; set; }
        public byte[] Data { get; set; }
        public string Json { get; set; }

        public static implicit operator string(HttpClientDataResponse httpClientDataResponse)
        {
            if (string.IsNullOrEmpty(httpClientDataResponse.Json))
                return httpClientDataResponse.Encoding.GetString(httpClientDataResponse.Data);
            return httpClientDataResponse.Json;
        }

        public static implicit operator Exception(HttpClientDataResponse httpClientDataResponse)
        {
            return new Exception(httpClientDataResponse);
        }
    }

    /// <summary>
    /// reponse of http request
    /// </summary>
    public class HttpClientResponse : HttpClientResponseBase
    {
        /// <summary>
        /// data of response
        /// </summary>
        public HttpClientDataResponse Data { get; set; }
    }

    public interface IHttpClient
    {
        Encoding Encoding { get; set; }
        SignalGo.Shared.Http.WebHeaderCollection RequestHeaders { get; set; }

        T Deserialize<T>(HttpClientDataResponse data);
        HttpClientResponse Post(string url, ParameterInfo[] parameterInfoes, BaseStreamInfo streamInfo = null);
#if (!NET35 && !NET40 && !NETSTANDARD1_6)
        Task<HttpClientResponse> PostAsync(string url, ParameterInfo[] parameterInfoes, BaseStreamInfo streamInfo = null);
#endif
    }
    /// <summary>
    /// http clinet over tcp
    /// </summary>
    public class HttpClient : IHttpClient
    {
        public static CustomSerializerAttribute CurrentCustomOutputSerializer;
        public HttpClient()
        {
            JsonSettingHelper.Initialize();
        }

        /// <summary>
        /// default 
        /// </summary>
        public static JsonSettingHelper DefaultJsonSettingHelper { get; set; } = new JsonSettingHelper();

        JsonSettingHelper _JsonSettingHelper;
        public JsonSettingHelper JsonSettingHelper
        {
            get
            {
                if (_JsonSettingHelper == null)
                    return DefaultJsonSettingHelper;
                return _JsonSettingHelper;
            }
            set
            {
                _JsonSettingHelper = value;
                _JsonSettingHelper.Initialize();
            }
        }
        /// <summary>
        /// encoding system
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.ASCII;
        /// <summary>
        /// request post data headers
        /// </summary>
        public SignalGo.Shared.Http.WebHeaderCollection RequestHeaders { get; set; } = new Shared.Http.WebHeaderCollection();
        public string KeyParameterName { get; set; }
        public string KeyParameterValue { get; set; }
        public string Cookie { get; set; }
        public HttpClientResponseBase Response { get; set; }
        public bool UseJsonPost { get; set; }
        public virtual HttpClientResponseBase PostHead(string url, ParameterInfo[] parameterInfoes, BaseStreamInfo streamInfo = null)
        {
#if (NETSTANDARD1_6)
            throw new NotSupportedException();
#else
            string newLine = TextHelper.NewLine;
            Uri uri = new Uri(url);
            TcpClient tcpClient = new TcpClient(uri.Host, uri.Port);
            try
            {
                if (streamInfo != null && (!streamInfo.Length.HasValue || streamInfo.Length <= 0))
                    throw new Exception("Please set streamInfo.Length before upload your stream!");
                if (!string.IsNullOrEmpty(KeyParameterName))
                {
                    List<ParameterInfo> list = parameterInfoes.ToList();
                    list.Add(new SignalGo.Shared.Models.ParameterInfo() { Name = KeyParameterName, Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(KeyParameterValue) });
                    parameterInfoes = list.ToArray();
                }
                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                string headData = $"POST {uri.AbsolutePath} HTTP/1.1" + newLine + $"Host: {uri.Host}" + newLine + $"Content-Type: multipart/form-data; boundary={boundary}" + newLine;
                if (RequestHeaders == null)
                    RequestHeaders = new Shared.Http.WebHeaderCollection();
                if (!string.IsNullOrEmpty(Cookie))
                    RequestHeaders.Add("cookie", Cookie);
                if (RequestHeaders.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> item in RequestHeaders)
                    {
                        if (!item.Key.Equals("host", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("content-length", StringComparison.OrdinalIgnoreCase))
                        {
                            if (item.Value == null || item.Value.Length == 0)
                                continue;
                            headData += item.Key + ": " + string.Join(",", item.Value).TrimEnd() + newLine;
                        }
                    }
                }
                StringBuilder valueData = new StringBuilder();
                if (parameterInfoes != null)
                {
                    string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                    string boundaryinsert = TextHelper.NewLine + "--" + boundary + TextHelper.NewLine;
                    foreach (ParameterInfo item in parameterInfoes)
                    {
                        valueData.AppendLine(boundaryinsert);
                        valueData.Append(string.Format(formdataTemplate, item.Name, item.Value));
                    }
                }

                byte[] dataBytes = Encoding.GetBytes(valueData.ToString());
                headData += $"Content-Length: {dataBytes.Length}" + newLine + newLine;

                byte[] headBytes = Encoding.GetBytes(headData);

                Stream stream = uri.Port == 443 ? (Stream)new SslStream(tcpClient.GetStream()) : tcpClient.GetStream();

                if (uri.Port == 443)
                {
                    SslStream sslStream = (SslStream)stream;
                    sslStream.AuthenticateAsClient(uri.Host);
                }
                stream.Write(headBytes, 0, headBytes.Length);
                stream.Write(dataBytes, 0, dataBytes.Length);

                if (streamInfo != null)
                {
                    int sentBytesCount = 0;
                    int wantReadCount = 1024 * 512;
                    while (streamInfo.Length > sentBytesCount)
                    {
                        if (wantReadCount > streamInfo.Length - sentBytesCount)
                            wantReadCount = (int)(streamInfo.Length - sentBytesCount);
                        byte[] result = new byte[wantReadCount];
                        int readCount = streamInfo.Stream.Read(result, wantReadCount);
                        stream.Write(result, 0, readCount);
                        sentBytesCount += readCount;
                    }
                }
                PipeNetworkStream pipelineReader = new PipeNetworkStream(new NormalStream(stream), 30000);

                List<string> lines = new List<string>();
                string line = null;
                do
                {
                    if (line != null)
                        lines.Add(line.TrimEnd());
                    line = pipelineReader.ReadLine();
                }
                while (line != newLine);
                HttpClientResponseBase httpClientResponse = new HttpClientResponseBase
                {
                    Status = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), lines[0].Split(' ')[1]),
                    ResponseHeaders = SignalGo.Shared.Http.WebHeaderCollection.GetHttpHeaders(lines.Skip(1).ToArray()),
                    Stream = pipelineReader,
                    TcpClient = tcpClient
                };
                if (httpClientResponse.ResponseHeaders.TryGetValue("set-cookie", out string[] cookieHeader))
                {
                    Cookie = cookieHeader[0];
                }
                Response = httpClientResponse;
                return httpClientResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
#endif
        }
        /// <summary>
        /// post a data to server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameterInfoes"></param>
        /// <param name="streamInfo"></param>
        /// <returns></returns>
        public virtual HttpClientResponse Post(string url, ParameterInfo[] parameterInfoes, BaseStreamInfo streamInfo = null)
        {
#if (NETSTANDARD1_6)
            throw new NotSupportedException();
#else
            HttpClientResponseBase response = PostHead(url, parameterInfoes, streamInfo);
            try
            {
                HttpClientResponse httpClientResponse = new HttpClientResponse
                {
                    Status = response.Status,
                    ResponseHeaders = response.ResponseHeaders,
                    Stream = response.Stream,
                    TcpClient = response.TcpClient,
                };
                int length = int.Parse(httpClientResponse.ResponseHeaders["content-length"]);
                byte[] result = new byte[length];
                int readCount = 0;
                while (readCount < length)
                {
                    byte[] bytes = new byte[512];
                    int readedCount = 0;
                    readedCount = response.Stream.Read(bytes, bytes.Length);

                    for (int i = 0; i < readedCount; i++)
                    {
                        result[i + readCount] = bytes[i];
                    }
                    readCount += readedCount;
                }
                httpClientResponse.Data = new HttpClientDataResponse(result, Encoding);
                response = httpClientResponse;
                return httpClientResponse;
            }
            finally
            {
                response.TcpClient.Close();
            }
#endif
        }
        public T Deserialize<T>(HttpClientDataResponse data)
        {
            if (typeof(T) == typeof(ActionResult))
            {
                return (T)(object)new ActionResult(data.Data);
            }
            else
            {
                if (CurrentCustomOutputSerializer == null)
                    return SignalGo.Client.ClientSerializationHelper.DeserializeObject<T>(data);
                else
                    return (T)CurrentCustomOutputSerializer.Deserialize(typeof(T), data, null, null);
            }
        }

#if (!NET35 && !NET40 && !NETSTANDARD1_6)

        public virtual async Task<HttpClientResponseBase> PostHeadAsync(string url, ParameterInfo[] parameterInfoes, BaseStreamInfo streamInfo = null)
        {
            if (UseJsonPost)
                return await PostHeadJsonAsync(url, parameterInfoes, streamInfo);
            else
                return await PostHeadManualAsync(url, parameterInfoes, streamInfo);
        }

        async Task<HttpClientResponseBase> PostHeadJsonAsync(string url, ParameterInfo[] parameterInfoes, BaseStreamInfo streamInfo = null)
        {
            string newLine = TextHelper.NewLine;
            Uri uri = new Uri(url);
            TcpClient tcpClient = new TcpClient();
            try
            {
                await tcpClient.ConnectAsync(uri.Host, uri.Port).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Address to connect is {uri.Host}:{uri.Port}", ex);
            }

            try
            {
                if (streamInfo != null && (!streamInfo.Length.HasValue || streamInfo.Length <= 0))
                    throw new Exception("Please set streamInfo.Length before upload your stream!");
                string headData = $"POST {uri.AbsolutePath} HTTP/1.1" + newLine + $"Host: {uri.Host}" + newLine + $"Content-Type: application/json" + newLine;
                if (RequestHeaders == null)
                    RequestHeaders = new Shared.Http.WebHeaderCollection();
                if (!string.IsNullOrEmpty(Cookie))
                    RequestHeaders.Add("cookie", Cookie);
                if (RequestHeaders.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> item in RequestHeaders)
                    {
                        if (!item.Key.Equals("host", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("content-length", StringComparison.OrdinalIgnoreCase))
                        {
                            if (item.Value == null || item.Value.Length == 0)
                                continue;
                            headData += item.Key + ": " + string.Join(",", item.Value) + newLine;
                        }
                    }
                }

                StringBuilder valueData = new StringBuilder();
                if (parameterInfoes != null)
                {
                    valueData.Append('{');
                    foreach (var parameter in parameterInfoes)
                    {
                        valueData.Append($"\"{parameter.Name}\":");
                        valueData.Append($"\"{parameter.Value}\",");
                    }
                    if (parameterInfoes.Length > 0)
                        valueData = valueData.Remove(valueData.Length - 1, 1);
                    valueData.Append('}');
                }
                byte[] dataBytes = Encoding.GetBytes(valueData.ToString());
                long fullLength = dataBytes.Length;
                if (streamInfo != null)
                    fullLength += streamInfo.Length.GetValueOrDefault();
                headData += $"Content-Length: {fullLength}" + newLine + newLine;

                byte[] headBytes = Encoding.GetBytes(headData);

                Stream stream = uri.Port == 443 ? (Stream)new SslStream(tcpClient.GetStream()) : tcpClient.GetStream();

                if (uri.Port == 443)
                {
                    SslStream sslStream = (SslStream)stream;
                    await sslStream.AuthenticateAsClientAsync(uri.Host).ConfigureAwait(false);
                }
                stream.Write(headBytes, 0, headBytes.Length);
                stream.Write(dataBytes, 0, dataBytes.Length);

                if (streamInfo != null)
                {
                    int sentBytesCount = 0;
                    int wantReadCount = 1024 * 512;
                    while (streamInfo.Length > sentBytesCount)
                    {
                        if (wantReadCount > streamInfo.Length - sentBytesCount)
                            wantReadCount = (int)(streamInfo.Length - sentBytesCount);
                        byte[] result = new byte[wantReadCount];
                        int readCount = await streamInfo.Stream.ReadAsync(result, wantReadCount).ConfigureAwait(false);
                        await stream.WriteAsync(result, 0, readCount).ConfigureAwait(false);
                        sentBytesCount += readCount;
                    }
                }

                PipeNetworkStream pipelineReader = new PipeNetworkStream(new NormalStream(stream), 30000);

                List<string> lines = new List<string>();
                string line = null;
                do
                {
                    if (line != null)
                        lines.Add(line.TrimEnd());
                    line = await pipelineReader.ReadLineAsync().ConfigureAwait(false);
                }
                while (line != newLine);
                HttpClientResponseBase httpClientResponse = new HttpClientResponseBase
                {
                    Status = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), lines[0].Split(' ')[1]),
                    ResponseHeaders = SignalGo.Shared.Http.WebHeaderCollection.GetHttpHeaders(lines.Skip(1).ToArray()),
                    Stream = pipelineReader,
                    TcpClient = tcpClient
                };
                if (httpClientResponse.ResponseHeaders.TryGetValue("set-cookie", out string[] cookieHeader))
                {
                    Cookie = cookieHeader[0];
                }
                Response = httpClientResponse;
                return httpClientResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        async Task<HttpClientResponseBase> PostHeadManualAsync(string url, ParameterInfo[] parameterInfoes, BaseStreamInfo streamInfo = null)
        {
            string newLine = TextHelper.NewLine;
            Uri uri = new Uri(url);
            TcpClient tcpClient = new TcpClient();
            //#if (__ANDROID__)
            //            var cancelSource = new CancellationTokenSource();
            //            cancelSource.Token.ThrowIfCancellationRequested();
            //            _ = Task.Run(async () =>
            //                {
            //                    for (int i = 0; i < 6; i++)
            //                    {
            //                        await Task.Delay(1000).ConfigureAwait(false);
            //                    }
            //                    cancelSource.Cancel();
            //                });
            //            await Task.Run(async () =>
            //            {
            //                await tcpClient.ConnectAsync(uri.Host, uri.Port).ConfigureAwait(false);
            //            }, cancelSource.Token).ConfigureAwait(false);
            //#else
            try
            {
                await tcpClient.ConnectAsync(uri.Host, uri.Port).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Address to connect is {uri.Host}:{uri.Port}", ex);
            }
            //#endif
            try
            {
                if (streamInfo != null && (!streamInfo.Length.HasValue || streamInfo.Length <= 0))
                    throw new Exception("Please set streamInfo.Length before upload your stream!");
                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                string headData = $"POST {uri.AbsolutePath} HTTP/1.1" + newLine + $"Host: {uri.Host}" + newLine + $"Content-Type: multipart/form-data; boundary={boundary}" + newLine;
                if (RequestHeaders == null)
                    RequestHeaders = new Shared.Http.WebHeaderCollection();
                if (!string.IsNullOrEmpty(Cookie))
                    RequestHeaders.Add("cookie", Cookie);
                if (RequestHeaders.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> item in RequestHeaders)
                    {
                        if (!item.Key.Equals("host", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("content-length", StringComparison.OrdinalIgnoreCase))
                        {
                            if (item.Value == null || item.Value.Length == 0)
                                continue;
                            headData += item.Key + ": " + string.Join(",", item.Value) + newLine;
                        }
                    }
                }

                StringBuilder valueData = new StringBuilder();
                if (parameterInfoes != null)
                {
                    string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                    string boundaryinsert = TextHelper.NewLine + "--" + boundary + TextHelper.NewLine;
                    foreach (ParameterInfo item in parameterInfoes)
                    {
                        valueData.AppendLine(boundaryinsert);
                        valueData.Append(string.Format(formdataTemplate, item.Name, item.Value));
                    }
                }
                if (streamInfo != null)
                {
                    string boundaryinsert = TextHelper.NewLine + "--" + boundary + TextHelper.NewLine;
                    valueData.AppendLine(boundaryinsert);
                    valueData.AppendLine($"Content-Disposition: file; filename=\"{streamInfo.FileName}\"");
                    valueData.AppendLine($"Content-Length: {streamInfo.Length}");
                    valueData.AppendLine($"Content-Type: {streamInfo.ContentType}");
                    valueData.AppendLine();
                }
                byte[] dataBytes = Encoding.GetBytes(valueData.ToString());
                long fullLength = dataBytes.Length;
                if (streamInfo != null)
                    fullLength += streamInfo.Length.GetValueOrDefault();
                headData += $"Content-Length: {fullLength}" + newLine + newLine;

                byte[] headBytes = Encoding.GetBytes(headData);

                Stream stream = uri.Port == 443 ? (Stream)new SslStream(tcpClient.GetStream()) : tcpClient.GetStream();

                if (uri.Port == 443)
                {
                    SslStream sslStream = (SslStream)stream;
                    await sslStream.AuthenticateAsClientAsync(uri.Host).ConfigureAwait(false);
                }
                stream.Write(headBytes, 0, headBytes.Length);
                stream.Write(dataBytes, 0, dataBytes.Length);

                if (streamInfo != null)
                {
                    int sentBytesCount = 0;
                    int wantReadCount = 1024 * 512;
                    while (streamInfo.Length > sentBytesCount)
                    {
                        if (wantReadCount > streamInfo.Length - sentBytesCount)
                            wantReadCount = (int)(streamInfo.Length - sentBytesCount);
                        byte[] result = new byte[wantReadCount];
                        int readCount = await streamInfo.Stream.ReadAsync(result, wantReadCount).ConfigureAwait(false);
                        await stream.WriteAsync(result, 0, readCount).ConfigureAwait(false);
                        sentBytesCount += readCount;
                    }
                }

                PipeNetworkStream pipelineReader = new PipeNetworkStream(new NormalStream(stream), 30000);

                List<string> lines = new List<string>();
                string line = null;
                do
                {
                    if (line != null)
                        lines.Add(line.TrimEnd());
                    line = await pipelineReader.ReadLineAsync().ConfigureAwait(false);
                }
                while (line != newLine);
                HttpClientResponseBase httpClientResponse = new HttpClientResponseBase
                {
                    Status = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), lines[0].Split(' ')[1]),
                    ResponseHeaders = SignalGo.Shared.Http.WebHeaderCollection.GetHttpHeaders(lines.Skip(1).ToArray()),
                    Stream = pipelineReader,
                    TcpClient = tcpClient
                };
                if (httpClientResponse.ResponseHeaders.TryGetValue("set-cookie", out string[] cookieHeader))
                {
                    Cookie = cookieHeader[0];
                }
                Response = httpClientResponse;
                return httpClientResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameterInfoes"></param>
        /// <returns></returns>
        public virtual async Task<HttpClientResponse> PostAsync(string url, ParameterInfo[] parameterInfoes, BaseStreamInfo streamInfo = null)
        {
            HttpClientResponseBase response = await PostHeadAsync(url, parameterInfoes, streamInfo).ConfigureAwait(false);
            try
            {
                HttpClientResponse httpClientResponse = new HttpClientResponse
                {
                    Status = response.Status,
                    ResponseHeaders = response.ResponseHeaders,
                    Stream = response.Stream,
                    TcpClient = response.TcpClient,
                };
                int length = int.Parse(httpClientResponse.ResponseHeaders["content-length"]);
                byte[] result = new byte[length];
                int readCount = 0;
                while (readCount < length)
                {
                    byte[] bytes = new byte[512];
                    int readedCount = 0;
                    readedCount = await response.Stream.ReadAsync(bytes, bytes.Length).ConfigureAwait(false);
                    for (int i = 0; i < readedCount; i++)
                    {
                        result[i + readCount] = bytes[i];
                    }
                    readCount += readedCount;
                }
                httpClientResponse.Data = new HttpClientDataResponse(result, Encoding);
                response = httpClientResponse;
                return httpClientResponse;
            }
            finally
            {
                response.TcpClient.Close();
            }
        }
#endif
    }
}