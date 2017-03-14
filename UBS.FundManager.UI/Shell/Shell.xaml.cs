using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Windows;
using UBS.FundManager.Messaging;

namespace UBS.FundManager.UI.Shell
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : MetroWindow
    {
        public Shell(IMessagingClient messagingClient, IDialogCoordinator dialogService)
        {
            _messagingClient = messagingClient;
            _dialogService = dialogService;

            InitializeComponent();
            this.Closing += Shell_Closing;
            this.Closed += Shell_Closed;
        }

        /// <summary>
        /// Terminate application process and pass the exit code to the OS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shell_Closed(object sender, System.EventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Dispose resources while application is shutting down..
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shell_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _messagingClient.Dispose();
            Application.Current.Shutdown();
        }

        private IMessagingClient _messagingClient;
        private IDialogCoordinator _dialogService;
    }
}
