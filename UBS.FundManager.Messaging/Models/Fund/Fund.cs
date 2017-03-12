using Newtonsoft.Json;

namespace UBS.FundManager.Messaging.Models.Fund
{
    /// <summary>
    /// Describes properties of a fund
    /// </summary>
    public class Fund
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "stockInfo")]
        public Stock StockInfo { get; set; }
    }

    /// <summary>
    /// Models the supported fund type (i.e Equity or Bond)
    /// </summary>
    public class FundType
    {
        public string Name { get; set; }
    }

    public enum FundTypeEnum
    {
        Equity,
        Bond
    }
}
