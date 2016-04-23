using System;
using System.Configuration;
using System.Reflection;
using System.Security.Cryptography;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common application config file operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class ConfigurationUtils
    {

        /// <summary>
        /// Returns application config file value with provided key, which is decrypted or not according to the second method parameter.
        /// </summary>
        /// <param name="key">appSetting key</param>
        /// <param name="decrypted">specifies whether value is decrypted or open text value</param>
        /// <param name="userRsaPrivateKey">user private key to decrypt data</param>
        /// <returns>open text value</returns>
        public static string GetConfigurationFileAppSettingValue(string key, bool decrypted, RSAParameters userRsaPrivateKey)
        {
            if (decrypted)
                return EncryptionDecryptionUtils.Decrypt(userRsaPrivateKey, ConfigurationManager.AppSettings[key]);
            else
                return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Returns application config file value with provided key, which is decrypted or not according to the second method parameter.
        /// </summary>
        /// <param name="key">appSetting key</param>
        /// <param name="decrypted">specifies whether value is decrypted or open text value</param>
        /// <param name="externalConfigurationFileFullPath">path to external application configuration file</param>
        /// <param name="userRsaPrivateKey">user private key to decrypt data</param>
        /// <returns>open text value</returns>
        public static string GetConfigurationFileAppSettingValue(string key, bool decrypted, string externalConfigurationFileFullPath, RSAParameters userRsaPrivateKey)
        {
            InitializeCustomConfigurationManager(externalConfigurationFileFullPath);
            if (decrypted)
                return EncryptionDecryptionUtils.Decrypt(userRsaPrivateKey, _configurationManagerObject.AppSettings.Settings[key].Value);
            else
                return _configurationManagerObject.AppSettings.Settings[key].Value;

        }

        /// <summary>
        /// Returns application config file value with provided key, which is decrypted or not according to the second method parameter.
        /// </summary>
        /// <param name="key">connectionString setting key</param>
        /// <param name="decrypted">specifies whether value is decrypted or open text value</param>
        /// <param name="userRsaPrivateKey">user private key to decrypt data</param>
        /// <returns>open text value</returns>
        public static string GetConfigurationFileConnectionStringValue(string key, bool decrypted, RSAParameters userRsaPrivateKey)
        {
            if (decrypted)
                return EncryptionDecryptionUtils.Decrypt(userRsaPrivateKey, ConfigurationManager.ConnectionStrings[key].ConnectionString);
            else
                return ConfigurationManager.ConnectionStrings[key].ConnectionString;
        }

        /// <summary>
        /// Returns application config file value with provided key, which is decrypted or not according to the second method parameter.
        /// </summary>
        /// <param name="key">connectionString setting key</param>
        /// <param name="decrypted">specifies whether value is decrypted or open text value</param>
        /// <param name="externalConfigurationFileFullPath">path to external application configuration file</param>
        /// <param name="userRsaPrivateKey">user private key to decrypt data</param>
        /// <returns>open text value</returns>
        public static string GetConfigurationFileConnectionStringValue(string key, bool decrypted, string externalConfigurationFileFullPath, RSAParameters userRsaPrivateKey)
        {
            InitializeCustomConfigurationManager(externalConfigurationFileFullPath);
            if (decrypted)
                return EncryptionDecryptionUtils.Decrypt(userRsaPrivateKey, _configurationManagerObject.ConnectionStrings.ConnectionStrings[key].ConnectionString);
            else
                return _configurationManagerObject.ConnectionStrings.ConnectionStrings[key].ConnectionString;
        }



        private static ExeConfigurationFileMap _externalConfigurationFileMapper = null;
        private static Configuration _configurationManagerObject = null;

        private static void InitializeCustomConfigurationManager(string externalConfigurationFileFullPath)
        {
            if (_externalConfigurationFileMapper == null)
            {
                _externalConfigurationFileMapper = new ExeConfigurationFileMap();
                _externalConfigurationFileMapper.ExeConfigFilename = externalConfigurationFileFullPath;
                _configurationManagerObject = ConfigurationManager.OpenMappedExeConfiguration(_externalConfigurationFileMapper, ConfigurationUserLevel.None);
            }
        }

    }
}
