/*
 @ 0xCCCCCCCC
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyKeeper {
    public class VaultViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly PasswordVault _vault;
        private ObservableCollection<AccountInfoView> _accounts;

        public ObservableCollection<AccountInfoView> Accounts
        {
            get {
                if (_accounts == null) {
                    
                    // TODO: create
                }

                return _accounts;;
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
