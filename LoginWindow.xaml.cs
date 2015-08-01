/*
 @ 0xCCCCCCCC
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Win32 = Microsoft.Win32;


namespace EasyKeeper {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, INotifyPropertyChanged {
        private static readonly string HistoryFilePath;
        private List<string> _vaultLocations;
        private readonly ObservableCollection<ComboBoxItem> _locationBoxItems;
        private int _selectedLocationIndex = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        public int SelectedLocationItemIndex
        {
            get {
                return _selectedLocationIndex;
            }

            set {
                _selectedLocationIndex = value;
                OnPropertyChanged();
            }
        }

        static LoginWindow()
        {
            var appDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            HistoryFilePath = Path.Combine(appDir, @"EasyKeeper\vault_history");
        }

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = this;

            LoadVaultLocationFromHistory();
            var locationItems = from location in _vaultLocations
                                select new ComboBoxItem { Content = location };
            _locationBoxItems = new ObservableCollection<ComboBoxItem>(locationItems);
            var chooseLocationItem = new ComboBoxItem { FontStyle = FontStyles.Italic,
                                                        Content = FindResource("ChooseLocation") };
            _locationBoxItems.Add(chooseLocationItem);
            VaultLocationList.ItemsSource = _locationBoxItems;

            // If there are locations in history, display the first of them.
            if (_vaultLocations.Count != 0) {
                SelectedLocationItemIndex = 0;
            }
        }

        private void VaultLocationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var locationBox = ((ComboBox)sender);
            // When `choose a location` item is selected, display an open-file dialog for
            // choosing another vault.
            if (SelectedLocationItemIndex == locationBox.Items.Count - 1) {
                var dlg = new Win32.OpenFileDialog {
                    DefaultExt = ".ekp",
                    Filter = "EasyKeeper vault (*.ekp)|*.ekp"
                };

                bool? result = dlg.ShowDialog();
                if (result != true) {
                    SelectedLocationItemIndex = -1;
                    return;
                }

                var index = 0;
                for (; index < _locationBoxItems.Count; ++index) {
                    if (_locationBoxItems[index].Content.Equals(dlg.FileName)) {
                        break;
                    }
                }

                if (index == _locationBoxItems.Count) {
                    _locationBoxItems.Insert(0, new ComboBoxItem { Content = dlg.FileName });
                    index = 0;
                }

                SelectedLocationItemIndex = index;
            }
        }

        private void LoadVaultLocationFromHistory()
        {
            _vaultLocations = new List<string>();
            if (File.Exists(HistoryFilePath)) {
                _vaultLocations.AddRange(File.ReadLines(HistoryFilePath, Encoding.UTF8));
            }
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

            var setupPasswordDlg = new InputPasswordDialog();
            result = setupPasswordDlg.ShowDialog();
            if (result != true) {
                return;
            }

            var newVaultPassword = setupPasswordDlg.NewVaultPassword;

            _vaultLocations.Insert(0, newVaultPath);
            _locationBoxItems.Insert(0, new ComboBoxItem { Content = newVaultPath });

            SaveVaultLocationHistory();

            var passwordVault = VaultLoader.FromNew(newVaultPath, newVaultPassword);
            // create an instance of VaultViewWindow(passwordVault)

            // TODO: hide login window
            SelectedLocationItemIndex = 0;
        }

        private void OpenVault_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLocationItemIndex == -1) {
                MessageBox.Show((string)FindResource("NoVaultSelected"), "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Password.Password == string.Empty) {
                MessageBox.Show((string)FindResource("NoPasswordGiven"), "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                Password.Focus();
                return;
            }

            var selectedPath = (string)_locationBoxItems[SelectedLocationItemIndex].Content;
            if (!File.Exists(selectedPath)) {
                var result = MessageBox.Show((string)FindResource("VaultNotFound"), "Error",
                                             MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) {
                    _locationBoxItems.RemoveAt(SelectedLocationItemIndex);
                    // This location may not in the history.
                    if (_vaultLocations.Remove(selectedPath)) {
                        SaveVaultLocationHistory();
                    }
                }

                SelectedLocationItemIndex = -1;
                Password.Password = "";

                return;
            }

            MoveVaultLocationToTop(selectedPath);
            _locationBoxItems.Move(SelectedLocationItemIndex, 0);

            SaveVaultLocationHistory();

            var passwordVault = VaultLoader.FromProvided(selectedPath, Password.Password);
            // create an instance of VaultViewWindow(passwordVault)

            // TODO: hide login window
            SelectedLocationItemIndex = 0;
        }

        private void MoveVaultLocationToTop(string selectedPath)
        {
            _vaultLocations.Remove(selectedPath);
            _vaultLocations.Insert(0, selectedPath);
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}