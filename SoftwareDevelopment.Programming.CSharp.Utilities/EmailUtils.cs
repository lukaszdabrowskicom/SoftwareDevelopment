using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common email operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class EmailUtils
    {
        /// <summary>
        /// credential user
        /// </summary>
        public static string CREDENTIAL_USER;

        /// <summary>
        /// credential password
        /// </summary>
        public static string CREDENTIAL_PASSWORD;

        /// <summary>
        /// host
        /// </summary>
        public static string HOST;

        private static int _port = -1;
        /// <summary>
        /// port
        /// </summary>
        public static int PORT;

        /// <summary>
        /// connection timeout
        /// </summary>
        public static int CONNECTION_TIMEOUT;
        /// <summary>
        /// sender
        /// </summary>
        public static string SENDER;
        /// <summary>
        /// sender friendly name
        /// </summary>
        public static string SENDER_FRIENDLY_NAME;

        /// <summary>
        /// mail subject
        /// </summary>
        public static string MAIL_SUBJECT;

        /// <summary>
        //try-catch acts as as simple precaution in case you just need to use other util method rather than sending email, in which case exception will probably be thrown due to lack of config file.
        /// </summary>
        static EmailUtils()
        {
            try
            {
                EncryptionDecryptionUtils.ThrowExceptionIfRsaPrivateUserKeyIsNotInitialized();

                CREDENTIAL_USER = ConfigurationUtils.GetConfigurationFileAppSettingValue("credential_user", false, EncryptionDecryptionUtils.RSA_PRIVATE_USER_KEY);
                CREDENTIAL_PASSWORD = ConfigurationUtils.GetConfigurationFileAppSettingValue("credential_password", true, EncryptionDecryptionUtils.RSA_PRIVATE_USER_KEY);
                HOST = ConfigurationUtils.GetConfigurationFileAppSettingValue("host", true, EncryptionDecryptionUtils.RSA_PRIVATE_USER_KEY);
                PORT = Int32.Parse(ConfigurationUtils.GetConfigurationFileAppSettingValue("port", false, EncryptionDecryptionUtils.RSA_PRIVATE_USER_KEY));
                CONNECTION_TIMEOUT = Int32.Parse(ConfigurationUtils.GetConfigurationFileAppSettingValue("timeout", false, EncryptionDecryptionUtils.RSA_PRIVATE_USER_KEY));
                SENDER = ConfigurationUtils.GetConfigurationFileAppSettingValue("sender", false, EncryptionDecryptionUtils.RSA_PRIVATE_USER_KEY);
                SENDER_FRIENDLY_NAME = ConfigurationUtils.GetConfigurationFileAppSettingValue("senderFriendlyName", false, EncryptionDecryptionUtils.RSA_PRIVATE_USER_KEY);
                MAIL_SUBJECT = ConfigurationUtils.GetConfigurationFileAppSettingValue("mailSubject", false, EncryptionDecryptionUtils.RSA_PRIVATE_USER_KEY);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets recipients and their aliases arrays respectively.
        /// </summary>
        /// <param name="recipientsConfigurationFileData">comma separated list of recipient addresses</param>
        /// <param name="recipientsSeparator">recipient address separator</param>
        /// <param name="applyTranslation">whether to translate an alias to native tongue equivalent</param>
        /// <param name="old_new_letter_pair_list_FirstName">list of letter pairs of first name</param>
        /// <param name="old_new_letter_pair_LastName">list of letter pairs of last name</param>
        /// <param name="old_new_letter_pair_separator">separator of list pairs</param>
        /// <param name="old_new_letter_separator">separator of old & new letter</param>
        /// <param name="keepSideBySideDoubleLetters">whether to preserve double letters that are side by side</param>
        /// <param name="applyTranslationToFirstName">specifies whether to apply translation to first name</param>
        /// <param name="applyTranslationToLastName">specifies whether to apply translation to last name</param>
        /// <returns></returns>
        public static string[][] GetRecipientsAndTheirAliases(
                                                                string recipientsConfigurationFileData,
                                                                char recipientsSeparator,
                                                                bool applyTranslation = false,
                                                                string old_new_letter_pair_list_FirstName = "",
                                                                string old_new_letter_pair_list_LastName = "",
                                                                char old_new_letter_pair_separator = ';',
                                                                char old_new_letter_separator = ',',
                                                                bool keepSideBySideDoubleLetters = false,
                                                                bool applyTranslationToFirstName = false,
                                                                bool applyTranslationToLastName = false
                                                             )
        {
            string[][] result = new string[2][];


            string[] clientEmailListOfMissingDictionaryEntires = recipientsConfigurationFileData.Split(new char[] { recipientsSeparator }, StringSplitOptions.RemoveEmptyEntries);

            string[] recipientsOfMissingDictionaryEntires = new string[clientEmailListOfMissingDictionaryEntires.Length];
            string[] recipientsAliasesOfMissingDictionaryEntires = new string[clientEmailListOfMissingDictionaryEntires.Length];

            for (int i = 0, length = recipientsOfMissingDictionaryEntires.Length; i < length; i++)
            {
                recipientsOfMissingDictionaryEntires[i] = clientEmailListOfMissingDictionaryEntires[i];
                recipientsAliasesOfMissingDictionaryEntires[i] = GetRecipientFriendlyNameFromRecipientEmailAddress(
                                                                                                                    clientEmailListOfMissingDictionaryEntires[i],
                                                                                                                    applyTranslation,
                                                                                                                    old_new_letter_pair_list_FirstName,
                                                                                                                    old_new_letter_pair_list_LastName,
                                                                                                                    old_new_letter_pair_separator,
                                                                                                                    old_new_letter_separator,
                                                                                                                    keepSideBySideDoubleLetters,
                                                                                                                    applyTranslationToFirstName,
                                                                                                                    applyTranslationToLastName
                                                                                                                  );
            }

            result[0] = recipientsOfMissingDictionaryEntires;
            result[1] = recipientsAliasesOfMissingDictionaryEntires;


            return result;
        }



        /// <summary>
        /// Sends email to desired recipients with ability to provide one or multiple attachements.
        /// Bcc feature to be implemented in the future versions of this library.
        /// </summary>
        /// <param name="sender">email address of sender of this email</param>
        /// <param name="senderAlias">friendly name of sender of this email</param>
        /// <param name="recipients">array of recipients of this email</param>
        /// <param name="recipientsAliases">array of friendly names of recipients of this email</param>
        /// <param name="subject">email subject</param>
        /// <param name="mailContent">email content</param>
        /// <param name="connectionTiemout">time in which email should be send to recipients</param>
        /// <param name="host">host</param>
        /// <param name="port">port</param>
        /// <param name="renderAsHTML">whether to send email as HTML content or plain text</param>
        /// <param name="useDefaultCredentials">whether to use credentials associated with the user in which context of email is being send or not</param>
        /// <param name="credentialUser">user name having sufficient privilages allowing for sending email</param>
        /// <param name="credentialPassword">user password associated with the user</param>
        /// <param name="smtpDeliveryMethod">method of sending email</param>
        /// <param name="enableSsl">whether to enable SSL or not</param>
        /// <param name="attachementFilePathArray">array of full path file to be send as attachement</param>
        /// <returns>void</returns>
        public static void SendEmail(
                                        string sender, string senderAlias, string[] recipients, string[] recipientsAliases,
                                        string subject, string mailContent,
                                        int connectionTiemout,
                                        string host, int port,
                                        bool renderAsHTML = false,
                                        bool useDefaultCredentials = true,
                                        string credentialUser = "", string credentialPassword = "",
                                        SmtpDeliveryMethod smtpDeliveryMethod = SmtpDeliveryMethod.Network,
                                        bool enableSsl = true,
                                        string[] attachementFilePathArray = null
                                     )
        {
            MailMessage mail = new MailMessage();
            mail.Subject = subject;
            mail.Body = mailContent;
            mail.IsBodyHtml = renderAsHTML;

            if (recipients.Length != recipientsAliases.Length)
            {
                throw ExceptionUtils.CreateException("Number of recipients and recipientsAliases must match.");
            }

            for (int i = 0, length = recipients.Length; i < length; i++)
            {
                mail.To.Add(new MailAddress(recipients[i], recipientsAliases[i]));
            }

            mail.From = new MailAddress(sender, senderAlias);

            SmtpClient client = new SmtpClient();
            client.Host = host;
            client.Port = port;
            client.DeliveryMethod = smtpDeliveryMethod;
            if (useDefaultCredentials)
                client.UseDefaultCredentials = useDefaultCredentials;
            else
            {
                if (String.IsNullOrEmpty(credentialUser) || String.IsNullOrEmpty(credentialPassword))
                    throw ExceptionUtils.CreateException("Provide values for both  '{0}' and '{1}'", "credentialUser", "credentialPassword");
                client.Credentials = new NetworkCredential(credentialUser, credentialPassword);
            }
            client.EnableSsl = enableSsl;
            client.Timeout = connectionTiemout;
            if (attachementFilePathArray != null)
            {
                foreach (string fullFilePath in attachementFilePathArray)
                {
                    mail.Attachments.Add(new Attachment(fullFilePath));
                }
            }

            client.Send(mail);
        }


        private static string GetRecipientFriendlyNameFromRecipientEmailAddress(
                                                                                 string recipientEmailAddress, bool applyTranslation,
                                                                                 string old_new_letter_pair_list_FirstName,
                                                                                 string old_new_letter_pair_list_LastName,
                                                                                 char old_new_letter_pair_separator, char old_new_letter_separator,
                                                                                 bool keepSideBySideDoubleLetters,
                                                                                 bool applyTranslationToFirstName,
                                                                                 bool applyTranslationToLastName
                                                                               )
        {
            string recipientFriendlyName = String.Empty;
            string space = " ";

            string part1 = recipientEmailAddress.Split(new char[] { '@' })[0];
            string[] username_usersurname = part1.Split(new char[] { '.' });
            if (username_usersurname.Length > 0)
            {
                if (applyTranslation)
                {
                    if (username_usersurname.Length == 1)
                    {
                        recipientFriendlyName = username_usersurname[0].Substring(0, 1).ToUpper() + username_usersurname[0].Substring(1);
                    }
                    else if (username_usersurname.Length == 2)
                    {
                        recipientFriendlyName = username_usersurname[0].Substring(0, 1).ToUpper() + username_usersurname[0].Substring(1) +
                                                space +
                                                username_usersurname[1].Substring(0, 1).ToUpper() + username_usersurname[1].Substring(1);
                    }

                    recipientFriendlyName = ApplyTranslation(recipientFriendlyName, old_new_letter_pair_list_FirstName, old_new_letter_pair_list_LastName, old_new_letter_pair_separator, old_new_letter_separator, keepSideBySideDoubleLetters, applyTranslationToFirstName, applyTranslationToLastName);
                }
                else
                {
                    if (username_usersurname.Length == 1)
                    {
                        recipientFriendlyName = username_usersurname[0].Substring(0, 1).ToUpper() + username_usersurname[0].Substring(1);
                    }
                    else if (username_usersurname.Length == 2)
                    {
                        recipientFriendlyName = username_usersurname[0].Substring(0, 1).ToUpper() + username_usersurname[0].Substring(1) +
                                                space +
                                                username_usersurname[1].Substring(0, 1).ToUpper() + username_usersurname[1].Substring(1);
                    }
                }
            }

            return recipientFriendlyName.Trim();
        }


        private static string ApplyTranslation(
                                                string recipientFriendlyName,
                                                string old_new_letter_pair_list_FirstName,
                                                string old_new_letter_pair_list_LastName,
                                                char old_new_letter_pair_separator, char old_new_letter_separator,
                                                bool keepSideBySideDoubleLetters,
                                                bool applyTranslationToFirstName,
                                                bool applyTranslationToLastName
                                              )
        {
            IList<char> translatedAliasCharArray = new List<char>();

            string[] firstName_lastName_Array = recipientFriendlyName.Split(new char[] { ' ' });

            string[] firstNamePairs = old_new_letter_pair_list_FirstName.Split(new char[] { old_new_letter_pair_separator }, StringSplitOptions.RemoveEmptyEntries);
            string[] lastNamePairs = old_new_letter_pair_list_LastName.Split(new char[] { old_new_letter_pair_separator }, StringSplitOptions.RemoveEmptyEntries);

            if (applyTranslationToFirstName && applyTranslationToLastName)
            {
                ApplyCoreTranslationInternal(firstNamePairs, firstName_lastName_Array[0].Trim(), old_new_letter_separator, keepSideBySideDoubleLetters, ref translatedAliasCharArray);
                AddSpaceCharToArray(ref translatedAliasCharArray);
                ApplyCoreTranslationInternal(lastNamePairs, firstName_lastName_Array[1].Trim(), old_new_letter_separator, keepSideBySideDoubleLetters, ref translatedAliasCharArray);
            }
            else if (applyTranslationToFirstName)
            {
                ApplyCoreTranslationInternal(firstNamePairs, firstName_lastName_Array[0].Trim(), old_new_letter_separator, keepSideBySideDoubleLetters, ref translatedAliasCharArray);
                AddSpaceCharToArray(ref translatedAliasCharArray);
                AddCharsToArray(firstName_lastName_Array[1].Trim().ToCharArray(), keepSideBySideDoubleLetters, ref translatedAliasCharArray);

            }
            else if (applyTranslationToLastName)
            {
                AddCharsToArray(firstName_lastName_Array[0].Trim().ToCharArray(), keepSideBySideDoubleLetters, ref translatedAliasCharArray);
                AddSpaceCharToArray(ref translatedAliasCharArray);
                ApplyCoreTranslationInternal(lastNamePairs, firstName_lastName_Array[1].Trim(), old_new_letter_separator, keepSideBySideDoubleLetters, ref translatedAliasCharArray);
            }

            return MiscUtils.StringJoin(String.Empty, translatedAliasCharArray);
        }

        private static void ApplyCoreTranslationInternal(string[] pairs, string recipientAliasPart, char old_new_letter_separator, bool keepSideBySideDoubleLetters, ref IList<char> translatedAliasCharArray)
        {
            foreach (string old_new_letter_pair in pairs)
            {
                string[] old_new_letter_pair_array = old_new_letter_pair.Split(new char[] { old_new_letter_separator }, StringSplitOptions.RemoveEmptyEntries);
                recipientAliasPart = recipientAliasPart.Replace(old_new_letter_pair_array[0], old_new_letter_pair_array[1]);
            }

            ProcessDoubleLetters(recipientAliasPart, keepSideBySideDoubleLetters, ref translatedAliasCharArray);
        }

        private static void AddCharsToArray(char[] inputCharsArray, bool keepSideBySideDoubleLetters, ref IList<char> charsList)
        {
            ProcessDoubleLetters(new string(inputCharsArray), keepSideBySideDoubleLetters, ref charsList);
        }

        private static void AddSpaceCharToArray(ref IList<char> charsList)
        {
            charsList.Add(' ');
        }

        private static void ProcessDoubleLetters(string aliasPart, bool keepSideBySideDoubleLetters, ref IList<char> aliasPartChars)
        {
            char previousCharacter = Char.MinValue;
            char currentCharacter = Char.MinValue;

            foreach (char currentChar in aliasPart)
            {
                if (previousCharacter != Char.MinValue && currentCharacter != Char.MinValue)
                {
                    currentCharacter = currentChar;
                    if ((currentCharacter == previousCharacter) && !keepSideBySideDoubleLetters)
                    {
                        continue;
                    }
                    else
                    {
                        previousCharacter = currentCharacter;
                        aliasPartChars.Add(currentChar);
                    }
                }
                else
                {
                    previousCharacter = currentCharacter = currentChar;
                    aliasPartChars.Add(currentChar);
                }
            }
        }
    }
}
