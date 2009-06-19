using System;
using System.IO;
using Vbyte.DataSource.Unitity;
using System.Collections.Generic;

namespace Vbyte.DataSource
{
    /// <summary>
    /// 本地文件数据
    /// </summary>
    [Serializable]
    public sealed class LocalFileData : AbstractData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileData"/> class.
        /// </summary>
        public LocalFileData()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFileData"/> class.
        /// </summary>
        /// <param name="filePathName">本地文件路径</param>
        public LocalFileData(string filePathName)
        {
            IdentityName = filePathName;
        }

        /// <summary>
        /// 标志名称，类型URL地址定位等。
        /// </summary>
        /// <value></value>
        public override string IdentityName
        {
            get
            {
                return base.IdentityName;
            }
            set
            {
                IsContainer = Directory.Exists(value.Replace('/', '\\'));
                base.IdentityName = value;
            }
        }
        

        /// <summary>
        /// 获取子级数据项
        /// </summary>
        /// <returns></returns>
        public override IDataItem[] GetChildren()
        {
            if (!IsContainer)
            {
                return null;
            }
            else
            {
                DirectoryInfo curDirInfo = new DirectoryInfo(IdentityName.Replace('/', '\\'));
                List<LocalFileData> fList = new List<LocalFileData>();
                LocalFileData fDat = null;
                #region 文件
                foreach (FileInfo file in curDirInfo.GetFiles())
                {
                    fDat = new LocalFileData(file.FullName);
                    fDat.IsContainer = false;
                    fList.Add(fDat);
                }
                #endregion

                #region 目录
                foreach (DirectoryInfo di in curDirInfo.GetDirectories())
                {
                    fDat = new LocalFileData(di.FullName);
                    fDat.IsContainer = true;
                    fList.Add(fDat);
                }
                #endregion
                return fList.ToArray();
            }
        }

        /// <summary>
        /// 上级数据项
        /// </summary>
        /// <returns></returns>
        public override IDataItem GetParent()
        {
            DirectoryInfo tarDi = null;
            if (IsContainer) 
            {
                tarDi = new DirectoryInfo(IdentityName.Replace('/', '\\')).Parent; 
            }
            else
            {
                tarDi = new DirectoryInfo(Path.GetDirectoryName(IdentityName.Replace('/', '\\')));
            }

            if (!tarDi.Exists)
            {
                return null;
            }
            else
            {
                LocalFileData fDat = new LocalFileData(tarDi.FullName);
                fDat.IsContainer = true;
                return fDat;
            }
        }

        /// <summary>
        /// 相关属性数据绑定
        /// </summary>
        public override void DataBind()
        {
            string tarFile = (IsContainer) ? IdentityName + "\\" + FileSystemStorage.FS_DIRDAT : IdentityName;
            tarFile = tarFile.Replace('/', '\\');
            if (!File.Exists(tarFile)) return;

            byte[] diskDat = File.ReadAllBytes(tarFile);
            LocalFileData fDat = FileWrapHelper.UnWrapObject(diskDat) as LocalFileData;
            if (fDat != null)
            {
                this.Alias = fDat.Alias;
                this.IsContainer = fDat.IsContainer;
                this.CreateDateTimeUTC = fDat.CreateDateTimeUTC;
                this.ModifiedDateTimeUTC = fDat.ModifiedDateTimeUTC;
                this.Name = fDat.Name;
                this.RawData = fDat.RawData;
            }
        }

    }
}
