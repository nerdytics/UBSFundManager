using Prism.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.Messaging.Events;
using UBS.FundManager.Messaging.Models.ExchangeData;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.Messaging
{
    /// <summary>
    /// Exposes an implementation that enables duplex communication with an AMQP server.
    /// The core details of these is abstracted from the consuming client. However, it also
    /// exposes an overload for controlling how and where messages are routed.
    /// </summary>
    public class MessagingClient : IMessagingClient
    {
        /// <summary>
        /// Default Injection Constructor
        /// </summary>
        /// <param name="eventAggregator">PubSub Event Provider</param>
        /// <param name="connUtil">AMQP Connection Helper</param>
        public MessagingClient(IEventAggregator eventAggregator, MQConnectionHelper connUtil)
        {
            _eventAggregator = eventAggregator;
            _connectionUtil = connUtil;
            _connectionFactory = _connectionUtil.GetConnectionFactory();

            SetupConnection();
            SetupExchangeBindings(connUtil.GetDefaultExchangeData());
        }

        /// <summary>
        /// Simple message publishing to an AMQP endpoint (with default configuration options).
        /// </summary>
        /// <typeparam name="T">Type of nessage to publish</typeparam>
        /// <param name="payload">Message to publish</param>
        /// <param name="action">Describes action preceding message (i.e AddFund)</param>
        public void Publish<T>(T payload, TriggerAction action)
        {
            ExchangeInfo defaultExchange = _connectionUtil.GetDefaultExchangeData();

            IBasicProperties msgProperties = CreateBasicProperties();
            msgProperties.CorrelationId = _correlationId;

            ExchangeQueue processingQueue;
            switch (action)
            {
                case TriggerAction.AddFund:
                    processingQueue = defaultExchange.Queues
                                                     .First(q => q.Action == TriggerAction.AddFund && q.Type == QueueType.Processing);
                    Publish(payload, defaultExchange.Name, processingQueue.Bindings.First(), msgProperties);
                    break;

                case TriggerAction.DownloadFund:
                    processingQueue = defaultExchange.Queues
                                                     .First(q => q.Action == TriggerAction.DownloadFund && q.Type == QueueType.Processing);
                    Publish(payload, defaultExchange.Name, processingQueue.Bindings.First(), msgProperties);
                    break;

                default:
                    throw new NotSupportedException($"{ GetType().Name }: Not a supported action.");
            }
        }

        /// <summary>
        /// Publish message to an AMQP endpoint (requires provision of exchange and routing information)
        /// </summary>
        /// <typeparam name="T">Type of nessage to publish</typeparam>
        /// <param name="payload">Message to publish</param>
        /// <param name="exchange">Exchange to route message to</param>
        /// <param name="routingKey">Binding key to destination queue</param>
        /// <param name="properties">Message properties (i.e correlationId)</param>
        public void Publish<T>(T payload, string exchange, string routingKey, IBasicProperties properties)
        {
            _model.BasicPublish(exchange, routingKey, properties, payload.EncodeForTransfer());
        }

        /// <summary>
        /// Activates the messaging client (and also starts listening to designated response queues)
        /// </summary>
        public void Start()
        {
            foreach (ExchangeInfo exchangeInfo in _exchanges)
            {
                StartConsuming(exchangeInfo);
            }
        }

        /// <summary>
        /// Stops listening on designated response queues)
        /// </summary>
        public void Stop()
        {
            if (_connection != null)
            {
                foreach (ExchangeInfo exchangeInfo in _exchanges)
                {
                    if (!string.IsNullOrWhiteSpace(exchangeInfo.ConsumerTag))
                    {
                        _model.BasicCancel(exchangeInfo.ConsumerTag);
                    }
                }

                _connection?.Close();
                _connection?.Dispose();
            }
        }

        /// <summary>
        /// Exposes method for instructing the messaging client to listen for additional endpoints
        /// </summary>
        /// <param name="destination"></param>
        public void AddExchangeBindings(ExchangeInfo destination)
        {
            SetupExchangeBindings(destination);
            StartConsuming(destination);
        }

        /// <summary>
        /// Default message properties
        /// </summary>
        /// <returns></returns>
        public IBasicProperties CreateBasicProperties()
        {
            return _model.CreateBasicProperties();
        }

        /// <summary>
        /// Avoid memory leaks, execute cleanup
        /// </summary>
        public void Dispose()
        {
            if (_connection != null || _connection.IsOpen)
            {
                try
                {
                    _connection?.Dispose();
                }
                finally
                {
                    _exchanges = null;
                }
            }
        }

        #region Util methods
        /// <summary>
        /// Sets up connection to AMQP server
        /// </summary>
        private void SetupConnection()
        {
            _connection = _connectionFactory.CreateConnection();
            _model = _connection.CreateModel();
            _model.BasicReturn += OnModel_BasicReturn;
            _model.BasicQos(0, 1, false);

            _messageConsumer = new EventingBasicConsumer(_model);
            _messageConsumer.Received += OnMessageConsumer_Received;
        }

        /// <summary>
        /// Sets up binding to relevant exchange(s) and queue(s)
        /// </summary>
        /// <param name="exchangeInfos"></param>
        private void SetupExchangeBindings(params ExchangeInfo[] exchangeInfos)
        {
            foreach (ExchangeInfo exchangeInfo in exchangeInfos)
            {
                _model.ExchangeDeclare(exchangeInfo.Name, exchangeInfo.Type,
                                            exchangeInfo.Behaviour.Durable, exchangeInfo.Behaviour.AutoDelete);

                _exchanges.Add(exchangeInfo);
            }
        }

        /// <summary>
        /// Instructs messaging client to start listening on designated endpoints (queues)
        /// </summary>
        /// <param name="exchangeInfo"></param>
        public void StartConsuming(ExchangeInfo exchangeInfo)
        {
            if (exchangeInfo.Queues != null)
            {
                foreach (ExchangeQueue queueInfo in exchangeInfo.Queues)
                {
                    _model.QueueDeclare(queueInfo.Name, queueInfo.Behaviour.Durable,
                                    queueInfo.Behaviour.Exclusive, queueInfo.Behaviour.AutoDelete, queueInfo.Behaviour.Args);

                    string[] bindings = queueInfo.Bindings;
                    if (bindings != null && bindings.Length > 0)
                    {
                        foreach (string queueBinding in bindings)
                        {
                            if (queueInfo.Type == QueueType.Response)
                            {
                                _model.QueueBind(queueInfo.Name, exchangeInfo.Name, queueBinding);
                                exchangeInfo.ConsumerTag = _model.BasicConsume(queueInfo.Name, false, _messageConsumer);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Internal message listeners
        /// <summary>
        /// Messagingclient's internal handler for receiving messages. Broadcasts notifications
        /// based on the type of message received.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMessageConsumer_Received(object sender, BasicDeliverEventArgs e)
        {
            BaseFundMessage receivedMsg = e.Body.DecodeTransferedObject<BaseFundMessage>();         

            switch (receivedMsg.Action)
            {
                case TriggerAction.AddFund:
                    _model.BasicAck(e.DeliveryTag, false);
                    Fund fund = receivedMsg.Payload.ToObject<Fund>(); ;
                    _eventAggregator.GetEvent<NewFundAddedEvent>().Publish(fund);
                    break;

                case TriggerAction.DownloadFund:
                    _model.BasicAck(e.DeliveryTag, false);
                    IEnumerable<Fund> fundsList = receivedMsg.Payload.ToObject<IEnumerable<Fund>>();
                    _eventAggregator.GetEvent<DownloadedFundsListEvent>().Publish(fundsList);
                    break;
            }
        }

        /// <summary>
        /// Handler for unroutable messages (i.e when connection is down)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModel_BasicReturn(object sender, BasicReturnEventArgs e)
        {
            dynamic undeliveredMsg = e.Body.DecodeTransferedObject<dynamic>();
            _eventAggregator.GetEvent<UndeliveredMessageEvent>().Publish(undeliveredMsg);
        }
        #endregion

        #region Fields
        private IModel _model;
        private IConnection _connection;
        private IEventAggregator _eventAggregator;

        private ConnectionFactory _connectionFactory;
        private MQConnectionHelper _connectionUtil;
        private EventingBasicConsumer _messageConsumer;

        private List<ExchangeInfo> _exchanges = new List<ExchangeInfo>();
        private readonly string _correlationId = Guid.NewGuid().ToString();
        #endregion
    }
}
