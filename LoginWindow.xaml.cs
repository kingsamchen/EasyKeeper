/*
 @ 0xCCCCCCCC
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Win32 = Microsoft.Win32;

namespace EasyKeeper {
    public partial class LoginWindow : Window {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
        }

        private void OpenVault_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedItemIndex == -1) {
                MessageBox.Show((string)FindResource("NoVaultSelected"), "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Password.SecurePassword.Empty()) {
                MessageBox.Show((string)FindResource("NoPasswordGiven"), "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Debug.Assert(_viewModel.SelectedVaultPath != string.Empty, "SelectedPath != Empty");
            if (!File.Exists(_viewModel.SelectedVaultPath)) {
                var result = MessageBox.Show((string)FindResource("VaultNotFound"), "Error",
                                             MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes &&
                    _viewModel.RemoveLocationCommand.CanExecute(null)) {
                    _viewModel.RemoveLocationCommand.Execute(null);
                }

                _viewModel.SelectedItemIndex = -1;
                Password.Clear();

                return;
            }

            try {
                var cmd = (ExecuteCommand<SecureString, BindableObject>)_viewModel.OpenVaultCommand;
                if (!cmd.CanExecute(Password.SecurePassword)) {
                    return;
                }

                cmd.Execute(Password.SecurePassword);
                Debug.Assert(cmd.Result != null, "cmd.Result != null");
                var vaultViewer = new VaultViewWindow(cmd.Result);
                vaultViewer.Show();
                Close();
            } catch (IncorrectPassword ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Password.Clear();
            } catch (DataCorruptedException ex) {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewVault_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Win32.SaveFileDialog {
                DefaultExt = ".ekp",
                Filter = "EasyKeeper vault (*.ekp)|*.ekp"
            };

            bool? result = dlg.ShowDialog();
            if (result != true) {
                return;
            }

            var newVaultPath = dlg.FileName;

            var vm = new SetupPasswordViewModel();
            var dialog = new InputPasswordDialog { DataContext = vm };
            result = dialog.ShowDialog();
            if (result != true) {
                return;
            }

            var newVaultSecurePwd = vm.NewVaultPassword;

            var cmd = (ExecuteCommand<Tuple<string, SecureString>,
                                      BindableObject>)_viewModel.NewVaultCommand;
            var info = Tuple.Create(newVaultPath, newVaultSecurePwd);
            if (!_viewModel.NewVaultCommand.CanExecute(info)) {
                return;
            }

            _viewModel.NewVaultCommand.Execute(info);
            Debug.Assert(cmd.Result != null, "cmd.Result != null");
            var vaultViewer = new VaultViewWindow(cmd.Result);
            vaultViewer.Show();
            Close();
        }

        private void VaultLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When `choose a location` item is selected, display an open-file dialog for
            // choosing another vault.
            if (_viewModel.SelectedItemIndex != ((ComboBox)sender).Items.Count - 1) {
                return;
            }

            var dlg = new Win32.OpenFileDialog {
                DefaultExt = ".ekp",
                Filter = "EasyKeeper vault (*.ekp)|*.ekp"
            };

            bool? result = dlg.ShowDialog();
            if (result != true) {
                _viewModel.SelectedItemIndex = -1;
                return;
            }

            _viewModel.AddVaultLocationCommand.Execute(dlg.FileName);
        }
    }

    class LoginViewModel : BindableObject {
        private static readonly string HistoryFilePath;
        private List<string> _vaultLocations = new List<string>();
        private ObservableCollection<ComboBoxItem> _locationItems;
        private int _selectedItemIndex = -1;
        private ExecuteCommand<string, object> _addVaultLocationCommand;
        private ExecuteCommand<object, object> _removeGivenLocationCommand;
        private ExecuteCommand<SecureString, BindableObject> _openVaultCommand;
        private ExecuteCommand<Tuple<string, SecureString>, BindableObject> _newVaultCommand;

        static LoginViewModel()
        {
            var appDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            HistoryFilePath = Path.Combine(appDir, @"EasyKeeper\vault_history");
        }

        public LoginViewModel()
        {
            LoadVaultLocationFromHistory();
            _locationItems = new ObservableCollection<ComboBoxItem>(GetLocationItem());

            // If there are locations in history, display the first of them.
            if (_vaultLocations.Count != 0) {
                SelectedItemIndex = 0;
            }
        }

        public ObservableCollection<ComboBoxItem> LocationItems
        {
            get {
                return _locationItems;
            }
        }

        public int SelectedItemIndex
        {
            get {
                return _selectedItemIndex;
            }

            set {
                _selectedItemIndex = value;
                RaisePropertyChanged();
            }
        }

        public string SelectedVaultPath
        {
            get {
                if (_selectedItemIndex == -1 || _selectedItemIndex + 1 == _locationItems.Count) {
                    return string.Empty;
                }

                return (string)_locationItems[_selectedItemIndex].Content;
            }
        }

        public ICommand AddVaultLocationCommand
        {
            get {
                return _addVaultLocationCommand ??
                      (_addVaultLocationCommand = new ExecuteCommand<string, object>(location => {
                           var index = 0;
                           for (; index < _locationItems.Count; ++index) {
                               if (_locationItems[index].Content.Equals(location)) {
                                   break;
                               }
                           }

                           // Add the chosen location if there is no such item.
                           if (index == _locationItems.Count) {
                               _locationItems.Insert(0, new ComboBoxItem { Content = location });
                               index = 0;
                           }

                           SelectedItemIndex = index;

                           return null;
                       }));
            }
        }

        public ICommand RemoveLocationCommand
        {
            get {
                return _removeGivenLocationCommand ??
                      (_removeGivenLocationCommand = new ExecuteCommand<object, object>(x => {
                           // This location may not in the history.
                           if (_vaultLocations.Remove(SelectedVaultPath)) {
                               SaveVaultLocationHistory();
                           }

                           // Removing the item from `_locationItems` after trying to remove from
                           // `_vaultLocations` is crutial, because change of `SelectedItemIndex`
                           // would also make `SelectedVaultPath` changed.
                           _locationItems.RemoveAt(SelectedItemIndex);

                           return null;
                       }, x => SelectedVaultPath != string.Empty));
            }
        }

        public ICommand OpenVaultCommand
        {
            get {
                return _openVaultCommand ??
                      (_openVaultCommand = new ExecuteCommand<SecureString, BindableObject>(
                           securePwd => {
                               var selectedPath = SelectedVaultPath;

                               MoveVaultLocationToTop(selectedPath);
                               _locationItems.Move(SelectedItemIndex, 0);
                               SelectedItemIndex = 0;

                               SaveVaultLocationHistory();

                               var password = securePwd.ConvertToUnsecureString();
                               var passwordVault = VaultLoader.FromProvided(selectedPath, password);

                               return new VaultViewModel(passwordVault);
                       }, securePwd => !securePwd.Empty() && SelectedVaultPath != string.Empty));
            }
        }

        public ICommand NewVaultCommand
        {
            get {
                return _newVaultCommand ??
                      (_newVaultCommand = new ExecuteCommand<Tuple<string, SecureString>,
                                                             BindableObject>(vaultInfo => {
                           var path = vaultInfo.Item1;
                           _vaultLocations.Insert(0, path);
                           _locationItems.Insert(0, new ComboBoxItem { Content = path });
                           SelectedItemIndex = 0;

                           SaveVaultLocationHistory();

                           var password = vaultInfo.Item2.ConvertToUnsecureString();
                           var passwordVault = VaultLoader.FromNew(path, password);

                           return new VaultViewModel(passwordVault);
                       }, info => info.Item1 != string.Empty && !info.Item2.Empty()));
            }
        }

        private void LoadVaultLocationFromHistory()
        {
            if (File.Exists(HistoryFilePath)) {
                _vaultLocations.Clear();
                _vaultLocations.AddRange(File.ReadLines(HistoryFilePath, Encoding.UTF8));
            }
        }

        private IEnumerable<ComboBoxItem> GetLocationItem()
        {
            var items = from location in _vaultLocations
                        select new ComboBoxItem { Content = location };
            var chooseLocationItem = new ComboBoxItem {
                FontStyle = FontStyles.Italic,
                Content = Application.Current.FindResource("ChooseLocation")
            };

            return items.Concat(new[] { chooseLocationItem });
        }

        // We always overwrite vault location history.
        private void SaveVaultLocationHistory()
        {
            if (_vaultLocations.Count == 0) {
                return;
            }

            Task.Run(() => {
                var appDataDir = Path.GetDirectoryName(HistoryFilePath);
                Debug.Assert(appDataDir != null, "appDataDir cannot be null");
                if (!Directory.Exists(appDataDir)) {
                    Directory.CreateDirectory(appDataDir);
                }

                File.WriteAllLines(HistoryFilePath, _vaultLocations, Encoding.UTF8);
            });
        }

        private void MoveVaultLocationToTop(string selectedPath)
        {
            _vaultLocations.Remove(selectedPath);
            _vaultLocations.Insert(0, selectedPath);
        }
    }
}