using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 文件系统存储
    /// </summary>
    public class FileSystemStorage : IDataStorage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemStorage"/> class.
        /// </summary>
        public FileSystemStorage()
        { 
        
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemStorage"/> class.
        /// </summary>
        /// <param name="baseDir">本地文件存储的基础路径</param>
        public FileSystemStorage(string baseDir)
        {
            _BaseDirectory = baseDir;

            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);
        }

        private string _BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        /// <summary>
        /// 本地文件存储的基础目录
        /// </summary>
        public string BaseDirectory
        {
            get { return _BaseDirectory; }
            set { _BaseDirectory = value; }
        }

        /// <summary>
        /// 文件目录的数据信息文件名
        /// </summary>
        private const string FS_DIRDAT = ".dat";

        #region IDataStorage 成员

        /// <summary>
        /// 保存特定数据
        /// </summary>
        /// <param name="item">数据实例</param>
        public void Store(IDataItem item)
        {
            string tarPath = Path.Combine(BaseDirectory, item.IdentityName);
            if (item.IsContainer)
            {
                Directory.CreateDirectory(tarPath);
            }
            else
            {
                File.WriteAllBytes(tarPath, item.RawData); 
            }
        }

        /// <summary>
        /// 删除指定标志的数据
        /// </summary>
        /// <param name="identityName">数据标志号</param>
        public void Remove(string identityName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取指定标志的数据
        /// </summary>
        /// <param name="identityName">数据标志号</param>
        /// <returns>如果存在该数据则返回，否则为null。</returns>
        public IDataItem GetData(string identityName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取满足指定标志的数据集合
        /// </summary>
        /// <param name="filter">判断匹配规则</param>
        /// <param name="isMatch">匹配方向：true则匹配，false则为不匹配。</param>
        /// <returns>如果存在则返回集合，否则为null或空数组。</returns>
        public IDataItem[] GetDataList(Predicate<IDataItem> filter, bool isMatch)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// 获取本地文件目录下的本地文件数据封装
        /// </summary>
        /// <param name="localDir">本地数据文件目录</param>
        /// <returns>本地文件数据集合</returns>
        public static LocalFileData[] GetDirectoryFileData(string localDir)
        {
            List<LocalFileData> fList = new List<LocalFileData>();
            DirectoryInfo curDirInfo = new DirectoryInfo(localDir);
            foreach (FileInfo file in curDirInfo.GetFiles())
            { 
                
            }
            return fList.ToArray();
        }
    }

}
