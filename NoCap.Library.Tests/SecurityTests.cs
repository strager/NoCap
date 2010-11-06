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
    }
}
