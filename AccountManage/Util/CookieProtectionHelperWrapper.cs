using System;
using System.Web;
using System.Web.Security;
using System.Reflection;

namespace System.Web.Security {

    /// <summary>
    /// A wrapper arroud System.Web.Security.CookieProtectionHelper internal class
    /// </summary>
    public static class CookieProtectionHelperWrapper {

        private static MethodInfo _encode;
        private static MethodInfo _decode;

        /// <summary>
        /// Constructor
        /// </summary>
        static CookieProtectionHelperWrapper() {
            // obtaining a reference to System.Web assembly
            Assembly systemWeb = typeof(HttpContext).Assembly;
            if (systemWeb == null) {
                throw new InvalidOperationException("Unable to load System.Web.");
            }
            // obtaining a reference to the internal class CookieProtectionHelper
            Type cookieProtectionHelper = systemWeb.GetType("System.Web.Security.CookieProtectionHelper");
            if (cookieProtectionHelper == null) {
                throw new InvalidOperationException("Unable to get the internal class System.Web.Security.CookieProtectionHelper.");
            }
            // obtaining references to the methods of CookieProtectionHelper class
            _encode = cookieProtectionHelper.GetMethod("Encode", BindingFlags.NonPublic | BindingFlags.Static);
            _decode = cookieProtectionHelper.GetMethod("Decode", BindingFlags.NonPublic | BindingFlags.Static);

            if (_encode == null || _decode == null) {
                throw new InvalidOperationException("Unable to get the methods to invoke.");
            }
        }

        /// <summary>
        /// Wrapper arround CookieProtectionHelper.Encode
        /// </summary>
        /// <param name="cookieProtection">Protection Type</param>
        /// <param name="buf">Bytes buffer to encode</param>
        /// <param name="count">The number of bytes in the buffer</param>
        /// <returns>Encoded text</returns>
        public static string Encode(CookieProtection cookieProtection, byte[] buf, int count) {
            return (string)_encode.Invoke(null, new object[] { cookieProtection, buf, count });
        }

        /// <summary>
        /// Wrapper arround CookieProtectionHelper.Decode
        /// </summary>
        /// <param name="cookieProtection">Protection Type</param>
        /// <param name="data">String to decode</param>
        /// <returns>Decoded bytes</returns>
        public static byte[] Decode(CookieProtection cookieProtection, string data) {
            return (byte[])_decode.Invoke(null, new object[] { cookieProtection, data });
        }

    }

}