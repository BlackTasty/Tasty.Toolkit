using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Tasty.Patcher.Core.Configuration;

namespace Tasty.Patcher
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private static ProfileManager profileManager;

        public static ProfileManager ProfileManager => profileManager;

        [STAThread]
        public static void Main()
        {
            App app = new App()
            {
                ShutdownMode = ShutdownMode.OnMainWindowClose
            };
            app.InitializeComponent();
            app.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            profileManager = new ProfileManager();
        }
    }
}
