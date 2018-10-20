using SignalGo.Shared.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.Models
{
    public class CurrentUserInfo
    {
        public UserInfo UserInfo { get; set; }

        [HttpKey(KeyType = HttpKeyType.Cookie)]
        public string Session { get; set; }

        [HttpKey(KeyType = HttpKeyType.ExpireField)]
        public DateTime ExpireDate { get; set; }
    }
}
