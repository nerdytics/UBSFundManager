using System.Threading.Tasks;
using RabbitMQ.Client;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.DataAccess;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Models.ExchangeData;

namespace UBS.FundManager.Service.EventComponents
{
    /// <summary>
    /// Component listens for all 'AddFund' requests sent to the 'AddFunds' queue
    /// </summary>
    public class AddFundListeningComponent : BaseListeningComponent<Stock, BaseFundMessage>
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
        public AddFundListeningComponent(IConnectionFactory connFactory, 
                ILogging logger, 
                    TriggerAction triggerAction,
                        IFundRepository dbProvider, 
                            params ExchangeInfo[] srcExchanges) 
            : base(connFactory, logger, triggerAction, srcExchanges)
        {
            _dbProvider = dbProvider;
        }

        /// <summary>
        /// Basically handles incoming request, processes it and subsequently returns appropriate
        /// response based on the trigger action.
        /// </summary>
        /// <param name="input">Add funds request (which is just the stock info)</param>
        /// <returns></returns>
        internal async override Task<MsgListenerResponse<BaseFundMessage>> Process(Stock input)
        {
            try
            {
                Fund response = await _dbProvider.CreateItemAsync(new Fund { StockInfo = input });
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
        /// Let's avoid memory leaks
        /// </summary>
        /// <param name="disposing">flag determining if component is shutting down</param>
        protected override void Dispose(bool disposing)
        {
            _dbProvider?.CleanUp();
            base.Dispose(disposing);
        }
    }
}
