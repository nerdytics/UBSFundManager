using System;

namespace UBS.FundManager.Messaging.Models.ExchangeData
{
    /// <summary>
    /// Model for an AMQP exchange's queue
    /// </summary>
    public class ExchangeQueue
    {
        public string Name { get; set; }
        public string[] Bindings { get; set; }
        public QueueType Type { get; set; } = QueueType.Processing;
        public Type ListeningComponent { get; set; }
        public QueueBehaviour Behaviour { get; set; }
        public TriggerAction Action { get; set; }
    }

    /// <summary>
    /// Supported types of queues in the backend service
    /// </summary>
    public enum QueueType { Processing, Response, DeadLetter }

    /// <summary>
    /// Supported trigger operations in the appliction
    /// </summary>
    public enum TriggerAction { AddFund, DownloadFund }
}
