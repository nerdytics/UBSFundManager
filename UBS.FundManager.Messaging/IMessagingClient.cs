using RabbitMQ.Client;
using System;
using UBS.FundManager.Messaging.Models.ExchangeData;

namespace UBS.FundManager.Messaging
{
    /// <summary>
    /// Exposes operations that enable duplex communication with an AMQP server
    /// </summary>
    public interface IMessagingClient : IDisposable
    {
        #region Properties
        IBasicProperties CreateBasicProperties();
        #endregion

        #region Methods
        void Start();
        void Stop();
        void AddExchangeBindings(ExchangeInfo exchangeInfo);
        void Publish<T>(T payload, TriggerAction action);
        void Publish<T>(T payload, string exchange, string routingKey, IBasicProperties properties = null);
        #endregion
    }
}
