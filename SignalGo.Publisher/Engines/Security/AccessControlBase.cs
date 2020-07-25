using SignalGo.Publisher.Engines.Models;
using SignalGo.Publisher.Models;

namespace SignalGo.Publisher.Engines.Security
{
    public abstract class AccessControlBase
    {
        public static bool AuthorizeServer(string secret, ref ServerInfo serverInfo)
        {
            if (serverInfo.ProtectionPassword == PasswordEncoder.ComputeHash(secret))
            {
                ServerInfo.Servers.Add(serverInfo.Clone());
                return true;
            }
            else
                return false;
        }
        public static bool CheckMasterPassword(string secret)
        {
            if (UserSettingInfo.Current.UserSettings.ApplicationMasterPassword == PasswordEncoder.ComputeHash(secret))
                return true;
            else
                return false;
        }
    }
}
