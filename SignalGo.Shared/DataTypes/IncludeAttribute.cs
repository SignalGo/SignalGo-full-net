using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.DataTypes
{
    public class IncludeAttribute : CustomDataExchangerAttribute
    {
        public override CustomDataExchangerType ExchangeType { get; set; } = CustomDataExchangerType.Take;
    }
}
