using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace SignalGo.Client
{
    public static class WebServiceProtocolHelper
    {
        public static T CallWebServiceMethod<T>(string url, string targetNamespace, string methodName, ParameterInfo[] args)
        {
            //url = "https://www.zarinpal.com/pg/services/WebGate/service";
            StringBuilder stringBuilder = new StringBuilder();
            if (args != null)
            {
                foreach (ParameterInfo item in args)
                {
                    stringBuilder.AppendLine($"<ns1:{item.Name}>{(string.IsNullOrEmpty(item.Value) ? "?" : item.Value)}</ns1:{item.Name}>");
                }
            }
            string defaultData = $@"<SOAP-ENV:Envelope xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns1=""{targetNamespace}"">
              <SOAP-ENV:Body>
                <ns1:{methodName}>
                    {stringBuilder.ToString()}
                </ns1:{methodName}>
              </SOAP-ENV:Body>
            </SOAP-ENV:Envelope>";
#if (!NETSTANDARD1_6)
            using (WebClient client = new WebClient())
            {
                string data = client.UploadString(url, defaultData);
                XDocument doc = XDocument.Parse(data);
                List<XElement> elements = new List<XElement>();
                foreach (XElement item in doc.Elements())
                {
                    FindAllElements(item, elements, typeof(T).Name);
                }
                object result = Activator.CreateInstance(typeof(T));
                foreach (XElement item in elements)
                {
                    string name = item.Name.LocalName;
                    System.Reflection.PropertyInfo property = result.GetType().GetProperty(name);
                    if (property != null)
                    {
                        property.SetValue(result, Newtonsoft.Json.JsonConvert.DeserializeObject("\"" + item.Value + "\"", property.PropertyType), null);
                    }
                }
                return (T)result;
            }
#else
            throw new NotSupportedException();
#endif
        }

        private static void FindAllElements(XElement element, List<XElement> elements, string name)
        {
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
