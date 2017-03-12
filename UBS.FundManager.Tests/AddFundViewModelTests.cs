using MahApps.Metro.Controls.Dialogs;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Logging;
using UBS.FundManager.Messaging;
using UBS.FundManager.Messaging.Models.ExchangeData;
using UBS.FundManager.Messaging.Models.Fund;
using UBS.FundManager.UI.FundModule.UserControls;

namespace UBS.FundManager.Tests
{
    /// <summary>
    /// Test for the AddFundViewModel
    /// </summary>
    [TestFixture]
    public class AddFundViewModelTests
    {
        /// <summary>
        /// Instantiates the dependencies required by the AddFundViewModel
        /// </summary>
        [SetUp]
        public void SetupMocks()
        {
            _evtAggregatorMock = Mock.Of<IEventAggregator>();
            _dialogServiceMock = Mock.Of<IDialogCoordinator>();
            _loggerMock = Mock.Of<ILoggerFacade>();
            _messagingClient = Mock.Of<IMessagingClient>();
        }

        /// <summary>
        /// Execute tests to verify if a new fund can be pushed
        /// to an AMQP host (should fail as required values are not 
        /// provided)
        /// </summary>
        [Test]
        public void AddFundCommand_CanExecute_ShouldFail()
        {
            //Arrange view model to test (inject dependencies)
            var addFundVM = new AddFundViewModel(_evtAggregatorMock, 
                                    _messagingClient, _loggerMock, _dialogServiceMock);

            //Assert that addFundCommand cannot be triggered
            Assert.That(addFundVM != null);
            Assert.IsFalse(addFundVM.AddFundCommand.CanExecute());
        }

        /// <summary>
        /// Execute tests to verify if a new fund can be push to an AMQP host.
        /// Also verifies that the 'Publish()' of the 'IMessagingClient' is called.
        /// Should pass.
        /// </summary>
        [Test]
        public void AddFundCommand_CanExecute_ShouldPass()
        {
            //Arrange view model to test (inject dependencies)
            var addFundVM = new AddFundViewModel(_evtAggregatorMock,
                                    _messagingClient, _loggerMock, _dialogServiceMock);

            //Set up properties (to compose a stock object)
            addFundVM.SelectedFundIndex = 1;
            addFundVM.Price = 5.0M;
            addFundVM.Quantity = 2;

            //Assert that addFundCommand can be triggered
            Assert.That(addFundVM != null);
            Assert.IsTrue(addFundVM.AddFundCommand.CanExecute());

            //Simulate 'add fund' button click
            addFundVM.AddFundCommand.Execute();

            //Assert that the messaging client's publish method was invoked.
            Mock.Get(_messagingClient).Verify(mc =>
                    mc.Publish(It.IsAny<Stock>(), It.IsAny<TriggerAction>()), Times.Once);
        }

        #region Fields
        private IEventAggregator _evtAggregatorMock;
        private IDialogCoordinator _dialogServiceMock;
        private ILoggerFacade _loggerMock;
        private IMessagingClient _messagingClient;
        #endregion
    }
}
