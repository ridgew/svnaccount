using System;

namespace Vbyte.DataSource.Cache
{
    /// <summary>
    ///  通用单件缓存实例
    /// </summary>
    /// <typeparam name="T">实例类型</typeparam>
    public class Singleton<T> where T : new()
    {
        private static T _instance = default(T);

        /// <summary>
        /// 获取静态缓存实例
        /// </summary>
        /// <value>缓存实例</value>
        public static T Instance
        {
            get
            {
                if (_instance.Equals(default(T))) _instance = new T();
                return _instance;
            }
        }

        /// <summary>
        /// 设置静态缓存实例
        /// </summary>
        /// <param name="instance">实例的值</param>
        public static void Set(T instance)
        {
            _instance = instance;
        }

    }
}
