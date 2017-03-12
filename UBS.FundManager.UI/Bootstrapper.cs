using Prism.Unity;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Logging;
using Prism.Modularity;
using Microsoft.Practices.Unity.Configuration;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.UI.FundModule;

namespace UBS.FundManager.UI
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override ILoggerFacade CreateLogger()
        {
            return new PrismLog4NetLogger();
        }

        protected override void ConfigureModuleCatalog()
        {
            ModuleCatalog catalog = (ModuleCatalog)ModuleCatalog;
            catalog.AddModule(typeof(FundModuleModule));
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.LoadConfiguration();
        }

        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<Shell.Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }
    }
}
