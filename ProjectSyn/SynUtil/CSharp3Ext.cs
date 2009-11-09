using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    /// <summary>
    /// 封装一个方法，该方法不采用参数并且不返回值。
    /// </summary>
    public delegate void Action();

    /// <summary>
    /// 封装一个方法，该方法具有两个参数并且不返回值。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    public delegate void Action<T1, T2>(T1 arg1, T2 arg2);

    /// <summary>
    /// 封装一个方法，该方法采用三个参数并且不返回值。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数类型。</typeparam>
    /// <typeparam name="T3">此委托封装的方法的第三个参数类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    /// <param name="arg3">此委托封装的方法的第三个参数。</param>
    public delegate void Action<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);

    /// <summary>
    /// 封装一个方法，该方法具有四个参数并且不返回值。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数类型。</typeparam>
    /// <typeparam name="T3">此委托封装的方法的第三个参数类型。</typeparam>
    /// <typeparam name="T4">此委托封装的方法的第四个参数类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    /// <param name="arg3">此委托封装的方法的第三个参数。</param>
    /// <param name="arg4">此委托封装的方法的第四个参数。</param>
    public delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    /// <summary>
    /// 封装一个不具有参数但却返回 <paramref name="TResult" /> 参数指定的类型值的方法。
    /// </summary>
    /// <typeparam name="TResult">此委托封装的方法的返回值类型。</typeparam>
    /// <returns>此委托封装的方法的返回值。</returns>
    public delegate TResult Func<TResult>();

    /// <summary>
    /// 封装一个具有一个参数并返回 <paramref name="TResult" /> 参数指定的类型值的方法。
    /// </summary>
    /// <typeparam name="T">此委托封装的方法的参数类型。</typeparam>
    /// <typeparam name="TResult">此委托封装的方法的返回值类型。</typeparam>
    /// <param name="arg">此委托封装的方法的参数。</param>
    /// <returns>此委托封装的方法的返回值。</returns>
    public delegate TResult Func<T, TResult>(T arg);

    /// <summary>
    /// 封装一个具有两个参数并返回 <paramref name="TResult" /> 参数指定的类型值的方法。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数类型。</typeparam>
    /// <typeparam name="TResult">此委托封装的方法的返回值类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    /// <returns>此委托封装的方法的返回值。</returns>
    public delegate TResult Func<T1, T2, TResult>(T1 arg1, T2 arg2);

    /// <summary>
    /// 封装一个具有三个参数并返回 <paramref name="TResult" /> 参数指定的类型值的方法。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数类型。</typeparam>
    /// <typeparam name="T3">此委托封装的方法的第三个参数类型。</typeparam>
    /// <typeparam name="TResult">此委托封装的方法的返回值类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    /// <param name="arg3">此委托封装的方法的第三个参数。</param>
    /// <returns>此委托封装的方法的返回值。</returns>
    public delegate TResult Func<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);

    /// <summary>
    /// 封装一个具有四个参数并返回 <paramref name="TResult" /> 参数指定的类型值的方法。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数类型。</typeparam>
    /// <typeparam name="T3">此委托封装的方法的第三个参数类型。</typeparam>
    /// <typeparam name="T4">此委托封装的方法的第四个参数类型。</typeparam>
    /// <typeparam name="TResult">此委托封装的方法的返回值类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    /// <param name="arg3">此委托封装的方法的第三个参数。</param>
    /// <param name="arg4">此委托封装的方法的第四个参数。</param>
    /// <returns>此委托封装的方法的返回值。</returns>
    public delegate TResult Func<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
}

namespace System.Runtime.CompilerServices
{
    // This attribute allows us to define extension methods without requiring FW 3.5.

    /// <summary>
    /// 指示某个方法为扩展方法，或某个类或程序集包含扩展方法。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class ExtensionAttribute : Attribute { }

}


