/*
 @ 0xCCCCCCCC
*/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace EasyKeeper {
    public class VaultViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly PasswordVault _vault;
        private bool _vaultDataChanged;
        private ObservableCollection<AccountInfoView> _accountsView;
        private int _selectedAccountId = -1;
        private RelayCommand<object> _windowClosingCommand;
        private RelayCommand<object> _newAccountCommand;
        private RelayCommand<int?> _modifyAccountCommand;
        private RelayCommand<int?> _removeAccountCommand;

        public VaultViewModel(PasswordVault vault)
        {
            _vault = vault;
        }

        public ObservableCollection<AccountInfoView> AccountsView
        {
            get
            {
                if (_accountsView == null) {
                    var index = 0;
                    var infoViews = from info in _vault.AsAccountInfoEnumerable()
                                    orderby info.TimeCreated
                                    let id = ++index
                                    select new AccountInfoView() { Id = id,
                                                                   Label = info.Label,
                                                                   UserName = info.UserName,
                                                                   Password = info.Password };
                    _accountsView = new ObservableCollection<AccountInfoView>(infoViews);
                }

                return _accountsView;
            }
        }

        public int SelectedAccountId
        {
            get {
                return _selectedAccountId;
            }

            set {
                _selectedAccountId = value;
                OnPropertyChanged();
            }
        }

        public ICommand NewAccountCommand
        {
            get {
                if (_newAccountCommand == null) {
                    _newAccountCommand = new RelayCommand<object>(param => {
                        var viewModel = new EditAccountViewModel(_vault);
                        var dlg = new EditAccountDialog(viewModel);
                        bool? rv = dlg.ShowDialog();
                        if (rv == true) {
                            var newAccount = new AccountInfoView() {
                                Id = _accountsView.Count + 1,
                                Label = viewModel.Tag,
                                UserName = viewModel.UserName,
                                Password = viewModel.Password
                            };

                            _accountsView.Add(newAccount);
                            _vault.AddAccountInfo(newAccount.Label, newAccount.UserName,
                                                  newAccount.Password);
                            _vaultDataChanged = true;
                        }
                    });
                }

                return _newAccountCommand;
            }
        }

        public ICommand ModifyAccountCommand
        {
            get {
                if (_modifyAccountCommand == null) {
                    _modifyAccountCommand = new RelayCommand<int?>(selectedId => {
                        Debug.Assert(selectedId != null, "selectedId cannot be null");
                        var accountInfo = _accountsView[(int)selectedId];
                        var viewModel = new EditAccountViewModel(accountInfo.Label,
                                                                 accountInfo.UserName,
                                                                 accountInfo.Password,
                                                                 _vault);
                        var dlg = new EditAccountDialog(viewModel);
                        bool? rv = dlg.ShowDialog();
                        if (rv == true) {
                            accountInfo.UserName = viewModel.UserName;
                            accountInfo.Password = viewModel.Password;
                            _vault.UpdateAccountInfo(accountInfo.Label, accountInfo.UserName,
                                                     accountInfo.Password);
                            _vaultDataChanged = true;
                        }
                    }, selectedId => selectedId != null && selectedId != -1);
                }

                return _modifyAccountCommand;
            }
        }

        public ICommand RemoveAccountCommand
        {
            get {
                if (_removeAccountCommand == null) {
                    _removeAccountCommand = new RelayCommand<int?>(selectedId => {
                        var msg = (string)Application.Current.FindResource("ConfirmDeleteAccount");
                        var rv = MessageBox.Show(msg, "Question", MessageBoxButton.YesNo,
                                                 MessageBoxImage.Question);
                        if (rv == MessageBoxResult.Yes) {
                            Debug.Assert(selectedId != null, "selectedId cannot be null");
                            var index = (int)selectedId;
                            var accountInfo = _accountsView[index];
                            _accountsView.RemoveAt(index);
                            FixAccountIdAfterRemoval(index);
                            _vault.RemoveAccountInfo(accountInfo.Label);
                            _vaultDataChanged = true;
                        }
                    }, selectedId => selectedId != null && selectedId != -1);
                }

                return _removeAccountCommand;
            }
        }

        public ICommand WindowClosing
        {
            get {
                if (_windowClosingCommand == null) {
                    _windowClosingCommand = new RelayCommand<object>(param => {
                        if (_vaultDataChanged) {
                            _vault.Save().Wait();
                        }
                    });
                }

                return _windowClosingCommand;
            }
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void FixAccountIdAfterRemoval(int removedIndex)
        {
            for (var i = removedIndex; i < _accountsView.Count; i++) {
                _accountsView[i].Id--;
            }
        }
    }

    // An adapter for vault model to view.
    public class AccountInfoView : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _id;
        private string _username;
        private string _password;

        public int Id
        {
            get {
                return _id;
            }

            set {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Label { get; set; }

        public string UserName
        {
            get {
                return _username;
            }

            set {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get {
                return _password;
            }

            set {
                _password = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
