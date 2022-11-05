using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.ServiceManager.Core.Engines
{
    public static class HealthCheckEngine
    {
        public static async void Start()
        {
            while(true)
            {
                try
                {
                    List<Task> tasks = new List<Task>();
                    foreach (var server in SettingInfo.Current.ServerInfo.ToList())
                    {
                        await Task.Delay(100);
                        foreach (var healthCheck in UserSettingInfo.Current.HealthChecks.ToList())
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await healthCheck.Check(server);
                                }
                                catch (Exception ex)
                                {
                                    AutoLogger.Default.LogError(ex, $"HealthCheckEngine Run server {server.Name}");
                                }
                            }));
                        }
                    }

                    await Task.WhenAll(tasks);
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
