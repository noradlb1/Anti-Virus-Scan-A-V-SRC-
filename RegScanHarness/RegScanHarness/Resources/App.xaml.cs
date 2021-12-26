using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ScanX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            //StartupUri = new Uri("Window1.xaml", UriKind.Relative);
            StartupUri = new Uri("wndMain.xaml", UriKind.Relative);
            base.OnStartup(e);
        }
    }
}
