using System;
using System.Security.Cryptography;
using System.Text;

namespace VProofix.Helpers
{
    public static class SecurityHelper
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("VProofix_Secure_Salt");

        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string Unprotect(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(cipherText);
                byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
