namespace UBS.FundManager.Messaging.Models.ExchangeData
{
    /// <summary>
    /// Configuration options for an AMQP exchange
    /// </summary>
    public class ExchangeBehaviour
    {
        public bool Durable { get; set; } = true;
        public bool AutoDelete { get; set; } = false;
    }
}
