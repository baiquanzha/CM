using System;
using System.Security.Cryptography;

namespace MTool.Core.Security.Cryptography
{
    public class Aes
    {

        /// <summary>
        /// AES加密 
        /// </summary>
        /// <param name="plainText">原字节</param>
        /// <param name="secretKey">密钥</param>
        /// <returns></returns>
        public static byte[] Encrypt(byte[] plainText, byte[] secretKey)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 128;
            rijndaelCipher.BlockSize = 128;
            rijndaelCipher.Key = secretKey;
            var ivBytes16Value = new byte[16];
            Array.Copy(secretKey, ivBytes16Value, 16);
            rijndaelCipher.IV = ivBytes16Value;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            return transform.TransformFinalBlock(plainText, 0, plainText.Length);
        }
        
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="encryptedData">加密字节</param>
        /// <param name="secretKey">密钥</param>
        /// <returns></returns>
        public static byte[] Decrypt(byte[] encryptedData, byte[] secretKey)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;
            rijndaelCipher.KeySize = 128;
            rijndaelCipher.BlockSize = 128;
            rijndaelCipher.Key = secretKey;
            var ivBytes16Value = new byte[16];
            Array.Copy(secretKey, ivBytes16Value, 16);
            rijndaelCipher.IV = ivBytes16Value;
            ICryptoTransform transform = rijndaelCipher.CreateDecryptor();
            return transform.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }
    }
}