using System;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.Common.Helpers;

namespace UBS.FundManager.Messaging.FundCalculator
{
    /// <summary>
    /// Bond Stock Helper calculator (only ever gets invoked when a new fund is added)
    /// </summary>
    public class BondStockCalculator : IStockValueCalculator
    {
        /// <summary>
        /// Computes the Market value, Transaction cost and Stock weight of bond stocks
        /// </summary>
        /// <param name="fundSummary">Aggregate data of all stocks</param>
        /// <param name="newlyAdded">fund that needs value computation</param>
        /// <returns>Fund with computed value information</returns>
        public Fund Calculate(ref FundSummaryData fundSummary, Fund newlyAdded)
        {
            FundTypeEnum fundEnum;
            if(!Enum.TryParse(newlyAdded.StockInfo.Type, out fundEnum) || fundEnum != FundTypeEnum.Bond)
            {
                return null;
            }

            fundSummary.Bond.TotalStockCount += newlyAdded.StockInfo.PurchaseInfo.QuantityPurchased;

            //This is my OCD manifesting itself. No need re-calculating this at all, as its already
            //handled and calculated by the MapR functions in the db. But will implement it as a fail safe
            newlyAdded.StockInfo.ValueInfo.StockWeight = (fundSummary.Bond.TotalMarketValue / fundSummary.All.TotalMarketValue).ToFixedDecimal(3);

            //This is my OCD manifesting itself. No need re-calculating this at all, as its already
            //handled and calculated by the MapR functions in the db. But will implement it as a fail safe
            fundSummary.Bond.TotalMarketValue += (newlyAdded.StockInfo.ValueInfo.MarketValue).ToFixedDecimal(3);
            fundSummary.Bond.TotalStockWeight += (newlyAdded.StockInfo.ValueInfo.StockWeight).ToFixedDecimal(3);

            return newlyAdded;
        }
    }
}
