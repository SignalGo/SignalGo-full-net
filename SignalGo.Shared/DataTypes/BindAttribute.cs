using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Shared.DataTypes
{
    public class BindAttribute : CustomDataExchangerAttribute
    {
        public BindAttribute()
        {
            base.LimitationMode = LimitExchangeType.Both;
        }

        string _Exclude;
        string _Include;
        string[] _Excludes;
        string[] _Includes;

        public string Exclude
        {
            get
            {
                return _Exclude;
            }
            set
            {
                _Exclude = value;
                Properties = new string[] { value };
                ExchangeType = CustomDataExchangerType.Ignore;
            }
        }

        public string Include
        {
            get
            {
                return _Include;
            }
            set
            {
                _Include = value;
                Properties = new string[] { value };
                ExchangeType = CustomDataExchangerType.Take;
            }
        }

        public string[] Excludes
        {
            get
            {
                return _Excludes;
            }
            set
            {
                _Excludes = value;
                Properties = value;
                ExchangeType = CustomDataExchangerType.Ignore;
            }
        }

        public string[] Includes
        {
            get
            {
                return _Includes;
            }
            set
            {
                _Includes = value;
                Properties = value;
                ExchangeType = CustomDataExchangerType.Take;
            }
        }
    }
}
