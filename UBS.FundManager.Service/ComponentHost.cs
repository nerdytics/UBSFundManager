using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.DataAccess;
using UBS.FundManager.DataAccess.Helpers;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Models.ExchangeData;
using UBS.FundManager.Service.EventComponents;

namespace UBS.FundManager.Service
{
    /// <summary>
    /// Hosts all the supported components in the service and subsequently starts them.
    /// </summary>
    public class ComponentHost : IListeningComponent
    {
        public ComponentHost(IFundRepository dbProvider, 
                ILogging logger,
                    MQConnectionHelper connFactory, 
                        BootstrapDB dbUtil,
                            params ExchangeInfo[] exchanges)
        {
            _logger = logger;
            _mqConnectionHelper = connFactory;
            SetupComponents(dbProvider, dbUtil, exchanges);
        }

        /// <summary>
        /// Starts all components (invoked when the window service is started)
        /// </summary>
        public void Start()
        {
            SetupExchangeBindings();

            for (int i = 0; i < _componentList.Count; i++)
            {
                _componentList[i].Start();
            }
        }

        /// <summary>
        /// Stops all components (invoked when the window service is stopped)
        /// </summary>
        public void Stop()
        {
            foreach (IListeningComponent component in _componentList)
            {
                component.Stop();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _logger?.CleanUp();
                    _mqConnectionHelper?.CleanUp();

                    Stop();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Correctly implement the disposable pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// -   Sets up the database and relevant collections if they don't already exist. As part of 
        ///     this operation, database functions such as stored procedures and user defined functions are also created,
        ///     (these are stored in the DbFunctions directory in the UBS.FundManager.DataAccess project).
        /// -   Sets up connection to relevant exchanges and queues. If these don't already exists 
        ///     (this should never happen, as when window service is started, 
        /// it creates these if they don't already exist), they get created.
        /// </summary>
        /// <param name="dbProvider">Data access provider (exposes CRUD operations)</param>
        /// <param name="dbUtil">Helper utility for performing db operations</param>
        /// <param name="exchanges">AMQP exchange(s) and queue(s)</param>
        private void SetupComponents(IFundRepository dbProvider, BootstrapDB dbUtil, ExchangeInfo[] exchanges)
        {
            try
            {
                _logger.Log(LogLevel.Info, $"In { this.GetType().Name}, setting up repository");
                dbUtil.SetupRepository();
                _logger.Log(LogLevel.Info, "Finished setting up repo - ");

                _componentList.Add(new AddFundListeningComponent(_mqConnectionHelper.GetConnectionFactory(), _logger, TriggerAction.AddFund, dbProvider, exchanges));
                _componentList.Add(new FundsDownloadListeningComponent(_mqConnectionHelper.GetConnectionFactory(), _logger, TriggerAction.DownloadFund, dbProvider, exchanges));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Sets up AMQP bindings (including dead letter exchange and queue)
        /// </summary>
        private void SetupExchangeBindings()
        {
            try
            {
                SetupDeadLetterExchange();

                ExchangeInfo defaultExchange = _mqConnectionHelper.GetDefaultExchangeData();

                _model.ExchangeDeclare(defaultExchange.Name, 
                                      defaultExchange.Type, 
                                      defaultExchange.Behaviour.Durable, 
                                      defaultExchange.Behaviour.AutoDelete);

                if (defaultExchange.Queues.Count() > 0)
                {
                    foreach (ExchangeQueue queue in defaultExchange.Queues)
                    {
                        _model.QueueDeclare(queue.Name,
                                                queue.Behaviour.Durable,
                                                queue.Behaviour.Exclusive,
                                                queue.Behaviour.AutoDelete,
                                                queue.Behaviour.Args);

                        foreach (string binding in queue.Bindings)
                        {
                            _model.QueueBind(queue.Name, defaultExchange.Name, binding, null);
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw exc;
            }
        }

        /// <summary>
        /// Sets up DLX and DLQ for receiving faulted messages
        /// </summary>
        private void SetupDeadLetterExchange()
        {
            try
            {
                _logger.Log(LogLevel.Info, "Obtaining connection in setupexchangebindings");
                _model = _mqConnectionHelper.GetConnectionFactory().CreateConnection().CreateModel();
                _deadLetterExchange = _mqConnectionHelper.GetDeadLetterExchangeInfo();

                _model.ExchangeDeclare(_deadLetterExchange.Name,
                                      _deadLetterExchange.Type,
                                      _deadLetterExchange.Behaviour.Durable,
                                      _deadLetterExchange.Behaviour.AutoDelete);

                if (_deadLetterExchange.Queues.Count() > 0)
                {
                    foreach (ExchangeQueue queue in _deadLetterExchange.Queues)
                    {
                        _model.QueueDeclare(queue.Name,
                                                queue.Behaviour.Durable,
                                                queue.Behaviour.Exclusive,
                                                queue.Behaviour.AutoDelete,
                                                queue.Behaviour.Args);

                        if(queue.Bindings == null || queue.Bindings.Count() == 0)
                        {
                            _model.QueueBind(queue.Name, _deadLetterExchange.Name, queue.Bindings.First(), null);
                        }
                        else
                        {
                            foreach (string binding in queue.Bindings)
                            {
                                _model.QueueBind(queue.Name, _deadLetterExchange.Name, binding, null);
                            }
                        }
                    }
                }
            }
            catch(Exception exc)
            {
                throw exc;
            }
        }

        #region Fields
        private IModel _model;
        private ILogging _logger;
        private MQConnectionHelper _mqConnectionHelper;
        private ExchangeInfo _deadLetterExchange;
        private List<IListeningComponent> _componentList = new List<IListeningComponent>();
        #endregion
    }
}
