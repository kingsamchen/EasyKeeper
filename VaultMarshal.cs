/*
 @ Kingsley Chen
*/

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace EasyKeeper {
    public static class VaultMarshal {
        private class Checksum : IEquatable<Checksum> {
            public const int HashSizeInBytes = 128 / 8;
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

            public bool Equals(Checksum other)
            {
                if (ReferenceEquals(null, other)) {
                    return false;
                }

                if (ReferenceEquals(this, other)) {
                    return true;
                }

                return _data.SequenceEqual(other._data);
            }

            public override bool Equals(object other)
            {
                if (ReferenceEquals(null, other)) {
                    return false;
                }

                if (ReferenceEquals(this, other)) {
                    return true;
                }

                var otherChecksum = other as Checksum;

                return (otherChecksum != null) && Equals(otherChecksum);
            }

            public override int GetHashCode()
            {
                return (_data != null ? _data.GetHashCode() : 0);
            }

            public static bool operator==(Checksum lhs, Checksum rhs)
            {
                return Equals(lhs, rhs);
            }

            public static bool operator!=(Checksum lhs, Checksum rhs)
            {
                return !(lhs == rhs);
            }
        }

        private const uint ProtocolVersion = 1U;

        public static void Marshal(string pwd, AccountStore store, Stream outStream)
        {
            var encryptedData = EncryptStoreData(AccountStoreToBytes(store), pwd);
            var dataChecksum = ComputeChecksum(BitConverter.GetBytes(ProtocolVersion)
                                                           .Concat(encryptedData)
                                                           .ToArray());

            using (var writer = new BinaryWriter(outStream)) {
                writer.Write(dataChecksum.Data);
                writer.Write(ProtocolVersion);
                writer.Write(encryptedData);
            }
        }

        public static AccountStore Unmarshal(Stream inStream, string pwd)
        {
            using (var reader = new BinaryReader(inStream)) {
                var checksum = new Checksum(reader.ReadBytes(Checksum.HashSizeInBytes));
                var protocolVersion = reader.ReadUInt32();
                // Now, we can read the whole rest off the stream.
                byte[] encryptedData = null;
                using (var mem = new MemoryStream()) {
                    reader.BaseStream.CopyTo(mem);
                    encryptedData = mem.ToArray();
                }

                // Check whether data has been corrupted.
                var computedChecksum
                        = ComputeChecksum(BitConverter.GetBytes(protocolVersion)
                                                      .Concat(encryptedData)
                                                      .ToArray());
                if (checksum != computedChecksum) {
                    throw new DataCorruptionException("Checksums don't match!");
                }

                // TODO: handle decryption exception caused by wrong password here
                var storeData = DecryptStoreData(encryptedData, pwd);

                var store = AccountStoreFromBytes(storeData);

                return store;
            }
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
                    crypto.Dispose();

                    return mem.ToArray();
                }
            }
        }

        private static byte[] DecryptStoreData(byte[] encrypted, string userPassword)
        {
            var keyGen = CreateKeyGenForCrypto(userPassword);
            using (Aes aes = new AesManaged()) {
                aes.Key = keyGen.GetBytes(32);
                aes.IV = keyGen.GetBytes(16);
                using (var mem = new MemoryStream())
                using (var crypto = new CryptoStream(mem, aes.CreateDecryptor(),
                                                     CryptoStreamMode.Write)) {
                    crypto.Write(encrypted, 0, encrypted.Length);
                    crypto.Dispose();

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

    public class DataCorruptionException : Exception {
        public DataCorruptionException()
        {}

        public DataCorruptionException(string message)
            : base(message)
        {}

        public DataCorruptionException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}