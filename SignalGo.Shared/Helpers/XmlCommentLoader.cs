#if (!PORTABLE)
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace SignalGo.Shared.Helpers
{
    /// <summary>
    /// commment of class type
    /// </summary>
    public class CommentOfClassInfo
    {
        /// <summary>
        /// name of class
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// summery of comment
        /// </summary>
        public string Summery { get; set; }
        /// <summary>
        /// list of methods
        /// </summary>
        public List<CommentOfMethodInfo> Methods { get; set; }
    }

    /// <summary>
    /// comment of method
    /// </summary>
    public class CommentOfMethodInfo
    {
        /// <summary>
        /// name of method
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// comment of summery
        /// </summary>
        public string Summery { get; set; }
        /// <summary>
        /// comment of return value
        /// </summary>
        public string Returns { get; set; }
        /// <summary>
        /// list of parameters comment
        /// </summary>
        public List<CommentOfParameterInfo> Parameters { get; set; }
        /// <summary>
        /// list of exceptions comments
        /// </summary>
        public List<CommentOfExceptionInfo> Exceptions { get; set; }
    }

    /// <summary>
    /// comment of parameters
    /// </summary>
    public class CommentOfParameterInfo
    {
        /// <summary>
        /// name of parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// comment of parameter
        /// </summary>
        public string Comment { get; set; }
    }

    /// <summary>
    /// comment of excetions
    /// </summary>
    public class CommentOfExceptionInfo
    {
        /// <summary>
        /// comment of exception
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// type of exception
        /// </summary>
        public string RefrenceType { get; set; }
    }

    /// <summary>
    /// load xml document files
    /// </summary>
    public class XmlCommentLoader : IDisposable
    {
        /// <summary>
        /// skip exceptions and errors
        /// </summary>
        public bool SkipErrors { get; set; } = true;
        /// <summary>
        /// get comment of method
        /// </summary>
        /// <param name="methodInfo">method info</param>
        /// <returns>comment of method</returns>
        public CommentOfMethodInfo GetCommment(MethodInfo methodInfo)
        {
            var documentation = GetElementFromMethod(methodInfo);
            if (documentation == null)
                return null;
            List<CommentOfParameterInfo> parameters = new List<CommentOfParameterInfo>();
            foreach (var item in methodInfo.GetParameters())
            {
                var param = (from x in documentation.ChildNodes.Cast<XmlElement>() where x.Name.ToLower() == "param" && x.GetAttribute("name") == item.Name select x).FirstOrDefault();
                if (param != null)
                {
                    parameters.Add(new CommentOfParameterInfo() { Name = item.Name, Comment = param.InnerText?.Trim() });
                }
            }
            List<CommentOfExceptionInfo> exceptions = new List<CommentOfExceptionInfo>();
            foreach (var item in (from x in documentation.ChildNodes.Cast<XmlElement>() where x.Name.ToLower() == "exception" select x).ToList())
            {
                var value = item.Attributes["cref"].Value;
                if (value.IndexOf(":") == 1)
                    value = value.Substring(2, value.Length - 2);
                exceptions.Add(new CommentOfExceptionInfo() { RefrenceType = value, Comment = item.InnerText?.Trim() });
            }

            return new CommentOfMethodInfo()
            {
                Summery = documentation["summary"]?.InnerText?.Trim(),
                Returns = documentation["returns"]?.InnerText?.Trim(),
                Parameters = parameters,
                Name = methodInfo.Name,
                Exceptions = exceptions
            };
        }

        /// <summary>
        /// get comment of class type
        /// </summary>
        /// <param name="classInfo">your type</param>
        /// <returns>comment of class</returns>
        public CommentOfClassInfo GetComment(Type classInfo)
        {
            var documentation = GetElementFromType(classInfo);
            if (documentation == null)
                return null;
            List<CommentOfMethodInfo> methods = new List<CommentOfMethodInfo>();
            foreach (var item in classInfo.GetListOfMethods())
            {
                var comment = GetCommment(item);
                if (comment == null)
                    continue;
                methods.Add(comment);
            }
            return new CommentOfClassInfo()
            {
                Name = classInfo.Name,
                Summery = documentation["summary"]?.InnerText?.Trim(),
                Methods = methods
            };
        }

        ConcurrentDictionary<string, XmlDocument> LoadedAssemblies { get; set; } = new ConcurrentDictionary<string, XmlDocument>();
        private XmlDocument LoadFromAssembly(Assembly assembly)
        {
            try
            {
                string fileName = Path.ChangeExtension(assembly.Location, ".xml");
                if (LoadedAssemblies.TryGetValue(fileName, out XmlDocument result))
                    return result;
                if (SkipErrors && !File.Exists(fileName))
                    return null;
                using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.Load(streamReader);
                        LoadedAssemblies.TryAdd(fileName, xmlDocument);
                        return xmlDocument;
                    }
                }
            }
            catch (FileNotFoundException exception)
            {
                if (!SkipErrors)
                    throw new Exception($"xml file not found! {assembly.Location}", exception);
                else
                    return null;
            }
        }

        private XmlElement GetElementFromType(Type type)
        {
            return GetElementFromName(type, 'T', "");
        }

        private XmlElement GetElementFromMethod(MethodInfo methodInfo)
        {
            string parameters = "";
            foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
            {
                if (!string.IsNullOrEmpty(parameters))
                    parameters += ",";
                parameters += GetParameterFullName(parameterInfo.ParameterType);
            }

            return GetElementFromName(methodInfo.DeclaringType, 'M', string.IsNullOrEmpty(parameters) ? methodInfo.Name : methodInfo.Name + "(" + parameters + ")");
        }

        string GetParameterFullName(Type type)
        {
            if (type.GetIsGenericType())
            {
                string generics = "";
                foreach (var item in type.GetListOfGenericArguments())
                {
                    if (!string.IsNullOrEmpty(generics))
                    {
                        generics += ",";
                    }
                    generics += GetParameterFullName(item);
                }
                string name = "";
                if (type.Name.IndexOf("`") != -1)
                {
                    name = type.Name.Substring(0, type.Name.IndexOf("`"));
                }
                return $"{type.Namespace}.{name}{{{generics}}}";
            }
            else
                return type.FullName;
        }

        private XmlElement GetElementFromName(Type type, char prefix, string name)
        {
            string nodeName;

            nodeName = prefix + ":" + type.FullName;

            if (!string.IsNullOrEmpty(name))
                nodeName += "." + name;

#if (NETSTANDARD1_6 || NETCOREAPP1_1)
            XmlDocument xmlDocument = LoadFromAssembly(type.GetTypeInfo().Assembly);
#else
            XmlDocument xmlDocument = LoadFromAssembly(type.Assembly);
#endif
            if (xmlDocument == null)
                return null;
            XmlElement findElement = null;

            foreach (var xmlElement in xmlDocument["doc"]["members"])
            {
                if (xmlElement is XmlElement)
                {
                    var element = xmlElement as XmlElement;
                    if (element.Attributes["name"].Value.Equals(nodeName))
                    {
                        if (findElement != null)
                        {
                            continue;
                        }

                        findElement = element;
                    }
                }
               
            }

            if (findElement == null)
            {
                if (!SkipErrors)
                    throw new Exception($"element {name} not found!");
            }

            return findElement;
        }

        /// <summary>
        /// dispose
        /// </summary>
        public void Dispose()
        {
            LoadedAssemblies.Clear();
        }
    }
}
#endif