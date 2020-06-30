using SignalGo.ServiceManager.Core.Models;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SignalGo.ServerManager.WpfApp.Helpers
{
    public static class ProcessTabLoader
    {
        static Dictionary<ServerInfo, TabInfo> Tabs { get; set; } = new Dictionary<ServerInfo, TabInfo>();
        public static void Add(ServerInfo serverInfo, TabItem tabItem)
        {
            if (Tabs.TryGetValue(serverInfo, out TabInfo tabInfo))
            {
                Tabs.Remove(serverInfo);
                tabInfo.Dispose();
            }
            Tabs[serverInfo] = new TabInfo(tabItem);
        }

        public static void SetEnabled(bool value, ServerInfo serverInfo)
        {
            if (Tabs.TryGetValue(serverInfo, out TabInfo tabInfo))
            {
                tabInfo.IsEnabled = value;
                if (tabInfo.ServerInfoViewModel.ServerInfo.CurrentServerBase?.BaseProcess != null)
                    TabInfo.UpdateServerInfoLayout(tabInfo.ServerInfoViewModel.ServerInfo.CurrentServerBase.BaseProcess);
            }
        }
    }
}
