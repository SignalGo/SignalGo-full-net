using System;
using System.Collections.Generic;
using System.IO;

namespace SignalGo.Publisher.Shared.Helpers
{
    public static class DailyBackup
    {
        static DailyBackup()
        {
            backupPath = Path.Combine(Environment.CurrentDirectory, "AppBackups", "Logs");
        }
        private readonly static string backupPath;
        private static void EnsureAppBackupPathIsExist()
        {
            try
            {
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
            }
            catch { }
        }
        /// <summary>
        /// get backup from log file's and move thm into app backup folder, then mak a refresh applog file.
        /// </summary>
        /// <param name="logPath">the path of AppLogs.log file</param>
        public static void GetBackupFromAppLog(string logPath)
        {
            // check if application log files is for past, and then backup
            EnsureAppBackupPathIsExist();
            try
            {
                var fileDate = File.GetCreationTime(logPath);

                if (fileDate.Date < DateTime.Now.Date)
                {
                    // backup logs to a new file with it's date
                    string outFileName = $"{Path.GetFileNameWithoutExtension(logPath)}{fileDate: _MMddyyyy}.log";
                    File.Move(logPath, Path.Combine(backupPath, outFileName));
                    // make new app log file and refresh it's datetime
                    File.Create(logPath).Dispose();
                    File.SetCreationTime(logPath, DateTime.Now);
                }
            }
            catch { }
        }
        
        /// <summary>
        /// get backup from a list of log file's to app backup folder
        /// </summary>
        /// <param name="appLogs">list if log's path</param>
        public static void GetBackupFromAppLogs(List<string> appLogs)
        {
            EnsureAppBackupPathIsExist();
            Dictionary<string, DateTime> logsInfo = new Dictionary<string, DateTime>();
            try
            {
                for (int i = 0; i < appLogs.Count; i++)
                {
                    if (File.Exists(appLogs[i]))
                        logsInfo.Add(appLogs[i], File.GetCreationTime(appLogs[i]));
                }

                foreach (var item in logsInfo)
                {
                    if (item.Value.Date != DateTime.Now.Date)
                    {
                        // backup logs to a new file with it's date
                        string outFileName = $"{Path.GetFileNameWithoutExtension(item.Key)}{item.Value: _MMddyyyy}.log";
                        try
                        {
                            File.Move(item.Key, Path.Combine(backupPath, outFileName));
                            File.Create(item.Key).Dispose();
                            File.SetCreationTime(item.Key, DateTime.Now);
                        }
                        catch { }
                    }
                }
            }
            catch { }
            finally
            {
                logsInfo = null;
            }
        }


    }
}
