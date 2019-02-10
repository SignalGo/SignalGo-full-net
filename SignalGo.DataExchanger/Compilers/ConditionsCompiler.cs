using SignalGo.DataExchanger.Conditions;
using SignalGo.DataExchanger.Helpers;
using SignalGo.DataExchanger.Methods.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SignalGo.DataExchanger.Compilers
{
    /// <summary>
    /// compile the conditions and wheres
    /// </summary>
    public class ConditionsCompiler
    {
        /// <summary>
        /// all of supported methods like 'count' 'sum' etc
        /// </summary>
        private Dictionary<string, IAddConditionSides> SupportedMethods { get; set; } = new Dictionary<string, IAddConditionSides>()
        {
            { "count" , new CountMethodInfo() },
            { "sum" , new SumMethodInfo() },
        };
        /// <summary>
        /// base variable on compiler 
        /// </summary>
        public VariableInfo VariableInfo { get; set; } = null;

        public void Compile(string query)
        {
            string selectQuery = query.Trim();
            //find first select word in the query we find this and then get select query from back
            int indexOfFirstSelect = selectQuery.IndexOf("var", StringComparison.OrdinalIgnoreCase);
            if (indexOfFirstSelect == 0)
            {
                selectQuery = query;
            }
            else
                return;
            int index = 0;
            VariableInfo = new VariableInfo()
            {
                WhereInfo = new WhereInfo()
            };
            VariableInfo.WhereInfo.Parent = VariableInfo;
            VariableInfo.WhereInfo.PublicVariables = VariableInfo.PublicVariables;
            ExtractVariables(selectQuery, ref index, VariableInfo.WhereInfo);
        }

        public object Run(object obj)
        {
            if (obj is IEnumerable)
                return GenerateArrayObject(obj, VariableInfo);
            else
                return VariableInfo.Run(obj);
        }

        public object Run<T>(object obj)
        {
            if (obj is IEnumerable)
                return GenerateArrayObject((IEnumerable<T>)obj, VariableInfo);
            else
                return (T)VariableInfo.Run(obj);
        }

        /// <summary>
        /// generate object that is in list
        /// </summary>
        /// <param name="data"></param>
        /// <param name="selectNode"></param>
        /// <returns></returns>
        internal static object GenerateArrayObject(object data, VariableInfo variableInfo)
        {
            return ((IEnumerable)data).Cast<object>().Where(x => (bool)variableInfo.Run(x));
        }

        internal static IEnumerable<T> GenerateArrayObject<T>(IEnumerable<T> data, VariableInfo variableInfo)
        {
            IEnumerable<T> result = data.Where(x =>
            {
                bool v = (bool)variableInfo.Run(x);
                return v;
            });
            return result;
        }

        private VariableInfo GetParentAsVariable(IAddConditionSides parent)
        {
            do
            {
                if (parent is VariableInfo variableInfo)
                    return variableInfo;
                parent = parent.Parent;
            }
            while (parent != null);
            return null;
        }

        /// <summary>
        /// extract full 'var'
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <param name="index"></param>
        /// <param name="parent"></param>
        private void ExtractVariables(string selectQuery, ref int index, IAddConditionSides parent)
        {
            //find variable with 'var' word
            byte foundVariableStep = 0;
            bool canSkip = true;
            //variable name that found after 'var'
            string variableName = "";
            string concat = "";
            //calculate all of char after select query
            for (int i = index; i < selectQuery.Length; i++)
            {
                //current char
                char currentChar = selectQuery[i];
                bool isWhiteSpaceChar = StringHelper.IsWhiteSpaceCharacter(currentChar);
                if (canSkip)
                {
                    //skip white space chars
                    if (isWhiteSpaceChar)
                        continue;
                    else
                        canSkip = false;
                }
                concat += currentChar;
                if (foundVariableStep == 2)
                {
                    if (string.IsNullOrEmpty(variableName) && string.IsNullOrEmpty(concat))
                        throw new Exception("I cannot name of variable before '{' char");
                    //breack char
                    if (StringHelper.IsWhiteSpaceCharacter(currentChar) || currentChar == '{')
                    {
                        variableName = concat.Trim('{').Trim();
                        GetParentAsVariable(parent).PublicVariables.Add(variableName, null);
                        index = i;
                        string afterIn = ExtractAfterIn(selectQuery, ref index);
                        if (afterIn != null)
                        {
                            i = index;
                            GetParentAsVariable(parent).PublicVariablePropertyNames.Add(variableName, afterIn.Trim());
                        }
                        concat = "";
                        index = i + 1;
                        ExtractInside(selectQuery, "where", '{', '}', ref index, parent);
                        break;
                    }
                }
                else if (foundVariableStep == 1)
                {
                    if (string.IsNullOrEmpty(variableName))
                    {
                        if (isWhiteSpaceChar)
                        {
                            variableName = concat.Trim();
                            concat = "";
                            canSkip = true;
                        }
                    }
                    else if (concat.Equals("in", StringComparison.OrdinalIgnoreCase))
                    {
                        concat = "";
                        foundVariableStep = 2;
                        canSkip = true;
                    }
                    else if (concat.Length >= 2)
                        throw new Exception($"I cannot find 'In' after variable name, are you sure don't forgot to write 'In' ? I found {concat}");
                }
                else if (concat.Equals("var", StringComparison.OrdinalIgnoreCase))
                {
                    concat = "";
                    canSkip = true;
                    //if that is first 'var' skip step 1 to find 'in' example (var x in user.posts) this is not first variable 
                    if (parent == GetParentAsVariable(parent).WhereInfo)
                        foundVariableStep = 2;
                    else
                        foundVariableStep = 1;
                }
            }
        }

        private string ExtractAfterIn(string selectQuery, ref int index)
        {
            bool canSkip = true;
            byte step = 0;
            string concat = "";
            for (int i = index; i < selectQuery.Length; i++)
            {
                //current char
                char currentChar = selectQuery[i];
                bool isWhiteSpaceChar = StringHelper.IsWhiteSpaceCharacter(currentChar);
                if (canSkip)
                {
                    //skip white space chars
                    if (isWhiteSpaceChar)
                        continue;
                    else
                        canSkip = false;
                }
                if (step == 0 && concat.Length == 2)
                {
                    if (concat.Equals("in", StringComparison.OrdinalIgnoreCase))
                    {
                        step = 1;
                        concat = "";
                        canSkip = true;
                    }
                    else
                        return null;
                }
                else if (step == 1)
                {
                    if (isWhiteSpaceChar || currentChar == '{')
                    {
                        index = i;
                        return concat;
                    }
                }
                concat += currentChar;
            }
            return null;
        }

        /// <summary>
        /// extract full 'where'
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <param name="index"></param>
        /// <param name="parent"></param>
        private void ExtractInside(string selectQuery, string breakString, char startChar, char breakChar, ref int index, IAddConditionSides parent)
        {
            //find variable with 'var' word
            byte foundVariableStep = 0;
            if (string.IsNullOrEmpty(breakString))
                foundVariableStep = 1;
            bool canSkip = true;
            //variable name that found after 'var'
            string variableName = "";
            string concat = "";
            //calculate all of char after select query
            for (int i = index; i < selectQuery.Length; i++)
            {
                //current char
                char currentChar = selectQuery[i];
                bool isWhiteSpaceChar = StringHelper.IsWhiteSpaceCharacter(currentChar);
                if (canSkip)
                {
                    //skip white space chars
                    if (isWhiteSpaceChar || (foundVariableStep == 0 && currentChar == startChar))
                        continue;
                    else
                        canSkip = false;
                }
                concat += currentChar;
                if (currentChar == breakChar)
                {
                    concat = concat.Trim('}');
                    CheckVariable(currentChar, ref i, ref index);
                    index = i;
                    break;
                }
                if (isWhiteSpaceChar && foundVariableStep == 0 && concat.Trim().Equals("in", StringComparison.OrdinalIgnoreCase))
                {
                    foundVariableStep = 3;
                    concat = "";
                    canSkip = true;
                }
                else if (foundVariableStep == 3 && isWhiteSpaceChar)
                {
                    parent.Add(new PropertyInfo() { PropertyPath = concat.Trim(), PublicVariables = parent.PublicVariables });
                    foundVariableStep = 0;
                    concat = "";
                    canSkip = true;
                }
                //find a side
                else if (foundVariableStep == 1)
                {
                    if (currentChar == '"')
                    {
                        index = i;
                        parent = ExtractString(selectQuery, ref index, parent);
                        i = index;
                        foundVariableStep = 2;
                        concat = "";
                        variableName = "";
                        canSkip = true;
                    }
                    else
                        CheckVariable(currentChar, ref i, ref index);
                }
                //find operator and parameter
                else if (foundVariableStep == 2)
                {
                    //find next parameter of method
                    if (currentChar == ',')
                    {
                        foundVariableStep = 1;
                        concat = concat.Trim().Trim(',').Trim();
                        canSkip = true;
                        continue;
                    }
                    //find operator
                    else
                    {
                        string trim = concat.Trim().ToLower();
                        //now this is a method
                        if (OperatorInfo.SupportedOperators.TryGetValue(trim, out OperatorType currentOperator))
                        {
                            if (OperatorInfo.OperatorStartChars.Contains(selectQuery[i + 1]))
                                continue;
                            parent.ChangeOperatorType(currentOperator);
                            //OperatorKey findEmpty = parent.WhereInfo.Operators.FirstOrDefault(x => x.OperatorType == OperatorType.None);
                            foundVariableStep = 1;
                            concat = "";
                            canSkip = true;
                        }
                        else if (concat.Equals("var", StringComparison.OrdinalIgnoreCase))
                        {
                            index = i;
                            do
                            {
                                index--;
                            }
                            while (selectQuery[index].ToString().ToLower() != "v");

                            VariableInfo newVariableInfo = new VariableInfo()
                            {
                                WhereInfo = new WhereInfo()
                            };
                            newVariableInfo.Parent = parent.Parent;
                            newVariableInfo.WhereInfo.Parent = newVariableInfo;
                            newVariableInfo.WhereInfo.PublicVariables = newVariableInfo.PublicVariables;
                            parent.Parent.Add(newVariableInfo);
                            ExtractVariables(selectQuery, ref index, newVariableInfo.WhereInfo);
                            i = index;
                            concat = "";
                            canSkip = true;
                        }
                        else if (concat.Length > 3)
                            throw new Exception($"I cannot found operator,I try but found '{concat}' are you sure you don't missed?");
                    }
                }
                else if (concat.Equals(breakString, StringComparison.OrdinalIgnoreCase))
                {
                    concat = "";
                    canSkip = true;
                    foundVariableStep = 1;
                }
            }

            void CheckVariable(char currentChar, ref int i, ref int index2)
            {
                string trim = concat.Trim();
                //step 1 of left side complete
                if (trim.Length > 0)
                {
                    bool isFindingOperator = OperatorInfo.OperatorStartChars.Contains(currentChar);
                    int findNextP = CheckFirstCharacterNoWitheSpace(selectQuery, i + 1, '(');
                    if (currentChar == '(' || findNextP >= 0)
                    {
                        if (string.IsNullOrEmpty(variableName))
                        {
                            IAddConditionSides addParent = null;
                            //check if variable name is nuot null so its a method
                            //else its just Parentheses
                            if (!string.IsNullOrEmpty(trim.Trim('(')))
                            {
                                //method name
                                variableName = trim.Trim('(').ToLower();
                                if (!SupportedMethods.TryGetValue(variableName, out IAddConditionSides addConditionSides))
                                    throw new Exception($"I cannot find method '{variableName}' are you sure you wrote true?");
                                else
                                {
                                    Tuple<IAddConditionSides, IAddConditionSides> value = addConditionSides.AddDouble(parent);
                                    parent = value.Item1;
                                    addParent = value.Item2;
                                }
                                variableName = "";
                            }
                            else
                                addParent = parent.Add();
                            concat = "";
                            if (findNextP >= 0)
                                index2 = findNextP + 1;
                            else
                                index2 = i + 1;
                            ExtractInside(selectQuery, null, '(', ')', ref index2, addParent);
                            i = index2;
                            foundVariableStep = 2;
                            canSkip = true;
                        }
                    }
                    else if (StringHelper.IsWhiteSpaceCharacter(currentChar) || isFindingOperator || currentChar == breakChar)
                    {
                        variableName = trim.Trim().Trim(breakChar).Trim(')').Trim(',').Trim();
                        concat = "";
                        canSkip = true;
                        //if there was no space this will fix that
                        //example user.name="ali" there is no space like user.name = "ali"
                        if (isFindingOperator)
                        {
                            variableName = variableName.Trim(OperatorInfo.OperatorStartChars);
                            i--;
                        }
                        if (foundVariableStep == 1)
                        {
                            //calculate left side
                            parent = CalculateSide(variableName, parent);
                            foundVariableStep++;
                            canSkip = true;
                            variableName = "";
                        }
                    }
                    else if (currentChar == ',')
                    {
                        variableName = trim.Trim().Trim(',').Trim();
                        if (!string.IsNullOrEmpty(variableName))
                        {
                            if (foundVariableStep == 1)
                            {
                                //calculate left side
                                parent = CalculateSide(variableName, parent);
                                foundVariableStep++;
                                canSkip = true;
                                variableName = "";
                                concat = "";
                            }
                            index2 = i;
                            ExtractInside(selectQuery, null, ',', ')', ref index2, parent);
                            i = index2 - 1;
                        }
                    }
                    //variable is method
                    //that is method so find method name and data
                    //or is just inside of Parentheses

                }
            }
        }
        /// <summary>
        /// check a character after you want to check
        /// for example we have this 'count      ('  i have to find last real char after ' ' to check if that is '(' because that will be a method
        /// </summary>
        /// <param name="query"></param>
        /// <param name="index"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        private int CheckFirstCharacterNoWitheSpace(string query, int index, char check)
        {
            for (int i = index; i < query.Length; i++)
            {
                char currentChar = query[i];
                bool isWhiteSpaceChar = StringHelper.IsWhiteSpaceCharacter(currentChar);
                if (isWhiteSpaceChar)
                    continue;
                else if (currentChar == check)
                    return i;
                else
                    return -1;
            }
            return -1;
        }

        /// <summary>
        /// extract string between two '"'
        /// </summary>
        /// <param name="selectQuery"></param>
        /// <param name="index"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private IAddConditionSides ExtractString(string selectQuery, ref int index, IAddConditionSides parent)
        {
            bool isStart = false;
            bool isAfterQuote = false;
            string concat = "";
            //calculate all of char after select query
            int i = index;
            for (; i < selectQuery.Length; i++)
            {
                //current char
                char currentChar = selectQuery[i];
                if (isStart)
                {
                    if (currentChar == '"')
                    {
                        //before this was '"'
                        if (isAfterQuote)
                        {
                            isAfterQuote = false;
                            concat += currentChar;
                        }
                        //is '"' after '"'
                        else if (selectQuery[i + 1] == '"')
                        {
                            isAfterQuote = true;
                            concat += currentChar;
                        }
                        else
                            break;
                    }
                    else
                    {
                        concat += currentChar;
                    }
                }
                else if (currentChar == '"')
                {
                    isStart = true;
                }
            }
            index = i;
            return CalculateSide(concat, parent, true);
        }


        /// <summary>
        /// after finished to read a side need to calculate
        /// sides are left and right of an operator like '=' '>' '>=' etc
        /// </summary>
        /// <param name="data"></param>
        /// <param name="parent"></param>
        /// <param name="isString"></param>
        private IAddConditionSides CalculateSide(string data, IAddConditionSides parent, bool isString = false)
        {
            IRunnable sideValue = null;
            if (isString || double.TryParse(data, out double newData))
                sideValue = new ValueInfo() { Value = data, PublicVariables = parent.PublicVariables };
            else
            {
                sideValue = new PropertyInfo() { PropertyPath = data, PublicVariables = parent.PublicVariables };
            }
            return parent.Add(sideValue);
        }
    }
}
