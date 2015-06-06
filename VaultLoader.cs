/*
 @ Kingsley Chen
*/

namespace EasyKeeper
{
    static class VaultLoader
    {
        public static PasswordVault LoadFromProvided(string path, string password)
        {
            return null;
        }

        public static PasswordVault LoadFromNew(string path, string password)
        {
            var vault = new PasswordVault(path, password);
            vault.Store();
            return vault;
        }
    }
}