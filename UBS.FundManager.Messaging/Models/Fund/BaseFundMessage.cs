using UBS.FundManager.Messaging.Models.ExchangeData;

namespace UBS.FundManager.Messaging.Models.Fund
{
    /// <summary>
    /// Base model for an outgoing response message from the backend service
    /// </summary>
    public class BaseFundMessage
    {
        public TriggerAction Action { get; set; }
        public dynamic Payload { get; set; }
        public string ContinuationToken { get; set; }
    }
}
