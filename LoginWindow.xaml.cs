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
    public partial class LoginWindow : Window {
        public LoginWindow()
        {
            InitializeComponent();

            var viewModel = new LoginViewModel();
            DataContext = viewModel;
        }
    }

    class LoginViewModel : BindableObject {
        private static readonly string HistoryFilePath;
        private List<string> _vaultLocations = new List<string>();
        private ObservableCollection<ComboBoxItem> _locationItems;
        private int _selectedItemIndex = -1;

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

            return items.Concat(new [] { chooseLocationItem });
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
    }
}