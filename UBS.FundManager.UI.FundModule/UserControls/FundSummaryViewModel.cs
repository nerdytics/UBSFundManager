using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UBS.FundManager.Messaging.Events;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.Common.Helpers;

namespace UBS.FundManager.UI.FundModule.UserControls
{
    /// <summary>
    /// View model for the FundSummary view
    /// </summary>
    public class FundSummaryViewModel : BindableBase
    {
        /// <summary>
        /// Default Injection constructor (always invoked by IoC container)
        /// </summary>
        /// <param name="eventAggregator">Event management service (for publishing and subscribing to events)</param>
        public FundSummaryViewModel(IEventAggregator eventAggregator, IDialogCoordinator dialogService, ILoggerFacade logger)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<DownloadedFundsListEvent>()
                            .Subscribe(OnDownloadedFundsList, ThreadOption.UIThread, false, DownloadedFundsListFilter);

            _eventAggregator.GetEvent<FundSummaryEvent>()
                            .Subscribe(OnFundSummaryReceived, ThreadOption.UIThread, false);

            _dialogService = dialogService;
            _logger = logger;
        }

        #region Notification Properties
        private string _equityStockTitle = "Summary: Equity Stocks";
        public string EquityStockTitle
        {
            get { return _equityStockTitle; }
            set { SetProperty(ref _equityStockTitle, value); }
        }

        private SeriesCollection _equityStocksChartData;
        public SeriesCollection EquityStocksChartData
        {
            get { return _equityStocksChartData; }
            set { SetProperty(ref _equityStocksChartData, value); }
        }

        private ObservableCollection<SummaryData> _equityGridData = new ObservableCollection<SummaryData>();
        public ObservableCollection<SummaryData> EquityGridData
        {
            get { return _equityGridData; }
            set { SetProperty(ref _equityGridData, value); }
        }

        private string _bondStockTitle = "Summary: Bond Stocks";
        public string BondStockTitle
        {
            get { return _bondStockTitle; }
            set { SetProperty(ref _bondStockTitle, value); }
        }

        private SeriesCollection _bondStocksChartData;
        public SeriesCollection BondStocksChartData
        {
            get { return _bondStocksChartData; }
            set { SetProperty(ref _bondStocksChartData, value); }
        }

        private ObservableCollection<SummaryData> _bondGridData = new ObservableCollection<SummaryData>();
        public ObservableCollection<SummaryData> BondGridData
        {
            get { return _bondGridData; }
            set { SetProperty(ref _bondGridData, value); }
        }

        private ObservableCollection<SummaryData> _allStocksGridData = new ObservableCollection<SummaryData>();
        public ObservableCollection<SummaryData> AllStocksGridData
        {
            get { return _allStocksGridData; }
            set { SetProperty(ref _allStocksGridData, value); }
        }

        private string[] _chartLabel = new string[] { "Total", "MValue", "SWeight" };
        public string[] ChartLabel
        {
            get { return _chartLabel; }
            set { SetProperty(ref _chartLabel, value); }
        }

        #endregion

        #region Aggregator Events Behaviour
        /// <summary>
        /// Handler gets called when an associated filter has determined that the received data
        /// meets the validation criteria. This handler subsequently instantiates the DownloadedFundsList
        /// property with these downloaded funds list.
        /// </summary>
        /// <param name="downloadedFunds">Downloaded funds from cloud</param>
        private void OnDownloadedFundsList(IEnumerable<Fund> downloadedFunds)
        {
            try
            {
                GenerateGridData(downloadedFunds);

                var fundSummary = new FundSummaryData
                {
                    Equity = EquityGridData.First(),
                    Bond = BondGridData.First(),
                    All = AllStocksGridData.First()
                };

                PopulateChart(fundSummary);

                //It's essential to broadcast this data, as there might be other modules in the application
                //that needs it to perform some operation (such as the FundsListViewModel) and in the event
                //no module is subscribed to this broadcast, the data is garbage collected.
                _eventAggregator.GetEvent<FundSummaryEvent>().Publish(fundSummary);
            }
            catch (Exception e)
            {
                _logger.Log($"{ GetType().Name }: Error occurred while processing  '{ nameof(OnDownloadedFundsList) }'" +
                            Environment.NewLine +
                                $"Exception: { e }", Category.Info, Priority.None);

                _dialogService.ShowMessageAsync(this, "Error Occurred", $"{ e.Message }."
                                    + Environment.NewLine + "Please inspect logs for further details.");
            }
        }

        /// <summary>
        /// Updates the All/Equity/Bond charts with updated fund summary data 
        /// (which includes newly added fund). This also updates the chart grids
        /// </summary>
        /// <param name="updatedFundSummary">Updated fund summary data</param>
        private void OnFundSummaryReceived(FundSummaryData updatedFundSummary)
        {
            try
            {
                PopulateChart(updatedFundSummary);

                //These chart's grids should only ever have a row of data
                //so existing row should be updated with the new fund summary
                BondGridData.RemoveAt(0);                   
                BondGridData.Add(updatedFundSummary.Bond);

                EquityGridData.RemoveAt(0);
                EquityGridData.Add(updatedFundSummary.Equity);

                AllStocksGridData.RemoveAt(0);
                AllStocksGridData.Add(updatedFundSummary.All);
            }
            catch (Exception e)
            {
                _logger.Log($"{ GetType().Name }: Error occurred while processing  '{ nameof(OnFundSummaryReceived) }'" +
                            Environment.NewLine +
                                $"Exception: { e }", Category.Info, Priority.None);

                _dialogService.ShowMessageAsync(this, "Error Occurred", $"{ e.Message }."
                                    + Environment.NewLine + "Please inspect logs for further details.");
            }
        }
        #endregion

