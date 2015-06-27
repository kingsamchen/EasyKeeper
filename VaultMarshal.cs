/*
 @ Kingsley Chen
*/

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

using MD5 = System.Security.Cryptography.MD5;

namespace EasyKeeper {
    public static class VaultMarshal {
        private class Checksum {
            private readonly byte[] _data;

            public Checksum(byte[] data)
            {
                _data = data;
            }

            public byte[] Data
            {
                get {
                    return _data;
                }
            }
        }

        private const uint ProtoclVersion = 1U;

        public static void Marshal(string pwd, AccountStore store, Stream outStream)
        {
            var storeDataBytes = AccountStoreToBytes(store);
            // TODO: get encrypted store data bytes.
            // TODO: compute checksum(MD5) for altogegher data.
            // TODO: write to `outStream`.
        }

        public static AccountStore Unmarshal(Stream inStream, string pwd)
        {
            return null;
        }

        private static byte[] AccountStoreToBytes(AccountStore store)
        {
            using (MemoryStream mem = new MemoryStream()) {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(mem, store);
                return mem.ToArray();
            }
        }

        private static AccountStore AccountStoreFromBytes(byte[] rawBytes)
        {
            using (MemoryStream mem = new MemoryStream(rawBytes)) {
                IFormatter formatter = new BinaryFormatter();
                var accountStore = formatter.Deserialize(mem) as AccountStore;
                return accountStore;
            }
        }

        private static byte[] EncryptStoreData(byte[] storeData, string userPassword)
        {
            var keyGen = CreateKeyGenForCrypto(userPassword);
            using (Aes aes = new AesManaged()) {
                aes.Key = keyGen.GetBytes(32);
                aes.IV = keyGen.GetBytes(16);
                using (var mem = new MemoryStream())
                using (var crypto = new CryptoStream(mem, aes.CreateEncryptor(),
                                                     CryptoStreamMode.Write)) {
                    crypto.Write(storeData, 0, storeData.Length);
                    crypto.Close();

                    return mem.ToArray();
                }
            }
        }

        private static Checksum ComputeChecksum(byte[] rawBytes)
        {
            MD5 md5 = MD5.Create();
            var hash = md5.ComputeHash(rawBytes);
            return new Checksum(hash);
        }

        private static Rfc2898DeriveBytes CreateKeyGenForCrypto(string userPassword)
        {
            const int iterationCount = 1000;
            const byte mask = 0x44;
            var salt = new byte[8];
            var saltCode = BitConverter.GetBytes(userPassword.GetHashCode());

            saltCode.CopyTo(salt, 0);
            for (int i = 0; i < 4; ++i) {
                salt[i + 4] = (byte)(salt[i] ^ mask);
            }

            return new Rfc2898DeriveBytes(userPassword, salt, iterationCount);
        }
    }
}