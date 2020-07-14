using SignalGo.Publisher.Helpers;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.ViewModels;

namespace SignalGo.Publisher.Extensions
{
    /// <summary>
    /// Make sure the user has execute access. 
    /// Using interactive and automatic authorization method's.
    /// </summary>
    public static class AccessControlExtensions
    {
        /// <summary>
        /// Check the server are valid and Make sure the user has access.
        /// </summary>
        /// <param name="server">server to validate</param>
        /// <returns></returns>
        public static bool HasAccess(this ServerInfo server)
        {
            //if(UserSettingInfo.Current.UserSettings.IsAccessControlUnlocked)
            if(ProjectManagerWindowViewModel.This.IsAccessControlUnlocked)
            {
                return true;
            }
            if (server == null)
                return false;
            else if (string.IsNullOrEmpty(server.ProtectionPassword))
                return true;
            else
            {
                return CommandAuthenticator.Authorize(ref server);
            }
        }


    }
}
