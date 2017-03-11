namespace UBS.FundManager.Messaging.Models.Fund
{
    public class FundSummaryData
    {
        public SummaryData Equity { get; set; }
        public SummaryData Bond { get; set; }
        public SummaryData All { get; set; }
    }

    public class SummaryData
    {
        public decimal TotalStockCount { get; set; }
        public decimal TotalStockWeight { get; set; }
        public decimal TotalMarketValue { get; set; }
    }
}
