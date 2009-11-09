using System;
using System.Collections.Generic;

namespace SynUtil
{
    [Serializable]
    public class SynchronizationFile
    {
        public SynchronizationFile()
        {
            Structure = new Dictionary<string, SynFile>(StringComparer.InvariantCultureIgnoreCase);
            AddFiles = new Dictionary<string, SynFile>(StringComparer.InvariantCultureIgnoreCase);
            UpdateFiles = new Dictionary<string, SynFile>(StringComparer.InvariantCultureIgnoreCase);
            RemoveFiles = new Dictionary<string, SynFile>(StringComparer.InvariantCultureIgnoreCase);
            SkipFiles = new Dictionary<string, SynFile>(StringComparer.InvariantCultureIgnoreCase);
            RenameList = new SynFileRename[0];
        }

        /// <summary>
        /// 同步时间UTC
        /// </summary>
        public DateTime SynTimeUTC { get; set; }

        public int TotalFileCount { get; set; }

        public long TotalFileSize { get; set; }

        /// <summary>
        /// 现有结构
        /// </summary>
        public Dictionary<string, SynFile> Structure { get; set; }

        /// <summary>
        /// 与上次相比添加了哪些文件
        /// </summary>
        public Dictionary<string, SynFile> AddFiles { get; set; }

        /// <summary>
        /// 与上次相比更新了哪些文件
        /// </summary>
        public Dictionary<string, SynFile> UpdateFiles { get; set; }

        /// <summary>
        /// 与上次相比删除了哪些文件
        /// </summary>
        public Dictionary<string, SynFile> RemoveFiles { get; set; }

        /// <summary>
        /// 与上次相比重命名了哪些文件
        /// </summary>
        public SynFileRename[] RenameList { get; set; }

        /// <summary>
        /// 忽略自动同步的文件集
        /// </summary>
        public Dictionary<string, SynFile> SkipFiles { get; set; }
    }

    [Serializable]
    public struct SynFileRename
    {
        public SynFile OldSynFile { get; set; }

        public SynFile NewSynFile { get; set; }
    }

    /// <summary>
    /// 同步文件对象引用
    /// </summary>
    [Serializable]
    public class SynFile : ICloneable
    {
        private string _filePathID = null;
        /// <summary>
        /// 命名标识
        /// </summary>
        public string FileID
        {
            get { return _filePathID; }
            set { _filePathID = value; }
        }

        public string HashCode { get; set; }

        public long FileSize { get; set; }

        public DateTime CreateDateUTC { get; set; }

        public DateTime LastModifyDateUTC { get; set; }

        public bool IsDirectory { get; set; }

        public bool Contains(SynFile file)
        {
            return (file != null
                && this.IsDirectory == true
                && this.FileID.Length < file.FileID.Length
                && file.FileID.StartsWith(this.FileID, StringComparison.InvariantCultureIgnoreCase));
        }


        #region ICloneable 成员

        /// <summary>
        /// 创建作为当前实例副本的新对象。
        /// </summary>
        /// <returns>作为此实例副本的新对象。</returns>
        public object Clone()
        {
            return new SynFile
            {
                FileID = this.FileID,
                HashCode = this.HashCode,
                FileSize = this.FileSize,
                CreateDateUTC = this.CreateDateUTC,
                IsDirectory = this.IsDirectory,
                LastModifyDateUTC = this.LastModifyDateUTC
            };
        }

        #endregion
    }
}
