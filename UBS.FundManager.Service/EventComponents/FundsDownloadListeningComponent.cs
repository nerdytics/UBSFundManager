using RabbitMQ.Client;
using System.Collections.Generic;
using System.Threading.Tasks;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.DataAccess;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Models.ExchangeData;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.Service.EventComponents
{
    /// <summary>
    /// Component listens for all 'DownloadFunds' requests sent to the 'DownloadFunds' queue
    /// </summary>
    public class FundsDownloadListeningComponent : BaseListeningComponent<DownloadFundsRequest, BaseFundMessage>
    {
        private IFundRepository _dbProvider;

        /// <summary>
        /// Default injection constructor (always invoked by IoC container)
        /// </summary>
        /// <param name="connFactory">AMQP Connection factory object</param>
        /// <param name="logger">Logging object</param>
        /// <param name="triggerAction">Action that triggered the request 
        ///         (helps component determine how to route response data)
        /// </param>
        /// <param name="dbProvider">Data access provider (exposes all CRUD db operations)</param>
        /// <param name="srcExchanges">Model for the AMQP exchange(s) and queue(s).</param>
        public FundsDownloadListeningComponent(IConnectionFactory connFactory,
                ILogging logger, 
                    TriggerAction triggerAction,
                        IFundRepository dbProvider, 
                            params ExchangeInfo[] srcExchanges) 
            : base(connFactory, logger, triggerAction, srcExchanges)
        {
            _dbProvider = dbProvider;
        }

        /// <summary>
        /// Basically handles incoming download funds request, processes it and subsequently returns appropriate
        /// response based on the trigger action.
        /// </summary>
        /// <param name="input">Download fund request object</param>
        /// <returns></returns>
        internal override async Task<MsgListenerResponse<BaseFundMessage>> Process(DownloadFundsRequest input)
        {
            try
            {
                IEnumerable<Fund> response = await _dbProvider.GetAll(input.DatasetSize);
                return ComposeResponse(new BaseFundMessage
                {
                    Payload = response,
                    Action = _triggerAction
                });
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Let's clean up to avoid memory leaks
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _dbProvider?.CleanUp();
            base.Dispose(disposing);
        }
    }
}
