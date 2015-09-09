/*
 @ 0xCCCCCCCC
*/

using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace EasyKeeper {
    public partial class EditAccountDialog : Window {
        public EditAccountDialog(EditAccountViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        private void EditDone_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (EditAccountViewModel)DataContext;
            if (string.IsNullOrWhiteSpace(viewModel.Tag) ||
                string.IsNullOrWhiteSpace(viewModel.UserName) ||
                string.IsNullOrWhiteSpace(viewModel.Password)) {
                MessageBox.Show((string)Application.Current.FindResource("AccountInfoIncomplete"),
                                "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!viewModel.TagReadOnly) {
                var cmd = (ExecuteCommand<object, bool>)viewModel.CheckTagExistedCommand;
                cmd.Execute(null);
                if (cmd.Result) {
                    MessageBox.Show((string)Application.Current.FindResource("TagAlreadyExists"),
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            DialogResult = true;
        }
    }

    public class EditAccountViewModel {
        private enum ViewType {
            Add,
            Edit
        }

        private readonly ViewType _type;
        private readonly PasswordVault _vault;
        private ExecuteCommand<object, bool> _checkTagExistedCommand;

        public EditAccountViewModel(PasswordVault vault)
        {
            _type = ViewType.Add;
            _vault = vault;

            DialogTitle = (string)Application.Current.FindResource("AddAccountTitle");
        }

        public EditAccountViewModel(string label, string userName, string password,
                                    PasswordVault vault)
        {
            _type = ViewType.Edit;
            _vault = vault;
            Tag = label;
            UserName = userName;
            Password = password;

            DialogTitle = (string)Application.Current.FindResource("ModifyAccountTitle");
        }

        public string DialogTitle { get; private set; }

        public string Tag { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool TagReadOnly
        {
            get {
                return _type != ViewType.Add;
            }
        }

        // Make sure there is no account info which has the same label as the one we
        // are going to add has, since we regard label as the key of an account info.
        public ICommand CheckTagExistedCommand
        {
            get {
                return _checkTagExistedCommand ??
                      (_checkTagExistedCommand = new ExecuteCommand<object, bool>(param => {
                          return _vault.AsAccountInfoEnumerable().Any(info => info.Label == Tag);
                       }));
            }
        }
    }
}
