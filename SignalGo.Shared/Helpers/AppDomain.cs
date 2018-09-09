#if (NETSTANDARD || NETCOREAPP)
//using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SignalGo.Shared.Helpers
{
    public class AppDomain
    {
        public static AppDomain CurrentDomain { get; private set; }

        static AppDomain()
        {
            CurrentDomain = new AppDomain();
        }

        public Assembly[] GetAssemblies()
        {
            var assemblies = new List<Assembly>();
            return assemblies.ToArray();
            //var dependencies = DependencyContext.Default.RuntimeLibraries;
            //foreach (var library in dependencies)
            //{
            //    if (IsCandidateCompilationLibrary(library))
            //    {
            //        var assembly = Assembly.Load(new AssemblyName(library.Name));
            //        assemblies.Add(assembly);
            //    }
            //}
            //return assemblies.ToArray();
        }

        //private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary)
        //{
        //    if (compilationLibrary.Name == ("Specify"))
        //        return true;
        //    else
        //    {
        //        var e = compilationLibrary.Dependencies.GetEnumerator();
        //        // check first
        //        if (!e.MoveNext())
        //            return false;
        //        // do some stuff, then enumerate the list
        //        do
        //        {
        //            if (e.Current.Name.StartsWith("Specify"))
        //                return true;
        //        } while (e.MoveNext());
        //    }
        //    return false;
        //}
    }
}
#endif
