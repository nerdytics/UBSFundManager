using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using Prism.Logging;
using Prism.Modularity;
using Prism.Regions;
using UBS.FundManager.UI.FundModule.UserControls;

namespace UBS.FundManager.UI.FundModule
{
    /// <summary>
    /// Entry point for the Fund module
    /// </summary>
    public class FundModuleModule : IModule
    {
        ILoggerFacade _logger;
        IRegionManager _regionManager;
        IUnityContainer _resolver;

        public FundModuleModule(ILoggerFacade logger, IRegionManager regionManager, IUnityContainer resolver)
        {
            _logger = logger;
            _regionManager = regionManager;
            _resolver = resolver;
        }

        /// <summary>
        /// Boostraps the module to the application domain
        /// </summary>
        public void Initialize()
        {
            _resolver.LoadConfiguration();

            _logger.Log($"Initialising { GetType().Name }", Category.Info, Priority.None);

            if (_regionManager != null)
            {
                _regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(HomeScreen));
                _regionManager.RegisterViewWithRegion(RegionNames.AddFundRegion, typeof(AddFund));
                _regionManager.RegisterViewWithRegion(RegionNames.FundListRegion, typeof(FundList));
                _regionManager.RegisterViewWithRegion(RegionNames.FundSummaryRegion, typeof(FundSummary));
            }
        }
    }
}