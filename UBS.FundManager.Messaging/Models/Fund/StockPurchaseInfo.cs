using Newtonsoft.Json;

namespace UBS.FundManager.Messaging.Models.Fund
{
    public enum StockType { Equity, Bond }

    /// <summary>
    /// Describes the attributes of a stock's purchase information
    /// </summary>
    public class StockPurchaseInfo
    {
        [JsonProperty(PropertyName = "unitPrice")]
        public decimal PricePerUnit { get; set; }

        [JsonProperty(PropertyName = "purchasedQ")]
        public int QuantityPurchased { get; set; }
    }
}
