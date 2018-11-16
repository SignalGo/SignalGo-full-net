using SignalGo.DataExchanger.Helpers;
using SignalGo.DataExchanger.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.DataExchanger.Compilers
{
    /// <summary>
    /// compile select query from string to and objectable 
    /// </summary>
    public class SelectCompiler
    {
        /// <summary>
        /// select nodes after compile will generates
        /// </summary>
        public List<SelectNode> CompilerSelectNodes { get; set; }
        /// <summary>
        /// compile query and return another query out of select query
        /// </summary>
        /// <param name="query">full query</param>
        /// <returns>another query out of select query</returns>
        public string Compile(string query)
        {
            string selectQuery = query.Trim();
            //find first select word in the query we find this and then get select query from back
            int indexOfFirstSelect = selectQuery.IndexOf("select", StringComparison.OrdinalIgnoreCase);
            if (indexOfFirstSelect == 0)
            {
                selectQuery = query.Substring(6);
            }
            else
                return query;
            int index = 0;
            CompilerSelectNodes = GetListOfNodes(selectQuery, ref index, null).ToList();
            //ignore '}' char of end
            index++;
            return selectQuery.Substring(index);
        }

        /// <summary>
        /// get list of all nodes in select query
        /// </summary>
        /// <param name="query">query of select block</param>
        /// <param name="index">index to start</param>
        /// <param name="parent">prent of node</param>
        /// <returns></returns>
        private List<SelectNode> GetListOfNodes(string selectQuery, ref int index, SelectNode parent)
        {
            List<SelectNode> result = new List<SelectNode>();
            SelectNode selectNode = new SelectNode() { Parent = parent };
            //find block is '{' char
            bool isFoundStartBlock = false;
            //property name that found in blocks
            string propertName = "";
            string lastProperty = "";
            //calculate all of char after select query
            for (int i = index; i < selectQuery.Length; i++)
            {
                //current char
                char currentChar = selectQuery[i];
                bool isWhiteSpaceChar = StringHelper.IsWhiteSpaceCharacter(currentChar);
                if (!isFoundStartBlock)
                {
                    //skip white space chars
                    if (isWhiteSpaceChar)
                        continue;
                }

                if (isFoundStartBlock)
                {
                    if (string.IsNullOrEmpty(lastProperty) && string.IsNullOrEmpty(propertName) && currentChar == '{')
                        throw new Exception($"I found illegal charcter before find property name char is '{currentChar}'");
                    //breack char
                    else if (currentChar == '}')
                    {
                        //if property not added add it
                        if (!string.IsNullOrEmpty(propertName))
                            AddProperty(propertName);
                        result.Add(selectNode);
                        index = i;
                        break;
                    }
                    //start new array or object character
                    else if (currentChar == '{')
                    {
                        if (!string.IsNullOrEmpty(propertName))
                        {
                            lastProperty = propertName;
                            propertName = "";
                        }
                        index = i;
                        //recursive find it again for new childs
                        selectNode.Properties[lastProperty] = GetListOfNodes(selectQuery, ref index, selectNode);
                        lastProperty = "";
                        i = index;
                        continue;
                    }
                    //ignore white spaces or find next property name with it
                    else if (StringHelper.IsWhiteSpaceCharacter(currentChar))
                    {
                        if (string.IsNullOrEmpty(propertName))
                            continue;
                        AddProperty(propertName);
                        continue;
                    }
                    propertName += currentChar.ToString().ToLower();
                }
                else if (currentChar == '{')
                    isFoundStartBlock = true;
                else
                    throw new Exception($"could not found start block from your select query from index {index}");
            }

            //add new property
            void AddProperty(string propertyName)
            {
                if (selectNode.Properties.ContainsKey(propertName))
                    throw new Exception($"property name '{propertName}' is exist or duplicated, mybe your made '{{' or '}}' char wrong, check your query string again");
                selectNode.Properties.Add(propertName, null);
                lastProperty = propertName;
                propertName = "";
            }

            return result;
        }

        /// <summary>
        /// run select query on your object and filter it to new query
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public object Run(object data)
        {
            return GenerateObject(data, CompilerSelectNodes.FirstOrDefault());
        }

        private object GenerateObject(object currentData, SelectNode selectNode)
        {
            if (selectNode == null || currentData == null)
                return currentData;
            else if (currentData is IEnumerable)
                return GenerateArrayObject(currentData, selectNode);
            var type = currentData.GetType();
            var properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var propertyName = property.Name.ToLower();
                if (selectNode.Properties.TryGetValue(propertyName, out List<SelectNode> nodes))
                {
                    if (nodes == null || nodes.Count == 0)
                        continue;
                    var value = property.GetValue(currentData);
                    GenerateObject(value, nodes.FirstOrDefault());
                }
                else
                {
                    property.SetValue(currentData, Shared.Converters.DataExchangeConverter.GetDefault(property.PropertyType));
                }
            }
            return currentData;
        }

        private object GenerateArrayObject(object data, SelectNode selectNode)
        {
            foreach (var item in (IEnumerable)data)
            {
                GenerateObject(item, selectNode);
            }
            return data;
        }
    }
}
