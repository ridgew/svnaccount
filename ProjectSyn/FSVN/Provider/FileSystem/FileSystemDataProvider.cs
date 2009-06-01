using System;
using System.Collections.Generic;
using System.IO;
using FSVN.Config;
using FSVN.Data;
using FSVN.Util;
using System.Text.RegularExpressions;

namespace FSVN.Provider
{
    /// <summary>
    /// 文件系统存储的数据提供者实现
    /// </summary>
    public class FileSystemDataProvider : IProjectDataProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemDataProvider"/> class.
        /// </summary>
        public FileSystemDataProvider()
            : base()
        {

        }

        /*
         TODO: 递归更新父级目录描述文件
         */

        /// <summary>
        /// 版本库根目录
        /// </summary>
        public string RootDirName { get; set; }


        #region IProjectDataProvider 成员

        /// <summary>
        /// 获取指定版本库标志的版本库数据
        /// </summary>
        /// <param name="repositoryID">版本库标志</param>
        public ProjectRepository GetRepositoryData(string repositoryID)
        {
            string reposDir = Path.Combine(RootDirName, repositoryID);
            InitialRepositoryDir(reposDir);

            string reposFile = Path.Combine(reposDir, "Repository.dat");
            if (!File.Exists(reposFile))
            {
                return null;
            }
            else
            {
                return File.ReadAllBytes(reposFile).GetObject<ProjectRepository>();
            }
        }

        private  string[] subDirs = new string[] { "dat",  //数据目录
                    "log",      //日志目录
                    "$del",     //删除备份目录
                    "$mov"};    //重命名和移动目录

        private void InitialRepositoryDir(string reposDir)
        {
            if (!Directory.Exists(reposDir))
            {
                Directory.CreateDirectory(reposDir);

                foreach (string sDir in subDirs)
                {
                    Directory.CreateDirectory(Path.Combine(reposDir, sDir));
                }
            }
        }

        /// <summary>
        /// 版本库目录类型
        /// </summary>
        internal enum RepositoryDirectory : byte
        {
             /// <summary>
            /// 数据目录
             /// </summary>
             Data = 0,
             /// <summary>
             /// 日志目录
             /// </summary>
             Log = 1,
             /// <summary>
             /// 删除备份目录
             /// </summary>
             Delete = 2,
             /// <summary>
             /// 重命名和移动目录
             /// </summary>
             Move = 3
        }

        /// <summary>
        /// 获取库的下级目录
        /// </summary>
        private string GetSubDirByType(string repositoryId, RepositoryDirectory dirType)
        {
            string reposDir = Path.Combine(RootDirName, repositoryId);
            return subDirs[dirType.GetHashCode()];
        }

        /// <summary>
        /// 存储库数据
        /// </summary>
        /// <param name="repos">库数据实例</param>
        public void StoreRepositoryData(ProjectRepository repos)
        {
            string reposDir = Path.Combine(RootDirName, repos.RepositoryId);
            InitialRepositoryDir(reposDir);

            //创建版本库配置文件
            File.WriteAllBytes(Path.Combine(reposDir, "Repository.dat"), ExtensionHelper.GetBytes(repos));

        }

        /// <summary>
        /// 初始化提供者
        /// </summary>
        public void Initialize()
        {
            RootDirName = @"D:\FSVNRepositories";
        }

        /// <summary>
        /// 判断版本库中是否存在标识号的数据
        /// </summary>
        /// <param name="repositoryID">版本库标志</param>
        /// <param name="identityName">项目数据标识号</param>
        /// <returns>存在则返回为true，不存在为false。</returns>
        public bool Exists(string repositoryID, string identityName)
        {
            string datDir = string.Concat(RootDirName, "\\", repositoryID, "\\", "dat", "\\");
            string localFilePath = Path.Combine(datDir, identityName.Replace('/', '\\'));
            return File.Exists(localFilePath);
        }

