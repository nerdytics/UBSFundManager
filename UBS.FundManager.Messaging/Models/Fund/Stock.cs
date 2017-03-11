using Newtonsoft.Json;

namespace UBS.FundManager.Messaging.Models.Fund
{
    /// <summary>
    /// Describes the attributes of a stock
    /// </summary>
    public class Stock
    {
        [JsonProperty(PropertyName = "purchaseInfo")]
        public StockPurchaseInfo PurchaseInfo { get; set; }

        [JsonProperty(PropertyName = "valueInfo")]
        public StockValueInfo ValueInfo { get; set; } = new StockValueInfo();

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
