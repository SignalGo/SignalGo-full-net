using SignalGo.Publisher.Helpers;
using SignalGo.Publisher.Models;
using SignalGo.Publisher.ViewModels;
using System.Linq;

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
        /// skip if server authorized already
        /// </summary>
        /// <param name="server">server to authenticate</param>
        /// <returns></returns>
        public static bool HasAccess(this ServerInfo server)
        {
            // if access control it unlock grant access auto
            if (ProjectManagerWindowViewModel.This.IsAccessControlUnlocked)
                return true;
            // if server authorized already
            if (ServerInfo.Servers.Any(x => x.ServerKey == server.ServerKey))
                return true;

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
