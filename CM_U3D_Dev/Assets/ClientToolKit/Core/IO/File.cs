using System;
using System.IO;
using System.Text;
using MTool.Core.Functional;
using MTool.Core.Security.Cryptography;
using StdFile = System.IO.File;

namespace MTool.Core.IO
{
    public sealed class File
    {
        /// <summary>
        /// 该文件头结构
        /// </summary>
        public struct FileHeader
        {
            public byte[] key;
            public uint blockCounter;
            public byte[] nonce;

            public void Write(BinaryWriter writer)
            {
                writer.Write(key);
                writer.Write(blockCounter);
                writer.Write(nonce);
            }

            public void Read(BinaryReader reader)
            {
                key = reader.ReadBytes(32);
                blockCounter= reader.ReadUInt32();
                nonce = reader.ReadBytes(12);
            }
        }

        /// <summary>
        /// 使用无BOM型的UTF-8编码文本
        /// </summary>
        private static volatile Encoding utf8Encoding;
        public static Encoding UTF8
        {
            get
            {
                if (utf8Encoding == null)
                    utf8Encoding = new UTF8Encoding(false ,true);//为了适应Unity的网络端下载需求
                return utf8Encoding;
            }
        }

        /// <summary>
        /// 文件加解密器
        /// </summary>
        private static ChaCha20 mCryptor;
        /// <summary>
        /// 流读取Buffer
        /// </summary>
        private static byte[] mReadBuffer = new byte[512];

        /// <summary>
        /// 初始化加解密器，在读取和写入必须调用该函数
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="BlockCounter"></param>
        /// <param name="Nonce"></param>
        private static void InitCryptor(in FileHeader header)
        {
            if (mCryptor == null)
            {
                mCryptor = new ChaCha20(header.key, header.blockCounter, header.nonce);
            }
            else
            {
                mCryptor.Reset(header.key, header.blockCounter, header.nonce);
            }
        }

        #region Text Read&Write

        public static string ReadAllText(string path)
        {
            return ReadAllText(path, UTF8);
        }

        public static string ReadAllText(string path, Encoding encoding)
        {
            if (!StdFile.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            string result = string.Empty;
            using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(path)))
            {
                ReadHeader(reader);
                long fileLength = reader.ReadInt64();
                byte[] resultBytes = new byte[fileLength];

                int pos = 0;
                while (true)
                {
                    int read = reader.Read(mReadBuffer, 0, mReadBuffer.Length);

                    if (read <= 0)
                        break;

                    Array.Copy(mReadBuffer, 0, resultBytes, pos, read);
                    pos += read;
                }
                mCryptor.EncryptOrDecrypt(resultBytes);

                result = encoding.GetString(resultBytes);
            }

            return result;
        }

        public static void WriteAllText(string path, string contents)
        {
            WriteAllText(path, contents, UTF8);
        }
        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            byte[] input = encoding.GetBytes(contents);

            if (StdFile.Exists(path))
            {
                StdFile.Delete(path);
            }

            using (var writer = new BinaryWriter(StdFile.OpenWrite(path)))
            {
                WriteHeader(writer);
                long fileLength = input.Length;
                writer.Write(fileLength);
                mCryptor.EncryptOrDecrypt(input);
                writer.Write(input);
            }
        }

        #endregion


        #region Binary Read&Write

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            if (StdFile.Exists(path))
            {
                StdFile.Delete(path);
            }

            using (var writer = new BinaryWriter(StdFile.OpenWrite(path)))
            {
                WriteHeader(writer);
                long fileLength = bytes.Length;
                writer.Write(fileLength);
                mCryptor.EncryptOrDecrypt(bytes);
                writer.Write(bytes);
            }
        }
        public static byte[] ReadAllBytes(string path)
        {
            if (!StdFile.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            byte[] resultBytes = null;

            using (BinaryReader reader = new BinaryReader(StdFile.OpenRead(path)))
            {
                ReadHeader(reader);
                long fileLength = reader.ReadInt64();
                resultBytes = new byte[fileLength];
                int pos = 0;
                while (true)
                {
                    int read = reader.Read(mReadBuffer, 0, mReadBuffer.Length);
                    if (read <= 0)
                        break;

                    Array.Copy(mReadBuffer, 0, resultBytes, pos, read);
                    pos += read;
                }

                mCryptor.EncryptOrDecrypt(resultBytes);
            }
            return resultBytes;
        }

        #endregion

        /// <summary>
        /// Write file header date to writer
        /// </summary>
        /// <param name="writer"></param>
        private static void WriteHeader(BinaryWriter writer)
        {
            FileHeader header = CreateRandomHeader();
            header.Write(writer);
            InitCryptor(in header);
        }


        private static FileHeader CreateRandomHeader()
        {
            byte[] key = GenerateRandomBytes(32);
            uint blockCounter = GenerateBlockCounter();
            byte[] nonce = GenerateRandomBytes(12);

            FileHeader header = new FileHeader();
            header.key = key;
            header.blockCounter = blockCounter;
            header.nonce = nonce;

            return header;
        }

        private static void ReadHeader(BinaryReader reader)
        {
            FileHeader header = new FileHeader();
            header.Read(reader);
            InitCryptor(in header);
        }

        private static byte[] GenerateRandomBytes(int keyLength)
        {
            byte[] result = new byte[keyLength];
            int seed = DateTime.Now.Millisecond;
            Random random = new Random(seed);
            result.ForCall((x,y)=>result[y] = (byte)random.Next(0,256));
            return result;
        }

        private static uint GenerateBlockCounter()
        {
            int seed = DateTime.Now.Millisecond;
            Random random = new Random(seed);

            return (uint)random.Next(0,int.MaxValue);
        }

        public static bool Exists(string path)
        {
            return StdFile.Exists(path);
        }

        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            StdFile.Copy(sourceFileName,destFileName,overwrite);
        }

        public static void Copy(string sourceFileName, string destFileName)
        {
            StdFile.Copy(sourceFileName,destFileName);
        }
    }
}
