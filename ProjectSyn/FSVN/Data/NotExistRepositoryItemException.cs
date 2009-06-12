using System;
using System.Runtime.Serialization;

namespace FSVN.Data
{
    /// <summary>
    /// 不存在库项数据异常
    /// </summary>
	public class NotExistRepositoryItemException : Exception
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="NotExistRepositoryItemException"/> class.
        /// </summary>
        public NotExistRepositoryItemException() : base()
        {
        
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExistRepositoryItemException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NotExistRepositoryItemException(string message) : base (message)
        {
        
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExistRepositoryItemException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NotExistRepositoryItemException(string message, Exception innerException) : base (message, innerException)
        {
        
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExistRepositoryItemException"/> class.
        /// </summary>
        /// <param name="info"><see cref="T:System.Runtime.Serialization.SerializationInfo"/>，它存有有关所引发异常的序列化的对象数据。</param>
        /// <param name="context"><see cref="T:System.Runtime.Serialization.StreamingContext"/>，它包含有关源或目标的上下文信息。</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="info"/> 参数为 null。</exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">类名为 null 或 <see cref="P:System.Exception.HResult"/> 为零 (0)。</exception>
        public NotExistRepositoryItemException(SerializationInfo info, StreamingContext context) : base (info, context)
        {
        
        }        
	}
}
