using RabbitMQ.Client;
using System.Collections.Generic;
using UBS.FundManager.Common;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.Messaging.Models.ExchangeData;

namespace UBS.FundManager.Messaging
{
    public class MQConnectionHelper
    {
        /// <summary>
        /// Returns a connection factory instance (reads parameters from config)
        /// </summary>
        /// <returns></returns>
        public ConnectionFactory GetConnectionFactory()
        {
            ConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = ConfigManager.GetSetting<string>("AMQPHost"),
                Port = ConfigManager.GetSetting<int>("AMQPPort"),
                UserName = ConfigManager.GetSetting<string>("AMQPUsername"),
                Password = ConfigManager.GetSetting<string>("AMQPPassword"),
                VirtualHost = ConfigManager.GetSetting<string>("AMQPVirtualHost")
            };

            connectionFactory.RequestedHeartbeat = ushort.Parse(ConfigManager.GetSetting("AMQPHeartbeatInterval", "60"));

            return connectionFactory;
        }

        /// <summary>
        /// Default AMQP exchange and queues
        /// </summary>
        /// <returns></returns>
        public ExchangeInfo GetDefaultExchangeData()
        {
            return new ExchangeInfo
            {
                Name = Constants.DEFAULT_EXCHANGE,
                Type = ExchangeType.Topic,
                Behaviour = new ExchangeBehaviour(),
                Queues = new ExchangeQueue[]
                {
                    new ExchangeQueue
                    {
                        Name = Constants.ADDFUND_REQUEST_QUEUE,
                        Bindings = new string[] { Constants.ADDFUND_REQUEST_QUEUE_BINDING },
                        Behaviour = new QueueBehaviour
                        {
                            Args = new Dictionary<string, object>
                            {
                                { Constants.DEAD_LETTER_KEY, Constants.DEAD_LETTER_EXCHANGE }
                            }
                        },
                        Type = QueueType.Processing,
                        Action = TriggerAction.AddFund
                    },
                    new ExchangeQueue
                    {
                        Name = Constants.DOWNLOADFUND_REQUEST_QUEUE,
                        Bindings = new string[] { Constants.DOWNLOADFUND_REQUEST_QUEUE_BINDING },
                        Behaviour = new QueueBehaviour
                        {
                            Args = new Dictionary<string, object>
                            {
                                { Constants.DEAD_LETTER_KEY, Constants.DEAD_LETTER_EXCHANGE }
                            }
                        },
                        Type = QueueType.Processing,
                        Action = TriggerAction.DownloadFund
                    },
                    new ExchangeQueue
                    {
                        Name = Constants.ADDFUND_RESPONSE_QUEUE,
                        Bindings = new string[] { Constants.ADDFUND_RESPONSE_QUEUE_BINDING },
                        Behaviour = new QueueBehaviour
                        {
                            Args = new Dictionary<string, object>
                            {
                                { Constants.DEAD_LETTER_KEY, Constants.DEAD_LETTER_EXCHANGE }
                            }
                        },
                        Type = QueueType.Response,
                        Action = TriggerAction.AddFund
                    },
                    new ExchangeQueue
                    {
                        Name = Constants.DOWNLOAD_FUNDS_RESPONSE_QUEUE,
                        Bindings = new string[] { Constants.DOWNLOAD_FUNDS_RESPONSE_QUEUE_BINDING },
                        Behaviour = new QueueBehaviour
                        {
                            Args = new Dictionary<string, object>
                            {
                                { Constants.DEAD_LETTER_KEY, Constants.DEAD_LETTER_EXCHANGE }
                            }
                        },
                        Type = QueueType.Response,
                        Action = TriggerAction.DownloadFund
                    }
                }
            };
        }

        /// <summary>
        /// Deadletter exchange information
        /// </summary>
        /// <returns></returns>
        public ExchangeInfo GetDeadLetterExchangeInfo()
        {
            return new ExchangeInfo
            {
                Name = Constants.DEAD_LETTER_EXCHANGE,
                Type = ExchangeType.Fanout,
                Behaviour = new ExchangeBehaviour(),
                Queues = new ExchangeQueue[]
                {
                    new ExchangeQueue
                    {
                        Name = Constants.DEAD_LETTER_QUEUE,
                        Behaviour = new QueueBehaviour(),
                        Type = QueueType.DeadLetter,
                        Bindings = new string[] { "ubs.fundmanager.deadletter" }
                    }
                }
            };
        }
    }
}
