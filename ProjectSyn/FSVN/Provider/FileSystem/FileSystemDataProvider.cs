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
            if (!Directory.Exists(reposDir))
            {
                Directory.CreateDirectory(reposDir);
                //创建数据目录
                Directory.CreateDirectory(Path.Combine(reposDir, "dat"));

                //创建日志目录
                Directory.CreateDirectory(Path.Combine(reposDir, "log"));
            }
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

        /// <summary>
        /// 存储库数据
        /// </summary>
        /// <param name="repos">库数据实例</param>
        public void StoreRepositoryData(ProjectRepository repos)
        {
            string reposDir = Path.Combine(RootDirName, repos.RepositoryId);
            if (!Directory.Exists(reposDir))
            {
                Directory.CreateDirectory(reposDir);
                //创建数据目录
                Directory.CreateDirectory(Path.Combine(reposDir, "dat"));

                //创建日志目录
                Directory.CreateDirectory(Path.Combine(reposDir, "log"));
            }

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
                fsvnFile = datDir + datArray[i].IdentityName.Replace('/', '\\') + ".fsvn";
                //文件删除
                if (File.Exists(fsvnFile))
                {
                    byte[] fileDat = File.ReadAllBytes(fsvnFile);
                    ProjectData oldData = fileDat.GetObject<ProjectData>();

                    //备份
                    File.WriteAllBytes(fsvnFile + ".r" + oldData.Reversion, fileDat);
                    ListDel.Add(datArray[i].IdentityName);
                }
                else
                {
                    //目录删除 (.del)

                }

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

        #endregion
    }

}
