using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
        }

        private void Shell_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _messagingClient.Dispose();
            Application.Current.Shutdown();
            //MessageDialogResult dialogResult = await _dialogService.ShowMessageAsync(this, "Shutting Down",
            //                                                            "Are you sure about shutting down?", MessageDialogStyle.AffirmativeAndNegative);

            //if (dialogResult == MessageDialogResult.Affirmative)
            //{
            //    _messagingClient.Dispose();
            //    Application.Current.Shutdown();
            //}
        }

        private IMessagingClient _messagingClient;
        private IDialogCoordinator _dialogService;
    }
}
