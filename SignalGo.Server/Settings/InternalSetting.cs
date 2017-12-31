using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Server.Settings
{
    public class InternalSetting
    {
        public bool IsEnabledDataExchanger { get; set; }
        public bool IsEnabledReferenceResolver { get; set; } = true;
    }
}
