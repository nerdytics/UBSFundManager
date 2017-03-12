using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.Messaging.Models.ExchangeData;
using Prism.Logging;
using MahApps.Metro.Controls.Dialogs;
using System;

namespace UBS.FundManager.UI.FundModule.UserControls
{
    /// <summary>
    /// View model for the AddFund view
    /// </summary>
    public class AddFundViewModel : BindableBase
    {
        /// <summary>
        /// Default Injection constructor (always invoked by IoC container)
        /// </summary>
        /// <param name="eventAggregator">Event management service (for publishing and subscribing to events)</param>
        /// <param name="messagingClient">Messaging client for publishing and receiving messages from the AMQP queue</param>
        /// <param name="logger">Logger (logs to file and the event log)</param>
        /// <param name="dialogService">dialog service for showing messages</param>
        public AddFundViewModel(IEventAggregator evtAggregator, IMessagingClient msgClient, 
                ILoggerFacade logger, IDialogCoordinator dialogService)
        {
            _evtAggregator = evtAggregator;
            _messagingClient = msgClient;
            _logger = logger;

            AddFundCommand = new DelegateCommand(OnAddFund, CanAddFund);
        }

        #region Commands
        /// <summary>
        /// Command is triggered when the 'add funds' button is clicked (if enabled)
        /// </summary>
        public DelegateCommand AddFundCommand { get; set; }
        #endregion

        #region Delegate Command Validation Behaviours
        /// <summary>
        /// Basically determines the following:
        ///     - If the 'add funds' button can be enabled
        ///     - Even if enabled, if the designated command can be executed.
        /// by validating the following set of rules:
        ///     - Stock type must have been specified
        ///     - Price must have been specified
        ///     - Quantity purchased must have been specified
        /// </summary>
        /// <returns>flag indicating if action can be executed</returns>
        private bool CanAddFund()
        {
            return (SelectedFundIndex > 0) && (Price > 0.00M) && (Quantity > 0);
        }

        /// <summary>
        /// Handler that publishes newly added stock to an AMQP host.
        /// Triggered when 'add fund' button is clicked (and all relevant data
        /// has been provided).
        /// </summary>
        private void OnAddFund()
        {
            try
            {
                _messagingClient.Publish(new Stock
                {
                    Type = FundsTypeList[SelectedFundIndex].Name,
                    PurchaseInfo = new StockPurchaseInfo { PricePerUnit = Price, QuantityPurchased = Quantity }
                }, TriggerAction.AddFund);

                SelectedFundIndex = 0;
                Price = 0;
                Quantity = 0;
            }
            catch (System.Exception e)
            {
                _logger.Log($"{ GetType().Name }: Error occurred while processing  '{ nameof(OnAddFund) }'" +
                        Environment.NewLine +
                            $"Exception: { e }", Category.Info, Priority.None);

                _dialogService.ShowMessageAsync(this, "Error Occurred", $"{ e.Message }."
                                    + Environment.NewLine + "Please inspect logs for further details.");
            }
        }
        #endregion

        #region Notification Properties
        /// <summary>
        /// Index of selected fund type
        /// </summary>
        private int _selectedFundIndex = 0;
        public int SelectedFundIndex
        {
            get { return _selectedFundIndex; }
            set
            {
                _logger.Log($"Updating the value of { nameof(SelectedFundIndex) } with value: { value }", 
                            Category.Info,
                                Priority.None);
                SetProperty(ref _selectedFundIndex, value);
                AddFundCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Price per unit of stock
        /// </summary>
        private decimal _price = 0.00M;
        public decimal Price
        {
            get { return _price; }
            set
            {
                _logger.Log($"Updating the value of { nameof(Price) } with value: { value }",
                            Category.Info,
                                Priority.None);
                SetProperty(ref _price, value);
                AddFundCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Quantity of stock purchased
        /// </summary>
        private int _quantity = 0;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _logger.Log($"Updating the value of { nameof(Quantity) } with value: { value }", Category.Info, Priority.None);
                SetProperty(ref _quantity, value);
                AddFundCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Default supported list of stocks
        /// </summary>
        private ObservableCollection<FundType> _fundTypesList = new ObservableCollection<FundType>
        {
            new FundType { Name = "Select Fund" },
            new FundType { Name = FundTypeEnum.Equity.ToString() },
            new FundType { Name = FundTypeEnum.Bond.ToString() }
        };

        public ObservableCollection<FundType> FundsTypeList
        {
            get { return _fundTypesList; }
            set
            {
                SetProperty(ref _fundTypesList, value);
            }
        }
        #endregion

        #region Fields
        private IEventAggregator _evtAggregator;
        private IMessagingClient _messagingClient;
        private ILoggerFacade _logger;
        private IDialogCoordinator _dialogService;
        #endregion
    }
}
