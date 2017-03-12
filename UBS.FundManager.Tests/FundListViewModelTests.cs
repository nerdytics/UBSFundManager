using MahApps.Metro.Controls.Dialogs;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Logging;
using System;
using System.Collections.Generic;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Events;
using UBS.FundManager.Messaging.FundCalculator;
using UBS.FundManager.Messaging.Models.ExchangeData;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.UI.FundModule.UserControls;

namespace UBS.FundManager.Tests
{
    [TestFixture]
    public class FundListViewModelTests
    {
        /// <summary>
        /// Instantiates the dependencies required by the FundListViewModel
        /// </summary>
        [SetUp]
        public void SetupMocks()
        {
            //Setup the mock of the 'DownloadedFundsListEvent' to inject into the event aggregator
            SetupDownloadedFundsListEvent();

            //Setup the mock of the 'NewFundAddedEvent' to inject into the event aggregator
            SetupNewFundAddedEvent();

            //Setup the mock of the 'FundSummaryEvent' to inject into the event aggregator
            SetupFundSummaryEvent();

            //Setup the event aggregator's 'EventAggregator'
            SetupEventAggregator();

            _dialogServiceMock = Mock.Of<IDialogCoordinator>();
            _loggerMock = Mock.Of<ILoggerFacade>();

            _messagingClient = Mock.Of<IMessagingClient>();
            _stockValueCalculator = new IStockValueCalculator[] { Mock.Of<IStockValueCalculator>() };
        }

        /// <summary>
        /// Execute tests to verify if a new fund can be push to an AMQP host.
        /// Also verifies that the 'Publish()' of the 'IMessagingClient' is called.
        /// Should pass.
        /// </summary>
        [Test]
        public void FundListVM_TriggersDownloadRequest_ShouldPass()
        {
            //Arrange view model to test (inject dependencies)
            var addFundVM = new FundListViewModel(_evtAggregatorMock.Object, 
                                _messagingClient, _dialogServiceMock, _stockValueCalculator, _loggerMock);

            //Assert that the messaging client's start() was invoked.
            Mock.Get(_messagingClient).Verify(mc => mc.Start(), Times.Once);

            //Assert that the messaging client's publish() was invoked.
            Mock.Get(_messagingClient).Verify(mc => mc.Publish(
                                        It.IsAny<object>(), It.IsAny<TriggerAction>()), Times.AtLeastOnce);

            //Assert that logger's log() was invoked.
            Mock.Get(_loggerMock).Verify(
                                    logger => logger.Log(It.IsAny<string>(), 
                                                        It.IsAny<Category>(), 
                                                        It.IsAny<Priority>()), 
                                    Times.AtLeastOnce);

            //Assert that event aggregator's downloadedfunds / newfundsaddedevent / fundsummaryevent were
            //subscribed to an instantiation of the FundListViewModel.
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<DownloadedFundsListEvent>(), Times.Once);
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<NewFundAddedEvent>(), Times.Once);
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<FundSummaryEvent>(), Times.Once);
        }

        #region Helper methods
        /// <summary>
        /// Instantiates the event aggregator and sets up the
        /// events listened to.
        /// </summary>
        private void SetupEventAggregator()
        {
            _evtAggregatorMock = new Mock<IEventAggregator>();
            _evtAggregatorMock.Setup(evt => evt.GetEvent<DownloadedFundsListEvent>())
                              .Returns(_dlFundsEventMock.Object);

            _evtAggregatorMock.Setup(evt => evt.GetEvent<NewFundAddedEvent>())
                              .Returns(_newFundEventMock.Object);

            _evtAggregatorMock.Setup(evt => evt.GetEvent<FundSummaryEvent>())
                              .Returns(_fundSummaryEventMock.Object);
        }

        /// <summary>
        /// Instantiates the fundsummaryevent mock and sets up the
        /// callback returned to the eventaggregator when event is invoked.
        /// </summary>
        private void SetupFundSummaryEvent()
        {
            _fundSummaryEventMock = new Mock<FundSummaryEvent>();
            _fundSummaryEventMock.Setup(p => p.Subscribe(
                It.IsAny<Action<FundSummaryData>>(),
                It.IsAny<ThreadOption>(),
                It.IsAny<bool>(),
                It.IsAny<Predicate<FundSummaryData>>()))
                    .Callback<Action<FundSummaryData>, ThreadOption, bool, Predicate<FundSummaryData>>
                        ((newFundSummary, threadOption, keepReferenceAlive, filter) =>
                            _fundSummaryEventArgsCallback = newFundSummary);
        }

        /// <summary>
        /// Instantiates the newfundaddedvent mock and sets up the
        /// callback returned to the eventaggregator when event is invoked.
        /// </summary>
        private void SetupNewFundAddedEvent()
        {
            _newFundEventMock = new Mock<NewFundAddedEvent>();
            _newFundEventMock.Setup(p => p.Subscribe(
                It.IsAny<Action<Fund>>(),
                It.IsAny<ThreadOption>(),
                It.IsAny<bool>(),
                It.IsAny<Predicate<Fund>>()))
                    .Callback<Action<Fund>, ThreadOption, bool, Predicate<Fund>>
                        ((newFund, threadOption, keepReferenceAlive, filter) => _newFundEventArgsCallback = newFund);
        }

        /// <summary>
        /// Instantiates the downloadedfundslistevent mock and sets up the
        /// callback returned to the eventaggregator when event is invoked.
        /// </summary>
        private void SetupDownloadedFundsListEvent()
        {
            _dlFundsEventMock = new Mock<DownloadedFundsListEvent>();
            _dlFundsEventMock.Setup(p => p.Subscribe(
                It.IsAny<Action<IEnumerable<Fund>>>(),
                It.IsAny<ThreadOption>(),
                It.IsAny<bool>(),
                It.IsAny<Predicate<IEnumerable<Fund>>>()))
                    .Callback<Action<IEnumerable<Fund>>, ThreadOption, bool, Predicate<IEnumerable<Fund>>>
                        ((downloadedFunds, threadOption, keepReferenceAlive, filter) =>
                            _dlEventArgsCallback = downloadedFunds);
        }
        #endregion

        #region Fields
        private Action<IEnumerable<Fund>> _dlEventArgsCallback;
        private Mock<DownloadedFundsListEvent> _dlFundsEventMock;

        private Action<Fund> _newFundEventArgsCallback;
        private Mock<NewFundAddedEvent> _newFundEventMock;

        private Action<FundSummaryData> _fundSummaryEventArgsCallback;
        private Mock<FundSummaryEvent> _fundSummaryEventMock;

        private Mock<IEventAggregator> _evtAggregatorMock;
        private IDialogCoordinator _dialogServiceMock;

        private ILoggerFacade _loggerMock;
        private IMessagingClient _messagingClient;
        private IStockValueCalculator[] _stockValueCalculator;
        #endregion
    }
}
