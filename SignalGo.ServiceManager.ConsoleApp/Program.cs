using System;
using System.IO;
using SignalGo.ServiceManager.Models;
using SignalGo.ServiceManager.BaseViewModels;
using SignalGo.ServiceManager.BaseViewModels.Core;
using SignalGo.ServiceManager.ConsoleApp.Helpers;

namespace SignalGo.ServiceManager.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Welcome to Service manager :)");
            Console.ResetColor();
            ServerProcessBaseInfo.Instance = () => new ServerProcessInfo();
            StartUp.Initialize();

            if (SettingInfo.Current.ServerInfo.Count == 0)
            {
            Start:
                Console.WriteLine("Please set your service name:");
                var serviceName = Console.ReadLine().TrimStart().TrimEnd();
                if (string.IsNullOrEmpty(serviceName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Your service name is empty!");
                    Console.ResetColor();
                    goto Start;
                }
            Path:
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Please set your service executable file path:");
                Console.ResetColor();
                var assemblyPath = Console.ReadLine();
                var dir = Path.GetDirectoryName(assemblyPath);
                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(ex);
                        Console.ResetColor();
                        goto Path;
                    }
                }

                AddNewServerBaseViewModel addNewServerBaseViewModel = new AddNewServerBaseViewModel();
                addNewServerBaseViewModel.Name = serviceName;
                addNewServerBaseViewModel.AssemblyPath = assemblyPath;
                addNewServerBaseViewModel.SaveCommand.Execute();
                StartUp.Load();
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Wait for publisher!");
            Console.ResetColor();
            Console.ReadLine();
        }
    }
}
