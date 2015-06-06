/*
 @ Kingsley Chen
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyKeeper
{
    class PasswordVault
    {
        private class AccountInfo
        {
            public string UserName { get; set; }
            public string Password { get; set; }

            public AccountInfo(string userName, string password)
            {
                UserName = userName;
                Password = password;
            }
        }

        private string _path;
        private string _accessPassword;
        private SortedList<string, AccountInfo> _accountStore;

        /// <summary>
        /// Builds a brand new vault.
        /// </summary>
        public PasswordVault(string storePath, string accessPassword)
        {
            _path = storePath;
            _accessPassword = accessPassword;
            _accountStore = new SortedList<string, AccountInfo>();
        }

        /// <summary>
        /// Builds a vault from given data.
        /// </summary>
        /// <param name="data"></param>
        public PasswordVault(string storePath, string accessPassword, byte[] data)
        {}

        public void Store()
        {}
    }
}
