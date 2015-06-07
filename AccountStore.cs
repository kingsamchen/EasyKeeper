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
            public string UserName { get; set; }
            public string Password { get; set; }

            public AccountInfo(string userName, string password)
            {
                UserName = userName;
                Password = password;
            }
        }

        private SortedList<string, AccountInfo> _accountData;

        public AccountStore()
        {
            _accountData = new SortedList<string, AccountInfo>();
        }

        // TODO: Model Operations
    }
}