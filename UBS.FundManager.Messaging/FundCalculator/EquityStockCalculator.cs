using System;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.Common.Helpers;

namespace UBS.FundManager.Messaging.FundCalculator
{
    public interface IStockValueCalculator
    {
        Fund Calculate(ref FundSummaryData fundSummary, Fund input);
    }

    /// <summary>
    /// Equity Stock Helper calculator (only ever gets invoked when a new fund is added)
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
            SummaryData equitySummary = null, allSummary = null;
            
            if (!Enum.TryParse(newlyAdded.StockInfo.Type, out fundEnum) || fundEnum != FundTypeEnum.Equity)
            {
                return null;
            }

            int quantity = newlyAdded.StockInfo.PurchaseInfo.QuantityPurchased;
            StockValueInfo newlyAddedInfo = newlyAdded.StockInfo.ValueInfo;

            if (fundSummary != null)
            {
                if(fundSummary.Equity != null && fundSummary.Equity.TotalStockCount > 0)
                {

                    equitySummary = fundSummary.Equity;
                    allSummary = fundSummary.All;

                    // Update stock counts
                    equitySummary.TotalStockCount = equitySummary.TotalStockCount.CalculateTotal(quantity);
                    fundSummary.All.TotalStockCount = allSummary.TotalStockCount.CalculateTotal(quantity);

                    // Update market values
                    equitySummary.TotalMarketValue = equitySummary.TotalMarketValue.CalculateTotal(newlyAddedInfo.MarketValue);
                    fundSummary.All.TotalMarketValue = allSummary.TotalMarketValue.CalculateTotal(newlyAddedInfo.MarketValue);

                    // Update stock weights
                    newlyAddedInfo.StockWeight = newlyAddedInfo.MarketValue.ToStockWeight(allSummary.TotalMarketValue);
                    equitySummary.TotalStockWeight = equitySummary.TotalMarketValue.ToStockWeight(allSummary.TotalMarketValue);
                    allSummary.TotalStockWeight = allSummary.TotalMarketValue.ToStockWeight(allSummary.TotalMarketValue);
                }
                else if(fundSummary.All != null && fundSummary.All.TotalStockCount > 0)
                {
                    allSummary = fundSummary.All;

                    //Found bond stocks in portfolio (update portfolio totals)
                    allSummary.TotalStockCount = allSummary.TotalStockCount.CalculateTotal(quantity);
                    allSummary.TotalMarketValue = allSummary.TotalMarketValue.CalculateTotal(newlyAddedInfo.MarketValue);

                    newlyAddedInfo.StockWeight = newlyAddedInfo.MarketValue.ToStockWeight(allSummary.TotalMarketValue);
                    allSummary.TotalStockWeight = allSummary.TotalMarketValue.ToStockWeight(allSummary.TotalMarketValue);
                }
            }
            else
            {
                // Empty portfolio, this is the first stock to be added
                newlyAddedInfo.StockWeight = newlyAddedInfo.MarketValue.ToStockWeight(newlyAddedInfo.MarketValue);

                equitySummary = new SummaryData
                {
                    TotalStockCount = quantity,
                    TotalMarketValue = newlyAddedInfo.MarketValue,
                    TotalStockWeight = newlyAddedInfo.StockWeight
                };

                fundSummary = new FundSummaryData
                {
                    Equity = equitySummary,
                    All = new SummaryData
                    {
                        TotalStockCount = equitySummary.TotalStockCount,
                        TotalMarketValue = equitySummary.TotalMarketValue,
                        TotalStockWeight = equitySummary.TotalStockWeight
                    }
                };
            }

            newlyAdded.StockInfo.ValueInfo = newlyAddedInfo;
            fundSummary.Equity = equitySummary;
            fundSummary.All = allSummary;

            return newlyAdded;
        }
    }
}
