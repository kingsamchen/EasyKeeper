/*
 @ 0xCCCCCCCC
*/

using System.Windows;

namespace EasyKeeper {
    /// <summary>
    /// Interaction logic for InputPasswordDialog.xaml
    /// </summary>
    public partial class InputPasswordDialog : Window {
        public string NewVaultPassword { get; private set; }

        public InputPasswordDialog()
        {
            InitializeComponent();
        }

        private void SetupButton_Click(object sender, RoutedEventArgs e)
        {
            if (VaultPasswordBox.Password == string.Empty ||
                ConfirmedVaultPasswordBox.Password == string.Empty) {
                MessageBox.Show("You are supposed to enter you password!", "Invalid Password",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (VaultPasswordBox.Password != ConfirmedVaultPasswordBox.Password) {
                MessageBox.Show("Two passwords don't match!", "Wrong Confirmed Password",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewVaultPassword = VaultPasswordBox.Password;

            DialogResult = true;
        }
    }
}
