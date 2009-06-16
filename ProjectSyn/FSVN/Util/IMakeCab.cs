using System;
using System.Collections.Generic;
using System.Text;

namespace FSVN.Util
{
    /// <summary>
    /// Library: COMMKCABLib 中 MakeCab的COM封装
    /// %system%\catsrvut.dll\6
    /// Description: COM MakeCab 1.0 Type Library 
    /// </summary>
    [ComProgId("MakeCab.MakeCab.1")]
    public interface IMakeCab : IDisposable
	{
        /// <summary>
        /// 创建本地CAB文件
        /// </summary>
        /// <param name="CabFileName">本地Cab文件路径</param>
        /// <param name="MakeSignable">是否可数字签名</param>
        /// <param name="ExtraSpace">抽取空格?</param>
        /// <param name="Use10Format">使用8+3命名格式?</param>
        void CreateCab(string CabFileName, bool MakeSignable, bool ExtraSpace, bool Use10Format);

        /// <summary>
        /// 在当前CAB中添加文件
        /// </summary>
        /// <param name="FileName">本地文件名路径</param>
        /// <param name="FileNameInCab">CAB中的文件路径</param>
        void AddFile(string FileName, string FileNameInCab);


        void CopyFile(string CabName, string FileNameInCab);

        /// <summary>
        /// 关闭当前CAB文件，释放占用内容
        /// </summary>
        void CloseCab();
	}
}
