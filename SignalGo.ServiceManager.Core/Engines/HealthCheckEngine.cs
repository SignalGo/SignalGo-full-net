using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalGo.ServiceManager.Core.Engines
{
    public static class HealthCheckEngine
    {
        public static async void Start()
        {
            while (true)
            {
                try
                {
                    foreach (var server in SettingInfo.Current.ServerInfo.ToList())
                    {
                        await Task.Delay(100);
                        List<bool> all = new List<bool>();
                        foreach (var healthCheck in UserSettingInfo.Current.HealthChecks.ToList())
                        {
                            try
                            {
                                all.Add(await healthCheck.Check(server));
                            }
                            catch (Exception ex)
                            {
                                AutoLogger.Default.LogError(ex, $"HealthCheckEngine Run server {server.Name}");
                                all.Add(false);
                            }
                        }
                        server.IsHealthy = all.Count == 0 || all.Any(x => x);
                    }
                }
                catch (Exception ex)
                {
                    AutoLogger.Default.LogError(ex, "HealthCheckEngine");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}
