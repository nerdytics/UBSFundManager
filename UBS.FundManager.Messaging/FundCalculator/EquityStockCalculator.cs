using System;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.Messaging.FundCalculator
{
    public interface IStockValueCalculator
    {
        Fund Calculate(ref FundSummaryData fundSummary, Fund input);
    }

    /// <summary>
    /// Equity Stock Helper calculator
    /// </summary>
    public class EquityStockCalculator : IStockValueCalculator
    {
        /// <summary>
        /// Computes the Market value, Transaction cost and Stock weight of equity stocks
        /// </summary>
        /// <param name="fundSummary">Aggregate data of all stocks</param>
        /// <param name="newlyAdded">fund that needs value computation</param>
        /// <returns>Fund with computed value information</returns>
        public Fund Calculate(ref FundSummaryData fundSummary, Fund newlyAdded)
        {
            FundTypeEnum fundEnum;
            if (!Enum.TryParse(newlyAdded.StockInfo.Type, out fundEnum) || fundEnum != FundTypeEnum.Equity)
            {
                return null;
            }

            fundSummary.Equity.TotalStockCount += 1;
            //Update fund name
            newlyAdded.Name += fundSummary.Equity.TotalStockCount;

            //Calculate the stock weight for the newly added fund
            newlyAdded.StockInfo.ValueInfo.StockWeight = fundSummary.Equity.TotalMarketValue / fundSummary.All.TotalMarketValue;

            //Update the fund summary data (that populates the charts)
            fundSummary.Equity.TotalMarketValue += newlyAdded.StockInfo.ValueInfo.MarketValue;
            fundSummary.Equity.TotalStockWeight += newlyAdded.StockInfo.ValueInfo.StockWeight;

            return newlyAdded;
        }
    }
}
