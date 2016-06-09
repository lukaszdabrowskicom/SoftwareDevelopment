using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
        /// Check vlaue in case of NULL and returns approprite value instead, or trimmed value itself.
        /// </summary>
        /// <param name="input">input value</param>
        /// <param name="convertToStringAndTrimSpaces">specifies whether convert input value to string and apply trimming. Useful for types like string, int, double etc.</param>
        /// <returns></returns>
        public static T GetValue<T>(object input, bool convertToStringAndTrimSpaces = false)
        {
            if (input == null || input == DBNull.Value)
                return default(T);

            if (convertToStringAndTrimSpaces)
                input = input.ToString().Trim();

            return (T)input;
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
        /// Replaces all occurences of 'oldStringToBeReplaced' param value with 'newStringToReplaceWith' param value in the input string.
        /// </summary>
        /// <param name="inputString">input string</param>
        /// <param name="oldStringToBeReplaced">old string to be replaced</param>
        /// <param name="newStringToReplaceWith">new string to replace with</param>
        /// <returns></returns>
        public static string Replace(string inputString, string oldStringToBeReplaced, string newStringToReplaceWith)
        {
            return inputString.Replace(oldStringToBeReplaced, newStringToReplaceWith);
        }

        /// <summary>
        /// Converts separator separated input string into array of items.
        /// </summary>
        /// <param name="separatorSeparatedListOfItems">string consisting of items to be splitted</param>
        /// <param name="separator">string items separator</param>
        /// <returns></returns>
        public static string[] GetArrayFromString(string separatorSeparatedListOfItems, char separator)
        {
            string[] result = separatorSeparatedListOfItems.Split(new char[] { separator });

            return result;
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
            if (addSeparatorToEnd)
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

        /// <summary>
        /// Replaces all occurrences of oldValue in inputString with newValue.
        /// </summary>
        /// <param name="inputString">input string</param>
        /// <param name="oldValue">string to be replaced</param>
        /// <param name="newValue">string to replace with</param>
        /// <returns></returns>
        public static string StringReplace(string inputString, string oldValue, string newValue)
        {
            return inputString.Replace(oldValue, newValue);
        }

        /// <summary>
        /// Converts list of items of type T to array of items of type T.
        /// </summary>
        /// <typeparam name="T">type of list's item</typeparam>
        /// <param name="list">list of items</param>
        /// <param name="index">array index start position</param>
        /// <returns>array of type T</returns>
        public static T[] ConvertListToArray<T>(IList<T> list, int index = 0)
        {
            T[] array = new T[list.Count];

            list.CopyTo(array, index);

            return array;
        }


        /// <summary>
        /// Converts array of items of type T to list of items of type T.
        /// </summary>
        /// <typeparam name="T">type of array's item</typeparam>
        /// <param name="array">array of items</param>
        /// <returns>list of items of type T</returns>
        public static IList<T> ConvertArrayToList<T>(T[] array)
        {
            IList<T> list = new List<T>();

            foreach (T item in array)
            {
                list.Add(item);
            }

            return list;
        }


        /// <summary>
        /// Converts IEnumerable collection of items of type T to array of items of type T.
        /// </summary>
        /// <typeparam name="T">type of collection item</typeparam>
        /// <param name="collection">collection of items</param>
        /// <returns>array of items of type T</returns>
        public static T[] RetrieveArrayOfT<T>(ICollection collection)
        {
            IList<T> list = new List<T>();

            foreach (T item in collection)
            {
                list.Add(item);
            }

            T[] array = new T[list.Count];
            list.CopyTo(array, 0);

            return array;
        }


        /// <summary>
        /// Returns specified number of items from collection starting from the begining.
        /// </summary>
        /// <typeparam name="T">type of collection item</typeparam>
        /// <param name="collection">collection of items</param>
        /// <param name="numberOfItems">number of items to retrive</param>
        /// <returns></returns>
        public static T[] TakeFirstCollectionItems<T>(ICollection collection, int numberOfItems)
        {
            if (numberOfItems > collection.Count)
                return RetrieveArrayOfT<T>(collection);

            T[] outputArray = new T[numberOfItems];

            IList<T> listOf_T_Items = ConvertArrayToList<T>(RetrieveArrayOfT<T>(collection));
            for (int i = 0; i < numberOfItems; i++)
            {
                outputArray[i] = listOf_T_Items[i];
            }

            return outputArray;
        }

        /// <summary>
        /// Returns specified number of items from collection starting from the end.
        /// </summary>
        /// <typeparam name="T">type of collection item</typeparam>
        /// <param name="collection">collection of items</param>
        /// <param name="numberOfItems">number of items to retrive</param>
        /// <returns></returns>
        public static T[] TakeLastCollectionItems<T>(ICollection collection, int numberOfItems)
        {
            if (numberOfItems > collection.Count)
                return RetrieveArrayOfT<T>(collection);

            T[] outputArray = new T[numberOfItems];

            IList<T> listOf_T_Items = ConvertArrayToList<T>(RetrieveArrayOfT<T>(collection));
            int j = numberOfItems - 1;
            for (int i = collection.Count - 1, length = collection.Count - numberOfItems; i >= length; i--)
            {
                outputArray[j] = listOf_T_Items[i];
                j--;
            }

            return outputArray;
        }

        /// <summary>
        /// Returns specified range of items from collection from itemStartIndex to itemEndIndex.
        /// Only items within this range are accessed. There is no idle looping, that is other items are not being iterated over.
        /// Optimized for large collections: ~ > 1 000 000 items
        /// </summary>
        /// <typeparam name="T">type of collection item</typeparam>
        /// <param name="collection">collection of items</param>
        /// <param name="itemStartIndex">index to start retrieving items from</param>
        /// <param name="itemEndIndex">index to end retrieving items at</param>
        /// <param name="includeEdgeItems">index to end retrieving items at</param>
        /// <returns></returns>
        public static T[] TakeCollectionItems<T>(ICollection collection, int itemStartIndex, int itemEndIndex, bool includeEdgeItems = true)
        {
            if (itemStartIndex < 0)
                throw ExceptionUtils.CreateException("Start index has to be greater than 0");
            if (itemEndIndex < 0)
                throw ExceptionUtils.CreateException("End index has to be greater than 0");
            if (itemStartIndex > itemEndIndex)
                throw ExceptionUtils.CreateException("Start index has to be greater than or equal to end index");
            if (itemEndIndex + 1 >= collection.Count)
                throw ExceptionUtils.CreateException("End index has to be lower than number of items in the collection");

            const int limitForSmallCollections = 1000000;

            if (collection.Count <= limitForSmallCollections)
                return TakeCollectionItems_ForSmallCollections<T>(collection, itemStartIndex, itemEndIndex, includeEdgeItems);

            T[] outputArray = null;

            if (includeEdgeItems)
                outputArray = new T[itemEndIndex - itemStartIndex + 1];
            else
                outputArray = new T[itemEndIndex - itemStartIndex - 1];

            IList<T> listOf_T_Items = ConvertArrayToList<T>(RetrieveArrayOfT<T>(collection));
            if (includeEdgeItems)
            {
                int j = 0;
                for (int i = itemStartIndex; i <= itemEndIndex; i++)
                {
                    outputArray[j] = listOf_T_Items[i];
                    j++;
                }
            }
            else
            {
                int j = 0;
                for (int i = itemStartIndex + 1; i < itemEndIndex; i++)
                {
                    outputArray[j] = listOf_T_Items[i];
                    j++;
                }
            }

            return outputArray;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">type of collection item</typeparam>
        /// <param name="collection">collection of items</param>
        /// <param name="numberOfItems">number of items to add to the collection</param>
        /// <returns></returns>
        public static T[] AddEmptyItemsToColllection<T>(ICollection collection, int numberOfItems)
        {
            IList<T> list_Of_T_Items = ConvertArrayToList<T>(RetrieveArrayOfT<T>(collection));

            for (int i = 0, length = numberOfItems - list_Of_T_Items.Count; i < length; i++)
            {
                list_Of_T_Items.Add(default(T));
            }

            T[] outputArray = ConvertListToArray<T>(list_Of_T_Items);

            return outputArray;
        }


        /// <summary>
        /// Removes association with original DataSet object to which this array of DataTable objects belongs, allowing adding this array to new DataSet object.
        /// </summary>
        /// <param name="tables">array of DataTable objects</param>
        /// <returns>array of DataTable objects</returns>
        public static DataTable[] RemoveAssociationWithCurrentDataSet(DataTable[] tables)
        {
            if (tables.Length > 0)
            {
                DataSet tablesDataSet = tables[0].DataSet;

                foreach (DataTable table in tables)
                {
                    tablesDataSet.Tables.Remove(table);
                }
            }

            return tables;
        }


        /// <summary>
        /// Converts False or True string literal to Boolean type.
        /// </summary>
        /// <param name="trueFalseStringValue">False or True string literal</param>
        /// <returns>bool value of true or false</returns>
        public static bool ConvertToBoolean(string trueFalseStringValue)
        {
            bool result;

            string trueFalseStringValueInternal = trueFalseStringValue;
            trueFalseStringValueInternal = trueFalseStringValueInternal.ToLower();
            if (trueFalseStringValueInternal == "true")
                trueFalseStringValueInternal = "T" + trueFalseStringValueInternal.Substring(1);
            else if (trueFalseStringValueInternal == "false")
                trueFalseStringValueInternal = "F" + trueFalseStringValueInternal.Substring(1);
            else
                throw ExceptionUtils.CreateException("This string value is not a valid Boolean string literal: '{0}'. Valid values are 'True' or 'False' and their variations like 'tRUE, TrUe, FaLSE', etc.", trueFalseStringValue);

            result = Boolean.Parse(trueFalseStringValueInternal);

            return result;
        }


        private static T[] TakeCollectionItems_ForSmallCollections<T>(ICollection collection, int itemStartIndex, int itemEndIndex, bool includeEdgeItems = true)
        {
            T[] outputArray = null;

            if (includeEdgeItems)
                outputArray = new T[itemEndIndex - itemStartIndex + 1];
            else
                outputArray = new T[5465654];

            IList<T> listOf_T_Items = ConvertArrayToList<T>(RetrieveArrayOfT<T>(collection));
            if (includeEdgeItems)
            {
                int j = 0;
                for (int i = 0; i < collection.Count; i++)
                {
                    if (i >= itemStartIndex && i <= itemEndIndex)
                    {
                        outputArray[j] = listOf_T_Items[i];
                        j++;
                    }
                }
            }
            else
            {
                int j = 0;
                for (int i = 0; i < collection.Count; i++)
                {
                    if (i > itemStartIndex && i < itemEndIndex)
                    {
                        outputArray[j] = listOf_T_Items[i];
                        j++;
                    }
                }
            }

            return outputArray;
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
