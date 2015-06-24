/*
 @ Kingsley Chen
*/

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace EasyKeeper {
    public static class VaultMarshal {
        private class Digest {
            private readonly byte[] _data;

            public Digest(byte[] data)
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
        {}

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

        private static Digest ComputeDigest(byte[] rawBytes)
        {
            using (SHA1 sha = new SHA1Managed()) {
                var hash = sha.ComputeHash(rawBytes);
                return new Digest(hash);
            }
        }
    }
}