//using SignalGo.Server.DataTypes;
//using SignalGo.ServerManager.Models;
//using SignalGo.Shared.Log;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Security.Policy;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace SignalGo.ServerManager.Helpers
//{
//    public class ServerInfoBase : MarshalByRefObject, IDisposable
//    {
//        public string ServerName { get; set; }
//        public ConsoleWriter Start(string serverName, string fileName)
//        {
//            ServerName = serverName;
//            var appdomain = BuildChildDomain(AppDomain.CurrentDomain, fileName);
//            CurrentAppDomain = appdomain;
//            Type loaderType = typeof(AssemblyLoader);
//            var loader =
//                (AssemblyLoader)appdomain.CreateInstanceFrom(
//                    loaderType.Assembly.Location,
//                    loaderType.FullName).Unwrap();
//            CurrentAssemblyLoader = loader;
//            return loader.LoadAssembly(ServerName, fileName, Path.GetDirectoryName(fileName));
//        }

//        public AssemblyLoader CurrentAssemblyLoader { get; set; }
//        public AppDomain CurrentAppDomain { get; set; }

//        private AppDomain BuildChildDomain(AppDomain parentDomain, string fileName)
//        {

//            Evidence evidence = new Evidence(parentDomain.Evidence);
//            AppDomainSetup setup = parentDomain.SetupInformation;
//            //setup.PrivateBinPath = ServerPath;
//            //setup.PrivateBinPathProbe = ServerPath;
//            setup.ApplicationBase = Path.GetDirectoryName(fileName);
//            setup.ConfigurationFile = fileName + ".Config";
//            return AppDomain.CreateDomain(ServerName, evidence, setup);
//        }

//        public void Dispose()
//        {
//            try
//            {
//                AutoLogger.Default.LogText("disposing server");
//                if (CurrentAssemblyLoader != null)
//                    CurrentAssemblyLoader.Dispose();
//                AutoLogger.Default.LogText("server dispose success");
//                CurrentAssemblyLoader = null;
//            }
//            catch (System.Runtime.Remoting.RemotingException ex)
//            {
//                CurrentAssemblyLoader = null;
//                AutoLogger.Default.LogError(ex, "Dispose ServerInfoBase AssemblyLoader.");
//            }
//            catch (Exception ex)
//            {
//                AutoLogger.Default.LogError(ex, "Dispose ServerInfoBase AssemblyLoader 2.");
//            }

//            AppDomain.Unload(CurrentAppDomain);
//            GC.SuppressFinalize(CurrentAppDomain);

//            GC.SuppressFinalize(this);
//            GC.Collect();
//            GC.WaitForFullGCComplete();
//            GC.Collect();
//        }
//    }

//    public class AssemblyLoader : MarshalByRefObject, IDisposable
//    {
//        public static Action<string, string> ConsoleWriterAction { get; set; }
//        private Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();
//        MethodInfo disposeMethod = null;
//        object disposeMethodClass = null;
//        Assembly mainAssembly = null;
//        public string BaseDirectory { get; set; }
//        MethodInfo DisposeMethod { get; set; }
//        internal ConsoleWriter LoadAssembly(string serverName, string assemblyFileName, string path)
//        {
//            serverPath = path;
//            BaseDirectory = Path.GetDirectoryName(assemblyFileName);
//            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
//            AppDomain.CurrentDomain.UnhandledException += Appdomain_UnhandledException;

//            //var seti = AppDomain.CurrentDomain.SetupInformation;
//            mainAssembly = Assembly.Load(File.ReadAllBytes(assemblyFileName));
//            var type = mainAssembly.GetTypes().FirstOrDefault(x => x.GetCustomAttributes<MainStructureAttribute>().Count() > 0);
//            if (type == null)
//                throw new Exception("MainStructureAttribute on your types not found!");
//            var method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttributes<MainStructureAttribute>().Count(y => y.Mode == MainStructureEnum.MainMethod) > 0);
//            if (type == null)
//                throw new Exception("MainStructureAttribute on your method not found!");

//            method.Invoke(null, null);
//            DisposeMethod = type.GetMethods().FirstOrDefault(x => x.GetCustomAttributes<MainStructureAttribute>().Count(y => y.Mode == MainStructureEnum.DisposeMethod) > 0);
//            //var console = new ConsoleWriter() { ServerName = serverName };
//            //console.TextAddedAction = ConsoleWriterAction;
//            //Console.SetOut(console);
//            return null;
//            //bool isCall = false;
//            //bool haveDisposeItem = false;
//            //foreach (var type in mainAssembly.GetTypes())
//            //{
//            //    var attribs = type.GetCustomAttributes(true);
//            //    foreach (dynamic item in attribs)
//            //    {
//            //        if (!(item.TypeId is Type))
//            //            continue;
//            //        Type name = (Type)item.TypeId;
//            //        if (name.FullName == "SignalGo.Server.DataTypes.MainStructureAttribute")
//            //        {
//            //            var mode = item.Mode;
//            //            if ((int)mode == 0)
//            //            {
//            //                foreach (var method in type.GetMethods())
//            //                {
//            //                    var methodAttributes = method.GetCustomAttributes(true);
//            //                    var attribCount = (from dynamic x in methodAttributes where x.TypeId.FullName == "SignalGo.Server.DataTypes.MainStructureAttribute" && (int)x.Mode == 1 select x).Count();
//            //                    if (attribCount > 0)
//            //                    {
//            //                        method.Invoke(type, null);
//            //                        isCall = true;
//            //                    }
//            //                    attribCount = (from dynamic x in methodAttributes where x.TypeId.FullName == "SignalGo.Server.DataTypes.MainStructureAttribute" && (int)x.Mode == 2 select x).Count();
//            //                    if (attribCount > 0)
//            //                    {
//            //                        disposeMethod = method;
//            //                        disposeMethodClass = type;
//            //                        haveDisposeItem = true;
//            //                    }
//            //                }
//            //            }

