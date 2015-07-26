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
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Win32 = Microsoft.Win32;

namespace EasyKeeper {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {
        private static readonly string HistoryFilePath;
        private List<string> _vaultLocations;
        private ObservableCollection<ComboBoxItem> _locationBoxItems;

        static LoginWindow()
        {
            var appDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            HistoryFilePath = Path.Combine(appDir, @"EasyKeeper\vault_history");
        }

        public LoginWindow()
        {
            InitializeComponent();

            LoadVaultLocationFromHistory();
            var locationItems = from location in _vaultLocations
                                select new ComboBoxItem { Content = location };
            _locationBoxItems = new ObservableCollection<ComboBoxItem>(locationItems);
            var chooseLocationItem = new ComboBoxItem { FontStyle = FontStyles.Italic,
                                                        Content = "Choose a Location..." };
            _locationBoxItems.Add(chooseLocationItem);
            VaultLocationList.ItemsSource = _locationBoxItems;

            // If there are locations in history, display the first of them.
            if (_vaultLocations.Count != 0) {
                VaultLocationList.SelectedIndex = 0;
            }
        }

        private void VaultLocationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var locationBox = ((ComboBox)sender);
            // When `choose a location` item is selected, display an open-file dialog for
            // choosing another vault.
            if (locationBox.SelectedIndex == locationBox.Items.Count - 1) {
                var dlg = new Win32.OpenFileDialog {
                    DefaultExt = ".ekp",
                    Filter = "EasyKeeper vault (*.ekp)|*.ekp"
                };

                bool? result = dlg.ShowDialog();
                if (result != true) {
                    locationBox.SelectedIndex = -1;
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

                locationBox.SelectedIndex = index;
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

            var appDataDir = Path.GetDirectoryName(HistoryFilePath);
            Debug.Assert(appDataDir != null, "appDataDir cannot be null");
            if (!Directory.Exists(appDataDir)) {
                Directory.CreateDirectory(appDataDir);
            }

            File.WriteAllLines(HistoryFilePath, _vaultLocations, Encoding.UTF8);
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
            Debug.WriteLine(newVaultPath);
        }
    }
}