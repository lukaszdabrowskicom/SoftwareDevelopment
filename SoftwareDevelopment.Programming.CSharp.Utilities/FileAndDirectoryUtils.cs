using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SoftwareDevelopment.Programming.CSharp.Utilities
{
    /// <summary>
    /// Util class for common file and directory operations.
    /// </summary>
    [Obfuscation(ApplyToMembers = true, Exclude = false, StripAfterObfuscation = true)]
    public static class FileAndDirectoryUtils
    {
        private const string _fileExtensionPattern = @"\.[a-zA-Z]{3,4}$";
        private const string _fileNameUnwantedCharacters = @"[\s:]";

        /// <summary>
        /// Creates file name based on provided array of strings.
        /// </summary>
        /// <param name="fileNamePartsWithoutDotAndExtension">array of strings</param>
        /// <returns>file name</returns>
        public static string ComposeFileNameWithoutExtension(params string[] fileNamePartsWithoutDotAndExtension)
        {
            string fileName = String.Empty;

            fileName = String.Concat(fileNamePartsWithoutDotAndExtension);

            return fileName;
        }

        /// <summary>
        /// Creates full path to file or subdirectory.
        /// </summary>
        /// <param name="directory">name of a directory</param>
        /// <param name="fileOrDirectoryName">name of a file or subdirectory</param>
        /// <returns>full path of a file</returns>
        public static string ComposeFullPath(string directory, string fileOrDirectoryName)
        {
            string fullPath = String.Empty;

            fullPath = Path.Combine(directory, fileOrDirectoryName);

            return fullPath;
        }

        /// <summary>
        /// Returns file name with extension part of a file path.
        /// </summary>
        /// <param name="path">path to file</param>
        /// <returns>file name with extension</returns>
        public static string GetFileNameWithExtension(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Returns string array of file name and extension.
        /// </summary>
        /// <param name="fileNameWithExtension">file name with extension</param>
        /// <returns>string array of file name and extension</returns>
        public static string [] GetFileNameAndExtensionSplitted(string fileNameWithExtension)
        {
            return fileNameWithExtension.Split(new char[] { '.' });
        }

        /// <summary>
        /// Returns FileInfo object based on provided file path.
        /// </summary>
        /// <param name="fullPapthToFile">full path to file</param>
        /// <returns>FileInfo object based on provided file path</returns>
        public static FileInfo GetFileInfo(string fullPapthToFile)
        {
            FileInfo fileInfo = new FileInfo(fullPapthToFile);

            return fileInfo;
        }

        /// <summary>
        /// Returns array of file paths for given directory name and search pattern.
        /// </summary>
        /// <param name="directoryNameFullPath">full path to directory</param>
        /// <param name="searchPattern">search pattern</param>
        /// <returns>array of file path for given directory name and search pattern</returns>
        public static string[] GetFiles(string directoryNameFullPath, string searchPattern = "")
        {
            const string empty_Pattern = "";
;
            if (searchPattern == empty_Pattern)
            {
                return Directory.GetFiles(directoryNameFullPath);
            }
            else
            {
                return Directory.GetFiles(directoryNameFullPath, searchPattern);
            }
        }

        /// <summary>
        /// Filters array of file paths and returns only those whose creation date is between start and end date.
        /// </summary>
        /// <param name="files">array of file paths</param>
        /// <param name="fromDate">start date</param>
        /// <param name="toDate">end date</param>
        /// <param name="withoutMinutesAndSeconds">whether to filter files based on date part only</param>
        /// <returns>filtered array of file paths</returns>
        public static string[] FilesArrayApplyDateFilter(string[] files, DateTime fromDate, DateTime toDate, bool withoutMinutesAndSeconds = true)
        {
            IList<string> filteredFiles = new List<string>();

            DateTime fileCreatedDate = DateTime.MinValue;
            FileInfo fileInfo = null;
            foreach (string file in files)
            {
                fileInfo = GetFileInfo(file);
                if(withoutMinutesAndSeconds)
                    fileCreatedDate = new DateTime(fileInfo.CreationTime.Year, fileInfo.CreationTime.Month, fileInfo.CreationTime.Day, fileInfo.CreationTime.Hour, 0, 0);
                else
                    fileCreatedDate = new DateTime(fileInfo.CreationTime.Year, fileInfo.CreationTime.Month, fileInfo.CreationTime.Day, fileInfo.CreationTime.Hour, fileInfo.CreationTime.Minute, fileInfo.CreationTime.Second);

                if (fileCreatedDate >= fromDate && fileCreatedDate <= toDate)
                    filteredFiles.Add(file);
            }

            string[] filteredFilesArray = new string[filteredFiles.Count];
            filteredFiles.CopyTo(filteredFilesArray, 0);

            return filteredFilesArray;
        }
        
        /// <summary>
        /// Creates file name with given file name and extension separated.
        /// </summary>
        /// <param name="fileNameWithoutExtension"> file name part</param>
        /// <param name="dotWithExtension">extension of the file including dot</param>
        /// <returns>combined file name and extension including dot</returns>
        public static string CreateFileName(string fileNameWithoutExtension, string dotWithExtension)
        {
            return Regex.Replace(fileNameWithoutExtension, _fileNameUnwantedCharacters, String.Empty) + dotWithExtension;
        }

        /// <summary>
        /// Creates a file physically on provided storage with some content.
        /// </summary>
        /// <param name="fileName">name of a file</param>
        /// <param name="fileContent">content of a file</param>
        /// <param name="isLocalPath">whether directory of a file is a currently executing assembly directory or not</param>
        /// <param name="pathToFileDirectory">full path to a file directory</param>
        /// <returns>full path to physical file</returns>
        public static string CreateFile(string fileName, string fileContent, bool isLocalPath, string pathToFileDirectory = "")
        {
            string outputFile = String.Empty;

            if (String.IsNullOrEmpty(fileName))
                throw ExceptionUtils.CreateException(ExceptionUtils.ArgumentNullException_MessageFormat, "'fileName'");

            if (!Regex.IsMatch(fileName, _fileExtensionPattern))
                throw ExceptionUtils.CreateException(ExceptionUtils.FormatException_MessageFormat, "'fileName'");

            if (!isLocalPath && String.IsNullOrEmpty(pathToFileDirectory))
                throw ExceptionUtils.CreateException(ExceptionUtils.ArgumentNullException_MessageFormat, "'pathToFileDirectory'");

            if (isLocalPath)
            {
                outputFile = Path.Combine(Environment.CurrentDirectory, fileName);
                File.WriteAllText(outputFile, fileContent);
            }
            else if (!isLocalPath)
            {
                outputFile = Path.Combine(pathToFileDirectory, fileName);
                File.WriteAllText(outputFile, fileContent);
            }

            return outputFile;
        }

        /// <summary>
        /// Moves file to another location provided that file specified in the first parameter actually exists.
        /// </summary>
        /// <param name="fileName">full path to a file</param>
        /// <param name="fullPath">location to move the file to</param>
        /// <returns>void</returns>
        public static void MoveFileIfExists(string fileName, string fullPath)
        {
            if (File.Exists(fileName))
                File.Move(fileName, fullPath);
        }

        /// <summary>
        /// Copies file to another location provided that file specified in the first parameter actually exists.
        /// </summary>
        /// <param name="fileName">full path to a file</param>
        /// <param name="fullPath">location to copy the file to</param>
        /// <param name="overrideExistingOne">override existing file</param>
        /// <returns>void</returns>
        public static void CopyFileIfExists(string fileName, string fullPath, bool overrideExistingOne)
        {
            if (File.Exists(fileName))
                File.Copy(fileName, fullPath, overrideExistingOne);
        }

        /// <summary>
        /// Creates or overrides directory.
        /// </summary>
        /// <param name="fullPath">full path to a directory</param>
        /// <param name="overrideExistingOne">whether to override existing directory or not</param>
        /// <returns>void</returns>
        public static void CreateOrOverrideExistingDirectory(string fullPath, bool overrideExistingOne)
        {
            bool exists = Directory.Exists(fullPath);

            if (overrideExistingOne && exists)
            {
                Directory.Delete(fullPath, true);
                exists = false;
            }

            if (!exists)
                Directory.CreateDirectory(fullPath);
        }

        /// <summary>
        /// Creates readonly stream to a file.
        /// </summary>
        /// <param name="path">full path to a file</param>
        /// <returns>FileStream object</returns>
        public static FileStream CreateStream(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            return stream;
        }

        /// <summary>
        /// Creates stream to a file with given fileMode and fileAccess.
        /// </summary>
        /// <param name="path">full path to a file</param>
        /// <param name="fileMode">file mode of the stream</param>
        /// <param name="fileAccess">file access of the file</param>
        /// <returns>FileStream object</returns>
        public static FileStream CreateStream(string path, FileMode fileMode, FileAccess fileAccess)
        {
            FileStream stream = new FileStream(path, fileMode, fileAccess);
            return stream;
        }

        /// <summary>
        /// Closes a stream.
        /// </summary>
        /// <param name="stream">stream to close</param>
        public static void CloseStream(Stream stream)
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }

        }

        /// <summary>
        /// Returns directory name for provided path.
        /// </summary>
        /// <param name="path">directory path</param>
        /// <returns></returns>
        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Creates directory tree. i.e. dir1/dir2/dir3/dir4  etc.
        /// </summary>
        /// <param name="rootDirectory">directory to start with</param>
        /// <param name="arrayOfDirectoryNames">array of directory names</param>
        /// <param name="overrideExistingOnes">specyfies whether override existing directory tree</param>
        /// <returns>full path to the last created directory</returns>
        public static string CreateDirectoryTree(string rootDirectory, string[] arrayOfDirectoryNames, bool overrideExistingOnes = false)
        {
            string resultDirectoryTree = rootDirectory;

            CreateOrOverrideExistingDirectory(resultDirectoryTree, overrideExistingOnes);
            foreach (string dirName in arrayOfDirectoryNames)
            {
                resultDirectoryTree = ComposeFullPath(resultDirectoryTree, dirName);
                CreateOrOverrideExistingDirectory(resultDirectoryTree, overrideExistingOnes);
            }

            return resultDirectoryTree;
        }

        /// <summary>
        /// Creates directory tree. i.e. dir1/dir2/dir3/dir4  etc.
        /// </summary>
        /// <param name="rootDirectory">directory to start with</param>
        /// <param name="listOfDirectoryNames">list of directory names</param>
        /// <param name="overrideExistingOnes">specyfies whether override existing directory tree</param>
        /// <returns>full path to the last created directory</returns>
        public static string CreateDirectoryTree(string rootDirectory, List<string> listOfDirectoryNames, bool overrideExistingOnes = false)
        {
            string[] array = MiscUtils.ConvertListToArray<string>(listOfDirectoryNames);

           return CreateDirectoryTree(rootDirectory, array, overrideExistingOnes);
        }
    }
}
