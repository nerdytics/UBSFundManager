namespace UBS.FundManager.Messaging.Models.ExchangeData
{
    /// <summary>
    /// Model for an AMQP exchange
    /// </summary>
    public class ExchangeInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ConsumerTag { get; set; }
        public ExchangeBehaviour Behaviour { get; set; }
        public ExchangeQueue[] Queues { get; set; }
    }
}
