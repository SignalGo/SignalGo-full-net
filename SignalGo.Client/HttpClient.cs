using SignalGo.Shared.Helpers;
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
    }

    /// <summary>
    /// reponse of http request
    /// </summary>
    public class HttpClientResponse: HttpClientResponseBase
    {
        /// <summary>
        /// data of response
        /// </summary>
        public string Data { get; set; }
    }

    /// <summary>
    /// http clinet over tcp
    /// </summary>
    public class HttpClient
    {
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
        public HttpClientResponseBase Response { get; set; }

        public HttpClientResponseBase PostHead(string url, ParameterInfo[] parameterInfoes)
        {
#if (NETSTANDARD1_6)
            throw new NotSupportedException();
#else
            string newLine = TextHelper.NewLine;
            Uri uri = new Uri(url);
            TcpClient tcpClient = new TcpClient(uri.Host, uri.Port);
            try
            {
                if (!string.IsNullOrEmpty(KeyParameterName))
                {
                    List<ParameterInfo> list = parameterInfoes.ToList();
                    list.Add(new SignalGo.Shared.Models.ParameterInfo() { Name = KeyParameterName, Value = SignalGo.Client.ClientSerializationHelper.SerializeObject(KeyParameterValue) });
                    parameterInfoes = list.ToArray();
                }
                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                string headData = $"POST {uri.AbsolutePath} HTTP/1.1" + newLine + $"Host: {uri.Host}" + newLine + $"Content-Type: multipart/form-data; boundary={boundary}" + newLine;
                if (RequestHeaders != null && RequestHeaders.Count > 0)
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
        /// <returns></returns>
        public HttpClientResponse Post(string url, ParameterInfo[] parameterInfoes)
        {
#if (NETSTANDARD1_6)
            throw new NotSupportedException();
#else
            var response = PostHead(url, parameterInfoes);
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
                httpClientResponse.Data = Encoding.GetString(result);
                response = httpClientResponse;
                return httpClientResponse;
            }
            finally
            {
                response.TcpClient.Close();
            }
#endif
        }

#if (!NET35 && !NET40 && !NETSTANDARD1_6)
        public async Task<HttpClientResponseBase> PostHeadAsync(string url, ParameterInfo[] parameterInfoes)
        {
            string newLine = TextHelper.NewLine;
            Uri uri = new Uri(url);
            TcpClient tcpClient = new TcpClient(uri.Host, uri.Port);
            try
            {
                string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
                string headData = $"POST {uri.AbsolutePath} HTTP/1.1" + newLine + $"Host: {uri.Host}" + newLine + $"Content-Type: multipart/form-data; boundary={boundary}" + newLine;
                if (RequestHeaders != null && RequestHeaders.Count > 0)
                {
                    foreach (KeyValuePair<string, string[]> item in RequestHeaders)
                    {
                        if (!item.Key.Equals("host", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("content-type", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("content-length", StringComparison.OrdinalIgnoreCase))
                        {
                            if (item.Value == null || item.Value.Length == 0)
                                continue;
                            headData += item.Key + ": " + string.Join(",", item.Value);
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
                    await sslStream.AuthenticateAsClientAsync(uri.Host);
                }
                stream.Write(headBytes, 0, headBytes.Length);
                stream.Write(dataBytes, 0, dataBytes.Length);

                PipeNetworkStream pipelineReader = new PipeNetworkStream(new NormalStream(stream), 30000);

                List<string> lines = new List<string>();
                string line = null;
                do
                {
                    if (line != null)
                        lines.Add(line.TrimEnd());
                    line = await pipelineReader.ReadLineAsync();
                }
                while (line != newLine);
                HttpClientResponseBase httpClientResponse = new HttpClientResponseBase
                {
                    Status = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), lines[0].Split(' ')[1]),
                    ResponseHeaders = SignalGo.Shared.Http.WebHeaderCollection.GetHttpHeaders(lines.Skip(1).ToArray()),
                    Stream = pipelineReader,
                    TcpClient = tcpClient
                };
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
        public async Task<HttpClientResponse> PostAsync(string url, ParameterInfo[] parameterInfoes)
        {
            var response = await PostHeadAsync(url, parameterInfoes);
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
                    readedCount = await response.Stream.ReadAsync(bytes, bytes.Length);
                    for (int i = 0; i < readedCount; i++)
                    {
                        result[i + readCount] = bytes[i];
                    }
                    readCount += readedCount;
                }
                httpClientResponse.Data = Encoding.GetString(result);
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