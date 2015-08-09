/*
 @ 0xCCCCCCCC
*/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EasyKeeper {
    public class VaultViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly PasswordVault _vault;
        private ObservableCollection<AccountInfoView> _accountsView;

        public ObservableCollection<AccountInfoView> AccountsView
        {
            get {
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

        public VaultViewModel(PasswordVault vault)
        {
            _vault = vault;
        }
    }

    public class AccountInfoView {
        public int Id { get; set; }
        public string Label { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
