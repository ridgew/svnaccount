using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using VistaDB.Provider;
using FIPFAPI.Cases;

namespace FIPFAPI
{
    /// <summary>
    /// 
    /// </summary>
    public class VistaDBProjectPackage : IProjectPackage
    {
        public VistaDBProjectPackage()
        {
            conn = new VistaDBConnection("Data Source=" + AppDomain.CurrentDomain.BaseDirectory 
                + "\\App_Data\\FmqStore.vdb3");
            conn.Open();
        }

        private VistaDBConnection conn;
        
        #region IProjectPackage 成员
        public void PackageFile(FileInfo pkgFileInfo)
        {
            string sql = "insert into [FileSystem](FileName,FileDir,FileSize,HashCode,BinData,FileVersion,CreateDate,LastChangeDate) values("
                    + "@FileName,@FileDir,@FileSize,@HashCode,@BinData,1, @CreateDate, @LastChangeDate"
                    + ")";
            VistaDBCommand cmd = new VistaDBCommand(sql, conn);
            cmd.Parameters.AddWithValue("@FileName", pkgFileInfo.Name);
            cmd.Parameters.AddWithValue("@FileDir", pkgFileInfo.DirectoryName.Replace(BaseDir, "").Replace("\\", "/"));
            cmd.Parameters.AddWithValue("@FileSize", pkgFileInfo.Length);

            byte[] fBin = InitialProject.GetFileBytes(pkgFileInfo.FullName);
            cmd.Parameters.AddWithValue("@HashCode", InitialProject.GetMD5Hash(fBin));
            cmd.Parameters.AddWithValue("@BinData", fBin);
            cmd.Parameters.AddWithValue("@CreateDate", pkgFileInfo.CreationTimeUtc);
            cmd.Parameters.AddWithValue("@LastChangeDate", pkgFileInfo.LastWriteTimeUtc);

            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        /// <summary>
        /// 同步文件
        /// </summary>
        /// <param name="currentFile">The current file.</param>
        public void SynFileInfo(FileInfo currentFile)
        { 
            //检查项目中是否存在该文件(目录+文件名)
            //比较文件：是否更新？
            string sql = string.Format("select top 1 FileID from [FileSystem] where ProjectID={0} AND FileDir='{1}' AND FileName='{2}'",
                    1,
                    currentFile.DirectoryName.Replace(BaseDir, "").Replace("\\", "/"),
                    currentFile.Name);
            VistaDBCommand cmd = new VistaDBCommand(sql, conn);
            object oFileID = cmd.ExecuteScalar();
            if (oFileID == null)
            {
                PackageFile(currentFile);
            }
            else
            {
                #region 比较版本

                #endregion

                #region 插入新文件
                sql = "insert into [FileSystem](FileName,FileDir,FileSize,HashCode,BinData,FileVersion,CreateDate,LastChangeDate) values("
                            + "@FileName,@FileDir,@FileSize,@HashCode,@BinData,2, @CreateDate, @LastChangeDate"
                            + ")";
                VistaDBCommand cmdAdd = new VistaDBCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FileName", currentFile.Name);
                cmd.Parameters.AddWithValue("@FileDir", currentFile.DirectoryName.Replace(BaseDir, "").Replace("\\", "/"));
                cmd.Parameters.AddWithValue("@FileSize", currentFile.Length);

                byte[] fBin = InitialProject.GetFileBytes(currentFile.FullName);
                cmd.Parameters.AddWithValue("@HashCode", InitialProject.GetMD5Hash(fBin));
                cmd.Parameters.AddWithValue("@BinData", fBin);
                cmd.Parameters.AddWithValue("@CreateDate", currentFile.CreationTimeUtc);
                cmd.Parameters.AddWithValue("@LastChangeDate", currentFile.LastWriteTimeUtc);

                cmd.ExecuteNonQuery();
                cmd.Dispose(); 
                #endregion
            }  
        }

        public void SynProject(float targetVersion)
        {
            string sql = "select ";
        }

        public List<FileInfo> GetPackageFileInfo()
        {
            throw new NotImplementedException();
        }

        public ProjectInfo ProjectItem
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public string BaseDir
        {
            get; set;
        }

        

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (conn != null)
            {
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion
    }
}
