/*
 @ Kingsley Chen
*/

using System;
using System.Security.Cryptography;
using System.Text;
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
    }
}
