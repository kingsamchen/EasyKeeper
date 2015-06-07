/*
 @ Kingsley Chen
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyKeeper {
    public class PasswordVault {
        private string _path;
        private string _accessPassword;
        private AccountStore _accountStore;

        /// <summary>
        /// Builds a brand new vault.
        /// </summary>
        public PasswordVault(string storePath, string accessPassword)
        {
            _path = storePath;
            _accessPassword = accessPassword;
            _accountStore = new AccountStore();
        }

        /// <summary>
        /// Builds a vault from given data.
        /// </summary>
        /// <param name="data"></param>
        public PasswordVault(string storePath, string accessPassword, byte[] data)
        {}

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
