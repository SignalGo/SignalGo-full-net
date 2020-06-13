using System;
using System.Collections.Generic;
using System.IO;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.Shared.Log;

namespace SignalGo.ServiceManager.Core.Helpers
{
    public class Utils
    {

        /// <summary>
        /// using to search dotnet core binary(executable) in host system
        /// </summary>
        public static string FindDotNetPath()
        {
            string result = string.Empty;
            List<string> recommendedPaths = null;
            //if (!string.IsNullOrEmpty(UserSettingInfo.Current.UserSettings.DotNetPath))
            //    return result = UserSettingInfo.Current.UserSettings.DotNetPath;
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    recommendedPaths = new List<string>
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "dotnet.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "dotnet", "dotnet.exe"),
                        Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).Name, "dotnet", "dotnet.exe")
                    };
                    foreach (var dir in recommendedPaths)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("current dotnet(NT) search path: " + dir);
                        Console.ResetColor();
                        if (File.Exists(dir))
                        {
                            result = dir;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("dotnet found! in " + dir);
                            Console.ResetColor();
                            break;
                        }
                        else
                            Console.WriteLine("going to next search...");
                    }
                    return result;
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    recommendedPaths = new List<string>
                    {
                        Path.Combine("/", "home", "dotnet","dotnet"),
                        Path.Combine("/", "opt", "dotnet","dotnet"),
                        Path.Combine("/", "usr", "share","dotnet","dotnet"),
                        Path.Combine("/", "media","dotnet","dotnet"),
                        Path.Combine("/", "usr","local","share","dotnet","dotnet")
                    };
                    foreach (var dir in recommendedPaths)
                    {
                        Console.WriteLine("current dotnet(Unix) search path: " + dir);
                        if (File.Exists(dir))
                        {
                            result = dir;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("dotnet found! in " + dir);
                            Console.ResetColor();
                            break;
                        }
                        else
                            Console.WriteLine("going to next search...");
                    }
                    return result;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("result: " + result);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                result = "/opt/dotnet/dotnet";
                AutoLogger.Default.LogError(ex, "Find Dotnet binary path");
            }
            finally
            {
                recommendedPaths.Clear();
                result = string.IsNullOrEmpty(result) ? "/opt/dotnet/dotnet" : result;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Dotnet Path not found! but we set it to default. you can change it in user setting config file");
            Console.ResetColor();
            return result;
        }

        public static string FindBackupPath()
        {
            string backupPathByOS = string.Empty;
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    backupPathByOS = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).FullName;
                    if (!Directory.Exists(Path.Combine(backupPathByOS, "SericeManagerBackups")))
                    {
                        backupPathByOS = Directory.CreateDirectory(Path.Combine(backupPathByOS, "ServiceManagerBackups")).Exists ? Path.Combine(backupPathByOS, "ServiceManagerBackups") : "";

                        ;
                    }
                }
                else if (Environment.OSVersion.Platform == PlatformID.Unix)
                    backupPathByOS = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("couldn't set backup path. please specify it manually, on UserData.Json File. we set it to os temp directory at now.");
                    Console.ResetColor();
                    backupPathByOS = Path.GetTempPath();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
            }
            return backupPathByOS;
        }
    }
}
