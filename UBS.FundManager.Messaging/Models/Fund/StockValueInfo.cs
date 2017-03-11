using Newtonsoft.Json;

namespace UBS.FundManager.Messaging.Models.Fund
{
    /// <summary>
    /// Describes the attributes of a stock's valuation
    /// </summary>
    public class StockValueInfo
    {
        [JsonProperty(PropertyName = "marketValue")]
        public decimal MarketValue { get; set; }

        [JsonProperty(PropertyName = "transactionCost")]
        public decimal TransactionCost { get; set; }

        [JsonProperty(PropertyName = "stockWeight")]
        public decimal StockWeight { get; set; }
    }
}
