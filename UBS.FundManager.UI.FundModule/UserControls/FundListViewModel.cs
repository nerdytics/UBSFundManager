using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Events;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.Messaging.Models.ExchangeData;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using UBS.FundManager.Messaging.FundCalculator;
using System.Windows.Data;
using Prism.Logging;
using UBS.FundManager.Common.Helpers;

namespace UBS.FundManager.UI.FundModule.UserControls
{
    /// <summary>
    /// View model for the FundList view
    /// </summary>
    public class FundListViewModel : BindableBase
    {
        /// <summary>
        /// Default Injection constructor (always invoked by IoC container)
        /// </summary>
        /// <param name="eventAggregator">Event management service (for publishing and subscribing to events)</param>
        /// <param name="messagingClient">Messaging client for publishing and receiving messages from the AMQP queue</param>
        /// <param name="dialogService"></param>
        /// <param name="stockCalculators"></param>
        /// <param name="logger"></param>
        public FundListViewModel(IEventAggregator eventAggregator, IMessagingClient messagingClient, 
                    IDialogCoordinator dialogService, IStockValueCalculator[] stockCalculators, ILoggerFacade logger)
        {
            _eventAggregator = eventAggregator;
            _messagingClient = messagingClient;
            _dialogService = dialogService;
            _stockCalculators = stockCalculators;
            _logger = logger;

            TriggerFundsDownload();

            _eventAggregator.GetEvent<DownloadedFundsListEvent>()
                          .Subscribe(OnDownloadedFundsList, ThreadOption.UIThread, false, DownloadedFundsListFilter);

            _eventAggregator.GetEvent<NewFundAddedEvent>()
                          .Subscribe(OnNewFundAdded, ThreadOption.BackgroundThread, false);

            _eventAggregator.GetEvent<FundSummaryEvent>()
                            .Subscribe(OnFundsSummaryReceived, ThreadOption.BackgroundThread, false, FundSummaryDataFilter);
        }

        #region Aggregator Events Filter
        /// <summary>
        /// This filter determines if the subsriber for DownloadedFundsListEvent should receive the data or not.
        /// As an instance if the downloaded funds list was empty, the subscriber would not be activated)
        /// </summary>
        Predicate<IEnumerable<Fund>> DownloadedFundsListFilter = (fundsList) => fundsList.Count() > 0;

        /// <summary>
        /// Filter determines if a broadcasted funds summary should be received or ignored by this viewmodel
        /// </summary>
        Predicate<FundSummaryData> FundSummaryDataFilter = (fundSummaryData) => fundSummaryData.Equity.TotalStockCount > 0 &&
                                                                          fundSummaryData.Bond.TotalStockCount > 0 &&
                                                                          fundSummaryData.All.TotalStockCount > 0;
        #endregion

        #region Aggregator Events Behaviour
        /// <summary>
        /// Handler gets called when an associated filter has determined that the received data
        /// meets the validation criteria. This handler subsequently instantiates the DownloadedFundsList
        /// property with these downloaded funds list.
        /// </summary>
        /// <param name="downloadedFunds">Downloaded funds from cloud</param>
        private async void OnDownloadedFundsList(IEnumerable<Fund> downloadedFunds)
        {
            _dialogController = await ShowProgress(true);
            _logger.Log($"Updating the value of { nameof(DownloadedFundsList) } with value: { downloadedFunds.Serialise() }",
                        Category.Info,
                                Priority.None);

            DownloadedFundsList = new ObservableCollection<Fund>(downloadedFunds);
            BindingOperations.EnableCollectionSynchronization(DownloadedFundsList, monitor);
            _dialogController?.CloseAsync();
        }

        /// <summary>
        /// Assign the received fund summary data (from the FundSummaryViewModel broadcast)
        /// and subsequently stores in a field specific to this view model. When new stocks get
        /// added, this data will be used in performing some computations that will enable live
        /// updates of the chart data and grid.
        /// </summary>
        /// <param name="fundSummaryData"></param>
        private void OnFundsSummaryReceived(FundSummaryData fundSummaryData)
        {
            _logger.Log($"{ GetType().Name }: FundSummary message received: { fundSummaryData.Serialise() }",
                        Category.Info,
                            Priority.None);
            _fundSummaryData = fundSummaryData;
        }

