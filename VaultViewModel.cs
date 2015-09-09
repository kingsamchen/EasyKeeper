/*
 @ 0xCCCCCCCC
*/

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace EasyKeeper {
    public class VaultViewModel : BindableObject {
        private readonly PasswordVault _vault;
        private bool _vaultDataChanged;
        private ObservableCollection<AccountInfoView> _accountsView;
        private int _selectedAccountId = -1;
        private RelayCommand<object> _windowClosingCommand;
        private ExecuteCommand<EditAccountViewModel, object> _newAccountCommand;
        private ExecuteCommand<EditAccountViewModel, object> _modifyAccountCommand;
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
                RaisePropertyChanged();
            }
        }

        public EditAccountViewModel NewAccountViewModel
        {
            get {
                return new EditAccountViewModel(_vault);
            }
        }

        public EditAccountViewModel ModifyAccountViewModel
        {
            get {
                if (SelectedAccountId == -1) {
                    return null;
                }

                var accountInfo = _accountsView[SelectedAccountId];
                return new EditAccountViewModel(accountInfo.Label, accountInfo.UserName,
                                                accountInfo.Password, _vault);
            }
        }

        public ICommand NewAccountCommand
        {
            get {
                return _newAccountCommand ??
                      (_newAccountCommand = new ExecuteCommand<EditAccountViewModel, object>(vm => {
                          var newAccount = new AccountInfoView {
                              Id = _accountsView.Count + 1,
                              Label = vm.Tag,
                              UserName = vm.UserName,
                              Password = vm.Password
                          };

                          _accountsView.Add(newAccount);
                          _vault.AddAccountInfo(newAccount.Label, newAccount.UserName,
                                                newAccount.Password);
                          _vaultDataChanged = true;

                          return null;
                       }));
            }
        }

        public ICommand ModifyAccountCommand
        {
            get {
                return _modifyAccountCommand ??
                      (_modifyAccountCommand = new ExecuteCommand<EditAccountViewModel, object>(
                          vm => {
                              var accountInfo = _accountsView[SelectedAccountId];
                              accountInfo.UserName = vm.UserName;
                              accountInfo.Password = vm.Password;
                              _vault.UpdateAccountInfo(accountInfo.Label, accountInfo.UserName,
                                                       accountInfo.Password);
                              _vaultDataChanged = true;

                              return null;
                          }));
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

        private void FixAccountIdAfterRemoval(int removedIndex)
        {
            for (var i = removedIndex; i < _accountsView.Count; i++) {
                _accountsView[i].Id--;
            }
        }
    }

    // An adapter for vault model to view.
    public class AccountInfoView : BindableObject {
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
                RaisePropertyChanged();
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
                RaisePropertyChanged();
            }
        }

        public string Password
        {
            get {
                return _password;
            }

            set {
                _password = value;
                RaisePropertyChanged();
            }
        }
    }
}