//            //        }
//            //    }
//            //}
//            //if (!isCall)
//            //    throw new Exception("MainStructureAttribute Main Method not found");
//            //if (!haveDisposeItem)
//            //    throw new Exception("MainStructureAttribute Dispose Method not found");
//            //foreach (var item in (from x in LoadedAssemblies where Path.GetFileName(x.Key).StartsWith("SignalGo.Shared") select x).FirstOrDefault().Value.GetTypes())
//            //{
//            //    if (item.FullName == "SignalGo.Shared.Log.AutoLogger")
//            //    {
//            //        var property = item.GetProperty("ApplicationDirectory");
//            //        property.SetValue(item, path);
//            //    }
//            //}
//        }

//        private void Appdomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
//        {
//            AutoLogger.Default.LogError((Exception)e.ExceptionObject, " new Appdomain_UnhandledException");
//        }

//        public void StopServer(Action stopAction)
//        {
//            //bool finded = false;
//            //foreach (var type in mainAssembly.GetTypes())
//            //{
//            //    var attribs = type.GetCustomAttributes(true);
//            //    foreach (dynamic item in attribs)
//            //    {
//            //        if (!(item.TypeId is Type))
//            //            continue;
//            //        Type name = (Type)item.TypeId;
//            //        if (name.FullName == "SignalGo.Server.DataTypes.MainStructureAttribute")
//            //        {
//            //            var mode = item.Mode;
//            //            if ((int)mode == 0)
//            //            {
//            //                foreach (var property in type.GetProperties())
//            //                {
//            //                    var methodAttributes = property.GetCustomAttributes(true);
//            //                    var attribCount = (from dynamic x in methodAttributes where x.TypeId.FullName == "SignalGo.Server.DataTypes.MainStructureAttribute" && (int)x.Mode == 3 select x).Count();
//            //                    if (attribCount > 0)
//            //                    {
//            //                        dynamic value = property.GetValue(type);
//            //                        value.StopMethodsCallsForFinishServer(stopAction);
//            //                        finded = true;
//            //                    }
//            //                }
//            //            }

//            //        }
//            //    }
//            //}
//            //if (!finded)
//            //    throw new Exception("SignalGo.Server.DataTypes.MainStructureAttribute Mode 'ServerObject' from server property not found");
//        }

//        public bool ExistAssemblyFile(string path)
//        {
//            //foreach (var item in LoadedAssemblies.ToList())
//            //{
//            //    var path2 = Path.GetFullPath(item.Key);
//            //    if (string.Equals(path, path2, StringComparison.OrdinalIgnoreCase))
//            //    {
//            //        return true;
//            //    }
//            //}
//            return false;
//        }

//        internal void UnloadAppdomain()
//        {
//            //disposeMethod.Invoke(disposeMethodClass, null);
//            //var ok = AppDomain.CurrentDomain.IsFinalizingForUnload();
//        }

//        string serverPath = "";
//        string currentLocationLoding = null;
//        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
//        {
//            MessageBox.Show("okok");
//            var fileName = System.IO.Path.Combine(serverPath, new AssemblyName(args.Name).Name + ".dll");
//            if (currentLocationLoding == fileName)
//                throw new Exception($"Assembly duplicate load {args.Name}");
//            currentLocationLoding = fileName;
//            if (LoadedAssemblies.ContainsKey(fileName))
//            {
//                currentLocationLoding = null;
//                return LoadedAssemblies[fileName];
//            }
//            Assembly asm = null;
//            if (File.Exists(fileName))
//            {
//                //string newFile = Path.Combine(BaseDirectory, Path.GetFileName(fileName));
//                //File.Copy(fileName, newFile, true);
//                //string pdbbaseFileName = Path.Combine(BaseDirectory, Path.GetFileNameWithoutExtension(fileName) + ".pdb");
//                //string pdbfileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + ".pdb");
//                //if (File.Exists(pdbfileName))
//                //    File.Copy(pdbfileName, pdbbaseFileName, true);

//                asm = Assembly.Load(File.ReadAllBytes(fileName));
//                LoadedAssemblies.Add(fileName, asm);
//            }

//            if (asm == null)
//            {
//                asm = Assembly.Load(args.Name);
//                if (asm == null)
//                    throw new Exception($"Assembly not found {args.Name}");
//            }
//            currentLocationLoding = null;
//            return asm;
//        }

//        public void Dispose()
//        {
//            if (DisposeMethod != null)
//                DisposeMethod.Invoke(null, null);
//            LoadedAssemblies.Clear();
//            GC.SuppressFinalize(this);
//        }
//    }

//}
