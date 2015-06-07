/*
 @ Kingsley Chen
*/

namespace EasyKeeper {
    public static class VaultLoader {
        public static PasswordVault LoadFromProvided(string path, string password)
        {
            return null;
        }

        public static PasswordVault LoadFromNew(string path, string password)
        {
            var vault = new PasswordVault(path, password);
            vault.StoreAsync();
            return vault;
        }
    }
}