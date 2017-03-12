using System;
using Topshelf;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.DataAccess;
using UBS.FundManager.DataAccess.Helpers;
using Microsoft.Practices.Unity;
using UBS.FundManager.Messaging;

namespace UBS.FundManager.Service
{
    /// <summary>
    /// Window Service entry point. To aid testing, the default ServiceBaseImpl of a window
    /// service has been encapsulated in the use of the 'TopShelf' library. This ensures i can 
    /// test this service as a console application rather than installing the service and subsequently 
    /// attaching a debugger to test (as would normally have been the case)
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            logger = BootstrapContainer.GetConfiguredContainer().Resolve<ILogging>();
            dbProvider = BootstrapContainer.GetConfiguredContainer().Resolve<IFundRepository>();

            connectionFactory = BootstrapContainer.GetConfiguredContainer().Resolve<MQConnectionHelper>();
            repositoryHelper = BootstrapContainer.GetConfiguredContainer().Resolve<BootstrapDB>();

            try
            {
                HostFactory.Run(serviceConfig =>
                {
                    serviceConfig.Service<ComponentHost>(config =>
                    {
                        logger.Log(LogLevel.Info, "Constructing Service");
                        config.ConstructUsing(service => new ComponentHost(dbProvider,
                                                                            logger, 
                                                                            connectionFactory, 
                                                                            repositoryHelper, 
                                                                            connectionFactory.GetDefaultExchangeData()));
                        config.WhenStarted((service) =>
                        {
                            logger.Log(LogLevel.Info, "Constructing start method");
                            service.Start();
                        });

                        config.WhenStopped(service => service.Stop());
                    });

                    serviceConfig.RunAsLocalSystem();
                    serviceConfig.StartAutomatically();

                    serviceConfig.SetServiceName("UBSFundManagerWindowService");
                    serviceConfig.SetDisplayName("UBS FundManager Worker Service");
                    serviceConfig.SetDescription("Background Service listening for MQ activities...");

                    serviceConfig.EnableServiceRecovery(r =>
                    {
                        r.RestartService(0);
                        r.OnCrashOnly();
                        r.SetResetPeriod(1);
                    });

                    serviceConfig.OnException(exc =>
                    {
                        logger.Log(exc, "Something blew up in service");
                    });
                });
            }
            catch (Exception exception)
            {
                logger.Log(exception, "Error occurred while executing hostfactory run.");
            }
        }

        #region Fields
        private static ILogging logger;
        private static IFundRepository dbProvider;

        private static MQConnectionHelper connectionFactory;
        private static BootstrapDB repositoryHelper;
        #endregion
    }
}
