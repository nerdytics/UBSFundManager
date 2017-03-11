using System.Collections.Generic;

namespace UBS.FundManager.Messaging.Models.ExchangeData
{
    /// <summary>
    /// Configuration options for an exchange's queue
    /// </summary>
    public class QueueBehaviour : ExchangeBehaviour
    {
        public bool Exclusive { get; set; } = false;
        public IDictionary<string, object> Args { get; set; } = new Dictionary<string, object>();
    }
}
