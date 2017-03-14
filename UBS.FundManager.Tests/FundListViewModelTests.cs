using MahApps.Metro.Controls.Dialogs;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Logging;
using System;
using System.Collections.Generic;
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
            SetupDownloadedFundsListEvent();
            SetupNewFundAddedEvent();
            SetupFundListViewSummaryEvent();

            SetupFundSummaryEvent();
            SetupEventAggregator();
            SetupDialogServiceMock();

            SetupMessagingClientMock();
            SetupStockValueCalculatorMock();
            _loggerMock = Mock.Of<ILoggerFacade>();
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
                                _messagingClientMock.Object, null, _stockValueCalculator, _loggerMock);

            //Messaging client's start() was invoked.
            Mock.Get(_messagingClientMock.Object).Verify(mc => mc.Start(), Times.Once);

            //Messaging client's publish() was invoked.
            Mock.Get(_messagingClientMock.Object).Verify(mc => mc.Publish(
                                        It.IsAny<object>(), It.IsAny<TriggerAction>()), Times.AtLeastOnce);

            //Assert that all events have been subscribed to
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<DownloadedFundsListEvent>(), Times.Once);
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<NewFundAddedEvent>(), Times.Once);
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<FundsListViewSummaryEvent>(), Times.Once);

            // Asserts that the DownloadedFundsList is empty
            Assert.That(fundListVM.DownloadedFundsList.Count == 0);

            // Simulate the publish of downloadedfundslist event
            var asJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFundList.json"));
            IEnumerable<Fund> testFundsList = asJson.Deserialise<IEnumerable<Fund>>();
            _dlEventArgsCallback(testFundsList);

            // Assert that the DownloadedFunds list (datasource for the portfolio grid) now has data
            Assert.That(fundListVM.DownloadedFundsList.Count > 0);
            Assert.That(fundListVM.DownloadedFundsList.Count == testFundsList.Count());

            // Assert that a log was written
            Mock.Get(_loggerMock).Verify(
                                    logger => logger.Log(It.IsAny<string>(),
                                                        It.IsAny<Category>(),
                                                        It.IsAny<Priority>()),
                                    Times.AtLeastOnce);

            // Assert that FundSummaryViewModel was pinged the downloaded funds
            Mock.Get(_fundSummaryDListEventsMock.Object).Verify(
                                    evt => evt.Publish(It.IsAny<IEnumerable<Fund>>()), Times.AtLeastOnce);
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
                                _messagingClientMock.Object, null, _stockValueCalculator, _loggerMock);

            //Assert that the messaging client's start() was invoked.
            Mock.Get(_messagingClientMock.Object).Verify(mc => mc.Start(), Times.Once);

            //Assert that the messaging client's publish() was invoked.
            Mock.Get(_messagingClientMock.Object).Verify(mc => mc.Publish(
                                        It.IsAny<object>(), It.IsAny<TriggerAction>()), Times.AtLeastOnce);

            //Assert that 'NewFundAddedEvent' has been subscribed to
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<NewFundAddedEvent>(), Times.Once);

            // Asserts that the DownloadedFundsList (notification property storing the downloaded funds list) is empty.
            Assert.That(fundListVM.DownloadedFundsList.Count == 0);

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
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => 
                            evt.GetEvent<FundSummaryEvent>(), Times.Once);

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

            _evtAggregatorMock.Setup(evt => evt.GetEvent<FundsListViewSummaryEvent>())
                              .Returns(_fundListViewSummaryEventMock.Object);

            _evtAggregatorMock.Setup(evt => evt.GetEvent<FundSummaryEvent>())
                              .Returns(_fundSummaryEventMock.Object);

            _evtAggregatorMock.Setup(evt => evt.GetEvent<FundSummaryDownloadListEvent>())
                              .Returns(_fundSummaryDListEventsMock.Object);

            _evtAggregatorMock.Setup(evt => evt.GetEvent<FundsListViewSummaryEvent>())
                              .Returns(_fundsListVSummaryEventMock.Object);
        }

        /// <summary>
        /// Instantiates the FundsListViewSummaryEvent mock and sets up the
        /// callback returned to the eventaggregator when event is invoked.
        /// </summary>
        private void SetupFundListViewSummaryEvent()
        {
            _fundListViewSummaryEventMock = new Mock<FundsListViewSummaryEvent>();
            _fundListViewSummaryEventMock.Setup(p => p.Publish(It.IsAny<FundSummaryData>()))
                     .Callback(() => { });
            //_fundListViewSummaryEventMock.Setup(p => p.Subscribe(
            //    It.IsAny<Action<FundSummaryData>>(),
            //    It.IsAny<ThreadOption>(),
            //    It.IsAny<bool>(),
            //    It.IsAny<Predicate<FundSummaryData>>()))
            //        .Callback<Action<FundSummaryData>, ThreadOption, bool, Predicate<FundSummaryData>>
            //            ((newFundSummary, threadOption, keepReferenceAlive, filter) =>
            //                _fundSummaryEventArgsCallback = newFundSummary);
        }

        /// <summary>
        /// Mocks the FundSummaryEvent
        /// </summary>
        private void SetupFundSummaryEvent()
        {
            _fundSummaryEventMock = new Mock<FundSummaryEvent>();
            _fundSummaryEventMock.Setup(p => p.Publish(It.IsAny<FundSummaryData>()))
                     .Callback(() => { });

            _fundSummaryDListEventsMock = new Mock<FundSummaryDownloadListEvent>();
            _fundListViewSummaryEventMock.Setup(p => p.Publish(It.IsAny<FundSummaryData>()))
                                         .Callback(() => { });

            _fundsListVSummaryEventMock = new Mock<FundsListViewSummaryEvent>();
            _fundListViewSummaryEventMock.Setup(p => p.Publish(It.IsAny<FundSummaryData>()))
                             .Callback(() => { });
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

        /// <summary>
        /// Instantiates the test stock value calculators
        /// </summary>
        private void SetupStockValueCalculatorMock()
        {
            _fundSummaryDataMock = Mock.Of<FundSummaryData>();
            Mock<IStockValueCalculator> calculatorMock = new Mock<IStockValueCalculator>();

            calculatorMock.Setup(c => c.Calculate(ref _fundSummaryDataMock, It.IsAny<Fund>()))
                          .Returns((FundSummaryData fd, Fund f) => f);

            _stockValueCalculator = new IStockValueCalculator[] { calculatorMock.Object };
        }

        /// <summary>
        /// Mocks the dialog service
        /// </summary>
        private void SetupDialogServiceMock()
        {
            _dialogServiceMock = new Mock<IDialogCoordinator>();
            _dialogServiceMock.Setup(dlg => dlg.ShowProgressAsync(It.IsAny<object>(),
                                                                    It.IsAny<string>(),
                                                                    It.IsAny<string>(),
                                                                    It.IsAny<bool>(),
                                                                    It.IsAny<MetroDialogSettings>()))
                              .Returns(() => Task.FromResult(Mock.Of<ProgressDialogController>()));
        }

        /// <summary>
        /// Mocks the messaging client start and publish methods
        /// </summary>
        private void SetupMessagingClientMock()
        {
            _messagingClientMock = new Mock<IMessagingClient>();
            _messagingClientMock.Setup(mc => mc.Start()).Callback(() => { });
            _messagingClientMock.Setup(mc => mc.Publish(It.IsAny<object>(), It.IsAny<TriggerAction>()))
                                .Callback(() => { });
        }
        #endregion

        #region Fields
        private Action<IEnumerable<Fund>> _dlEventArgsCallback;
        private Mock<DownloadedFundsListEvent> _dlFundsEventMock;

        private Action<Fund> _newFundEventArgsCallback;
        private Mock<NewFundAddedEvent> _newFundEventMock;
        private Action<FundSummaryData> _fundSummaryEventArgsCallback;
        private Mock<FundSummaryEvent> _fundSummaryEventMock;

        private Mock<FundsListViewSummaryEvent> _fundsListVSummaryEventMock;
        private Mock<FundsListViewSummaryEvent> _fundListViewSummaryEventMock;
        private Mock<FundSummaryDownloadListEvent> _fundSummaryDListEventsMock;
        private FundSummaryData _fundSummaryDataMock;

        private Mock<IEventAggregator> _evtAggregatorMock;
        private Mock<IDialogCoordinator> _dialogServiceMock;

        private ILoggerFacade _loggerMock;
        private Mock<IMessagingClient> _messagingClientMock;
        private IStockValueCalculator[] _stockValueCalculator;
        #endregion
    }
}
