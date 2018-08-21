using System.Text;

namespace System
{
    public static class ExtensionHelper
    {
        /// <summary>
        /// exception to full text message
        /// </summary>
        /// <param name="ex">your exception</param>
        /// <returns></returns>
        public static string ToTextMesage(this Exception ex)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("Start Exception");
            result.AppendLine(ex.Message);
            if (!string.IsNullOrEmpty(ex.StackTrace))
                result.AppendLine(ex.StackTrace);
            string inner = InitInnerExceptions(ex.InnerException);
            if (!string.IsNullOrEmpty(inner))
            {
                result.AppendLine("Start Inners");
                result.AppendLine(inner);
                result.AppendLine("End Inners");
            }
            result.AppendLine("End Exception");
            return result.ToString();
        }

        private static string InitInnerExceptions(Exception ex)
        {
            if (ex == null)
                return null;
            StringBuilder result = new StringBuilder();
            result.AppendLine(ex.Message);
            if (!string.IsNullOrEmpty(ex.StackTrace))
                result.AppendLine(ex.StackTrace);
            result.AppendLine(InitInnerExceptions(ex.InnerException));
            return result.ToString();
        }
    }
}
