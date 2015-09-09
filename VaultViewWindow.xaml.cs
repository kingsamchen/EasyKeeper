/*
 @ 0xCCCCCCCC
*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EasyKeeper {
    public partial class VaultViewWindow : Window {
        private readonly VaultViewModel _viewModel;

        public VaultViewWindow(BindableObject viewModel)
        {
            InitializeComponent();

            _viewModel = (VaultViewModel)viewModel;
            DataContext = viewModel;
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var editAccountViewModel = _viewModel.NewAccountViewModel;
            var editDialog = new EditAccountDialog(editAccountViewModel);
            bool? rv = editDialog.ShowDialog();
            if (rv != true) {
                return;
            }

            _viewModel.NewAccountCommand.Execute(editAccountViewModel);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var editAccountViewModel = _viewModel.ModifyAccountViewModel;
            Debug.Assert(editAccountViewModel != null, "editAccountViewModel != null");
            var editDialog = new EditAccountDialog(editAccountViewModel);
            bool? rv = editDialog.ShowDialog();
            if (rv != true) {
                return;
            }

            _viewModel.ModifyAccountCommand.Execute(editAccountViewModel);
        }
    }

    class EditableConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedId = (int)value;
            return selectedId != -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
