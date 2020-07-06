using System;
using MvvmGo.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using SignalGo.Shared.Log;

namespace SignalGo.Publisher.Models.Extra
{
    public class LogModule
    {

        public static Dictionary<string, Dictionary<SectorType, ObservableCollection<LogInfo>>> LogsDictionary { get; set; } = new Dictionary<string, Dictionary<SectorType, ObservableCollection<LogInfo>>>();

        /// <summary>
        /// add log to specified SectorName of program and specified section of page
        /// </summary>
        /// <param name="sector">program Sector Name Like ProjectName</param>
        /// <param name="sectorType">section of sector (Like ServerManagment,Builder Tab)</param>
        /// <param name="logText">message</param>
        /// <param name="dateTime">time</param>
        /// <param name="logType">type of message like Error,Info</param>
        public static void AddLog(string sector, SectorType sectorType, string logText, string dateTime, LogTypeEnum logType = LogTypeEnum.Info)
        {
            GenerateSector(sector, sectorType, out ObservableCollection<LogInfo> items);
            BaseViewModel.RunOnUIAction(() =>
            {
                items.Add(new LogInfo(logText, dateTime, logType));
            });
        }

        public static void AddLog(string sector, SectorType sectorType, string logText, LogTypeEnum logType = LogTypeEnum.Info)
        {
            GenerateSector(sector, sectorType, out ObservableCollection<LogInfo> items);
            BaseViewModel.RunOnUIAction(() =>
            {
                items.Add(new LogInfo(logText, logType));
            });
        }
        static void GenerateSector(string sector, SectorType sectorType, out ObservableCollection<LogInfo> logs)
        {
            if (LogsDictionary.TryGetValue(sector, out Dictionary<SectorType, ObservableCollection<LogInfo>> sectors))
            {
                if (!sectors.TryGetValue(sectorType, out logs))
                {
                    logs = sectors[sectorType] = new ObservableCollection<LogInfo>();
                }
            }
            else
            {
                sectors = new Dictionary<SectorType, ObservableCollection<LogInfo>>();
                LogsDictionary[sector] = sectors;
                logs = new ObservableCollection<LogInfo>();
                sectors[sectorType] = logs;
            }
        }
        /// <summary>
        /// fully free memory & Ui from logs and cached objects
        /// </summary>
        /// <returns></returns>
        public static async Task FullApplicationClean()
        {
            Array sectorTypes = Enum.GetValues(typeof(SectorType));
            List<string> sectors = LogsDictionary.Keys.ToList();
            try
            {
                //LogModule.LogsDictionary.Clear();
                for (int s = 0; s < sectors.Count; s++)
                {
                    for (int t = 0; t < sectorTypes.Length; t++)
                    {
                        //object sectorType = sectorTypes.GetValue(t);
                        LogsDictionary[sectors[s]][(SectorType)sectorTypes.GetValue(t)].Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "FullApplicationClean, LogModule");
            }
            finally
            {
                sectors = null;
                sectorTypes = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                await Task.Delay(2000);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        /// <summary>
        /// Clear a specific sector(project) logs and it child's from memory and ui
        /// </summary>
        /// <param name="sector"></param>
        /// <param name="sectorType"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static async Task ClearLogs(string sector, SectorType sectorType, bool force = false)
        {
            try
            {
                LogsDictionary[sector][sectorType].Clear();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (force)
                {
                    await Task.Delay(2000);
                    //LogsDictionary[sector][sectorType].Clear();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "ClearLogs, LogModule");
            }
        }

        public static void TryGetLogs(string sector, SectorType sectorType, out ObservableCollection<LogInfo> logs)
        {
            GenerateSector(sector, sectorType, out logs);
        }

        //public void Dispose()
        //{
        //    GC.SuppressFinalize(this);
        //}
    }
}
