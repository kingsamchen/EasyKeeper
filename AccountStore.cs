/*
 @ Kingsley Chen
*/

using System;
using System.Collections.Generic;

namespace EasyKeeper {
    [Serializable]
    public class AccountStore {
        [Serializable]
        private class AccountInfo {
            // Use ID to keep each record in a relative order of sequence as it was added,
            // which is appropriate when being displayed.
            public int Id { get; private set; }
            public string UserName { get; set; }
            public string Password { get; set; }

            public AccountInfo(int id, string userName, string password)
            {
                UserName = userName;
                Password = password;
                Id = id;
            }
        }

        private SortedList<string, AccountInfo> _accountData;

        public AccountStore()
        {
            _accountData = new SortedList<string, AccountInfo>();
        }

        public bool AddAccountInfo(string label, string username, string password)
        {
            if (_accountData.ContainsKey(label)) {
                return false;
            }

            var id = _accountData.Count + 1;
            _accountData[label] = new AccountInfo(id, username, password);

            return true;
        }

        public void RemoveAccountInfo(string label)
        {
            _accountData.Remove(label);
        }

        public void UpdateAccountInfo(string label, string newUsername, string newPassword)
        {
            var info = _accountData[label];
            info.UserName = newUsername;
            info.Password = newPassword;
        }
    }
}