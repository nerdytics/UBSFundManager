using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Threading.Tasks;
using UBS.FundManager.Common;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Models.ExchangeData;

namespace UBS.FundManager.Service.EventComponents
{
    public abstract class BaseListeningComponent<K, V> : IListeningComponent
    {
        /// <summary>
        /// Default constructor (invoked and constructed by IoC container when instantiating child object)
        /// </summary>
        /// <param name="connFactory">AMQP Connection factory object</param>
        /// <param name="logger">Logging object</param>
        /// <param name="triggerAction">Action that triggered the request 
        ///         (helps component determine how to route response data)
        /// </param>
        /// <param name="srcExchanges">Model for the AMQP exchange(s) and queue(s).</param>
        public BaseListeningComponent(IConnectionFactory connFactory, ILogging logger,  TriggerAction triggerAction, params ExchangeInfo[] srcExchanges)
        {
            _connectionFactory = connFactory;
            _logger = logger;
            _triggerAction = triggerAction;
            _srcExchanges = srcExchanges;
        }

        /// <summary>
        /// Starts the components (starts listening for messages)
        /// </summary>
        public void Start()
        {
            try
            {
                _logger.Log(LogLevel.Info, "In base message listening comp");
                _connection = _connectionFactory.CreateConnection();
                _model = _connection.CreateModel();
                _model.BasicQos(0, 1, false);

                _incomingMsgListener = new EventingBasicConsumer(_model);
                _incomingMsgListener.Received += OnIncomingMessage_Received;

                SetupExchangeBindings(_srcExchanges, _triggerAction);
            }
            catch (Exception exc)
            {
                _logger.Log(exc, $"Error while starting '{ this.GetType().Name }' service");
            }
        }

        /// <summary>
        /// Stop the component (stops listening for messages)
        /// </summary>
        public void Stop()
        {
            FreeUpResources();
        }

        /// <summary>
        /// Basically handles incoming request, processes it and subsequently returns appropriate
        /// response based on the trigger action. Implementation will be provided by inheriting objects
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Response data (i.e collection of funds)</returns>
        internal abstract Task<MsgListenerResponse<V>> Process(K input);

        /// <summary>
        /// Builds the response graph that will be returned to the client
        /// </summary>
        /// <param name="payload">Response data (i.e fund object)</param>
        /// <returns>Complete response graph</returns>
        protected MsgListenerResponse<V> ComposeResponse(V payload)
        {
            try
            {
                string routingKey = string.Empty;
                ExchangeInfo defaultExchange = _srcExchanges.First();

                switch (_triggerAction)
                {
                    case TriggerAction.AddFund:
                        routingKey = defaultExchange.Queues
                                                    .First(q => q.Action == TriggerAction.AddFund && q.Type == QueueType.Response)
                                                    .Bindings
                                                    .First();
                        break;

                    case TriggerAction.DownloadFund:
                        routingKey = defaultExchange.Queues
                                                    .First(q => q.Action == TriggerAction.DownloadFund && q.Type == QueueType.Response)
                                                    .Bindings
                                                    .First();
                        break;

                    default:
                        throw new NotSupportedException($"{ GetType().Name }: Cannot determine the requested client action");
                }

                return new MsgListenerResponse<V>
                {
                    ExchangeName = defaultExchange.Name,
                    RoutingKey = routingKey,
                    Payload = payload
                };
            }
            catch
            {
                throw;
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
                    FreeUpResources();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        /// <summary>
        /// Listener handles all incoming messages, performs some validation checks then subsequently
        /// invokes the process() to 'process' and return a complete response graph that will be sent
        /// to interested clients via an AMQP host
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIncomingMessage_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                K receivedMsg = e.Body.DecodeTransferedObject<K>();
                string correlationId = e.BasicProperties.IsCorrelationIdPresent()
                                            ? e.BasicProperties.CorrelationId : string.Empty;

                if(string.IsNullOrWhiteSpace(correlationId))
                {
                    throw new ArgumentException("Cannot process message, correlationId is missing");
                }

                MsgListenerResponse<V> response = Process(receivedMsg).Result;

                if (response != null)
                {
                    IBasicProperties responseProperties = _model.CreateBasicProperties();
                    if (e.BasicProperties.IsCorrelationIdPresent())
                    {
                        responseProperties.CorrelationId = correlationId;
                        responseProperties.ContentType = Constants.JSON_MEDIA_TYPE;
                    }

                    if (!string.IsNullOrWhiteSpace(response.RoutingKey))
                    {
                        Publish(response.Payload, response.ExchangeName, response.RoutingKey, responseProperties);
                    }

                    if (e.BasicProperties.IsReplyToPresent())
                    {
                        Publish(response.Payload, response.ExchangeName, e.BasicProperties.ReplyTo);
                    }

                    _model.BasicAck(e.DeliveryTag, false);
                }
            }
            catch (Exception exc)
            {
                _logger.Log(exc, $"{ GetType().Name.ToUpperInvariant() }: Something went wrong while processing an incoming message");
                _model.BasicNack(e.DeliveryTag, false, !e.Redelivered);
            }
        }

