using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MTool.AppBuilder.Editor.Builds
{
    internal static class EditorUtils
    {
        public static List<string> CopyDirecotryToDestination(string sourceFolderPath, string desFolderPath, Func<string, bool> filter = null)
        {

            if (!Directory.Exists(sourceFolderPath))
            {
                throw new DirectoryNotFoundException("SourceFolderPath is not exist ! sourceFolderPath->" + sourceFolderPath);
            }

            List<string> currentFileList = new List<string>();

            ClearAndCreateDirectory(desFolderPath);

            sourceFolderPath = Path.GetFullPath(sourceFolderPath);
            sourceFolderPath = sourceFolderPath.Replace(@"\", @"/");
            DirectoryInfo dirInfo = new DirectoryInfo(sourceFolderPath);

            FileInfo[] fileInfos = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (var fileInfo in fileInfos)
            {
                string fileFullName = Path.GetFullPath(fileInfo.FullName);
                fileFullName = fileFullName.Replace(@"\", @"/");

                if (filter != null && filter(fileFullName))
                {
                    continue;
                }

                string newFileInfoName = fileFullName.Replace(sourceFolderPath, "");
                newFileInfoName = desFolderPath + newFileInfoName;

                newFileInfoName = OptimazePath(newFileInfoName);

                currentFileList.Add(newFileInfoName);

                int idx = newFileInfoName.LastIndexOf(@"/", StringComparison.Ordinal);
                string folderPath = newFileInfoName.Substring(0, idx);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                try
                {
                    byte[] bytes = File.ReadAllBytes(fileFullName);

                    File.WriteAllBytes(newFileInfoName, bytes);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Error msg  : " + ex.Message + " stackTrace : " + ex.StackTrace);
                }

            }

            return currentFileList;
        }

        public static void ClearAndCreateDirectory(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                throw new ArgumentException("dir");
            }

            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }

            Directory.CreateDirectory(dir);
        }

        public static string OptimazePath(string path , bool getFullPath = true)
        {
            if(getFullPath)
                path = Path.GetFullPath(path);
            path = path.Replace(@"\", @"/");
            return path;
        }


        public static string GetMD5(byte[] bytes)
        {
            var md5 = new MD5CryptoServiceProvider();
            var md5Bytes = md5.ComputeHash(bytes);
            return GetMd5String(md5Bytes);
        }

        public static string GetMD5(string path)
        {
            byte[] retval = null;
            FileInfo file = new FileInfo(path);
            if (file.Attributes != FileAttributes.Normal)
            {
                file.Attributes = FileAttributes.Normal;
            }

            using (FileStream fs = file.OpenRead())
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                retval = md5.ComputeHash(fs);
            }
            return GetMd5String(retval);
        }

        private static string GetMd5String(byte[] md5)
        {
            StringBuilder sc = new StringBuilder();
            for (int i = 0; i < md5.Length; i++)
            {
                sc.Append(md5[i].ToString("x2"));
            }
            return sc.ToString();
        }

        
    }
}
