/*
 @ Kingsley Chen
*/

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

using Checksum = EasyKeeper.Validator<EasyKeeper.ChecksumPolicy>;
using Signature = EasyKeeper.Validator<EasyKeeper.SignaturePolicy>;

[assembly:InternalsVisibleTo("Test")]

namespace EasyKeeper {
    public static class VaultMarshal {
        private const uint ProtocolVersion = 1U;

        public static void Marshal(string pwd, AccountStore store, Stream outStream)
        {
            var storeData = AccountStoreToBytes(store);
            var dataSignature = Signature.FromRawData(storeData, Encoding.UTF8.GetBytes(pwd));
            var encryptedData = EncryptStoreData(storeData, pwd);
            var dataChecksum = Checksum.FromRawData(BitConverter.GetBytes(ProtocolVersion)
                                                                .Concat(dataSignature.Data)
                                                                .Concat(encryptedData)
                                                                .ToArray());
            using (var writer = new BinaryWriter(outStream)) {
                writer.Write(dataChecksum.Data);
                writer.Write(ProtocolVersion);
                writer.Write(dataSignature.Data);
                writer.Write(encryptedData);
            }
        }

        public static AccountStore Unmarshal(Stream inStream, string pwd)
        {
            using (var reader = new BinaryReader(inStream)) {
                var checksumRead = Checksum.FromData(reader.ReadBytes(Checksum.HashSizeInBytes));
                var protocolVersion = reader.ReadUInt32();
                var signatureRead = Signature.FromData(reader.ReadBytes(Signature.HashSizeInBytes));
                // Now, we can read the whole rest off the stream.
                byte[] encryptedData = null;
                using (var mem = new MemoryStream()) {
                    reader.BaseStream.CopyTo(mem);
                    encryptedData = mem.ToArray();
                }

                // Check whether data has been corrupted.
                var checksum = Checksum.FromRawData(BitConverter.GetBytes(protocolVersion)
                                                                        .Concat(signatureRead.Data)
                                                                        .Concat(encryptedData)
                                                                        .ToArray());
                if (checksumRead != checksum) {
                    throw new DataCorruptedException("Checksums don't match!");
                }

                // TODO: handle decryption exception caused by wrong password here
                var storeData = DecryptStoreData(encryptedData, pwd);

                var signature = Signature.FromRawData(storeData, Encoding.UTF8.GetBytes(pwd));
                if (signatureRead != signature) {
                    throw new IncorrectPassword("Password is incorrect!");
                }

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
            using (var crypto = CreateCrypto(userPassword))
            using (var mem = new MemoryStream())
            using (var encryptor = new CryptoStream(mem, crypto.CreateEncryptor(),
                                                    CryptoStreamMode.Write)) {
                encryptor.Write(storeData, 0, storeData.Length);
                encryptor.Dispose();

                return mem.ToArray();
            }
        }

        private static byte[] DecryptStoreData(byte[] encrypted, string userPassword)
        {
            using (var crypto = CreateCrypto(userPassword))
            using (var mem = new MemoryStream())
            using (var decryptor = new CryptoStream(mem, crypto.CreateDecryptor(),
                                                    CryptoStreamMode.Write)) {
                decryptor.Write(encrypted, 0, encrypted.Length);
                decryptor.Dispose();

                return mem.ToArray();
            }
        }

        private static Aes CreateCrypto(string userPassword)
        {
            const int iterationCount = 1000;
            const byte mask = 0x44;
            var salt = new byte[8];
            var saltCode = BitConverter.GetBytes(userPassword.GetHashCode());

            saltCode.CopyTo(salt, 0);
            for (int i = 0; i < 4; ++i) {
                salt[i + 4] = (byte)(salt[i] ^ mask);
            }

            const int keySize = 32;
            const int ivSize = 16;
            var keyGen = new Rfc2898DeriveBytes(userPassword, salt, iterationCount);
            Aes aes = new AesManaged();
            aes.Key = keyGen.GetBytes(keySize);
            aes.IV = keyGen.GetBytes(ivSize);

            return aes;
        }
    }

    public class DataCorruptedException : Exception {
        public DataCorruptedException()
        {}

        public DataCorruptedException(string message)
            : base(message)
        {}

        public DataCorruptedException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }

    public class IncorrectPassword : Exception {
        public IncorrectPassword()
        {}

        public IncorrectPassword(string message)
            : base(message)
        {}

        public IncorrectPassword(string message, Exception innerException)
            : base(message, innerException)
        {}
    }

    interface IHashPolicy {
        byte[] ComputeHash(byte[] rawData);
        byte[] ComputeHash(byte[] rawData, byte[] key);
        int GetHashSize();
    }

    class ChecksumPolicy : IHashPolicy {
        public byte[] ComputeHash(byte[] rawData)
        {
            var md5 = MD5.Create();
            return md5.ComputeHash(rawData);
        }

        public byte[] ComputeHash(byte[] rawData, byte[] key)
        {
            throw new NotSupportedException();
        }

        public int GetHashSize()
        {
            return 128;
        }
    }

    class SignaturePolicy : IHashPolicy {
        public byte[] ComputeHash(byte[] rawData)
        {
            throw new NotSupportedException();
        }

        public byte[] ComputeHash(byte[] rawData, byte[] key)
        {
            using (var hmac = new HMACSHA256(key)) {
                return hmac.ComputeHash(rawData);
            }
        }

        public int GetHashSize()
        {
            return 256;
        }
    }

    class Validator<T> : IEquatable<Validator<T>> where T : IHashPolicy, new() {
        private readonly byte[] _data;

        private Validator(byte[] data)
        {
            _data = data;
        }

        public static Validator<T> FromData(byte[] data)
        {
            return new Validator<T>(data);
        }

        public static Validator<T> FromRawData(byte[] rawData)
        {
            var policy = new T();
            var hashed = policy.ComputeHash(rawData);

            return new Validator<T>(hashed);
        }

        public static Validator<T> FromRawData(byte[] rawData, byte[] key)
        {
            var policy = new T();
            var hashed = policy.ComputeHash(rawData, key);

            return new Validator<T>(hashed);
        }

        public byte[] Data
        {
            get {
                return _data;
            }
        }

        public static int HashSizeInBytes
        {
            get {
                var policy = new T();
                return policy.GetHashSize() / 8;
            }
        }

        public bool Equals(Validator<T> other)
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
            return (other is Validator<T>) && Equals((Validator<T>)other);
        }

        public override int GetHashCode()
        {
            return (_data != null ? _data.GetHashCode() : 0);
        }

        public static bool operator==(Validator<T> lhs, Validator<T> rhs)
        {
            if (ReferenceEquals(lhs, rhs)) {
                return true;
            }

            if ((object)lhs == null || (object)rhs == null) {
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator!=(Validator<T> lhs, Validator<T> rhs)
        {
            return !(lhs == rhs);
        }
    }
}