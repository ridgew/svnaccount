using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SynUtil
{
    public class BuildSynFileWorker : IFileItemWorker
    {
        public BuildSynFileWorker(string baseDirectory, SynchronizationFile current, SynchronizationFile old)
        {
            BaseDir = baseDirectory;
            CurrentSynFile = current;
            CmpSynFile = old;
        }

        SynchronizationFile CurrentSynFile = null;
        SynchronizationFile CmpSynFile = null;

        #region IFileItemWorker 成员

        /// <summary>
        /// Packages the file.
        /// </summary>
        /// <param name="pkgFileInfo">The PKG file info.</param>
        public void PackageFile(FileInfo pkgFileInfo)
        {
            
            //pkgFileInfo.FullName.Replace(BaseDir, "").Replace('/', '\\').TrimStart('\\');
            string md5Hash = "N/A";
            try
            {
                md5Hash = GlobalUtil.GetMD5Hash(pkgFileInfo.FullName);
            }
            catch (Exception) {  }
            //Console.WriteLine("MD5:" + md5Hash);

            string fileID = pkgFileInfo.FullName.Replace(BaseDir, "").Replace('\\', '/').TrimStart('/');

            SynFile currentFile = new SynFile
                    {
                        IsDirectory = false,
                        FileID = fileID,
                        FileSize = pkgFileInfo.Length,
                        HashCode = md5Hash,
                        CreateDateUTC = pkgFileInfo.CreationTimeUtc,
                        LastModifyDateUTC = pkgFileInfo.LastWriteTimeUtc
                    };

            if (!CurrentSynFile.Structure.ContainsKey(fileID))
            {
                CurrentSynFile.Structure.Add(fileID, currentFile);
                CurrentSynFile.TotalFileCount++;
                CurrentSynFile.TotalFileSize += pkgFileInfo.Length;
            }

            if (CmpSynFile != null)
            {
                //旧的不存在而新的存在
                if (!CmpSynFile.Structure.ContainsKey(fileID))
                {
                    CurrentSynFile.AddFiles.Add(fileID, currentFile);
                }
                else
                {
                    SynFile oldFile = CmpSynFile.Structure[fileID];
                    if (!CurrentSynFile.Structure.ContainsKey(fileID))
                    {
                        //CurrentSynFile.TotalFileSize -= oldFile.FileSize;
                        CurrentSynFile.RemoveFiles.Add(fileID, currentFile);
                    }
                    else
                    {
                        if (string.Compare(oldFile.HashCode, currentFile.HashCode, true) != 0)
                        {
                            //CurrentSynFile.TotalFileSize -= oldFile.FileSize;
                            //CurrentSynFile.TotalFileSize += pkgFileInfo.Length;
                            CurrentSynFile.UpdateFiles.Add(fileID, currentFile);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 基础根路径
        /// </summary>
        /// <value>The base dir.</value>
        public string BaseDir
        {
            get;
            set;
        }

        #endregion

        #region IDisposable 成员

        /// <summary>
        /// 执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {

        }

        #endregion
    }
}
