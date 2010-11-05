using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace NoCap.Library {
    public static class Security {
        // Code based off of work by Jon Galloway
        // http://weblogs.asp.net/jgalloway/archive/2008/04/13/encrypting-passwords-in-a-net-app-config-file.aspx
        private const string EntropyString = "SshUploader entropy data here";

        private static byte[] Entropy {
            get {
                return Encoding.Unicode.GetBytes(EntropyString);
            }
        }

        public static byte[] EncryptString(SecureString input, DataProtectionScope protectionScope = DataProtectionScope.CurrentUser) {
            return ProtectedData.Protect(
                Encoding.Unicode.GetBytes(ToInsecureString(input)),
                Entropy,
                protectionScope
            );
        }

        public static SecureString DecryptString(byte[] encryptedData, DataProtectionScope protectionScope = DataProtectionScope.CurrentUser) {
            try {
                byte[] decryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    Entropy,
                    protectionScope
                );

                return ToSecureString(Encoding.Unicode.GetString(decryptedData));
            } catch {
                return new SecureString();
            }
        }

        public static SecureString ToSecureString(string input) {
            var secureString = new SecureString();

            foreach (char c in input) {
                secureString.AppendChar(c);
            }

            secureString.MakeReadOnly();

            return secureString;
        }

        public static string ToInsecureString(SecureString input) {
            IntPtr stringHandle = Marshal.SecureStringToBSTR(input);

            try {
                return Marshal.PtrToStringBSTR(stringHandle);
            } finally {
                Marshal.ZeroFreeBSTR(stringHandle);
            }
        }
    }
}