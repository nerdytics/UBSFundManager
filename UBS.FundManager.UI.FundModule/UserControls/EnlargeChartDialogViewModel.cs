using LiveCharts;
using LiveCharts.Wpf;
using MahApps.Metro.Controls.Dialogs;
using Prism.Events;
using Prism.Mvvm;
using System;
using UBS.FundManager.Messaging.Events;
using UBS.FundManager.Messaging.Models.Fund;

namespace UBS.FundManager.UI.FundModule.UserControls
{
    public class EnlargeChartDialogViewModel : BindableBase
    {
        public EnlargeChartDialogViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<EnlargeChartFundSummaryEvent>()
                            .Subscribe(OnFundSummaryReceived, ThreadOption.UIThread, false, FundSummaryFilter);
        }

        private SeriesCollection _fundsChartData;
        public SeriesCollection FundsChartData
        {
            get { return _fundsChartData; }
            set { SetProperty(ref _fundsChartData, value); }
        }

        private string[] _chartLabel = new string[] { "Total", "MValue", "SWeight" };
        public string[] ChartLabel
        {
            get { return _chartLabel; }
            set { SetProperty(ref _chartLabel, value); }
        }

        private void OnFundSummaryReceived(FundSummaryData fundSummary)
        {
            FundsChartData = new SeriesCollection
            {
                new ColumnSeries {
                    Title = string.Empty,
                    Values = new ChartValues<decimal>
                    {
                        fundSummary.Equity.TotalStockCount,
                        fundSummary.Equity.TotalMarketValue,
                        fundSummary.Equity.TotalStockWeight
                    }
                },
                new ColumnSeries {
                    Title = string.Empty,
                    Values = new ChartValues<decimal>
                    {
                        fundSummary.Bond.TotalStockCount,
                        fundSummary.Bond.TotalMarketValue,
                        fundSummary.Bond.TotalStockWeight
                    }
                }
            };
        }

        Predicate<FundSummaryData> FundSummaryFilter = (fundSummaryData) => fundSummaryData.Equity.TotalStockCount > 0 &&
                                                                          fundSummaryData.Bond.TotalStockCount > 0 &&
                                                                          fundSummaryData.All.TotalStockCount > 0;

        private IEventAggregator _eventAggregator;
        private IDialogCoordinator _dialogService;
        private EnlargeChartDialog _chartDialog;
    }
}
