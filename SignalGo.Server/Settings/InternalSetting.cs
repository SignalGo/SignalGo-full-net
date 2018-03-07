using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Server.Settings
{
    public class InternalSetting
    {
        public bool IsEnabledDataExchanger { get; set; } = true;
        public bool IsEnabledReferenceResolver { get; set; } = true;
        public bool IsEnabledReferenceResolverForArray { get; set; } = true;
        public List<HttpKeyAttribute> HttpKeyResponses { get; set; }
    }
}