        /// <summary>
        /// 存储项目数据
        /// </summary>
        /// <param name="datArray">项目数据队列集合</param>
        /// <param name="memo">变更备忘</param>
        public void Store(ProjectData[] datArray, string memo)
        {
            if (datArray == null || datArray.Length < 1) return;
            /*
             1.存储目录
             2.存储文件名
             3.存储数据
             4.设置创建、修改日期
             5.记录变更日志
             */
            List<string> ListAdd = new List<string>();
            List<string> ListUpdate = new List<string>();

            int total = datArray.Length;
            ProjectData CommonData = datArray[0];
            ProjectRepository repos = new ProjectRepository() { RepositoryId = CommonData.RepositoryId };

            string ReversionID = repos.GetNextReversionID();
            string datDir = string.Concat(RootDirName, "\\", repos.RepositoryId, "\\", "dat", "\\");
            string logDir = string.Concat(RootDirName, "\\", repos.RepositoryId, "\\", "log", "\\");
            string fsvnFile = string.Empty;

            for (int i = 0; i < total; i++)
            {
                #region 循环项目数据

                if (datArray[i].IncludeBinary)
                {
                    //文件
                    fsvnFile = datDir + datArray[i].IdentityName.Replace('/', '\\') + ".fsvn";
                }
                else
                { 
                    //目录
                    string tarDir = Path.Combine(datDir, datArray[i].IdentityName.Replace('/', '\\'));
                    if (!Directory.Exists(tarDir))  Directory.CreateDirectory(tarDir);
                    fsvnFile = tarDir + "\\.fsvn";
                }

                datArray[i].Reversion = ReversionID;
                //添加、修改
                if (File.Exists(fsvnFile))
                {
                    byte[] fileDat = File.ReadAllBytes(fsvnFile);
                    ProjectData oldData = fileDat.GetObject<ProjectData>();
                    datArray[i].LastModifiedVersion = oldData.Reversion;

                    //备份
                    File.WriteAllBytes(fsvnFile + ".r" + oldData.Reversion, fileDat);
                    ListAdd.Add(datArray[i].IdentityName);
                }
                else
                {
                    datArray[i].LastModifiedVersion = ReversionID;
                    ListUpdate.Add(datArray[i].IdentityName); 
                }

                File.WriteAllBytes(fsvnFile, datArray[i].GetBytes());
                Console.WriteLine("正在提交：{0}", datArray[i].IdentityName);

                #endregion
            }

            #region 变更日志
            List<ChangeAction> cActs = new List<ChangeAction>();
            if (ListAdd.Count > 0)
            {
                cActs.Add(new AddAction { IdentityNames = ListAdd.ToArray() });
            }
            if (ListUpdate.Count > 0)
            {
                cActs.Add(new UpdateAction { IdentityNames = ListUpdate.ToArray() });
            }

            ChangeLog log = new ChangeLog();
            log.Author = CommonData.Author;
            log.Message = memo;
            log.RepositoryId = repos.RepositoryId;
            log.ReversionId = ReversionID;
            log.Summary = cActs.ToArray().GetBytes();
            File.WriteAllBytes(Path.Combine(logDir, "Rev" +log.ReversionId+".dat"), log.GetBytes());
            #endregion

        }

        /// <summary>
        /// 删除项目数据
        /// </summary>
        /// <param name="datArray">项目数据标志队列集合</param>
        /// <param name="memo">变更备忘</param>
        public void Delete(ProjectDataID[] datArray, string memo)
        {
            if (datArray == null || datArray.Length < 1) return;
            int total = datArray.Length;
            ProjectDataID CommonData = datArray[0];
            ProjectRepository repos = new ProjectRepository() { RepositoryId = CommonData.RepositoryId };

            List<string> ListDel = new List<string>();

            string ReversionID = repos.GetNextReversionID();
            string datDir = string.Concat(RootDirName, "\\", repos.RepositoryId, "\\", "dat", "\\");
            string logDir = string.Concat(RootDirName, "\\", repos.RepositoryId, "\\", "log", "\\");
            string fsvnFile = string.Empty;

            for (int i = 0; i < total; i++)
            {
                if (!Exists(repos.RepositoryId, datArray[i].IdentityName))
                {
                    Console.WriteLine("{0} 在版本库中不存在！", datArray[i].IdentityName);
                    continue;
                }

                ListDel.Add(datArray[i].IdentityName);

                fsvnFile = datDir + datArray[i].IdentityName.Replace('/', '\\') + ".fsvn";
                //文件删除
                if (File.Exists(fsvnFile))
                {
                    byte[] fileDat = File.ReadAllBytes(fsvnFile);
                    ProjectData oldData = fileDat.GetObject<ProjectData>();

                    //备份
                    File.WriteAllBytes(fsvnFile + ".r" + oldData.Reversion, fileDat);

                    //移动文件 [fsvnFile].r*

                    //删除
                    File.Delete(fsvnFile);
                }
                else
                {
                    //目录删除 (.del)
                    string oldDir = datDir + datArray[i].IdentityName.Replace('/', '\\');
                    string delDir = GetSubDirByType(repos.RepositoryId, RepositoryDirectory.Delete) + "\\rev" + ReversionID;
                    delDir += datArray[i].GetParentName();
                    delDir = Path.Combine(RootDirName, repos.RepositoryId + "\\" + delDir);

                    Console.WriteLine("创建目录：{0}", delDir);
                    Console.WriteLine("移动目录：{0} -> {1} ", oldDir, delDir + "\\" + datArray[i].GetName());

                    if (!Directory.Exists(delDir)) Directory.CreateDirectory(delDir);
                    Directory.Move(oldDir, delDir + "\\" + datArray[i].GetName());
                }

                //更新目录下的 .fsvn文件

                Console.WriteLine("正在删除：{0}", datArray[i].IdentityName);
            }

            #region 变更日志
            List<ChangeAction> cActs = new List<ChangeAction>();
            if (ListDel.Count > 0)
            {
                cActs.Add(new DeleteAction { IdentityNames = ListDel.ToArray() });
            }

            ChangeLog log = new ChangeLog();
            log.Author = "ridge";
            log.Message = memo;
            log.RepositoryId = repos.RepositoryId;
            log.ReversionId = ReversionID;
            log.Summary = cActs.ToArray().GetBytes();
            File.WriteAllBytes(Path.Combine(logDir, "Rev" + log.ReversionId + ".dat"), log.GetBytes());
            #endregion
        }

