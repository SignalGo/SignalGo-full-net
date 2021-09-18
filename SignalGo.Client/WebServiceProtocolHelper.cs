using SignalGo.Shared.Helpers;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SignalGo.Client
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope<T>
    {
        [XmlElement(ElementName = "Header", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public string Header { get; set; }
        [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public T Body { get; set; }
        [XmlAttribute(AttributeName = "soap", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Soap { get; set; }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
    }

    public class WebServiceProtocolSettings
    {
        public int RetryCount { get; set; }
        public TimeSpan? Timeout { get; set; }
        public string EncodingName { get; set; } = "utf-8";
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public WebHeaderCollection ResponseHeaders { get; set; }
        public WebHeaderCollection RequestHeaders { get; set; } = new WebHeaderCollection();
    }

    /// <summary>ISO-8859-1
    /// log service
    /// </summary>
    public interface IWebServiceProtocolLogger
    {
        /// <summary>
        /// before method call
        /// </summary>
        LoggerAction BeforeCallAction { get; set; }
        /// <summary>
        /// after method call
        /// </summary>
        LoggerAction AfterCallAction { get; set; }

        WebServiceProtocolSettings Settings { get; set; }
    }
    public delegate void LoggerAction(string url, string actionUrl, string methodName, ParameterInfo[] args, object data);
    public static class WebServiceProtocolHelper
    {

        public static T CallWebServiceMethod<T>(IWebServiceProtocolLogger logger, string headerTemplate, string url, string actionUrl, string targetNamespace, string methodName, ParameterInfo[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (args != null)
            {
                foreach (ParameterInfo item in args)
                {
                    stringBuilder.AppendLine(item.Value.Replace($"<?xml version=\"1.0\" encoding=\"{logger.Settings.EncodingName}\"?>", ""));
                }
            }
            string defaultData = $@"<?xml version=""1.0"" encoding=""{logger.Settings.EncodingName}""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<soap:Header>
          {headerTemplate}
	</soap:Header>
	<soap:Body>{stringBuilder.ToString().Trim()}</soap:Body>
</soap:Envelope>";
#if (!NETSTANDARD1_6)

            using (IO.TimeoutWebClient client = new IO.TimeoutWebClient(logger.Settings.Timeout))
            {
                if (!string.IsNullOrEmpty(actionUrl))
                    client.Headers["SOAPAction"] = actionUrl;
                client.Headers.Add(HttpRequestHeader.ContentType, $"text/xml; charset={logger.Settings.EncodingName};");
                foreach (var item in logger.Settings.RequestHeaders.AllKeys)
                {
                    client.Headers.Add(item, logger.Settings.RequestHeaders[item]);
                }
                logger?.BeforeCallAction?.Invoke(url, actionUrl, methodName, args, defaultData);
                string data = client.UploadString(url, defaultData);
                logger.Settings.ResponseHeaders = client.ResponseHeaders;
                if (typeof(T) == typeof(object))
                    return default(T);
                logger?.AfterCallAction?.Invoke(url, actionUrl, methodName, args, data);
                XDocument doc = XDocument.Parse(data);
                List<XElement> elements = new List<XElement>();
                var firstElement = doc.Elements().First();//
                var findElement = FindElement(doc.Elements(), typeof(T).Name);
                var attributes = firstElement.Attributes().ToList();
                attributes.AddRange(findElement.Attributes());
                findElement.ReplaceAttributes(attributes);
                var myResult = (T)Deserialize(logger, findElement, typeof(T));
                return myResult;
            }
#else
            throw new NotSupportedException();
#endif
        }
#if (!NETSTANDARD1_6 && !NET35 && !NET40)
        static System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

        private static string SerializeHeaders(this System.Net.Http.Headers.HttpRequestHeaders headers)
        {
            var response = new System.Text.StringBuilder();
            foreach (var k in headers)
                response.AppendLine(k.Key + ": " + k.Value.FirstOrDefault());
            return response.ToString();
        }

        public static async Task<T> CallWebServiceMethodAsync<T>(IWebServiceProtocolLogger logger, string headerTemplate, string url, string actionUrl, string targetNamespace, string methodName, ParameterInfo[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (args != null)
            {
                foreach (ParameterInfo item in args)
                {
                    stringBuilder.AppendLine(item.Value.Replace($"<?xml version=\"1.0\" encoding=\"{logger.Settings.EncodingName}\"?>", ""));
                }
            }
            string defaultData = $@"<?xml version=""1.0"" encoding=""{logger.Settings.EncodingName}""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<soap:Header>
          {headerTemplate}
	</soap:Header>
	<soap:Body>{stringBuilder.ToString().Trim()}</soap:Body>
</soap:Envelope>";
            //using (IO.TimeoutWebClient client = new IO.TimeoutWebClient(logger.Settings.Timeout))
            //{

            int tryCount = 0;
            logger?.BeforeCallAction?.Invoke(url, actionUrl, methodName, args, defaultData);

            TryAgainLabel:
            var httpRequestMessage = new System.Net.Http.HttpRequestMessage();
            var content_ = new System.Net.Http.StringContent(defaultData);
            content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/xml");
            httpRequestMessage.Content = content_;

            if (!string.IsNullOrEmpty(actionUrl))
                httpRequestMessage.Headers.TryAddWithoutValidation("SOAPAction", actionUrl);
            httpRequestMessage.Headers.TryAddWithoutValidation("ContentType", "application/xml");
            foreach (var item in logger.Settings.RequestHeaders.AllKeys)
            {
                var value = logger.Settings.RequestHeaders[item];
                httpRequestMessage.Headers.TryAddWithoutValidation(item, value);
            }

            httpRequestMessage.RequestUri = new Uri(url);
            httpRequestMessage.Method = new System.Net.Http.HttpMethod("POST");
            try
            {
                var cts = new CancellationTokenSource();
                if (logger.Settings.Timeout.HasValue)
                    cts.CancelAfter(logger.Settings.Timeout.Value);
                try
                {
                    using (System.Net.Http.HttpResponseMessage response = await _httpClient.SendAsync(httpRequestMessage, System.Net.Http.HttpCompletionOption.ResponseContentRead, cts.Token))
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        if (response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> values))
                        {
                            string type = values.FirstOrDefault();
                            if (type != null && !type.ToLower().Contains("text/xml"))
                            {
                                tryCount = logger.Settings.RetryCount;
                                throw new Exception($"I just support text/xml as content type your type is {type} and your data is :\\r\\n {data}");
                            }
                        }

                        logger.Settings.ResponseHeaders = new WebHeaderCollection();
                        foreach (var item in response.Headers)
                        {
                            logger.Settings.ResponseHeaders.Add(item.Key, item.Value.FirstOrDefault());
                        }

                        //System.Diagnostics.Debug.WriteLine(data, $"Response: {url} ac:{actionUrl}");
                        if (typeof(T) == typeof(object))
                            return default(T);
                        logger?.AfterCallAction?.Invoke(url, actionUrl, methodName, args, data);
                        XDocument doc = XDocument.Parse(data);
                        var firstElement = doc.Elements().First();//
                        var findElement = FindElement(doc.Elements(), typeof(T).Name);
                        if (findElement == null)
                        {
                            findElement = doc.Elements().FirstOrDefault(x => x.Name.LocalName.EndsWith("envelope", StringComparison.OrdinalIgnoreCase));
                            findElement = findElement.Elements().FirstOrDefault(x => x.Name.LocalName.EndsWith("body", StringComparison.OrdinalIgnoreCase));
                            findElement = findElement.Elements().FirstOrDefault();
                            XmlDocument node = new XmlDocument();
                            node.LoadXml(findElement.Elements().First().ToString());
                            //var name = findElement.Name.LocalName;
                            //data = data.Replace(name, typeof(T).Name);
                            //doc = XDocument.Parse(data);
                            //firstElement = doc.Elements().First();//
                            //findElement = FindElement(doc.Elements(), typeof(T).Name);
                            string json = Newtonsoft.Json.JsonConvert.SerializeXmlNode(node);
                            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, JsonSettingHelper.GlobalJsonSetting);
                            //if (typeof(T).GetListOfProperties().Count() == 2)
                            //{
                            //    var property = typeof(T).GetListOfProperties().First();
                            //    findElement = FindElement(doc.Elements(), property.Name);
                            //    var obj = Activator.CreateInstance(typeof(T));
                            //    try
                            //    {
                            //        property.SetValue(obj, Convert.ChangeType(findElement.Value, property.PropertyType));
                            //        return (T)obj;
                            //    }
                            //    catch (Exception)
                            //    {
                            //    }
                            //}
                        }
                        var attributes = firstElement.Attributes().ToList();
                        attributes.AddRange(findElement.Attributes());
                        findElement.ReplaceAttributes(attributes);
                        var myResult = (T)Deserialize(logger, findElement, typeof(T));
                        return myResult;
                    }
                }
                catch (Exception ex)
                {
                    tryCount++;
                    if (tryCount >= logger.Settings.RetryCount)
                        throw ex;
                    goto TryAgainLabel;
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, $"UploadString has error to url {url} with request {defaultData} with headers {SerializeHeaders(httpRequestMessage.Headers)}");
                throw;
            }
        }
#endif
        public static string Serialize(IWebServiceProtocolLogger logger, object data, string targetNamespace)
        {
            var type = data.GetType();
            var attribute = type.GetCustomAttributes<XmlTypeAttribute>();
            string nameSpace = targetNamespace;
            if (attribute.Length > 0)
                nameSpace = attribute.FirstOrDefault().Namespace;
            if (nameSpace == "SignalGoStuff")
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in type.GetListOfProperties())
                {
                    var propertyAttributes = item.PropertyType.GetCustomAttributes<XmlTypeAttribute>();
                    if (propertyAttributes.Length > 0)
                    {
                        nameSpace = propertyAttributes.FirstOrDefault().Namespace;
                        var value = item.GetValue(data, null);
                        if (value != null)
                        {
                            stringBuilder.AppendLine(Serialize(logger, value, nameSpace));
                        }
                    }
                    else
                    {
                        nameSpace = attribute.FirstOrDefault().Namespace;
                        var value = item.GetValue(data, null);
                        if (value != null)
                        {
                            stringBuilder.AppendLine($"<{item.Name}>{value}<{item.Name}/>");
                        }
                    }
                }
                stringBuilder.Replace($"<?xml version=\"1.0\" encoding=\"{logger.Settings.EncodingName}\"?>", "");
                stringBuilder.Replace($"<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                stringBuilder.Insert(0, $"<?xml version=\"1.0\" encoding=\"{logger.Settings.EncodingName}\"?>");
                return stringBuilder.ToString();
            }
            XmlSerializer ser = null;
            if (string.IsNullOrEmpty(nameSpace))
                ser = new XmlSerializer(data.GetType());
            else
                ser = new XmlSerializer(data.GetType(), nameSpace);
            // Creates a DataSet; adds a table, column, and ten rows.
            using (var baseStream = new MemoryStream())
            {
                using (var stream = new StreamWriter(baseStream, logger.Settings.Encoding))
                {

                    ser.Serialize(stream, data);
                    baseStream.Seek(0, SeekOrigin.Begin);
                    var result = logger.Settings.Encoding.GetString(baseStream.ToArray());
                    //result = result.Replace($"<?xml version=\"1.0\" encoding=\"{logger.Settings.EncodingName}\"?>", "");
                    //result = result.Replace($"<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                    //result = $"<?xml version=\"1.0\" encoding=\"{logger.Settings.EncodingName}\"?>" + result;
                    return result;
                }
            }
        }
        public static object Deserialize(IWebServiceProtocolLogger logger, XElement element, Type type)
        {
            var xml = element.ToString();
            xml = $"<?xml version=\"1.0\" encoding=\"{logger.Settings.EncodingName}\"?>" + xml;
            XmlSerializer ser = new XmlSerializer(type);
            using (var stream = new MemoryStream(logger.Settings.Encoding.GetBytes(xml)))
            {
                stream.Seek(0, SeekOrigin.Begin);
                return ser.Deserialize(stream);
            }
        }
        //        public static T CallWebServiceMethod<T>(string url, string targetNamespace, string methodName, ParameterInfo[] args)
        //        {
        //            //url = "https://www.zarinpal.com/pg/services/WebGate/service";
        //            StringBuilder stringBuilder = new StringBuilder();
        //            if (args != null)
        //            {
        //                foreach (ParameterInfo item in args)
        //                {
        //                    stringBuilder.AppendLine($"<ns1:{item.Name}>{(string.IsNullOrEmpty(item.Value) ? "?" : item.Value)}</ns1:{item.Name}>");
        //                }
        //            }
        //            string defaultData = $@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""{targetNamespace}"">
        //              <SOAP-ENV:Body>
        //                <ns1:{methodName}>
        //                    {stringBuilder.ToString()}
        //                </ns1:{methodName}>
        //              </SOAP-ENV:Body>
        //            </SOAP-ENV:Envelope>";
        //#if (!NETSTANDARD1_6)
        //            using (WebClient client = new WebClient())
        //            {
        //                string data = client.UploadString(url, defaultData);
        //                XDocument doc = XDocument.Parse(data);
        //                List<XElement> elements = new List<XElement>();
        //                foreach (XElement item in doc.Elements())
        //                {
        //                    FindAllElements(item, elements, typeof(T).Name);
        //                }
        //                object result = Activator.CreateInstance(typeof(T));
        //                foreach (XElement item in elements)
        //                {
        //                    string name = item.Name.LocalName;
        //                    System.Reflection.PropertyInfo property = result.GetType().GetProperty(name);
        //                    if (property != null)
        //                    {
        //                        property.SetValue(result, Newtonsoft.Json.JsonConvert.DeserializeObject("\"" + item.Value + "\"", property.PropertyType), null);
        //                    }
        //                }
        //                return (T)result;
        //            }
        //#else
        //            throw new NotSupportedException();
        //#endif
        //        }
        public static XElement FindElement(IEnumerable<XElement> elements, string name)
        {
            foreach (XElement item in elements)
            {
                if (item.Name.LocalName == name)
                {
                    return item;
                }
                var find = FindElement(item.Elements(), name);
                if (find != null)
                    return find;
            }
            return null;
        }
        public static void FindAllElements(XElement element, List<XElement> elements, string name)
        {
            if (element.Name.LocalName == name)
            {
                foreach (XElement find in element.Elements())
                    elements.Add(find);
                return;
            }
            foreach (XElement item in element.Elements())
            {
                if (item.Name.LocalName == name)
                {
                    foreach (XElement find in item.Elements())
                        elements.Add(find);
                    break;
                }
                FindAllElements(item, elements, name);
            }
        }
    }
}
