using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;

namespace UBS.FundManager.Service
{
    /// <summary>
    /// Loads the container in this service, not really a necessity in this service
    /// but still saves having to manually construct object instances
    /// </summary>
    public class BootstrapContainer
    {
        /// <summary>
        /// Lazy loads container (and also configures container with mappings from unity configuration file)
        /// </summary>
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            IUnityContainer container = new UnityContainer().LoadConfiguration();
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
    }
}