        /// <summary>
        /// Sets up connection to relevant exchanges and queues. If these don't already exists (this should never happen, as when window service is started, 
        /// it creates these if they don't already exist), they get created and subsequently starts listening on..
        /// </summary>
        /// <param name="_sourceExchanges">AMQP exchanges and queues metadata</param>
        /// <param name="action">Trigger action (i.e add fund or download funds list)</param>
        private void SetupExchangeBindings(ExchangeInfo[] _sourceExchanges, TriggerAction action)
        {
            foreach (ExchangeInfo exchange in _sourceExchanges)
            {
                _model.ExchangeDeclare(exchange.Name, exchange.Type, exchange.Behaviour.Durable, exchange.Behaviour.AutoDelete);
                if (exchange.Queues.Count() > 0)
                {
                    foreach (ExchangeQueue queue in exchange.Queues)
                    {
                        _model.QueueDeclare(queue.Name,
                                                queue.Behaviour.Durable,
                                                queue.Behaviour.Exclusive,
                                                queue.Behaviour.AutoDelete,
                                                queue.Behaviour.Args);

                        foreach (string binding in queue.Bindings)
                        {
                            _model.QueueBind(queue.Name, exchange.Name, binding, null);

                            if (queue.Type == QueueType.Processing && queue.Action == action)
                            {
                                exchange.ConsumerTag = _model.BasicConsume(queue.Name, false, _incomingMsgListener);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Pushes messages to the queue
        /// </summary>
        /// <typeparam name="T">Datatype of message to be published</typeparam>
        /// <param name="payload">Message to be published</param>
        /// <param name="exchange">Exchange to publish to</param>
        /// <param name="routingKey">Binding to relevant queue</param>
        /// <param name="properties">Message properties (i.e correlationId)</param>
        private void Publish<T>(T payload, string exchange, string routingKey, IBasicProperties properties = null)
        {
            _model.BasicPublish(exchange, routingKey, properties, payload.EncodeForTransfer());
        }

        /// <summary>
        /// Perform cleanup to avoid memory leaks
        /// </summary>
        private void FreeUpResources()
        {
            try
            {
                if (_connection.IsOpen || _connection != null)
                {
                    foreach (ExchangeInfo exchange in _srcExchanges)
                    {
                        if (!string.IsNullOrWhiteSpace(exchange.ConsumerTag))
                        {
                            _model.BasicCancel(exchange.ConsumerTag);
                        }
                    }

                    _connection?.Close();
                    _connection?.Dispose();
                }
            }
            catch (Exception exc)
            {
                _logger.Log(exc, $"Error while stopping '{ this.GetType().Name }' service");
            }
        }

        #region Fields
        protected ILogging _logger;
        protected TriggerAction _triggerAction;
        private IConnection _connection;
        private IConnectionFactory _connectionFactory;

        private ExchangeInfo[] _srcExchanges;
        protected IModel _model;
        protected EventingBasicConsumer _incomingMsgListener;
        #endregion
    }
}
