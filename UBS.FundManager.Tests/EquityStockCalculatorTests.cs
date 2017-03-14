using Moq;
using NUnit.Framework;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.Messaging.FundCalculator;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.Tests
{
    [TestFixture]
    public class EquityStockCalculatorTests
    {
        [SetUp]
        public void Setup()
        {
            _equityStockCalc = Mock.Of<EquityStockCalculator>();
        }

        /// <summary>
        /// Verify that an equity calculator cannot process a 'bond stock'. Should pass...
        /// </summary>
        [Test]
        public void Calculate_BondStockValues_ShouldReturnNull()
        {
            FundSummaryData initialSummaryData = new FundSummaryData();
            Fund newlyAdded = new Fund { StockInfo = new Stock { Type = FundTypeEnum.Bond.ToString() } };

            newlyAdded = _equityStockCalc.Calculate(ref initialSummaryData, newlyAdded);
            Assert.IsNull(newlyAdded);
        }

        /// <summary>
        /// Test that an equity stock can be added to an empty portfolio
        /// </summary>
        [Test]
        public void Calculate_EquityStockValues_WithEmptyPortfolio_ShouldPass()
        {
            FundSummaryData expectedResult = new FundSummaryData
            {
                Equity = new SummaryData { TotalStockCount = 1, TotalMarketValue = 100, TotalStockWeight = 100 },
                All = new SummaryData { TotalStockCount = 1, TotalMarketValue = 100, TotalStockWeight = 100 }
            };

            FundSummaryData initialSummaryData = default(FundSummaryData);
            Fund newlyAdded = new Fund
            {
                StockInfo = new Stock
                {
                    PurchaseInfo = new StockPurchaseInfo { QuantityPurchased = 1 },
                    ValueInfo = new StockValueInfo { MarketValue = (1 * 100), TransactionCost = (1 * 100) * 0.5M },
                    Type = FundTypeEnum.Equity.ToString()
                }
            };

            newlyAdded = _equityStockCalc.Calculate(ref initialSummaryData, newlyAdded);

            Assert.That(newlyAdded.StockInfo.ValueInfo.StockWeight == expectedResult.Equity.TotalStockWeight);

            Assert.That(initialSummaryData.Equity.TotalStockCount == expectedResult.Equity.TotalStockCount);
            Assert.That(initialSummaryData.Equity.TotalMarketValue == expectedResult.Equity.TotalMarketValue);
            Assert.That(initialSummaryData.Equity.TotalStockWeight == expectedResult.Equity.TotalStockWeight);

            Assert.That(initialSummaryData.Equity.TotalStockCount == expectedResult.All.TotalStockCount);
            Assert.That(initialSummaryData.Equity.TotalMarketValue == expectedResult.All.TotalMarketValue);
            Assert.That(initialSummaryData.Equity.TotalStockWeight == expectedResult.All.TotalStockWeight);
        }

        /// <summary>
        /// Tests that an equity calculator can successfully compute the values of a newly added stock
        /// even though the portfolio already has 'Bond' stocks added to it.
        /// </summary>
        [Test]
        public void Calculate_EquityStockValues_WithPreExistingBonds_ShouldPass()
        {
            FundSummaryData expectedResult = new FundSummaryData
            {
                Equity = new SummaryData { TotalStockCount = 1, TotalMarketValue = 1000, TotalStockWeight = 50 },
                Bond = new SummaryData { TotalStockCount = 1, TotalMarketValue = 1000, TotalStockWeight = 100 },
                All = new SummaryData { TotalStockCount = 2, TotalMarketValue = 2000, TotalStockWeight = 100 }
            };

            FundSummaryData initialSummaryData = new FundSummaryData
            {
                Bond = new SummaryData { TotalStockCount = 1, TotalMarketValue = 1000, TotalStockWeight = 1000M.ToStockWeight(1000) },
                All = new SummaryData { TotalStockCount = 1, TotalMarketValue = 1000, TotalStockWeight = 1000M.ToStockWeight(1000) }
            };

            Fund newlyAdded = new Fund
            {
                StockInfo = new Stock
                {
                    PurchaseInfo = new StockPurchaseInfo { QuantityPurchased = 1 },
                    ValueInfo = new StockValueInfo { MarketValue = (1 * 1000), TransactionCost = (1 * 100) * 2M },
                    Type = FundTypeEnum.Equity.ToString()
                }
            };

            newlyAdded = _equityStockCalc.Calculate(ref initialSummaryData, newlyAdded);

            Assert.That(newlyAdded.StockInfo.ValueInfo.StockWeight == expectedResult.Equity.TotalStockWeight);
            Assert.That(newlyAdded.StockInfo.PurchaseInfo.QuantityPurchased == expectedResult.Equity.TotalStockCount);
            Assert.That(newlyAdded.StockInfo.ValueInfo.MarketValue == expectedResult.Equity.TotalMarketValue);

            Assert.That(initialSummaryData.Bond.TotalStockCount == expectedResult.Bond.TotalStockCount);
            Assert.That(initialSummaryData.Bond.TotalMarketValue == expectedResult.Bond.TotalMarketValue);
            Assert.That(initialSummaryData.Bond.TotalStockWeight == expectedResult.Bond.TotalStockWeight);

            Assert.That(initialSummaryData.All.TotalStockCount == expectedResult.All.TotalStockCount);
            Assert.That(initialSummaryData.All.TotalMarketValue == expectedResult.All.TotalMarketValue);
            Assert.That(initialSummaryData.All.TotalStockWeight == expectedResult.All.TotalStockWeight);
        }

        /// <summary>
        /// Tests that an equity calculator can successful compute the values of a newly added equity
        /// even though the portfolio already has 'equity' stocks in it.
        /// </summary>
        [Test]
        public void Calculate_EquityStockValues_WithPreExistingEquities_ShouldPass()
        {
            FundSummaryData expectedResult = new FundSummaryData
            {
                Equity = new SummaryData { TotalStockCount = 2, TotalMarketValue = 1100, TotalStockWeight = 100 },
                All =new SummaryData { TotalStockCount = 2, TotalMarketValue = 1100, TotalStockWeight = 100 }
            };

            FundSummaryData initialSummaryData = new FundSummaryData
            {
                Equity = new SummaryData { TotalStockCount = 1, TotalMarketValue = 1000, TotalStockWeight = 1000M.ToStockWeight(1000) },
                All = new SummaryData { TotalStockCount = 1, TotalMarketValue = 1000, TotalStockWeight = 1000M.ToStockWeight(1000) }
            };

            Fund newlyAdded = new Fund
            {
                StockInfo = new Stock
                {
                    PurchaseInfo = new StockPurchaseInfo { QuantityPurchased = 1 },
                    ValueInfo = new StockValueInfo { MarketValue = (1 * 100), TransactionCost = (1 * 100) * 0.5M }, Type = FundTypeEnum.Equity.ToString()
                }
            };

            newlyAdded = _equityStockCalc.Calculate(ref initialSummaryData, newlyAdded);

            Assert.That(newlyAdded.StockInfo.ValueInfo.StockWeight == 9.091M);

            Assert.That(initialSummaryData.Equity.TotalStockCount == expectedResult.Equity.TotalStockCount);
            Assert.That(initialSummaryData.Equity.TotalMarketValue == expectedResult.Equity.TotalMarketValue);
            Assert.That(initialSummaryData.Equity.TotalStockWeight == expectedResult.Equity.TotalStockWeight);

            Assert.That(initialSummaryData.All.TotalStockCount == expectedResult.All.TotalStockCount);
            Assert.That(initialSummaryData.All.TotalMarketValue == expectedResult.All.TotalMarketValue);
            Assert.That(initialSummaryData.All.TotalStockWeight == expectedResult.All.TotalStockWeight);
        }

        #region Fields
        private EquityStockCalculator _equityStockCalc;
        #endregion
    }
}
