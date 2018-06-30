using SignalGo.ServerManager.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SignalGo.ServerManager
{
    public class Loader : MarshalByRefObject
    {
        public Assembly Load(byte[] bytes, AppDomain domain)
        {
            return domain.Load(bytes);
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<ServerInfoBase> Domains = new List<ServerInfoBase>();
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            var path = @"C:\Users\ASUS\source\repos\TestClassLoaderinAppDomain\TestClassLoaderinAppDomain\bin\Debug\";

            try
            {
                ServerInfoBase loader = new ServerInfoBase("test", System.IO.Path.Combine(path, "TestClassLoaderinAppDomain.dll"), path);
                Domains.Add(loader);
                GC.Collect();
                GC.WaitForFullGCComplete();
                GC.Collect();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                //AppDomain.Unload(newDomainName);
            }
        }

        private void btnUnLoad_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Domains)
            {
                item.Dispose();
            }
            Domains.Clear();
            GC.Collect();
            GC.WaitForFullGCComplete();
            GC.Collect();
        }
    }
}
