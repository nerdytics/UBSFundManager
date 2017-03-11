namespace UBS.FundManager.Messaging
{
    /// <summary>
    /// Response object from a components process()
    /// </summary>
    /// <typeparam name="T">Type of payload</typeparam>
    public class MsgListenerResponse<T>
    {
        public T Payload { get; set; }
        public string RoutingKey { get; set; }
        public string ExchangeName { get; set; }
    }
}
