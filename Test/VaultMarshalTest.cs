/*
 @ Kingsley Chen
*/

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EasyKeeper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Checksum = EasyKeeper.Validator<EasyKeeper.ChecksumPolicy>;
using Signature = EasyKeeper.Validator<EasyKeeper.SignaturePolicy>;

namespace Test {
    [TestClass]
    public class VaultMarshalTest {
        [TestMethod]
        public void TestValidator()
        {
            string data = "testtesttest";
            string anotherData = "test123";
            string pwd = "kckckc";
            string anotherPwd = "kc123";

            Assert.AreEqual(MD5.Create().HashSize / 8, Checksum.HashSizeInBytes);

            var chkFromRaw = Checksum.FromRawData(Encoding.UTF8.GetBytes(data));
            var chk = Checksum.FromData(chkFromRaw.Data);
            Assert.IsTrue(chk == chkFromRaw);

            Assert.AreNotEqual(Checksum.FromRawData(Encoding.UTF8.GetBytes(data)),
                               Checksum.FromRawData(Encoding.UTF8.GetBytes(anotherData)));

            Assert.AreEqual(new HMACSHA256().HashSize / 8, Signature.HashSizeInBytes);

            var sigFromRaw = Signature.FromRawData(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(pwd));
            var signature = Signature.FromData(sigFromRaw.Data);
            Assert.AreEqual(sigFromRaw, signature);

            Assert.AreNotEqual(Signature.FromRawData(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(pwd)),
                               Signature.FromRawData(Encoding.UTF8.GetBytes(anotherData), Encoding.UTF8.GetBytes(pwd)));

            Assert.AreNotEqual(Signature.FromRawData(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(pwd)),
                               Signature.FromRawData(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(anotherPwd)));
        }

        [TestMethod]
        public void TestMarshal()
        {
            string pwd = "kc123";
            AccountStore store = new AccountStore();
            byte[] data = null;

            using (MemoryStream mem = new MemoryStream()) {
                VaultMarshal.Marshal(pwd, store, mem);
                data = mem.ToArray();
            }

            Assert.AreNotEqual(null, data);

            TryUnmarsal(data, pwd);

            try {
                var corrupted = new byte[data.Length];
                data.CopyTo(corrupted, 0);
                Assert.AreNotSame(corrupted, data);
                corrupted[0] ^= 0x44;
                corrupted[corrupted.Length - 2] ^= 0x44;

                TryUnmarsal(corrupted, pwd);
                Assert.Fail("Checksum didn't work");
            } catch(DataCorruptedException ex) {
                // keep silence
            }

            try {
                string fakePwd = "kckckc";
                TryUnmarsal(data, fakePwd);
                Assert.Fail("HMAC validation didn't work");
            } catch (IncorrectPassword ex) {
                // keep silence
            }
        }

        private AccountStore TryUnmarsal(byte[] data, string pwd)
        {
            using (MemoryStream mem = new MemoryStream(data)) {
                var storeGet = VaultMarshal.Unmarshal(mem, pwd);
                return storeGet;
            }
        }
    }
}
