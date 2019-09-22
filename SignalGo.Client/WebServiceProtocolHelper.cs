using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SignalGo.Client
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope
    {
        [XmlElement(ElementName = "Header", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public string Header { get; set; }
        [XmlElement(ElementName = "Body", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
        public object Body { get; set; }
        [XmlAttribute(AttributeName = "soap", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Soap { get; set; }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
    }
    public static class WebServiceProtocolHelper
    {
        public static T CallWebServiceMethod<T>(string headerTemplate, string url, string actionUrl, string targetNamespace, string methodName, ParameterInfo[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (args != null)
            {
                foreach (ParameterInfo item in args)
                {
                    stringBuilder.AppendLine(item.Value.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ""));
                }
            }
            string defaultData = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<soap:Header>
          {headerTemplate}
	</soap:Header>
	<soap:Body>{stringBuilder.ToString().Trim()}</soap:Body>
</soap:Envelope>";
#if (!NETSTANDARD1_6)
            using (WebClient client = new WebClient())
            {
                if (!string.IsNullOrEmpty(actionUrl))
                    client.Headers["SOAPAction"] = actionUrl;
                client.Headers.Add(HttpRequestHeader.ContentType, "text/xml; charset=utf-8;");
                string data = client.UploadString(url, defaultData);
                if (typeof(T) == typeof(object))
                    return default;
                XDocument doc = XDocument.Parse(data);
                List<XElement> elements = new List<XElement>();
                var element = FindElement(doc.Elements(), typeof(T).Name);
                return (T)Deserialize(element, typeof(T));
            }
#else
            throw new NotSupportedException();
#endif
        }
#if (!NETSTANDARD1_6 && !NET35 && !NET40)

        public static async Task<T> CallWebServiceMethodAsync<T>(string headerTemplate, string url, string actionUrl, string targetNamespace, string methodName, ParameterInfo[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (args != null)
            {
                foreach (ParameterInfo item in args)
                {
                    stringBuilder.AppendLine(item.Value.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", ""));
                }
            }
            string defaultData = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<soap:Header>
          {headerTemplate}
	</soap:Header>
	<soap:Body>{stringBuilder.ToString().Trim()}</soap:Body>
</soap:Envelope>";
            using (WebClient client = new WebClient())
            {
                if (!string.IsNullOrEmpty(actionUrl))
                    client.Headers["SOAPAction"] = actionUrl;
                client.Headers.Add(HttpRequestHeader.ContentType, "text/xml; charset=utf-8;");
                string data = await client.UploadStringTaskAsync(url, defaultData);
                if (typeof(T) == typeof(object))
                    return default;
                XDocument doc = XDocument.Parse(data);
                List<XElement> elements = new List<XElement>();
                var element = FindElement(doc.Elements(), typeof(T).Name);
                return (T)Deserialize(element, typeof(T));
            }
        }
#endif
        public static string Serialize(object data, string targetNamespace)
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
                            stringBuilder.AppendLine(Serialize(value, nameSpace));
                        }
                    }
                }
                stringBuilder.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");
                stringBuilder.Insert(0, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
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
                using (var stream = new StreamWriter(baseStream))
                {
                    ser.Serialize(stream, data);
                    baseStream.Seek(0, SeekOrigin.Begin);
                    return Encoding.UTF8.GetString(baseStream.ToArray());
                }
            }
        }
        public static object Deserialize(XElement element, Type type)
        {
            var xml = element.ToString();
            xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + xml;
            XmlSerializer ser = new XmlSerializer(type);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
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
