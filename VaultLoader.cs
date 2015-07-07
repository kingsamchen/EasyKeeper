/*
 @ Kingsley Chen
*/

namespace EasyKeeper {
    public static class VaultLoader {
        public static PasswordVault FromProvided(string path, string password)
        {
            return null;
        }

        public static PasswordVault FromNew(string path, string password)
        {
            var vault = new PasswordVault(path, password);
            vault.StoreAsync();

            return vault;
        }
    }
}