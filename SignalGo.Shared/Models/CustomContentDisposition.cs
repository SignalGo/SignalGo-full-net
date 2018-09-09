using System;
using System.Collections.Generic;

namespace SignalGo.Shared.Models
{
    public class CustomContentDisposition
    {
        public CustomContentDisposition(string content)
        {
            if (string.IsNullOrEmpty(content))
                return;
            string value = content;
            if (content.ToLower().Contains("content-disposition:"))
            {
                int ctypeLen = content.ToLower().Contains("content-disposition: ") ? "content-disposition: ".Length : "content-disposition:".Length;
                value = content.Substring(ctypeLen, content.Length - ctypeLen);
            }

            foreach (string block in value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] keyValue = block.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (keyValue.Length > 0)
                {
                    keyValue[0] = keyValue[0].Trim();
                    if (keyValue.Length == 1)
                    {
                        if (keyValue[0].ToLower().Contains("form-data"))
                            continue;
                        Parameters.Add(keyValue[0], null);
                    }
                    else
                    {
                        keyValue[1] = keyValue[1].TrimStart().Trim('"');
                        Parameters.Add(keyValue[0], keyValue[1]);
                        if (keyValue[0].ToLower() == "filename")
                        {
                            FileName = keyValue[1];
                        }
                    }

                }
            }
        }

        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public string FileName { get; set; }
    }
}
