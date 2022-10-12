using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MTool.Core.Utilities
{
    public enum HashType
    {
        SHA1,
        MD5
    }

    public static class CryptoUtility
    {

        public static string GetHash(string path, HashType type = HashType.MD5)
        {
            byte[] retval = null;
            FileInfo file = new FileInfo(path);
            if (file.Attributes != FileAttributes.Normal)
            {
                file.Attributes = FileAttributes.Normal;
            }

            using (FileStream fs = file.OpenRead())
            {
                if (type == HashType.MD5)
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    retval = md5.ComputeHash(fs);
                }
                else
                {
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    retval = sha1.ComputeHash(fs);
                }
                fs.Close();
            }
            StringBuilder sc = new StringBuilder();
            for (int i = 0; i < retval.Length; i++)
            {
                sc.Append(retval[i].ToString("x2"));
            }
            return sc.ToString();
        }

        public static string GetHash(byte[] bytes, HashType type = HashType.MD5)
        {
            byte[] cryptoBytes = null;
            if (type == HashType.MD5)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                cryptoBytes = md5.ComputeHash(bytes);
            }
            else
            {
                SHA1 sha1 = new SHA1CryptoServiceProvider();
                cryptoBytes = sha1.ComputeHash(bytes);
            }

            StringBuilder sc = new StringBuilder();
            for (int i = 0; i < cryptoBytes.Length; i++)
            {
                sc.Append(cryptoBytes[i].ToString("x2"));
            }
            return sc.ToString();
        }

        public static string GetMD5(FileInfo file) {
            byte[] retval = null;
            if (file.Attributes != FileAttributes.Normal) {
                file.Attributes = FileAttributes.Normal;
            }

            using (FileStream fs = file.OpenRead()) {
                MD5 md5 = new MD5CryptoServiceProvider();
                retval = md5.ComputeHash(fs);
            }
            
            StringBuilder sc = new StringBuilder();
            for (int i = 0; i < retval.Length; i++) {
                sc.Append(retval[i].ToString("x2"));
            }
            return sc.ToString();
        }
    }
}