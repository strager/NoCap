using System;
using NUnit.Framework;

namespace NoCap.Library.Tests {
    [TestFixture]
    class SecurityTests {
        [Test]
        public void EncryptDecrypt() {
            const string input = "foobar";

            string output =
                Security.ToInsecureString(
                    Security.DecryptString(
                        Security.EncryptString(
                            Security.ToSecureString(
                                input
                            )
                        )
                    )
                );

            Assert.AreEqual(input, output);
        }

        [Test]
        public void SecureInsecure() {
            const string input = "foobar";

            string output = Security.ToInsecureString(Security.ToSecureString(input));

            Assert.AreEqual(input, output);
        }

        [Test]
        public void BadDecryptionReturnsNull() {
            byte[] badData = { 0 };

            var output = Security.DecryptString(badData);

            Assert.IsNull(output);
        }

        [Test]
        public void ToSecureStringNullArgThrows() {
            Assert.Throws<ArgumentNullException>(() => Security.ToSecureString(null));
        }

        [Test]
        public void ToInsecureStringNullArgThrows() {
            Assert.Throws<ArgumentNullException>(() => Security.ToInsecureString(null));
        }

        [Test]
        public void EncryptStringNullArgThrows() {
            Assert.Throws<ArgumentNullException>(() => Security.EncryptString(null));
        }

        [Test]
        public void DecryptStringNullArgThrows() {
            Assert.Throws<ArgumentNullException>(() => Security.DecryptString(null));
        }
    }
}
