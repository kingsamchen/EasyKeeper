/*
 @ 0xCCCCCCCC
*/

using System.IO;

namespace EasyKeeper {
    public static class VaultLoader {
        public static PasswordVault FromProvided(string path, string password)
        {
            using (var fs = File.Open(path, FileMode.Open)) {
                return new PasswordVault(path, password, fs);
            }
        }

        public static PasswordVault FromNew(string path, string password)
        {
            var vault = new PasswordVault(path, password);
            vault.Save();

            return vault;
        }
    }
}