using MahApps.Metro.Controls.Dialogs;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBS.FundManager.Common.Helpers;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Events;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.UI.FundModule.UserControls;

namespace UBS.FundManager.Tests
{
    [TestFixture]
    public class FundSummaryViewModelTests
    {
        /// <summary>
        /// Instantiates the dependencies required by the FundListViewModel
        /// </summary>
        [SetUp]
        public void SetupMocks()
        {
            //Setup the mock of the 'DownloadedFundsListEvent' to inject into the event aggregator
            SetupDownloadedFundsListEvent();

            //Setup the mock of the 'FundSummaryEvent' to inject into the event aggregator
            SetupFundSummaryEvent();

            //Setup the event aggregator's 'EventAggregator'
            SetupEventAggregator();

            _dialogServiceMock = Mock.Of<IDialogCoordinator>();
            _loggerMock = Mock.Of<ILoggerFacade>();
            _chartDialog = new Mock<EnlargeChartDialog>();
        }

        /// <summary>
        /// Execute tests to verify if a new fund can be push to an AMQP host.
        /// Also verifies that the 'Publish()' of the 'IMessagingClient' is called.
        /// Should pass.
        /// </summary>
        [Test]
        public void FundSummaryVM_SummaryData_ShouldPass()
        {
            //Arrange view model to test (inject dependencies)
            var fundSummaryVM = new FundSummaryViewModel(_evtAggregatorMock.Object,
                                        _dialogServiceMock, _loggerMock, null);

            // Assert that event aggregator's downloadedfunds / fundsummaryevent were
            // subscribed to on instantiation of the FundSummaryViewModel.
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<DownloadedFundsListEvent>(), Times.Once);
            Mock.Get(_evtAggregatorMock.Object).Verify(evt => evt.GetEvent<FundSummaryEvent>(), Times.Once);

            // Verify that the grid data is empty
            Assert.IsTrue(fundSummaryVM.EquityGridData.Count == 0);
            Assert.IsTrue(fundSummaryVM.BondGridData.Count == 0);
            Assert.IsTrue(fundSummaryVM.AllStocksGridData.Count == 0);

            // Simulate the publish of downloadedfundslist event
            var asJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFundList.json"));
            IEnumerable<Fund> testFundsList = asJson.Deserialise<IEnumerable<Fund>>();
            _dlEventArgsCallback(testFundsList);

            // Assert that the grid data now has values
            Assert.IsTrue(fundSummaryVM.EquityGridData.Count > 0);
            Assert.IsTrue(fundSummaryVM.BondGridData.Count > 0);
            Assert.IsTrue(fundSummaryVM.AllStocksGridData.Count > 0);
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

        private Action<FundSummaryData> _fundSummaryEventArgsCallback;
        private Mock<FundSummaryEvent> _fundSummaryEventMock;

        private Mock<IEventAggregator> _evtAggregatorMock;
        private IDialogCoordinator _dialogServiceMock;

        private ILoggerFacade _loggerMock;
        private Mock<EnlargeChartDialog> _chartDialog;
        #endregion
    }
}