        #region Aggregator Events Filter
        /// <summary>
        /// This filter determines if the subsriber for DownloadedFundsListEvent should receive the data or not.
        /// As an instance if the downloaded funds list was empty, the subscriber would not be activated)
        /// </summary>
        Predicate<IEnumerable<Fund>> DownloadedFundsListFilter = (fundsList) => fundsList.Count() > 0;
        #endregion

        #region Fields
        private IEventAggregator _eventAggregator;
        private IDialogCoordinator _dialogService;
        private ILoggerFacade _logger;
        #endregion

        #region Helper methods
        /// <summary>
        /// Transforms the fund summary data to the chart compatible 
        /// model and subsequently updates the respective charts
        /// </summary>
        /// <param name="summaryData"></param>
        private void PopulateChart(FundSummaryData summaryData)
        {
            try
            {
                EquityStocksChartData = new SeriesCollection
                {
                    new ColumnSeries {
                        Title = string.Empty,
                        Values = new ChartValues<decimal>
                        {
                            summaryData.Equity.TotalStockCount,
                            summaryData.Equity.TotalMarketValue,
                            summaryData.Equity.TotalStockWeight
                        }
                    }
                };

                BondStocksChartData = new SeriesCollection
                {
                    new ColumnSeries {
                        Title = string.Empty,
                        Values = new ChartValues<decimal>
                        {
                            summaryData.Bond.TotalStockCount,
                            summaryData.Bond.TotalMarketValue,
                            summaryData.Bond.TotalStockWeight
                        }
                    }
                };
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Generates data for the rendering the Chart and Grid 
        /// </summary>
        /// <param name="downloadedFunds">Downloaded funds list</param>
        private void GenerateGridData(IEnumerable<Fund> downloadedFunds)
        {
            try
            {
                IEnumerable<StockValueInfo> bondStockValue, equityStockValue;
                GetFundValue(downloadedFunds, out bondStockValue, out equityStockValue);

                int bondsStockCount, equityStockCount;
                GetFundCount(downloadedFunds, out bondsStockCount, out equityStockCount);

                EquityGridData = new ObservableCollection<SummaryData>
                {
                    new SummaryData
                    {
                        TotalStockCount = equityStockCount,
                        TotalMarketValue = equityStockValue.Select(s => s.MarketValue).Sum().ToFixedDecimal(3),
                        TotalStockWeight = equityStockValue.Select(s => s.StockWeight).Sum().ToFixedDecimal(3)
                    }
                };

                BondGridData = new ObservableCollection<SummaryData>
                {
                    new SummaryData
                    {
                        TotalStockCount = bondsStockCount,
                        TotalMarketValue = bondStockValue.Select(s => s.MarketValue).Sum().ToFixedDecimal(3),
                        TotalStockWeight = bondStockValue.Select(s => s.StockWeight).Sum().ToFixedDecimal(3)
                    }
                };

                AllStocksGridData = new ObservableCollection<SummaryData>
            {
                new SummaryData
                {
                    TotalStockCount = EquityGridData.First().TotalStockCount + BondGridData.First().TotalStockCount,
                    TotalMarketValue = EquityGridData.First().TotalMarketValue + BondGridData.First().TotalMarketValue,
                    TotalStockWeight = EquityGridData.First().TotalStockWeight + BondGridData.First().TotalStockWeight,
                }
            };
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Calculate the total number of both equity and bond stocks
        /// </summary>
        /// <param name="downloadedFunds">List of all downloaded funds</param>
        /// <param name="bondsStockCount">Count of bond stocks</param>
        /// <param name="equityStockCount">Count of equity stocks</param>
        private void GetFundCount(IEnumerable<Fund> downloadedFunds, out int bondsStockCount, out int equityStockCount)
        {

            equityStockCount = downloadedFunds.Where(f => string.Equals(f.StockInfo.Type,
                                                                            FundTypeEnum.Equity.ToString(),
                                                                            StringComparison.InvariantCultureIgnoreCase))
                                                  .Select(f => f.StockInfo.PurchaseInfo.QuantityPurchased).Sum();

            bondsStockCount = downloadedFunds.Where(f => string.Equals(f.StockInfo.Type,
                                                                            FundTypeEnum.Bond.ToString(),
                                                                            StringComparison.InvariantCultureIgnoreCase))
                                                .Select(f => f.StockInfo.PurchaseInfo.QuantityPurchased).Sum();
        }

        /// <summary>
        /// Extract the list of stock value information for both equity and bond stocks
        /// </summary>
        /// <param name="downloadedFunds">List of all downloaded funds</param>
        /// <param name="bondValueInfo">List of bond stock value information</param>
        /// <param name="equityValueInfo">List of equity stock value information</param>
        private void GetFundValue(IEnumerable<Fund> downloadedFunds, 
            out IEnumerable<StockValueInfo> bondValueInfo, out IEnumerable<StockValueInfo> equityValueInfo)
        {
            bondValueInfo = downloadedFunds.Where(f => string.Equals(f.StockInfo.Type,
                                                                            FundTypeEnum.Bond.ToString(),
                                                                            StringComparison.InvariantCultureIgnoreCase))
                                                .Select(f => f.StockInfo.ValueInfo);

            equityValueInfo = downloadedFunds.Where(f => string.Equals(f.StockInfo.Type,
                                                    FundTypeEnum.Equity.ToString(),
                                                    StringComparison.InvariantCultureIgnoreCase))
                                                .Select(f => f.StockInfo.ValueInfo);
        }
        #endregion
    }
}
