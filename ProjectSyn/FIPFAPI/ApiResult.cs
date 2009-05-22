using System;

namespace FIPFAPI
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult"/> class.
        /// </summary>
        /// <param name="isSuccess">if set to <c>true</c> [is success].</param>
        public ApiResult(bool isSuccess)
        {
            Success = isSuccess;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult"/> class.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="msg">The MSG.</param>
        public ApiResult(Exception exp, string msg)
        {
            Success = false;
            InnerException = exp;
            Message = msg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResult"/> class.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public ApiResult(string msg)
        {
            Success = false;
            Message = msg;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApiResult"/> is success.
        /// </summary>
        /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the inner exception.
        /// </summary>
        /// <value>The inner exception.</value>
        public Exception InnerException { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }
    }
}
