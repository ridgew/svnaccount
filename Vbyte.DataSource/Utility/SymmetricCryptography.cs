using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Vbyte.DataSource.Utility
{
    /// <summary>
    /// 通用对称加解密算法辅助函数
    /// <para>Code: Tony Selke</para>
    /// <para>http://www.codeproject.com/KB/security/SymmetricAlgorithmHelper.aspx</para>
    /// </summary>
    /// <typeparam name="T">对称加解密算法类型</typeparam>
    /// <example>
    /// <![CDATA[
    ///     T 中现有的.NET实现版本
    ///     ----------------------
    ///     * DESCryptoServiceProvider
    ///     * RC2CryptoServiceProvider
    ///     * RijndaelManaged
    ///     * TripleDESCryptoServiceProvider
    /// ]]>
    /// </example>
    public sealed class SymmetricCryptography<T> where T : SymmetricAlgorithm, new()
    {
        #region Fields

        private T _provider = new T();
        private UTF8Encoding _utf8 = new UTF8Encoding();

        #endregion Fields

        #region Properties

        private byte[] _key;
        public byte[] Key
        {
            get { return _key; }
            set { _key = value; }
        }

        private byte[] _iv;
        public byte[] IV
        {
            get { return _iv; }
            set { _iv = value; }
        }

        #endregion Properties

        #region Constructors

        public SymmetricCryptography()
        {
            _provider.GenerateKey();
            _key = _provider.Key;
            _provider.GenerateIV();
            _iv = _provider.IV;
        }

        public SymmetricCryptography(byte[] key, byte[] iv)
        {
            _key = key;
            _iv = iv;
        }

        #endregion Constructors

        #region Byte Array Methods

        public byte[] Encrypt(byte[] input)
        {
            return Encrypt(input, _key, _iv);
        }

        public byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, _key, _iv);
        }
        
        public byte[] Encrypt(byte[] input, byte[] key, byte[] iv)
        {
            return Transform(input,
                   _provider.CreateEncryptor(key, iv));
        }

        public byte[] Decrypt(byte[] input, byte[] key, byte[] iv)
        {
            return Transform(input,
                   _provider.CreateDecryptor(key, iv));
        }

        #endregion Byte Array Methods

        #region String Methods

        public string Encrypt(string text)
        {
            return Encrypt(text, _key, _iv);
        }

        public string Decrypt(string text)
        {
            return Decrypt(text, _key, _iv);
        }

        public string Encrypt(string text, byte[] key, byte[] iv)
        {
            byte[] output = Transform(_utf8.GetBytes(text),
                            _provider.CreateEncryptor(key, iv));
            return Convert.ToBase64String(output);
        }

        public string Decrypt(string text, byte[] key, byte[] iv)
        {
            byte[] output = Transform(Convert.FromBase64String(text),
                            _provider.CreateDecryptor(key, iv));
            return _utf8.GetString(output);
        }

        #endregion String Methods
        
        #region SecureString Methods
        
        public byte[] Encrypt(SecureString input)
        {
            return Encrypt(input, _key, _iv);
        }

        public void Decrypt(byte[] input, out SecureString output)
        {
            Decrypt(input, out output, _key, _iv);
        }

        public byte[] Encrypt(SecureString input, byte[] key, byte[] iv)
        {
            // defensive argument checking
            if (input == null)
                throw new ArgumentNullException("input");

            IntPtr inputPtr = IntPtr.Zero;

            try
            {
                // copy the SecureString to an unmanaged BSTR
                // and get back the pointer to the memory location
                inputPtr = Marshal.SecureStringToBSTR(input);
                if (inputPtr == IntPtr.Zero)
                    throw new InvalidOperationException("Unable to allocate necessary unmanaged resources.");

                char[] inputBuffer = new char[input.Length];

                try
                {
                    // pin the buffer array so the GC doesn't move it while we are
                    // doing an unmanaged memory copy, but make sure we release the
                    // pin when we are done so that the CLR can do its thing later
                    GCHandle handle = GCHandle.Alloc(inputBuffer, GCHandleType.Pinned);
                    try
                    {
                        Marshal.Copy(inputPtr, inputBuffer, 0, input.Length);
                    }
                    finally
                    {
                        handle.Free();
                    }

                    // encode the input as UTF8 first so that we have a
                    // way to explicitly "flush" the byte array afterwards
                    byte[] utf8Buffer = _utf8.GetBytes(inputBuffer);
                    try
                    {
                        return Encrypt(utf8Buffer, key, iv);
                    }
                    finally
                    {
                        Array.Clear(utf8Buffer, 0, utf8Buffer.Length);
                    }
                }
                finally
                {
                    Array.Clear(inputBuffer, 0, inputBuffer.Length);
                }
            }
            finally
            {
                // because we are using unmanaged resources, we *must*
                // explicitly deallocate those resources ourselves
                if (inputPtr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(inputPtr);
            }
        }

        public void Decrypt(byte[] input, out SecureString output, byte[] key, byte[] iv)
        {
            byte[] decryptedBuffer = null;

            try
            {
                // do our normal decryption of a byte array
                decryptedBuffer = Decrypt(input, key, iv);

                char[] outputBuffer = null;
                
                try
                {
                    // convert the decrypted array to an explicit
                    // character array that we can "flush" later
                    outputBuffer = _utf8.GetChars(decryptedBuffer);

                    // Create the result and copy the characters
                    output = new SecureString();
                    try
                    {
                        for (int i = 0; i < outputBuffer.Length; i++)
                            output.AppendChar(outputBuffer[i]);
                        return;
                    }
                    finally
                    {
                        output.MakeReadOnly();
                    }
                }
                finally
                {
                    if (outputBuffer != null)
                        Array.Clear(outputBuffer, 0, outputBuffer.Length);
                }
            }
            finally
            {
                if (decryptedBuffer != null)
                    Array.Clear(decryptedBuffer, 0, decryptedBuffer.Length);
            }
        }

        #endregion SecureString Methods

        #region Stream Methods

        public void Encrypt(Stream input, Stream output)
        {
            Encrypt(input, output, _key, _iv);
        }

        public void Decrypt(Stream input, Stream output)
        {
            Decrypt(input, output, _key, _iv);
        }

        public void Encrypt(Stream input, Stream output, byte[] key, byte[] iv)
        {
            TransformStream(true, ref input, ref output, key, iv);
        }

        public void Decrypt(Stream input, Stream output, byte[] key, byte[] iv)
        {
            TransformStream(false, ref input, ref output, key, iv);
        }

        #endregion Stream Methods

        #region Private Methods

        private byte[] Transform(byte[] input, 
                       ICryptoTransform CryptoTransform)
        {
            // create the necessary streams
            MemoryStream memStream = new MemoryStream();
            CryptoStream cryptStream = new CryptoStream(memStream, 
                         CryptoTransform, CryptoStreamMode.Write);
            // transform the bytes as requested
            cryptStream.Write(input, 0, input.Length);
            cryptStream.FlushFinalBlock();
            // Read the memory stream and
            // convert it back into byte array
            memStream.Position = 0;
            byte[] result = memStream.ToArray();
            // close and release the streams
            memStream.Close();
            cryptStream.Close();
            // hand back the encrypted buffer
            return result;
        }

        private void TransformStream(bool encrypt, ref Stream input, ref Stream output, byte[] key, byte[] iv)
        {
            // defensive argument checking
            if (input == null)
                throw new ArgumentNullException("input");
            if (output == null)
                throw new ArgumentNullException("output");
            if (!input.CanRead)
                throw new ArgumentException("Unable to read from the input Stream.", "input");
            if (!output.CanWrite)
                throw new ArgumentException("Unable to write to the output Stream.", "output");
            // make the buffer just large enough for 
            // the portion of the stream to be processed
            byte[] inputBuffer = new byte[input.Length - input.Position];
            // read the stream into the buffer
            input.Read(inputBuffer, 0, inputBuffer.Length);
            // transform the buffer
            byte[] outputBuffer = encrypt ? Encrypt(inputBuffer, key, iv) 
                                          : Decrypt(inputBuffer, key, iv);
            // write the transformed buffer to our output stream 
            output.Write(outputBuffer, 0, outputBuffer.Length);
        }

        #endregion Private Methods
    }

    #region Hash Class...

    /// <summary>CHashProtector is a password protection one way encryption algorithm</summary>
    /// <programmer>Chidi C. Ezeukwu</programmer>
    /// <written>May 16, 2004</written>
    /// <Updated>Aug 07, 2004</Updated>
    public class HashProvider
    {
        #region Private member variables...

        private string mSalt = string.Empty;
        private HashAlgorithm mCryptoService;
        private Encoding hashEncoding = Encoding.UTF8;

        #endregion

        #region Public interfaces...

        /// <summary>
        /// 内置的HASH算法
        /// </summary>
        public enum ServiceProviderEnum : int
        {
            // Supported algorithms
            /// <summary>
            /// 
            /// </summary>
            SHA1,
            /// <summary>
            /// 
            /// </summary>
            SHA256,
            /// <summary>
            /// 
            /// </summary>
            SHA384,
            /// <summary>
            /// 
            /// </summary>
            SHA512,
            /// <summary>
            /// MD5 哈希值
            /// </summary>
            MD5
        }

        public HashProvider()
        {
            // Default Hash algorithm
            mCryptoService = new SHA1Managed();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public HashProvider(ServiceProviderEnum serviceProvider)
        {
            // Select hash algorithm
            switch (serviceProvider)
            {
                case ServiceProviderEnum.MD5:
                    mCryptoService = new MD5CryptoServiceProvider();
                    break;
                case ServiceProviderEnum.SHA1:
                    mCryptoService = new SHA1Managed();
                    break;
                case ServiceProviderEnum.SHA256:
                    mCryptoService = new SHA256Managed();
                    break;
                case ServiceProviderEnum.SHA384:
                    mCryptoService = new SHA384Managed();
                    break;
                case ServiceProviderEnum.SHA512:
                    mCryptoService = new SHA512Managed();
                    break;
            }

            mCryptoService.Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="datEncode">The data encode.</param>
        public HashProvider(ServiceProviderEnum serviceProvider, Encoding datEncode)
            : this(serviceProvider)
        {
            hashEncoding = datEncode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HashProvider"/> class.
        /// </summary>
        /// <param name="serviceProviderName">Name of the service provider.</param>
        public HashProvider(string serviceProviderName)
        {
            try
            {
                // Set Hash algorithm
                mCryptoService = (HashAlgorithm)CryptoConfig.CreateFromName(serviceProviderName.ToUpper());
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 获取哈希码(16进制字符串)
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns></returns>
        public virtual string ComputeHash(string plainText)
        {
            return ComputeHash(hashEncoding.GetBytes(plainText + mSalt));
        }

        /// <summary>
        /// 获取哈希码(16进制字符串)
        /// </summary>
        /// <param name="hBytes">需要计算哈希码的字节数据</param>
        /// <returns></returns>
        public virtual string ComputeHash(byte[] hBytes)
        {
            byte[] cryptoByte = mCryptoService.ComputeHash(hBytes);
            return ByteArrayToHexStr(cryptoByte, true);
        }

        /// <summary>
        /// 二进制流的16进制字符串表达形式
        /// </summary>
        /// <param name="bytHash">二进制流</param>
        /// <param name="IsLowerCase">小写16进制字母</param>
        public static string ByteArrayToHexStr(byte[] bytHash, bool IsLowerCase)
        {
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X2");
            }
            return (IsLowerCase) ? sTemp.ToLower() : sTemp.ToUpper();
        }

        /// <summary>
        /// Gets or sets the salt.
        /// </summary>
        /// <value>The salt.</value>
        public string Salt
        {
            // Salt value
            get
            {
                return mSalt;
            }
            set
            {
                mSalt = value;
            }
        }
        #endregion
    }
    #endregion
}
