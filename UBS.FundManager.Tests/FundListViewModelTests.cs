using MahApps.Metro.Controls.Dialogs;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UBS.FundManager.Common.Helpers;
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

            _dialogServiceMock = new Mock<IDialogCoordinator>();
            _dialogServiceMock.Setup(dlg => dlg.ShowProgressAsync(It.IsAny<object>(),
                                                                    It.IsAny<string>(),
                                                                    It.IsAny<string>(),
                                                                    It.IsAny<bool>(),
                                                                    It.IsAny<MetroDialogSettings>()))
                              .Returns(() => Task.FromResult(Mock.Of<ProgressDialogController>()));

            _loggerMock = Mock.Of<ILoggerFacade>();
            _messagingClient = Mock.Of<IMessagingClient>();

            // Setup stockvalue calculator
            _fundSummaryDataMock = Mock.Of<FundSummaryData>();
            Mock<IStockValueCalculator> calculatorMock = new Mock<IStockValueCalculator>();

            calculatorMock.Setup(c => c.Calculate(ref _fundSummaryDataMock, It.IsAny<Fund>()))
                          .Returns((FundSummaryData fd, Fund f) => f);

            _stockValueCalculator = new IStockValueCalculator[] { calculatorMock.Object };
        }

        /// <summary>
        /// Execute tests to verify that a downloaded funds event can be received, 
        /// and that the event subsequently populates the funds portfolio grid by 
        /// instantiating the DownloadedFundsList property.
        /// Also verifies that the 'Log()' of the 'Logger' is called.
        /// Should pass.
        /// </summary>
        [Test]
        public void FundListVM_TriggersDownloadRequest_ShouldPass()
        {
            //Arrange view model to test (inject dependencies)
            var fundListVM = new FundListViewModel(_evtAggregatorMock.Object, 
                                _messagingClient, null, _stockValueCalculator, _loggerMock);

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

            // Asserts that the DownloadedFundsList (notification property storing the downloaded
            // funds list.
            Assert.That(fundListVM.DownloadedFundsList == null);

            // Simulate the publish of downloadedfundslist event
            var asJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFundList.json"));
            IEnumerable<Fund> testFundsList = asJson.Deserialise<IEnumerable<Fund>>();
            _dlEventArgsCallback(testFundsList);

            // Assert that the DownloadedFunds list (datasource for the portfolio grid) now had data
            Assert.That(fundListVM.DownloadedFundsList.Count > 0);
            Assert.That(fundListVM.DownloadedFundsList.Count == testFundsList.Count());

            // Assert that a log was written
            //Assert that logger's log() was invoked.
            Mock.Get(_loggerMock).Verify(
                                    logger => logger.Log(It.IsAny<string>(),
                                                        It.IsAny<Category>(),
                                                        It.IsAny<Priority>()),
                                    Times.AtLeastOnce);
        }

        /// <summary>
        /// Execute tests to verify that a new fund (stock) added event can be
        /// received. And subsequently added to the 'DownloadedFundsList' property 
        /// of the viewmodel (Basically test that when a new fund is received, its added to 
        /// the funds portfolio grid). Should pass
        /// </summary>
        [Test]
        public void FundListVM_NewFundsAdded_ShouldPass()
        {
            //Arrange view model to test (inject dependencies)
            var fundListVM = new FundListViewModel(_evtAggregatorMock.Object,
                                _messagingClient, null, _stockValueCalculator, _loggerMock);

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

            //Assert that event aggregator's newfundsaddedevent was
            //subscribed to on instantiation of the FundListViewModel.
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<NewFundAddedEvent>(), Times.Once);

            // Asserts that the DownloadedFundsList (notification property storing the downloaded
            // funds list.
            Assert.That(fundListVM.DownloadedFundsList == null);

            // Simulate the publish of downloadedfundslist event
            var asJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFundList.json"));
            IEnumerable<Fund> testFundsList = asJson.Deserialise<IEnumerable<Fund>>();
            _dlEventArgsCallback(testFundsList);

            // Assert that the DownloadedFunds list (datasource for the portfolio grid) now had data
            Assert.That(fundListVM.DownloadedFundsList.Count > 0);
            Assert.That(fundListVM.DownloadedFundsList.Count == testFundsList.Count());

            // Verify that DownloadedFundsList has existing data
            // Also the fundSummaryData field should not have values (this is a private field so we can't test it
            // but if this assumption is correct, then the newfund added event below will pass)
            Assert.That(fundListVM.DownloadedFundsList.Count > 0);
            Assert.That(fundListVM.DownloadedFundsList.Count == testFundsList.Count());

            // Simulate new fund added event
            fundListVM.FundSummaryData = _fundSummaryDataMock;
            _newFundEventArgsCallback(testFundsList.First());

            // Assert that DownloadedFundsList has been updated with new fund
            Assert.That(fundListVM.DownloadedFundsList.Count == (testFundsList.Count() + 1));

            // Assert that FundSummary event was broadcasted
            // Testing GetEvent was called twice because 1. was the subscription event
            // while 2. will be the publish fundsummary event.
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => 
                            evt.GetEvent<FundSummaryEvent>(), Times.Exactly(2));

            // Assert that a log was written
            //Assert that logger's log() was invoked.
            Mock.Get(_loggerMock).Verify(
                                    logger => logger.Log(It.IsAny<string>(),
                                                        It.IsAny<Category>(),
                                                        It.IsAny<Priority>()),
                                    Times.AtLeastOnce);
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
        private FundSummaryData _fundSummaryDataMock;

        private Mock<IEventAggregator> _evtAggregatorMock;
        private Mock<IDialogCoordinator> _dialogServiceMock;

        private ILoggerFacade _loggerMock;
        private IMessagingClient _messagingClient;
        private IStockValueCalculator[] _stockValueCalculator;
        #endregion
    }
}
