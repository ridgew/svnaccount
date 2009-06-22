using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Vbyte.DataSource.Utility;

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
        internal const string FS_DIRDAT = ".dat";

        #region IDataStorage 成员

        /// <summary>
        /// 保存特定数据
        /// </summary>
        /// <param name="item">数据实例</param>
        public void Store(IDataItem item)
        {
            string tarPath = Path.Combine(BaseDirectory, item.IdentityName.Replace('/', '\\').Trim('\\'));
            if (item.IsContainer)
            {
                Directory.CreateDirectory(tarPath);
                tarPath += "\\" + FS_DIRDAT;
            }
            File.WriteAllBytes(tarPath, FileWrapHelper.WrapObject(item)); 
        }

        /// <summary>
        /// 删除指定标志的数据
        /// </summary>
        /// <param name="identityName">数据标志号</param>
        public void Remove(string identityName)
        {
            string tarPath = Path.Combine(BaseDirectory, identityName.Replace('/', '\\').Trim('\\'));
            if (File.Exists(tarPath))
            {
                File.Delete(tarPath);
            }
            else
            {
                tarPath += "\\" + FS_DIRDAT;
                if (File.Exists(tarPath))
                {
                    Directory.Delete(Path.GetDirectoryName(tarPath));
                }
                else
                {
                    throw new NullReferenceException("指定标识数据[" + identityName +"]在存储系统中不存在！");
                }
            }
        }

        /// <summary>
        /// 获取指定标志的数据
        /// </summary>
        /// <param name="identityName">数据标志号</param>
        /// <returns>如果存在该数据则返回，否则为null。</returns>
        public IDataItem GetData(string identityName)
        {
            string tarPath = Path.Combine(BaseDirectory, identityName.Replace('/', '\\').Trim('\\'));
            if (!File.Exists(tarPath))
            {
                tarPath += "\\" + FS_DIRDAT;
                if (!File.Exists(tarPath))
                {
                    throw new NullReferenceException("指定标识数据[" + identityName + "]在存储系统中不存在！");
                }
            }
            return FileWrapHelper.UnWrapObject(File.ReadAllBytes(tarPath)) as IDataItem;
        }

        /// <summary>
        /// 获取满足指定标志的数据集合
        /// </summary>
        /// <param name="containerIdentityName">父级容器标识名称，若为null或空则获取顶层相关数据。</param>
        /// <param name="filter">判断匹配规则</param>
        /// <param name="isMatch">匹配方向：true则匹配，false则为不匹配。</param>
        /// <returns>如果存在则返回集合，否则为null或空数组。</returns>
        public IDataItem[] GetDataList(string containerIdentityName, Predicate<IDataItem> filter, bool isMatch)
        {
            string tarDir = BaseDirectory;
            if (string.IsNullOrEmpty(containerIdentityName))
            {
                tarDir = Path.Combine(BaseDirectory, containerIdentityName.Replace('/', '\\'));
            }
            DirectoryInfo curDirInfo = new DirectoryInfo(tarDir);
            if (!curDirInfo.Exists)
            {
                return null;
            }
            else
            {
                List<LocalFileData> fList = new List<LocalFileData>();
                LocalFileData fDat = null;
                #region 文件
                foreach (FileInfo file in curDirInfo.GetFiles())
                {
                    fDat = new LocalFileData(file.FullName);
                    fDat.IsContainer = false;
                    if (filter == null)
                    {
                        fList.Add(fDat);
                    }
                    else
                    {
                        bool doAdd = (isMatch) ? filter(fDat) : !filter(fDat);
                        if (doAdd) fList.Add(fDat);
                    }
                }
                #endregion

                #region 目录
                foreach (DirectoryInfo di in curDirInfo.GetDirectories())
                {
                    fDat = new LocalFileData(di.FullName);
                    fDat.IsContainer = true;

                    if (filter == null)
                    {
                        fList.Add(fDat);
                    }
                    else
                    {
                        bool doAdd = (isMatch) ? filter(fDat) : !filter(fDat);
                        if (doAdd) fList.Add(fDat);
                    }
                }
                #endregion
                return fList.ToArray();
            }
        }

        #endregion
    }

}
