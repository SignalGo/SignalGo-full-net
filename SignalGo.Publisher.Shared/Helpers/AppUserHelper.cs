using SignalGo.Publisher.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGo.Publisher.Shared.Helpers
{
    public class AppUserHelper
    {
        public static AppUserDto GetCurrentUserInfo()
        {
            return new AppUserDto()
            {
                Name = System.Environment.UserName,
                DomainName = System.Environment.UserDomainName,
                MachineName = System.Environment.MachineName,
                Ip = GetCurrentUserIpAddress(),
            };
        }

        #region Utilities
        private static string GetCurrentUserIpAddress()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                   .Where(x => x.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet && x.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                   .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                   .Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                   .Select(x => x.Address.ToString())
                   //.ToList();
                   .FirstOrDefault();
        }
        #endregion
    }
}
