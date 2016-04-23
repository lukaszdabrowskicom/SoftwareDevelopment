using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common encryption and decryption operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class EncryptionDecryptionUtils
    {

        static EncryptionDecryptionUtils()
        {
            _provider = new RSACryptoServiceProvider();
        }

        /// <summary>
        /// RSA private user key.
        /// </summary>
        public static RSAParameters RSA_PRIVATE_USER_KEY
        {
            get
            {
                return _rsaPrivateUserKey;
            }
        }

        /// <summary>
        /// Initializes RSA private user key.
        /// </summary>
        /// <param name="rsaPrivateUserKey">RSA private user key to decrypt data</param>
        public static void InitializeRsaPrivateUserKey(RSAParameters rsaPrivateUserKey)
        {
            _provider.ImportParameters(rsaPrivateUserKey);
            _rsaPrivateUserKey = rsaPrivateUserKey;
            _rsaPrivateUserKeyIsInitialized = true;
        }

        /// <summary>
        /// Encrypts input string according to internal RSACryptoServiceProvider alghoritm.
        /// </summary>
        /// <param name="openTextString">text to be encrypted</param>
        ///<returns>encrypted string</returns>
        public static string Encrypt(string openTextString)
        {
            ThrowExceptionIfRsaPrivateUserKeyIsNotInitialized();

            byte[] encryptedData = _provider.Encrypt(Encoding.UTF8.GetBytes(openTextString), false);

            return Encoding.UTF8.GetString(encryptedData);
        }

        /// <summary>
        /// Encrypts input string according to internal RSACryptoServiceProvider alghoritm.
        /// </summary>
        /// <param name="userRsaPublicKey">user public key to encrypt data</param>
        /// <param name="openTextString">text to be encrypted</param>
        ///<returns>encrypted string</returns>
        public static string Encrypt(RSAParameters userRsaPublicKey, string openTextString)
        {
           _provider.ImportParameters(userRsaPublicKey);
           byte[] encryptedData = _provider.Encrypt(Encoding.UTF8.GetBytes(openTextString), false);

           return Encoding.UTF8.GetString(encryptedData);
        }

        /// <summary>
        /// Decrypts input string according to internal RSACryptoServiceProvider alghoritm.
        /// </summary>
        /// <param name="encryptedTextString">text to be decrypted</param>
        ///<returns>decrypted string</returns>
        public static string Decrypt(string encryptedTextString)
        {
            ThrowExceptionIfRsaPrivateUserKeyIsNotInitialized();

            byte[] encryptedData = Encoding.UTF8.GetBytes(encryptedTextString);
            byte[] decryptedData = _provider.Decrypt(encryptedData, false);

            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// Decrypts input string according to internal RSACryptoServiceProvider alghoritm.
        /// </summary>
        /// <param name="userRsaPrivateKey">user private key to decrypt data</param>
        /// <param name="encryptedTextString">text to be decrypted</param>
        ///<returns>decrypted string</returns>
        public static string Decrypt(RSAParameters userRsaPrivateKey, string encryptedTextString)
        {
            _provider.ImportParameters(userRsaPrivateKey);
            byte[] encryptedData = Encoding.UTF8.GetBytes(encryptedTextString);
            byte[] decryptedData = _provider.Decrypt(encryptedData, false);

            return Encoding.UTF8.GetString(decryptedData);
        }

        /// <summary>
        /// Throws exception in case user private key is not initialized.
        /// </summary>
        public static void ThrowExceptionIfRsaPrivateUserKeyIsNotInitialized()
        {
            if (!_rsaPrivateUserKeyIsInitialized)
                throw ExceptionUtils.CreateException("Retrieving credential data requires initializing RSA private user key. Invoke InitializeRsaPrivateUserKey method first.");
        }

        private static RSACryptoServiceProvider _provider = null;
        private static RSAParameters _rsaPrivateUserKey;
        private static bool _rsaPrivateUserKeyIsInitialized;
    }
}
