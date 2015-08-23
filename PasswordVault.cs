/*
 @ 0xCCCCCCCC
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EasyKeeper {
    // PasswordVault plays the role of data model.
    public class PasswordVault {
        private readonly string _path;
        private readonly string _accessPassword;
        private readonly SortedSet<AccountInfo> _accountData;

        // Builds a brand new vault.
        public PasswordVault(string vaultPath, string accessPassword)
        {
            _path = vaultPath;
            _accessPassword = accessPassword;
            _accountData = new SortedSet<AccountInfo>();
        }

        // Builds a vault from a given source.
        public PasswordVault(string vaultPath, string accessPassword, Stream stream)
        {
            _path = vaultPath;
            _accessPassword = accessPassword;
            _accountData = VaultMarshal.Unmarshal(stream, accessPassword);
        }

        // It's up to the caller to decide whether to wait until this function completes.
        public Task Save()
        {
            return Task.Run(() => {
                using (var fs = new FileStream(_path, FileMode.Create)) {
                    VaultMarshal.Marshal(_accessPassword, _accountData, fs);
                }
            });
        }

        // Exposed to ViewModel.
        public IEnumerable<AccountInfo> AsAccountInfoEnumerable()
        {
            return _accountData;
        }

        public void AddAccountInfo(string label, string username, string password)
        {
            _accountData.Add(new AccountInfo(label, username, password));
        }

        public void UpdateAccountInfo(string label, string username, string password)
        {
            var accountInfo = _accountData.First(info => info.Label == label);
            accountInfo.UserName = username;
            accountInfo.Password = password;
        }

        public void RemoveAccountInfo(string label)
        {
            _accountData.RemoveWhere(info => info.Label == label);
        }
    }

    [Serializable]
    public class AccountInfo : IComparable<AccountInfo> {
        // Use `TimeCreated` to keep each record in a relative order of sequence as it was added,
        // which is appropriate when being displayed.
        public long TimeCreated { get; set; }
        public string Label { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        // This constructor is used for locating the item to be removed from data.
        public AccountInfo(string label)
        {
            Label = label;
        }

        public AccountInfo(string label, string username, string password)
        {
            Label = label;
            UserName = username;
            Password = password;
            TimeCreated = DateTime.UtcNow.Ticks;
        }

        public int CompareTo(AccountInfo other)
        {
            return string.Compare(Label, other.Label, StringComparison.Ordinal);
        }
    }
}
