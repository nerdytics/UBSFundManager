using System;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.Messaging.FundCalculator
{
    /// <summary>
    /// Bond Stock Helper calculator
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

            fundSummary.Bond.TotalStockCount += 1;
            //Update fund name
            newlyAdded.Name += fundSummary.Bond.TotalStockCount;

            //Calculate the stock weight for the newly added fund
            newlyAdded.StockInfo.ValueInfo.StockWeight = fundSummary.Bond.TotalMarketValue / fundSummary.All.TotalMarketValue;

            //Update the fund summary data (that populates the charts)
            fundSummary.Bond.TotalMarketValue += newlyAdded.StockInfo.ValueInfo.MarketValue;
            fundSummary.Bond.TotalStockWeight += newlyAdded.StockInfo.ValueInfo.StockWeight;

            return newlyAdded;
        }
    }
}
