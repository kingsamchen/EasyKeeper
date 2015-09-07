/*
 @ 0xCCCCCCCC
*/

using System.Diagnostics;
using System.Security;
using System.Windows;

namespace EasyKeeper {
    public partial class InputPasswordDialog : Window {
        public InputPasswordDialog()
        {
            InitializeComponent();
        }

        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (VaultPasswordBox.SecurePassword.Empty() ||
                ConfirmedVaultPasswordBox.SecurePassword.Empty()) {
                MessageBox.Show((string)FindResource("NoPasswordGiven"), "Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: add support for securestring equality
            if (VaultPasswordBox.SecurePassword.ConvertToUnsecureString() !=
                ConfirmedVaultPasswordBox.SecurePassword.ConvertToUnsecureString()) {
                MessageBox.Show((string)FindResource("PasswordMismatch"), "Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var vm = DataContext as SetupPasswordViewModel;
            Debug.Assert(vm != null, "DataContext != null");
            vm.NewVaultPassword = VaultPasswordBox.SecurePassword;

            DialogResult = true;
        }
    }

    class SetupPasswordViewModel : BindableObject {
        public SecureString NewVaultPassword { get; set; }
    }
}
