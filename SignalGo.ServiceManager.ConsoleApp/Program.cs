using System;
using System.IO;
using SignalGo.ServiceManager.Core.Models;
using SignalGo.ServiceManager.Core.BaseViewModels;
using SignalGo.ServiceManager.BaseViewModels.Core;
using SignalGo.ServiceManager.ConsoleApp.Helpers;
using System.Linq;

namespace SignalGo.ServiceManager.ConsoleApp
{
    class Program
    {
        static ConsoleKeyInfo keyinfo;
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Welcome to Service manager :)");
            Console.ResetColor();
            ServerProcessBaseInfo.Instance = () => new ServerProcessInfo();
            StartUp.Initialize();

            if (SettingInfo.Current.ServerInfo.Count == 0)
                Add();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Wait for publisher!");
            Console.ResetColor();
        UserHandler:
            do
            {
                keyinfo = Console.ReadKey(true);
                UserActionHandler();
            }
            while (keyinfo.Key != ConsoleKey.Escape);
            goto UserHandler;
        }


        public static void Add()
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
        public static bool Remove(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Can't remove specified service!");
                Console.ResetColor();
                return false;
            }
            ServerInfoBaseViewModel.Delete(serviceName);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("The Service Successfully Removed!");
            Console.ResetColor();
            return true;
        }
        public static void UserActionHandler()
        {
            Console.WriteLine("user menu:");
            Console.WriteLine("(1) Add a new service");
            Console.WriteLine("(3) Remove an service");
            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    Add();
                    break;
                case "3":
                    Console.WriteLine("enter service name:");
                    Remove(Console.ReadLine().Trim());
                    break;
                default:
                    break;
            }
        }


    }
}
