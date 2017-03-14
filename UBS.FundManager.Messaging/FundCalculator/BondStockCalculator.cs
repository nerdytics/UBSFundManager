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
            SummaryData bondSummary = null, allSummary = null;

            if (!Enum.TryParse(newlyAdded.StockInfo.Type, out fundEnum) || fundEnum != FundTypeEnum.Bond)
            {
                return null;
            }

            int quantity = newlyAdded.StockInfo.PurchaseInfo.QuantityPurchased;
            StockValueInfo newlyAddedInfo = newlyAdded.StockInfo.ValueInfo;

            if(fundSummary != null)
            {
                if (fundSummary.Bond != null && fundSummary.Bond.TotalStockCount > 0)
                {
                    bondSummary = fundSummary.Bond;
                    allSummary = fundSummary.All;

                    // Update stock counts
                    bondSummary.TotalStockCount = bondSummary.TotalStockCount.CalculateTotal(quantity);
                    fundSummary.All.TotalStockCount = allSummary.TotalStockCount.CalculateTotal(quantity);

                    // Update market values
                    bondSummary.TotalMarketValue = bondSummary.TotalMarketValue.CalculateTotal(newlyAddedInfo.MarketValue);
                    fundSummary.All.TotalMarketValue = allSummary.TotalMarketValue.CalculateTotal(newlyAddedInfo.MarketValue);

                    // Update stock weights
                    newlyAddedInfo.StockWeight = newlyAddedInfo.MarketValue.ToStockWeight(allSummary.TotalMarketValue);
                    bondSummary.TotalStockWeight = bondSummary.TotalMarketValue.ToStockWeight(allSummary.TotalMarketValue);
                    allSummary.TotalStockWeight = allSummary.TotalMarketValue.ToStockWeight(allSummary.TotalMarketValue);
                }
                else if (fundSummary.All != null && fundSummary.All.TotalStockCount > 0)
                {
                    allSummary = fundSummary.All;

                    //Found equity stocks in portfolio (update portfolio totals)
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

                bondSummary = new SummaryData
                {
                    TotalStockCount = quantity,
                    TotalMarketValue = newlyAddedInfo.MarketValue,
                    TotalStockWeight = newlyAddedInfo.StockWeight
                };

                fundSummary = new FundSummaryData
                {
                    Bond = bondSummary,
                    All = new SummaryData
                    {
                        TotalStockCount = bondSummary.TotalStockCount,
                        TotalMarketValue = bondSummary.TotalMarketValue,
                        TotalStockWeight = bondSummary.TotalStockWeight
                    }
                };
            }

            newlyAdded.StockInfo.ValueInfo = newlyAddedInfo;
            fundSummary.Bond = bondSummary;
            fundSummary.All = allSummary;

            return newlyAdded;
        }
    }
}
