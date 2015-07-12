/*
 @ Kingsley Chen
*/

using System.IO;
using System.Threading.Tasks;

namespace EasyKeeper {
    public class PasswordVault {
        private string _path;
        private string _accessPassword;
        private AccountStore _accountStore;

        // Builds a brand new vault.
        public PasswordVault(string storePath, string accessPassword)
        {
            _path = storePath;
            _accessPassword = accessPassword;
            _accountStore = new AccountStore();
        }

        // Builds a vault from a given source.
        public PasswordVault(string storePath, string accessPassword, Stream stream)
        {
            _path = storePath;
            _accessPassword = accessPassword;
            _accountStore = VaultMarshal.Unmarshal(stream, accessPassword);
        }

        public void StoreAsync()
        {
            Task.Run(() => {
                using (var fs = new FileStream(_path, FileMode.Create)) {
                    VaultMarshal.Marshal(_accessPassword, _accountStore, fs);
                }
            });
        }
    }
}
