using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace UBS.FundManager.UI.Shell
{
    public interface IShellViewModel { }

    public class ShellViewModel : BindableBase, IShellViewModel
    {
        private IDialogCoordinator _dialogService;

        public ShellViewModel(IDialogCoordinator dialogService)
        {
            _dialogService = dialogService;
        }
    }
}
