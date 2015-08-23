/*
 @ 0xCCCCCCCC
*/

using System.Windows;

namespace EasyKeeper {
    public partial class VaultViewWindow : Window {
        public VaultViewWindow(VaultViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }
    }
}
