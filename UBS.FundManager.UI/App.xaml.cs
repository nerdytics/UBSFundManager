using Microsoft.Practices.Unity;
using Prism.Mvvm;
using System;
using System.Reflection;
using System.Windows;

namespace UBS.FundManager.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IUnityContainer _container = new UnityContainer();

            base.OnStartup(e);

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                string viewName = viewType.FullName;
                string viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                string viewModelName = string.Format("{0}ViewModel, {1}", viewName, viewAssemblyName);

                return Type.GetType(viewModelName);
            });

            ViewModelLocationProvider.SetDefaultViewModelFactory((type) =>
            {
                return _container.Resolve(type);
            });

            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
