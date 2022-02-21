using SignalGo.Server.DataTypes;
using SignalGo.Server.Models;
using SignalGoTest.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SignalGoTest.DataTypes
{
    public class TestSecurityPermissionsAttribute : SecurityContractAttribute
    {
        public bool IsAdmin { get; set; }
        public bool IsUser { get; set; }

        public override bool CheckPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters)
        {
            CurrentUserInfo current = OperationContext<CurrentUserInfo>.CurrentSetting;
            if (current == null)
                return false;
            if (current.UserInfo.IsAdmin && IsAdmin)
                return true;
            else if (current.UserInfo.IsUser && IsUser)
                return true;
            return false;
        }

        public override object GetValueWhenDenyPermission(ClientInfo client, object service, MethodInfo method, List<object> parameters)
        {
            return new MessageContract() { IsSuccess = false, Message = "Session access denied!" };
        }
    }
}