        /// <summary>
        /// Subscriber listens for messages for when new funds are added to the cloud db.
        /// </summary>
        /// <param name="newFund">Fund that was just added (received from AMQP host)</param>
        private void OnNewFundAdded(Fund newFund)
        {
            try
            {
                _logger.Log($"{ GetType().Name }: New fund message received: { newFund.Serialise() }", Category.Info, Priority.None);

                foreach (IStockValueCalculator calculator in _stockCalculators)
                {
                    _logger.Log($"{ GetType().Name }: Running calculators on new fund.", Category.Info, Priority.None);
                    Fund updatedFund = calculator.Calculate(ref _fundSummaryData, newFund);

                    if (updatedFund != null)
                    {
                        _logger.Log($"{ GetType().Name }: Fund value info successfully updated: { updatedFund.Serialise() }",
                                    Category.Info,
                                        Priority.None);

                        DownloadedFundsList.Add(updatedFund);
                        DownloadedFundsList.OrderBy(f => f.Name);

                        //Broadcast updated fund summary data so that the chart and chart grids
                        //can be updated in realtime.
                        _logger.Log($"{ GetType().Name }: Broadcasting '{ nameof(FundSummaryEvent) }' with payload: " +
                                        $"{ _fundSummaryData.Serialise() }", Category.Info, Priority.None);

                        _eventAggregator.GetEvent<FundSummaryEvent>().Publish(_fundSummaryData);
                        _eventAggregator.GetEvent<EnlargeChartFundSummaryEvent>().Publish(_fundSummaryData); 
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log($"{ GetType().Name }: Error occurred while processing  '{ nameof(OnNewFundAdded) }'" +
                            Environment.NewLine +
                                $"Exception: { e }", Category.Info, Priority.None);

                _dialogService.ShowMessageAsync(this, "Error Occurred", $"{ e.Message }." 
                                    + Environment.NewLine + "Please inspect logs for further details.");
            }
        }
        #endregion

        #region Notification Properties
        /// <summary>
        /// Collection of downloaded funds in portfolio
        /// </summary>
        private ObservableCollection<Fund> _downloadedFundsList;
        public ObservableCollection<Fund> DownloadedFundsList
        {
            get { return _downloadedFundsList; }
            set
            {
                if (value.Count > 0 || _downloadedFundsList == null)
                {
                    _logger.Log($"{ GetType().Name }: Updating '{ nameof(DownloadedFundsList) }' with value: " +
                                $"{ value.Serialise() }", Category.Info, Priority.None);

                    SetProperty(ref _downloadedFundsList, value);
                }
            }
        }
        #endregion

        #region Fields
        private IEventAggregator _eventAggregator;
        private IMessagingClient _messagingClient;

        private IDialogCoordinator _dialogService;
        private IStockValueCalculator[] _stockCalculators;
        private ILoggerFacade _logger;

        private ProgressDialogController _dialogController;
        private FundSummaryData _fundSummaryData;
        static object monitor = new object();
        #endregion

        #region Helper methods
        /// <summary>
        /// Sends a funds download request to a cloud service via an AMQP host (using the 
        /// messaging library). All connection metadata are abstracted in the library.
        /// </summary>
        void TriggerFundsDownload()
        {
            try
            {
                _logger.Log($"{ GetType().Name }: Broadcasting '{ nameof(FundSummaryEvent) }' with payload: " +
            $"{ _fundSummaryData.Serialise() }", Category.Info, Priority.None);
                _messagingClient.Start();

                _messagingClient.Publish(new DownloadFundsRequest
                {
                    DatasetSize = 100
                }, TriggerAction.DownloadFund);
            }
            catch (Exception e)
            {
                _logger.Log($"{ GetType().Name }: Error occurred while processing  '{ nameof(TriggerFundsDownload) }'" +
                            Environment.NewLine +
                                $"Exception: { e }", Category.Info, Priority.None);

                _dialogService.ShowMessageAsync(this, "Error Occurred", $"{ e.Message }."
                                    + Environment.NewLine + "Please inspect logs for further details.");
            }
        }

        /// <summary>
        /// Loads a progress dialog
        /// </summary>
        /// <param name="canCancel"></param>
        /// <returns></returns>
        private async Task<ProgressDialogController> ShowProgress(bool canCancel)
        {
            ProgressDialogController dialogController = await _dialogService.ShowProgressAsync(this, "Downloading Portfolio",
                                                                                "Retrieving all registered devices from the hub.", canCancel);
            dialogController.SetIndeterminate();
            dialogController.SetCancelable(canCancel);

            return dialogController;
        }
        #endregion
    }
}
