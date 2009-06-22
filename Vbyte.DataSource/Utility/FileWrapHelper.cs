using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Vbyte.DataSource.Utility
{
    /// <summary>
    /// 本地文件封装辅助
    /// </summary>
    public sealed class FileWrapHelper
    {
        /// <summary>
        /// 获取对象序列化的二进制版本
        /// </summary>
        /// <param name="pObj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static byte[] GetBytes(object pObj)
        {
            if (pObj == null) { return null; }
            MemoryStream serializationStream = new MemoryStream();
            new BinaryFormatter().Serialize(serializationStream, pObj);
            serializationStream.Position = 0L;
            byte[] buffer = new byte[serializationStream.Length];
            serializationStream.Read(buffer, 0, buffer.Length);
            serializationStream.Close();
            return buffer;
        }

        /// <summary>
        /// 从已序列化数据中(byte[])获取对象实体
        /// </summary>
        /// <typeparam name="T">要返回的数据类型</typeparam>
        /// <param name="binData">二进制数据</param>
        /// <returns>对象实体</returns>
        public static T GetObject<T>(byte[] binData)
        {
            if (binData == null) { return default(T); }
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream(binData);
            return (T)formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// 从已序列化数据中(byte[])获取对象实体
        /// </summary>
        /// <param name="binData">二进制数据</param>
        /// <returns>对象实体</returns>
        public static object GetRawObject(byte[] binData)
        {
            if (binData == null) return null;
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream(binData);
            return formatter.Deserialize(serializationStream);
        }

        /// <summary>
        /// 获取二进制的十六进制查看方式数据
        /// </summary>
        /// <param name="binDat">二进制数据</param>
        /// <returns>二进制的16进制字符形式</returns>
        public static string GetHexViewString(byte[] binDat)
        {
            //tbxBinView.Text = "总长度：" + binDat.Length.ToString() + "字节"
            //    + Environment.NewLine + Environment.NewLine;
            StringBuilder sb = new StringBuilder();
            for (int i = 0, j = binDat.Length; i < j; i++)
            {
                if (i == 0)
                {
                    sb.Append("00000000  ");
                }
                sb.Append(binDat[i].ToString("X2") + " ");

                if (i > 0 && (i + 1) % 8 == 0 && (i + 1) % 16 != 0)
                {
                    sb.Append(" ");
                }

                if (i > 0 && (i + 1) % 16 == 0)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append((i + 1).ToString("X2").PadLeft(8, '0') + "  ");
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Tag to make sure this file is readable/decryptable by this class
        /// </summary>
        private const ulong FC_TAG = 0xFC2156446174CF00;
        private const ulong FE_TAG = 0xFC2156446174CFEF;

        /// <summary>
        /// The amount of bytes to read from the file
        /// </summary>
        private const int BUFFER_SIZE = 128 * 1024;

        /// <summary>
        /// 反转二进制字节序列
        /// </summary>
        public static byte[] ReverseBytes(byte[] bytes)
        {
            int num = bytes.Length / 2;
            byte by;
            int idx;
            for (int i = 0; i < num; i++)
            {
                by = bytes[i];
                idx = bytes.Length - i - 1;
                bytes[i] = bytes[idx];
                bytes[idx] = by;
            }
            return bytes;
        }
        
        /// <summary>
        /// Checks to see if two byte array are equal
        /// </summary>
        /// <param name="b1">the first byte array</param>
        /// <param name="b2">the second byte array</param>
        /// <returns>true if b1.Length == b2.Length and each byte in b1 is
        /// equal to the corresponding byte in b2</returns>
        private static bool CheckByteArrays(byte[] b1, byte[] b2)
        {
            if (b1.Length == b2.Length)
            {
                for (int i = 0; i < b1.Length; ++i)
                {
                    if (b1[i] != b2[i])
                        return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a Rijndael SymmetricAlgorithm for use in EncryptFile and DecryptFile
        /// </summary>
        /// <param name="password">the string to use as the password</param>
        /// <param name="salt">the salt to use with the password</param>
        /// <returns>A SymmetricAlgorithm for encrypting/decrypting with Rijndael</returns>
        private static SymmetricAlgorithm CreateRijndael(string password, byte[] salt)
        {
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(password, salt, "SHA256", 1000);
            SymmetricAlgorithm sma = Rijndael.Create();
            sma.KeySize = 256;
            sma.Key = pdb.GetBytes(32);
            sma.Padding = PaddingMode.PKCS7;
            return sma;
        }

        /// <summary>
        /// Crypto Random number generator for use in EncryptFile
        /// </summary>
        private static RandomNumberGenerator rand = new RNGCryptoServiceProvider();

        /// <summary>
        /// Generates a specified amount of random bytes
        /// </summary>
        /// <param name="count">the number of bytes to return</param>
        /// <returns>a byte array of count size filled with random bytes</returns>
        private static byte[] GenerateRandomBytes(int count)
        {
            byte[] bytes = new byte[count];
            rand.GetBytes(bytes);
            return bytes;
        }

        public static void EncryptStream(Stream fin, Stream fout, string password, WrapProgressCallBack callback)
        {
            long lSize = fin.Length; // the size of the input file for storing
            int size = (int)lSize;  // the size of the input file for progress
            byte[] bytes = new byte[BUFFER_SIZE]; // the buffer
            int read = -1; // the amount of bytes read from the input file
            int value = 0; // the amount overall read from the input file for progress

            // generate IV and Salt
            byte[] IV = GenerateRandomBytes(16);
            byte[] salt = GenerateRandomBytes(16);

            // create the crypting object
            SymmetricAlgorithm sma = FileWrapHelper.CreateRijndael(password, salt);
            sma.IV = IV;

            // write the IV and salt to the beginning of the file
            fout.Write(IV, 0, IV.Length);
            fout.Write(salt, 0, salt.Length);

            // create the hashing and crypto streams
            HashAlgorithm hasher = SHA256.Create();
            using (CryptoStream cout = new CryptoStream(fout, sma.CreateEncryptor(), CryptoStreamMode.Write),
                      chash = new CryptoStream(Stream.Null, hasher, CryptoStreamMode.Write))
            {
                // write the size of the file to the output file
                BinaryWriter bw = new BinaryWriter(cout);
                bw.Write(lSize);

                // write the file cryptor tag to the file
                bw.Write(FC_TAG);

                // read and the write the bytes to the crypto stream in BUFFER_SIZEd chunks
                while ((read = fin.Read(bytes, 0, bytes.Length)) != 0)
                {
                    cout.Write(bytes, 0, read);
                    chash.Write(bytes, 0, read);
                    value += read;
                    if (callback!=null) callback(0, size, value);
                }
                // flush and close the hashing object
                chash.Flush();
                chash.Close();

                // read the hash
                byte[] hash = hasher.Hash;

                // write the hash to the end of the file
                cout.Write(hash, 0, hash.Length);

                // flush and close the cryptostream
                cout.Flush();
                cout.Close();
                cout.Dispose();
            }
        }

        /// <summary>
        /// This takes an input file and encrypts it into the output file
        /// </summary>
        /// <param name="inFile">the file to encrypt</param>
        /// <param name="outFile">the file to write the encrypted data to</param>
        /// <param name="password">the password for use as the key</param>
        /// <param name="callback">the method to call to notify of progress</param>
        public static void EncryptFile(string inFile, string outFile, string password, WrapProgressCallBack callback)
        {
            using (FileStream fin = File.OpenRead(inFile),
                      fout = File.OpenWrite(outFile))
            {
                EncryptStream(fin, fout, password, callback);
            }
        }

        public static void DecryptStream(Stream fin, Stream fout, string password, WrapProgressCallBack callback)
        {
            int size = (int)fin.Length; // the size of the file for progress notification
            byte[] bytes = new byte[BUFFER_SIZE]; // byte buffer
            int read = -1; // the amount of bytes read from the stream
            int value = 0;
            int outValue = 0; // the amount of bytes written out

            // read off the IV and Salt
            byte[] IV = new byte[16];
            fin.Read(IV, 0, 16);

            byte[] salt = new byte[16];
            fin.Read(salt, 0, 16);

            // create the crypting stream
            SymmetricAlgorithm sma = FileWrapHelper.CreateRijndael(password, salt);
            sma.IV = IV;

            value = 32; // the value for the progress
            long lSize = -1; // the size stored in the input stream

            // create the hashing object, so that we can verify the file
            HashAlgorithm hasher = SHA256.Create();

            // create the cryptostreams that will process the file
            using (CryptoStream cin = new CryptoStream(fin, sma.CreateDecryptor(), CryptoStreamMode.Read),
                      chash = new CryptoStream(Stream.Null, hasher, CryptoStreamMode.Write))
            {
                // read size from file
                BinaryReader br = new BinaryReader(cin);
                lSize = br.ReadInt64();

                ulong tag = br.ReadUInt64();
                if (FC_TAG != tag)
                    throw new FileWrapException("File Corrupted!");

                //determine number of reads to process on the file
                long numReads = lSize / BUFFER_SIZE;

                // determine what is left of the file, after numReads
                long slack = (long)lSize % BUFFER_SIZE;

                // read the buffer_sized chunks
                for (int i = 0; i < numReads; ++i)
                {
                    read = cin.Read(bytes, 0, bytes.Length);
                    fout.Write(bytes, 0, read);
                    chash.Write(bytes, 0, read);
                    value += read;
                    outValue += read;
                    if (callback != null) callback(0, size, value);
                }

                // now read the slack
                if (slack > 0)
                {
                    read = cin.Read(bytes, 0, (int)slack);
                    fout.Write(bytes, 0, read);
                    chash.Write(bytes, 0, read);
                    value += read;
                    outValue += read;
                    if (callback != null) callback(0, size, value);
                }
                // flush and close the hashing stream
                chash.Flush();
                chash.Close();

                // flush and close the output file
                fout.Flush();
                fout.Close();

                // read the current hash value
                byte[] curHash = hasher.Hash;

                // get and compare the current and old hash values
                byte[] oldHash = new byte[hasher.HashSize / 8];
                read = cin.Read(oldHash, 0, oldHash.Length);
                if ((oldHash.Length != read) || (!CheckByteArrays(oldHash, curHash)))
                    throw new FileWrapException("File Corrupted!");
            }

            // make sure the written and stored size are equal
            if (outValue != lSize)
                throw new FileWrapException("File Sizes don't match!");
        }

        /// <summary>
        /// takes an input file and decrypts it to the output file
        /// </summary>
        /// <param name="inFile">the file to decrypt</param>
        /// <param name="outFile">the to write the decrypted data to</param>
        /// <param name="password">the password used as the key</param>
        /// <param name="callback">the method to call to notify of progress</param>
        public static void DecryptFile(string inFile, string outFile, string password, WrapProgressCallBack callback)
        {
            using (FileStream fin = File.OpenRead(inFile),  fout = File.OpenWrite(outFile))
            {
                DecryptStream(fin, fout, password, callback);  
            }
        }

        public static byte[] Encrypt(byte[] clearData, string Password)
        {
            MemoryStream ms = new MemoryStream();
            EncryptStream(new MemoryStream(clearData), ms, Password, null);

            byte[] eData = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return eData;
        }

        public static byte[] Decrypt(byte[] cipherData, string Password)
        {
            MemoryStream ms = new MemoryStream();
            DecryptStream(new MemoryStream(cipherData), ms, Password, null);

            byte[] rData = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return rData;
        }

        public static byte[] WrapObject(object pObj)
        {
            byte[] f8 = ReverseBytes(BitConverter.GetBytes(FC_TAG));
            byte[] eBytes = GetBytes(pObj);
            if (true)
            {
                //ETAG
                Buffer.SetByte(f8, 7, 0xEF);
                eBytes = Encrypt(eBytes, "!VDat");
            }
            byte[] fBytes = new byte[eBytes.Length + f8.Length];
            Buffer.BlockCopy(f8, 0, fBytes, 0, f8.Length);
            Buffer.BlockCopy(eBytes, 0, fBytes, f8.Length, eBytes.Length);
            return fBytes;
        }

        public static object UnWrapObject(byte[] binData)
        {
            if (binData.Length < 9)
            {
               throw new InvalidOperationException("文件数据长度不正确！");
            }
            else
            {
                byte[] f8 = new byte[8];
                byte[] fBytes = new byte[binData.Length - 8];
                Buffer.BlockCopy(binData, 0, f8, 0, 8);

                if (!CheckByteArrays(ReverseBytes(BitConverter.GetBytes(FC_TAG)), f8) 
                    && !CheckByteArrays(ReverseBytes(BitConverter.GetBytes(FE_TAG)), f8))
                {
                    throw new InvalidOperationException("文件数据格式不正确！");
                }
                else
                {
                    Buffer.BlockCopy(binData, f8.Length, fBytes, 0, fBytes.Length);
                    if (f8[7] == 0xEF)
                    {
                        return GetRawObject(Decrypt(fBytes, "!VDat"));
                    }
                    else
                    {
                        return GetRawObject(fBytes);
                    }
                }
            }
        }
    }

    /// <summary>
    /// This is the exception class that is thrown throughout the Decryption process
    /// </summary>
    public class FileWrapException : ApplicationException
    {
        public FileWrapException(string msg) : base(msg) { }
    }

    /// <summary>
    /// CallBack delegate for progress notification
    /// </summary>
    public delegate void WrapProgressCallBack(int min, int max, int value);

}
