using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGo.DataExchanger.Helpers
{
    /// <summary>
    /// helper text
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// check if your char is white space char
        /// </summary>
        /// <param name="currentChar"></param>
        /// <returns></returns>
        public static bool IsWhiteSpaceCharacter(char currentChar)
        {
            return currentChar == ' ' || currentChar == '\r' || currentChar == '\n' || currentChar == '\t';
        }

        /// <summary>
        /// if character is illegal char in dataexchanger
        /// </summary>
        /// <param name="currentChar"></param>
        /// <returns></returns>
        public static bool IsIllegalCharacter(char currentChar)
        {
            return currentChar == '!' || currentChar == '@' || currentChar == '#' || currentChar == '$' || currentChar == '%' || currentChar == '^'
                || currentChar == '&' || currentChar == '*' || currentChar == '(' || currentChar == ')' || currentChar == '-' || currentChar == '+' || currentChar == '=';
        }
    }
}
