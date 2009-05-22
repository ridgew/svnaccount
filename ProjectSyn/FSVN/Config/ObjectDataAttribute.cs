using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using FSVN.Util;
using System.Diagnostics;

namespace FSVN.Config
{
    /// <summary>
    /// 标志二进制数据的对象来源与转换
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ObjectDataAttribute : Attribute
    {
        /// <summary>
        /// 数据映射对象类型
        /// </summary>
        public Type ConvertType { get; set; }

        /// <summary>
        /// 从二进制数据还原对象
        /// </summary>
        /// <param name="oDat">原始二进制数据</param>
        /// <returns>ConvertType对象的实例</returns>
        public object Load(byte[] oDat)
        {
            if (oDat != null && oDat.Length > 0)
            {
                return oDat.GetObject();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取转换对象的原始二进制数据
        /// </summary>
        /// <param name="objInstance">ConvertType对象的实例</param>
        /// <returns>ConvertType对象的原始二进制数据</returns>
        public byte[] Get(object objInstance)
        {
            return objInstance.GetBytes(); 
        }

        /// <summary>
        /// 获取配置实例类型的的配置信息
        /// </summary>
        /// <param name="configType">配置实例类型</param>
        /// <param name="propName">公共属性名称</param>
        /// <returns>相关配置实例</returns>
        public static ObjectDataAttribute GetInstance(Type configType, string propName)
        {
            return (ObjectDataAttribute)Attribute.GetCustomAttribute(configType.GetProperty(propName), typeof(ObjectDataAttribute));
        }

        /// <summary>
        /// 验证属性设置是否符合配置
        /// </summary>
        public static void Validate(byte[] val, Type instanceType, string propName)
        {
            ObjectDataAttribute attr = GetInstance(instanceType, propName);
            if (val != null && val.Length > 0)
            {
                bool throwError = true;
                try
                {
                    object storeObj = val.GetObject();
                    Type valType = storeObj.GetType();
                    throwError = !(attr.ConvertType.Equals(valType) || valType.IsSubclassOf(attr.ConvertType));
                }
                catch (Exception) {  }
                if (throwError)
                {
                    throw new InvalidOperationException("二进制数据设置错误，二进制数据不是：" + attr.ConvertType.FullName);
                }
            }
        }

    }

}
