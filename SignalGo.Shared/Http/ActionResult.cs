using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Shared.Http
{
    public class ActionResult
    {
        public object Data { get; private set; }

        public ActionResult(object data)
        {
            Data = data;
        }

        public static implicit operator ActionResult(string text)
        {
            return new ActionResult(text);
        }
    }
}
