using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace NoCap.Library.Util {
    public static class Security {
        // Code based off of work by Jon Galloway
        // http://weblogs.asp.net/jgalloway/archive/2008/04/13/encrypting-passwords-in-a-net-app-config-file.aspx
        private const string EntropyString = "SshUploader entropy data here";

        private static byte[] Entropy {
            get {
                return Encoding.Unicode.GetBytes(EntropyString);
            }
        }

        public static byte[] EncryptString(SecureString input) {
            return EncryptString(input, DataProtectionScope.CurrentUser);
        }

        public static byte[] EncryptString(SecureString input, DataProtectionScope protectionScope) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            return ProtectedData.Protect(
                Encoding.Unicode.GetBytes(ToInsecureString(input)),
                Entropy,
                protectionScope
            );
        }

        public static SecureString DecryptString(byte[] encryptedData) {
            return DecryptString(encryptedData, DataProtectionScope.CurrentUser);
        }

        public static SecureString DecryptString(byte[] encryptedData, DataProtectionScope protectionScope) {
            if (encryptedData == null) {
                throw new ArgumentNullException("encryptedData");
            }

            byte[] decryptedData;

            try {
                decryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    Entropy,
                    protectionScope
                );
            } catch (CryptographicException) {
                return null;
            }

            return ToSecureString(Encoding.Unicode.GetString(decryptedData));
        }

        public static SecureString ToSecureString(string input) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            var secureString = new SecureString();

            foreach (char c in input) {
                secureString.AppendChar(c);
            }

            secureString.MakeReadOnly();

            return secureString;
        }

        public static string ToInsecureString(SecureString input) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            IntPtr stringHandle = Marshal.SecureStringToBSTR(input);

            try {
                return Marshal.PtrToStringBSTR(stringHandle);
            } finally {
                Marshal.ZeroFreeBSTR(stringHandle);
            }
        }
    }
}