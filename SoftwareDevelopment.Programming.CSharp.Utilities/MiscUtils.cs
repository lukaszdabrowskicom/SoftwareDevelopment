﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for miscellaneous operations that do not fit into other utilities classes.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class MiscUtils
    {
        /// <summary>
        /// Converts byte array to string.
        /// </summary>
        /// <param name="inputByteArray">array of bytes</param>
        /// <returns>bytes array converted to string representation</returns>
        public static string BytesToString(byte[] inputByteArray)
        {
            byte[] bytes = inputByteArray;
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb = sb.Append(b.ToString("X").PadLeft(2, '0'));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts bytes array converted to string representation back to bytes array.
        /// </summary>
        /// <param name="inputByteArrayString">string representation of bytes array</param>
        /// <returns>byte[]</returns>
        public static byte[] StringToByteArray(string inputByteArrayString)
        {
            char[] inputByteArrayChars = inputByteArrayString.ToCharArray();
            byte[] outputByteArray = new byte[inputByteArrayChars.Length];

            for (int i = 0, length = inputByteArrayChars.Length; i < length; i++)
            {
                outputByteArray[i] = (byte)inputByteArrayChars[i];
            }

            return outputByteArray;
        }

        /// <summary>
        /// Decrypts encrypted string back to plain text.
        /// </summary>
        /// <param name="encryptedText">encrypted text</param>
        /// <param name="userRsaPrivateKey">user private key to decrypt data</param>
        /// <returns>plain text</returns>
        public static string GetDecryptedString(string encryptedText, RSAParameters userRsaPrivateKey)
        {
            return EncryptionDecryptionUtils.Decrypt(userRsaPrivateKey, encryptedText);
        }

        /// <summary>
        /// Validates input string by applying simple 'contains' rule or 'regex' rule and returns bool flag as a result of this validation.
        /// </summary>
        /// <param name="input">input string to be validated</param>
        /// <param name="complexValidation">true -> regex validation takes place, false -> simple inputString contains pattern operation takes place)</param>
        /// <param name="pattern">paattern to validate input string</param>
        /// <returns></returns>
        public static bool ValidateInputString(string input, bool complexValidation, string pattern)
        {
            bool isValid = false;

            if (complexValidation)
                isValid = Regex.IsMatch(input, pattern);
            else
                isValid = input.Contains(pattern);

            return isValid;
        }

        /// <summary>
        /// Removes from a string characters that are considered as invalid path characters.
        /// </summary>
        /// <param name="path">file path to remove characters from</param>
        /// <returns>normalized path</returns>
        public static string NormalizePath(string path)
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();

            for (int i = 0, length = invalidPathChars.Length; i < length; i++)
            {
                path = path.Replace(invalidPathChars[i].ToString(), String.Empty);
            }

            return path;
        }
        /// <summary>
        /// Removes spaces from string.
        /// </summary>
        /// <param name="inputString">input string</param>
        /// <returns>string</returns>
        public static string RemoveSpacesFromString(string inputString)
        {
           return Regex.Replace(inputString, @"\b\s+\b", String.Empty);
        }

        /// <summary>
        /// Concatenates array of strings using provided 'separator'.
        /// </summary>
        /// <param name="separator">separator to join array strings with</param>
        /// <param name="array">array of strings</param>
        /// <param name="addSeparatorToEnd">whether to add separator to the end of a joined string or not</param>
        /// <returns>string</returns>
        public static string StringJoin(string separator, string[] array, bool addSeparatorToEnd = false)
        {
            if(addSeparatorToEnd)
                return String.Join(separator, array) + separator;
            else
                return String.Join(separator, array);
        }

        /// <summary>
        /// Concatenates array of chars using provided 'separator'.
        /// </summary>
        /// <param name="separator">separator to join array chars with</param>
        /// <param name="array">array of chars</param>
        /// <param name="addSeparatorToEnd">whether to add separator to the end of a joined string or not</param>
        /// <returns>string</returns>
        public static string StringJoin(string separator, char[] array, bool addSeparatorToEnd = false)
        {
            if (addSeparatorToEnd)
                return String.Join(separator, array) + separator;
            else
                return String.Join(separator, array);
        }

        /// <summary>
        /// Concatenates list of chars using provided 'separator'.
        /// </summary>
        /// <param name="separator">separator to join list chars with</param>
        /// <param name="array">list of chars</param>
        /// <param name="addSeparatorToEnd">whether to add separator to the end of a joined string or not</param>
        /// <returns>string</returns>
        public static string StringJoin(string separator, IList<char> array, bool addSeparatorToEnd = false)
        {
            if (addSeparatorToEnd)
                return String.Join(separator, array) + separator;
            else
                return String.Join(separator, array);
        }

        /// <summary>
        /// Concatenates array of strings using provided 'separator'.
        /// </summary>
        /// <param name="separator">separator to join array strings with</param>
        /// <param name="list">list of strings</param>
        /// <param name="addSeparatorToEnd">whether to add separator to the end of a joined string or not</param>
        /// <returns></returns>
        public static string StringJoin(string separator, IList<string> list, bool addSeparatorToEnd = false)
        {
            if (addSeparatorToEnd)
                return String.Join(separator, list) + separator;
            else
                return String.Join(separator, list);
        }

        /// <summary>
        /// Converts xml string to well formed xml.
        /// </summary>
        /// <param name="xmlString">xml data</param>
        /// <returns>well formed xml string</returns>
        public static string ConvertXmlStringToWellFormedXml(string xmlString)
        {
            StringBuilder wellFormedXmlBuilder = new StringBuilder();
            string openingXmlTagFormat = "<{0}";
            const string closeTagSign = ">";
            string closingXmlTagFormat = "</{0}>";
            const string space = " ";
            string enter = new string(new char[] { (char)13, (char)10 });

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            foreach (XmlNode rootNode in xmlDoc.ChildNodes)
            {
                wellFormedXmlBuilder.AppendFormat(openingXmlTagFormat, RemoveEnters(rootNode.Name));
                foreach (XmlAttribute xmlAttribute in rootNode.Attributes)
                {
                    wellFormedXmlBuilder.Append(space + RemoveEnters(xmlAttribute.OuterXml));
                }
                wellFormedXmlBuilder.AppendLine(closeTagSign);

                int indentationLevel = -1;
                foreach (XmlNode xmlNode in rootNode.ChildNodes)
                {
                    indentationLevel = 2;
                    AddWellFormedXmlNodeInternal(xmlNode, wellFormedXmlBuilder, openingXmlTagFormat, closingXmlTagFormat, closeTagSign, indentationLevel);
                }

                wellFormedXmlBuilder.AppendFormat(closingXmlTagFormat, rootNode.Name).Append(enter);
            }

            return wellFormedXmlBuilder.ToString();
        }

        /// <summary>
        /// Removes new line breakers '\r\n' from multiline string.
        /// </summary>
        /// <param name="stringValue">input string</param>
        /// <returns>input string without new line breakers '\r\n'</returns>
        public static string RemoveEnters(string stringValue)
        {
            stringValue = stringValue.Replace((char)10, (char)32);
            stringValue = stringValue.Replace((char)13, (char)32);

            return stringValue;
        }

        private static void AddWellFormedXmlNodeInternal(XmlNode xmlNode, StringBuilder xmlNodesBuilder, string openingXmlTagFormat, string closingXmlTagFormat, string closeTagSign, int indentationLevel)
        {
            const string space = " ";
            string enter = new string(new char[] { (char)13, (char)10 });

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                if (childNode.NodeType == XmlNodeType.Element && childNode == xmlNode.FirstChild)
                {
                    xmlNodesBuilder.AppendFormat(ApplyIndentationInternal(indentationLevel) + openingXmlTagFormat, RemoveEnters(xmlNode.Name));
                    foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
                    {
                        xmlNodesBuilder.Append(space + RemoveEnters(xmlAttribute.OuterXml));
                    }
                    xmlNodesBuilder.Append(closeTagSign).Append(enter);
                }
                else if (childNode.NodeType == XmlNodeType.Text)
                {
                    xmlNodesBuilder.AppendFormat(ApplyIndentationInternal(indentationLevel) + openingXmlTagFormat, RemoveEnters(xmlNode.Name));
                    xmlNodesBuilder.Append(closeTagSign);
                    xmlNodesBuilder.Append(RemoveEnters(xmlNode.InnerText));
                }

                if (childNode.NodeType == XmlNodeType.Element && childNode.HasChildNodes)
                {
                    AddWellFormedXmlNodeInternal(childNode, xmlNodesBuilder, openingXmlTagFormat, closingXmlTagFormat, closeTagSign, indentationLevel + 2);
                }

                if (childNode.NodeType == XmlNodeType.Element && childNode.HasChildNodes && childNode.ChildNodes[0].NodeType == XmlNodeType.Text)
                {
                    xmlNodesBuilder.AppendFormat(closingXmlTagFormat, RemoveEnters(childNode.Name)).Append(enter);
                }

                if (childNode.NodeType == XmlNodeType.Element && childNode == xmlNode.LastChild && childNode.ParentNode == xmlNode)
                {
                    xmlNodesBuilder.AppendFormat(ApplyIndentationInternal(indentationLevel) + closingXmlTagFormat, RemoveEnters(xmlNode.Name)).Append(enter);
                }
            }
        }

        private static string ApplyIndentationInternal(int level)
        {
            string result = String.Empty;
            for (int i = 0; i < level; i++)
            {
                result += " ";
            }
            return result;
        }

    }
}
