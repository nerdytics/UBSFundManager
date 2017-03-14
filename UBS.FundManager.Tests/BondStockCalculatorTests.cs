using Moq;
using NUnit.Framework;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.Messaging.FundCalculator;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.Tests
{
    [TestFixture]
    public class BondStockCalculatorTests
    {
        [SetUp]
        public void Setup()
        {
            _bondStockCalc = Mock.Of<BondStockCalculator>();
        }

        /// <summary>
        /// Tests that a 'BOND' calculator can successful compute the values of a newly added bond
        /// even though the portfolio already has 'Bond' stocks in it.
        /// </summary>
        [Test]
        public void Calculate_BondStockValues_WithPreExistingBonds_ShouldPass()
        {
            FundSummaryData expectedResult = new FundSummaryData
            {
                Bond = new SummaryData { TotalStockCount = 2, TotalMarketValue = 1100, TotalStockWeight = 100 },
                All = new SummaryData { TotalStockCount = 2, TotalMarketValue = 1100, TotalStockWeight = 100 }
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
                    ValueInfo = new StockValueInfo { MarketValue = (1 * 100), TransactionCost = (1 * 100) * 0.5M },
                    Type = FundTypeEnum.Bond.ToString()
                }
            };

            newlyAdded = _bondStockCalc.Calculate(ref initialSummaryData, newlyAdded);

            Assert.That(newlyAdded.StockInfo.ValueInfo.StockWeight == 9.091M);
            Assert.That(initialSummaryData.Bond.TotalStockCount == expectedResult.Bond.TotalStockCount);
            Assert.That(initialSummaryData.Bond.TotalMarketValue == expectedResult.Bond.TotalMarketValue);
            Assert.That(initialSummaryData.Bond.TotalStockWeight == expectedResult.Bond.TotalStockWeight);

            Assert.That(initialSummaryData.All.TotalStockCount == expectedResult.All.TotalStockCount);
            Assert.That(initialSummaryData.All.TotalMarketValue == expectedResult.All.TotalMarketValue);
            Assert.That(initialSummaryData.All.TotalStockWeight == expectedResult.All.TotalStockWeight);
        }

        /// <summary>
        /// Tests that a 'BOND' calculator can successfully compute the values of a newly added stock
        /// even though the portfolio already has 'Equity' stocks added to it.
        /// </summary>
        [Test]
        public void Calculate_BondStockValues_WithPreExistingEquities_ShouldPass()
        {
            FundSummaryData expectedResult = new FundSummaryData
            {
                Bond = new SummaryData { TotalStockCount = 1, TotalMarketValue = 1000, TotalStockWeight = 50 },
                // TotalStockWeight here is 100% because the bonds calculator only knows how to deal with 'Bonds' and
                // will ignore all non-bond stock(s). (S in SOLID). In real terms, its expected that an 'Equity' stock
                // will be passed to its own calculator that will fix its values.
                // Recall, TotalStockWeight = Stock Mkt Value / Total Market Value in Portfolio...so when an 'Equity'
                // calculator receives this summary data, it will use the All's 'TotalMarketValue' to fix the 'Equity'
                // stocks TotalStockWeight.
                Equity = new SummaryData { TotalStockCount = 1, TotalMarketValue = 1000, TotalStockWeight = 100 }, 
                All = new SummaryData { TotalStockCount = 2, TotalMarketValue = 2000, TotalStockWeight = 100 }
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
                    ValueInfo = new StockValueInfo { MarketValue = (1 * 1000), TransactionCost = (1 * 100) * 2M },
                    Type = FundTypeEnum.Bond.ToString()
                }
            };

            newlyAdded = _bondStockCalc.Calculate(ref initialSummaryData, newlyAdded);

            Assert.That(newlyAdded.StockInfo.ValueInfo.StockWeight == expectedResult.Bond.TotalStockWeight);
            Assert.That(newlyAdded.StockInfo.PurchaseInfo.QuantityPurchased == expectedResult.Bond.TotalStockCount);
            Assert.That(newlyAdded.StockInfo.ValueInfo.MarketValue == expectedResult.Bond.TotalMarketValue);

            Assert.That(initialSummaryData.Equity.TotalStockCount == expectedResult.Equity.TotalStockCount);
            Assert.That(initialSummaryData.Equity.TotalMarketValue == expectedResult.Equity.TotalMarketValue);
            Assert.That(initialSummaryData.Equity.TotalStockWeight == expectedResult.Equity.TotalStockWeight);

            Assert.That(initialSummaryData.All.TotalStockCount == expectedResult.All.TotalStockCount);
            Assert.That(initialSummaryData.All.TotalMarketValue == expectedResult.All.TotalMarketValue);
            Assert.That(initialSummaryData.All.TotalStockWeight == expectedResult.All.TotalStockWeight);
        }

        /// <summary>
        /// Test that a 'BOND' can be added to an empty portfolio
        /// </summary>
        [Test]
        public void Calculate_BondStockValues_WithEmptyPortfolio_ShouldPass()
        {
            FundSummaryData expectedResult = new FundSummaryData
            {
                Bond = new SummaryData { TotalStockCount = 1, TotalMarketValue = 100, TotalStockWeight = 100 },
                All = new SummaryData { TotalStockCount = 1, TotalMarketValue = 100, TotalStockWeight = 100 }
            };

            FundSummaryData initialSummaryData = default(FundSummaryData);
            Fund newlyAdded = new Fund
            {
                StockInfo = new Stock
                {
                    PurchaseInfo = new StockPurchaseInfo { QuantityPurchased = 1 },
                    ValueInfo = new StockValueInfo { MarketValue = (1 * 100), TransactionCost = (1 * 100) * 0.5M },
                    Type = FundTypeEnum.Bond.ToString()
                }
            };

            newlyAdded = _bondStockCalc.Calculate(ref initialSummaryData, newlyAdded);

            Assert.That(newlyAdded.StockInfo.ValueInfo.StockWeight == expectedResult.Bond.TotalStockWeight);

            Assert.That(initialSummaryData.Bond.TotalStockCount == expectedResult.Bond.TotalStockCount);
            Assert.That(initialSummaryData.Bond.TotalMarketValue == expectedResult.Bond.TotalMarketValue);
            Assert.That(initialSummaryData.Bond.TotalStockWeight == expectedResult.Bond.TotalStockWeight);

            Assert.That(initialSummaryData.Bond.TotalStockCount == expectedResult.All.TotalStockCount);
            Assert.That(initialSummaryData.Bond.TotalMarketValue == expectedResult.All.TotalMarketValue);
            Assert.That(initialSummaryData.Bond.TotalStockWeight == expectedResult.All.TotalStockWeight);
        }

        /// <summary>
        /// Verify that a bond calculator cannot process an 'equity stock'. Should pass...
        /// </summary>
        [Test]
        public void Calculate_BondStockValues_ShouldReturnNull()
        {
            FundSummaryData initialSummaryData = new FundSummaryData();
            Fund newlyAdded = new Fund { StockInfo = new Stock { Type = FundTypeEnum.Equity.ToString() } };

            newlyAdded = _bondStockCalc.Calculate(ref initialSummaryData, newlyAdded);
            Assert.IsNull(newlyAdded);
        }

        #region Fields
        private BondStockCalculator _bondStockCalc;
        #endregion
    }
}