        /// <summary>
        /// 获取指定容器标志内的项目数据集合
        /// </summary>
        /// <param name="RepositoryId">版本库标志编号</param>
        /// <param name="ContainerIdentityName">包含容器的项目数据标志号，为null则为根下的项目数据。</param>
        /// <param name="rev">获取库中的特定版本：最新版本为$HEAD$。</param>
        /// <returns>项目数据集合</returns>
        public ProjectData[] GetDataList(string RepositoryId, string ContainerIdentityName, string rev)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取指定容器标志内的项目数据集合
        /// </summary>
        /// <param name="RepositoryId">版本库标志编号</param>
        /// <param name="IdentityName">项目数据标志编号</param>
        /// <param name="rev">获取库中的特定版本：最新版本为$HEAD$。</param>
        /// <returns>如果存在则返回项目数据，否则为null。</returns>
        public ProjectData GetProjectData(string RepositoryId, string IdentityName, string rev)
        {
            string datDir = string.Concat(RootDirName, "\\", RepositoryId, "\\", "dat", "\\");
            string localFilePath = Path.Combine(datDir, IdentityName.Replace('/', '\\'));
            if (rev.Equals("$HEAD$", StringComparison.InvariantCultureIgnoreCase))
            {
                localFilePath += ".fsvn";
            }
            else
            {
                datDir = Path.Combine(Path.GetDirectoryName(localFilePath), "Rev" + rev);
                localFilePath = Path.Combine(datDir, Path.GetFileName(localFilePath) + ".fsvn");
            }

            if (!File.Exists(localFilePath))
            {
                //目录查找
                localFilePath = Regex.Replace(localFilePath, @"(\.fsvn)$", "\\$1", RegexOptions.IgnoreCase);
                if (!File.Exists(localFilePath)) return null;
            }
            //Console.WriteLine(localFilePath);
            byte[] svnBytes = File.ReadAllBytes(localFilePath);
            return svnBytes.GetObject<ProjectData>();
        }

        /// <summary>
        /// 获取变更到指定版本的变更日志
        /// </summary>
        /// <param name="RepositoryId">版本库标志编号</param>
        /// <param name="startRev">起始版本</param>
        /// <param name="rev">获取库中的特定版本：最新版本为$HEAD$。</param>
        /// <returns>变更日志数据集合</returns>
        public ChangeLog[] GetReversionLogs(string RepositoryId, long startRev, string rev)
        {
            ProjectRepository repos = new ProjectRepository() { RepositoryId = RepositoryId };
            repos.DataBind();

            List<ChangeLog> logList = new List<ChangeLog>();
            long targetVer =  Convert.ToInt64(repos.Reversion);

            if (!rev.Equals("$HEAD$", StringComparison.InvariantCultureIgnoreCase))
            {
                targetVer = Convert.ToInt64(rev);
            }

            if (targetVer > startRev && startRev >= 1)
            {
                string logDir = string.Concat(RootDirName, "\\", RepositoryId, "\\", "log", "\\");

                //Path.Combine(logDir, "Rev" +log.ReversionId+".dat")
                string logFile = string.Empty;
                for (; targetVer >= startRev; targetVer--)
                {
                    logFile = Path.Combine(logDir, "Rev" + targetVer + ".dat");
                    if (!File.Exists(logFile))
                    {
                        Console.WriteLine("版本为[{0}]的变更日志已不存在！", targetVer);
                    }
                    else
                    {
                        logList.Add(File.ReadAllBytes(logFile).GetObject<ChangeLog>());
                    }
                }
            }
            return logList.ToArray();
        }
        #endregion
    }

}
